using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using Google.Protobuf;

using HermesProxy.Framework.Constants;
using HermesProxy.Framework.Logging;
using HermesProxy.Network.BattleNet.Session;

namespace HermesProxy.Network.BattleNet.Services
{
    public static class ServiceHandler
    {
        static Dictionary<(ServiceHash Hash, uint MethodID), BattlenetHandler> _serviceHandlers = new();

        /// <summary>
        /// Initialize the <see cref="ServiceHandler"/> and loads all <see cref="BattlenetServiceAttribute"/> instances.
        /// </summary>
        public static void Initialize()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (var method in type.GetMethods())
                {
                    var attribute = method.GetCustomAttribute<BattlenetServiceAttribute>();
                    if (attribute == null)
                        continue;

                    var key = (attribute.Hash, attribute.MethodID);
                    if (_serviceHandlers.ContainsKey(key))
                    {
                        Log.Print(LogType.Error, $"Tried to override ServiceHandler: {_serviceHandlers[key]} with {method.Name} (ServiceHash: {attribute.Hash} MethodID: {attribute.MethodID})");
                        continue;
                    }

                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        Log.Print(LogType.Error, $"Method: {method.Name} has incorrect parameter count, needs to be atleast one (ServiceHash: {attribute.Hash} MethodID: {attribute.MethodID})");
                        continue;
                    }

                    _serviceHandlers.Add(key, new(method, parameters));
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="BattlenetHandler"/> instance from the provided <see cref="ServiceHash"/> and <paramref name="methodId"/>
        /// </summary>
        public static BattlenetHandler GetHandler(ServiceHash hash, uint methodId)
        {
            if (!_serviceHandlers.TryGetValue((hash, methodId), out var handler))
                return null;

            return handler;
        }
    }

    public class BattlenetHandler
    {
        readonly Delegate _methodCaller;
        readonly Type _requestType;
        readonly Type _responseType;

        public BattlenetHandler(MethodInfo info, ParameterInfo[] parameters)
        {
            _requestType = parameters[0].ParameterType;
            if (parameters.Length > 1)
                _responseType = parameters[1].ParameterType;

            if (_responseType != null)
                _methodCaller = info.CreateDelegate(Expression.GetDelegateType(new[] { typeof(BattlenetSession), _requestType, _responseType, info.ReturnType }));
            else                                                                       
                _methodCaller = info.CreateDelegate(Expression.GetDelegateType(new[] { typeof(BattlenetSession), _requestType, info.ReturnType }));
        }

        /// <summary>
        /// Invokes the <see cref="BattlenetHandler"/> instance with the provided <see cref="BattlenetSession"/>, <paramref name="token"/> and <see cref="CodedInputStream"/> instances.
        /// </summary>
        public async Task Invoke(BattlenetSession session, uint token, CodedInputStream stream)
        {
            var request = (IMessage)Activator.CreateInstance(_requestType);
            request.MergeFrom(stream);

            if (_responseType != null)
            {
                var response = (IMessage)Activator.CreateInstance(_responseType);

                BattlenetRpcErrorCode errorCode = BattlenetRpcErrorCode.Ok;
                if (_methodCaller.Method.ReturnType.IsSubclassOf(typeof(Task)))
                {
                    if (_methodCaller.Method.ReturnType.IsConstructedGenericType)
                    {
                        dynamic tmp = _methodCaller.DynamicInvoke(session, request, response);
                        errorCode = (BattlenetRpcErrorCode)tmp.GetAwaiter().GetResult();
                    }
                }
                else
                    errorCode = (BattlenetRpcErrorCode)_methodCaller.DynamicInvoke(session, request, response);

                Log.Print(LogType.Debug, $"Session: {session.GetRemoteEndpoint()} Request: {request} Response: {response} with status: {errorCode}");

                if (errorCode == BattlenetRpcErrorCode.Ok)
                    await session.SendResponse(token, response);
                else
                    await session.SendResponse(token, errorCode);
            }
            else
            {
                BattlenetRpcErrorCode errorCode = BattlenetRpcErrorCode.Ok;
                if (_methodCaller.Method.ReturnType.IsSubclassOf(typeof(Task)))
                {
                    if (_methodCaller.Method.ReturnType.IsConstructedGenericType)
                    {
                        dynamic tmp = _methodCaller.DynamicInvoke(session, request);
                        errorCode = (BattlenetRpcErrorCode)tmp.GetAwaiter().GetResult();
                    }
                }
                else
                    errorCode = (BattlenetRpcErrorCode)_methodCaller.DynamicInvoke(session, request);

                Log.Print(LogType.Debug, $"Session: {session.GetRemoteEndpoint()} Request: {request} with status: {errorCode}");

                if (errorCode != BattlenetRpcErrorCode.Ok)
                    await session.SendResponse(token, errorCode);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class BattlenetServiceAttribute : Attribute
    {
        public ServiceHash Hash { get; set; }
        public uint MethodID { get; set; }

        public BattlenetServiceAttribute(ServiceHash hash, uint methodID)
        {
            Hash = hash;
            MethodID = methodID;
        }
    }
}
