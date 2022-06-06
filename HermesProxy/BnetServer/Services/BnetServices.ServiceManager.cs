using System;
using System.Collections.Concurrent;
using System.Reflection;
using Framework.Constants;
using Framework.Logging;
using Google.Protobuf;
using HermesProxy;

namespace BNetServer.Services
{
    public partial class BnetServices
    {
        public class ServiceManager
        {
            static ServiceManager()
            {
                // TODO: Replace with compile time generator
                Assembly currentAsm = Assembly.GetExecutingAssembly();
                foreach (var type in currentAsm.GetTypes())
                {
                    foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
                    {
                        foreach (var serviceAttr in methodInfo.GetCustomAttributes<ServiceAttribute>())
                        {
                            if (serviceAttr == null)
                                continue;

                            var key = (serviceAttr.ServiceHash, serviceAttr.MethodId);
                            if (_serviceHandlers.ContainsKey(key))
                            {
                                Log.Print(LogType.Error, $"Tried to override ServiceHandler: {_serviceHandlers[key]} with {methodInfo.Name} (ServiceHash: {serviceAttr.ServiceHash} MethodId: {serviceAttr.MethodId})");
                                continue;
                            }

                            var parameters = methodInfo.GetParameters();
                            if (parameters.Length == 0)
                            {
                                Log.Print(LogType.Error, $"Method: {methodInfo.Name} needs atleast one parameter");
                                continue;
                            }

                            _serviceHandlers[key] = new BnetServiceHandlerInfo(serviceAttr.Requirement, methodInfo, parameters);
                        }
                    }
                }
            }

            private static readonly ConcurrentDictionary<(OriginalHash Service, uint MethodId), BnetServiceHandlerInfo> _serviceHandlers = new();
            private readonly BnetServices _serviceHolder;

            public ServiceManager(string connectionPath, INetwork net, GlobalSessionData? initialSession)
            {
                _serviceHolder = new BnetServices(connectionPath, net, initialSession);
            }

            public void SetClientSecret(byte[] key)
            {
                for (int i = 0; i < Math.Min(_serviceHolder._clientSecret.Length, key.Length); i++)
                {
                    _serviceHolder._clientSecret[i] = key[i];
                }
            }

            public void Invoke(uint serviceId, OriginalHash serviceHash, uint methodId, uint requestToken, CodedInputStream stream)
            {
                void SendRpcMessage(BattlenetRpcErrorCode status, IMessage? message)
                {
                    if (_serviceHolder._connectionPath == "WorldSocket")
                        _serviceHolder._net.SendRpcMessage(serviceId, serviceHash, methodId, requestToken, status, message);
                    else
                        _serviceHolder._net.SendRpcMessage(0xFE, serviceHash, methodId, requestToken, status, message);
                }

                void SendErrorResponse(BattlenetRpcErrorCode errorCode)
                {
                    SendRpcMessage(errorCode, null);
                }

                void SendResponse(IMessage response)
                {
                    SendRpcMessage(BattlenetRpcErrorCode.Ok, response);
                }

                if (!_serviceHandlers.TryGetValue((serviceHash, methodId), out var handler))
                {
                    _serviceHolder.ServiceLog(LogType.Warn, $"Client requested service {serviceHash}/m:{methodId} but this service is not implemented?");
                    SendErrorResponse(BattlenetRpcErrorCode.RpcNotImplemented);
                    return;
                }

                if (handler.Requirement != ServiceRequirement.Always && handler.Requirement != _serviceHolder.CurrentMatchingRequirement())
                {
                    _serviceHolder.ServiceLog(LogType.Warn, $"Client requested service {serviceHash}/m:{methodId} but with invalid state, required: {handler.Requirement} but only has {_serviceHolder.CurrentMatchingRequirement()}!");
                    SendErrorResponse(BattlenetRpcErrorCode.Denied);
                    return;
                }

                _serviceHolder.ServiceLog(LogType.Debug, $"Client requested service {serviceHash}/m:{methodId}");

                var request = (IMessage)Activator.CreateInstance(handler.RequestType);
                request.MergeFrom(stream);

                BattlenetRpcErrorCode status;
                if (handler.ResponseType != null)
                {
                    var response = (IMessage)Activator.CreateInstance(handler.ResponseType);
                    status = (BattlenetRpcErrorCode)handler.MethodCaller.DynamicInvoke(_serviceHolder, request, response);

                    if (status == BattlenetRpcErrorCode.Ok)
                        SendResponse(response);
                    else
                        SendErrorResponse(status);
                }
                else
                {
                    status = (BattlenetRpcErrorCode)handler.MethodCaller.DynamicInvoke(_serviceHolder, request);

                    if (status != BattlenetRpcErrorCode.Ok)
                        SendErrorResponse(status);
                }
            }
        }
    }
}
