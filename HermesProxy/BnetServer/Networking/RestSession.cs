// Copyright (c) CypherCore <http://github.com/CypherCore> All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE. See LICENSE file in the project root for full license information.

using Framework.Constants;
using Framework.Networking;
using Framework.Serialization;
using Framework.Web;
using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HermesProxy.Auth;
using HermesProxy.Enums;
using HermesProxy.World.Server;

namespace BNetServer.Networking
{
    public class RestSession : SSLSocket
    {
        private const string BNET_SERVER_BASE_PATH = "/bnetserver/";
        private const string TICKET_PREFIX = "HP-"; // Hermes Proxy
        
        public RestSession(Socket socket) : base(socket) { }

        public override void Accept()
        {
            // Setup SSL connection
            AsyncHandshake(Global.LoginServiceMgr.GetCertificate());
        }

        public override async void ReadHandler(byte[] data, int receivedLength)
        {
            var httpRequest = HttpHelper.ParseRequest(data, receivedLength);
            if (httpRequest == null || !RequestRouter(httpRequest))
            {
                CloseSocket();
                return;
            }

            await AsyncRead(); // Read next request
        }

        public bool RequestRouter(HttpHeader httpRequest)
        {
            if (!httpRequest.Path.StartsWith(BNET_SERVER_BASE_PATH))
            {
                SendEmptyResponse(HttpCode.NotFound);
                return false;
            }

            string path = httpRequest.Path.Substring(BNET_SERVER_BASE_PATH.Length);
            string[] pathElements = path.Split('/');

            switch (pathElements[0], httpRequest.Method) 
            {
                case ("login", "GET"):
                    SendResponse(HttpCode.Ok, Global.LoginServiceMgr.GetFormInput());
                    return true;
                case ("login", "POST"):
                    HandleLoginRequest(pathElements, httpRequest);
                    return true;
                default:
                    SendEmptyResponse(HttpCode.NotFound);
                    return false;
            };
        }

        public Task HandleLoginRequest(string[] pathElements, HttpHeader request)
        {
            LogonData loginForm = Json.CreateObject<LogonData>(request.Content);
            if (loginForm == null)
                return SendEmptyResponse(HttpCode.InternalServerError);

            HermesProxy.GlobalSessionData globalSession = new();

            // Format: "login/$platform/$build/$locale/"
            globalSession.OS = pathElements[1];
            globalSession.Build = UInt32.Parse(pathElements[2]);
            globalSession.Locale = pathElements[3];

            // Should never happen. Session.HandleLogon checks version already
            if (Framework.Settings.ClientBuild != (ClientVersionBuild) globalSession.Build)
                return SendAuthError(AuthResult.FAIL_WRONG_MODERN_VER);

            string login = "";
            string password = "";

            foreach (var field in loginForm.Inputs)
            {
                switch (field.Id)
                {
                    case "account_name": login = field.Value.Trim().ToUpperInvariant(); break;
                    case "password": password = field.Value; break;
                }
            }

            globalSession.AuthClient = new(globalSession);
            AuthResult response = globalSession.AuthClient.ConnectToAuthServer(login, password, globalSession.Locale);
            if (response != AuthResult.SUCCESS)
            { // Error handling
                return SendAuthError(response);
            }
            else
            {
                // Request realmlist now, we probably need it later anyways
                globalSession.AuthClient.RequestRealmListUpdate();

                // Ticket creation
                LogonResult loginResult = new();
                byte[] ticket = Array.Empty<byte>().GenerateRandomKey(20);
                string loginTicket = TICKET_PREFIX + ticket.ToHexString();

                globalSession.LoginTicket = loginTicket;
                globalSession.Username = login;
                globalSession.AccountMetaDataMgr = new AccountMetaDataManager(login);
                Global.AddNewSessionByName(login, globalSession);
                Global.AddNewSessionByTicket(loginTicket, globalSession);

                loginResult.LoginTicket = loginTicket;
                loginResult.AuthenticationState = "DONE";
                return SendResponse(HttpCode.Ok, loginResult);
            }
        }

        async Task SendResponse<T>(HttpCode code, T response)
        {
            await AsyncWrite(HttpHelper.CreateResponse(code, Json.CreateString(response)));
        }

        async Task SendAuthError(AuthResult response)
        {
            LogonResult loginResult = new();
            (loginResult.AuthenticationState, loginResult.ErrorCode, loginResult.ErrorMessage) = response switch
            {
                AuthResult.FAIL_UNKNOWN_ACCOUNT    => ("LOGIN", "UNABLE_TO_DECODE", "Invalid username or password."),
                AuthResult.FAIL_INCORRECT_PASSWORD => ("LOGIN", "UNABLE_TO_DECODE", "Invalid password."),
                AuthResult.FAIL_BANNED             => ("LOGIN", "UNABLE_TO_DECODE", "This account has been closed and is no longer available for use."),
                AuthResult.FAIL_SUSPENDED          => ("LOGIN", "UNABLE_TO_DECODE", "This account has been temporarily suspended."),
                AuthResult.FAIL_VERSION_INVALID    => ("LOGIN", "UNABLE_TO_DECODE", "Your version is not supported by this server.\nMost likely HermesProxy is not allowed."),

                AuthResult.FAIL_INTERNAL_ERROR     => ("LOGON", "UNABLE_TO_DECODE", "There was an internal error. Please try again later."),
                _ => ("LOGON", "UNABLE_TO_DECODE", $"Error: {response}"),
            };

            await SendResponse(HttpCode.BadRequest, loginResult);
        }

        async Task SendEmptyResponse(HttpCode code)
        {
            await SendResponse<object>(code, new{});
        }
    }
}
