using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using BNetServer.Networking;
using Framework.Constants;
using Framework.Logging;
using Google.Protobuf;
using HermesProxy;

namespace BNetServer.Services
{
    public partial class BnetServices
    {
        private static uint _serverInvokedRequestToken = 0;
        private Dictionary<uint /*requestId*/, Action<CodedInputStream> /*callbackHandler*/> _callbackHandlers = new();

        private GlobalSessionData _globalSession;
        private readonly byte[] _clientSecret = new byte[32];
        private readonly string _connectionPath;
        private readonly INetwork _net;

        private BnetServices()
        {
            // Private constructor only called by ServiceManager
        }

        private BnetServices(string connectionPath, INetwork net, GlobalSessionData? initialSession)
        {
            _connectionPath = connectionPath;
            _net = net;
            _globalSession = initialSession;
        }

        public GlobalSessionData GetSession()
        {
            return _globalSession;
        }

        public GlobalSessionData Session => _globalSession;

        private void SendRequest(OriginalHash service, uint methodId, IMessage? data)
        {
            _serverInvokedRequestToken++;
            _net.SendRpcMessage(0, service, methodId, _serverInvokedRequestToken, BattlenetRpcErrorCode.Ok, data);
        }

        private void CloseSocket()
        {
            _net.CloseSocket();
        }

        private IPEndPoint GetRemoteIpEndPoint()
        {
            return _net.GetRemoteIpEndPoint();
        }

        private void ServiceLog(LogType type, string message)
        {
            StringBuilder prefix = new StringBuilder();
            prefix.Append($"[{_connectionPath}]");
            prefix.Append($"[{GetRemoteIpEndPoint()}");
            
            if (GetSession() != null)
            {
                if (GetSession().AccountInfo != null && !GetSession().AccountInfo.Login.IsEmpty())
                    prefix.Append(", Account: " + GetSession().AccountInfo.Login);

                if (GetSession().GameAccountInfo != null)
                    prefix.Append(", Game account: " + GetSession().GameAccountInfo.Name);
            }
            
            prefix.Append(']');

            Log.Print(type, $"{prefix} {message}");
        }

        public ServiceRequirement CurrentMatchingRequirement()
        {
            return _globalSession == null
                ? ServiceRequirement.Unauthorized
                : ServiceRequirement.LoggedIn;
        }

        public class BnetServiceHandlerInfo
        {
            public readonly ServiceRequirement Requirement;

            public readonly Delegate MethodCaller;
            public readonly Type RequestType;
            public readonly Type ResponseType;

            public BnetServiceHandlerInfo(ServiceRequirement requirement, MethodInfo info, ParameterInfo[] parameters)
            {
                Requirement = requirement;

                RequestType = parameters[0].ParameterType;
                if (parameters.Length > 1)
                    ResponseType = parameters[1].ParameterType;

                MethodCaller = info.CreateDelegate(
                    ResponseType != null
                        ? Expression.GetDelegateType(typeof(BnetServices), RequestType, ResponseType, info.ReturnType)
                        : Expression.GetDelegateType(typeof(BnetServices), RequestType, info.ReturnType)
                );
            }
        }

        public interface INetwork
        {
            public void SendRpcMessage(uint serviceId, OriginalHash service, uint methodId, uint token, BattlenetRpcErrorCode status, IMessage? message);
            public void CloseSocket();

            IPEndPoint GetRemoteIpEndPoint();
        }
    }
}
