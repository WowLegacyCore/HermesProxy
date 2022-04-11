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

namespace BNetServer.Networking
{
    public class RestSession : SSLSocket
    {
        public RestSession(Socket socket) : base(socket) { }

        public override void Accept()
        {
            AsyncHandshake(Global.LoginServiceMgr.GetCertificate());
        }

        public async override void ReadHandler(byte[] data, int receivedLength)
        {
            var httpRequest = HttpHelper.ParseRequest(data, receivedLength);
            if (httpRequest == null)
                return;

            switch (httpRequest.Method)
            {
                case "GET":
                default:
                    SendResponse(HttpCode.Ok, Global.LoginServiceMgr.GetFormInput());
                    break;
                case "POST":
                    HandleLoginRequest(httpRequest);
                    return;
            }

            await AsyncRead();
        }

        public void HandleLoginRequest(HttpHeader request)
        {
            LogonData loginForm = Json.CreateObject<LogonData>(request.Content);
            LogonResult loginResult = new();
            if (loginForm == null)
            {
                loginResult.AuthenticationState = "LOGIN";
                loginResult.ErrorCode = "UNABLE_TO_DECODE";
                loginResult.ErrorMessage = "There was an internal error while connecting to Battle.net. Please try again later.";
                SendResponse(HttpCode.BadRequest, loginResult);
                return;
            }

            string login = "";
            string password = "";

            for (int i = 0; i < loginForm.Inputs.Count; ++i)
            {
                switch (loginForm.Inputs[i].Id)
                {
                    case "account_name":
                        login = loginForm.Inputs[i].Value;
                        break;
                    case "password":
                        password = loginForm.Inputs[i].Value;
                        break;
                }
            }

            HermesProxy.GlobalSessionData globalSession = new();

            // We pass OS, Build and Locale in Path from HandleLogon
            string str = request.Path;
            str = str.Replace("/bnetserver/login/", "");
            str = str.Substring(0, str.IndexOf("/"));
            globalSession.OS = str;
            str = request.Path;
            str = str.Replace("/bnetserver/login/", "");
            str = str.Substring(str.IndexOf('/') + 1);
            str = str.Substring(0, str.IndexOf("/"));
            globalSession.Build = UInt32.Parse(str);
            str = request.Path;
            str = str.Replace("/bnetserver/login/", "");
            str = str.Substring(str.IndexOf('/') + 1);
            str = str.Substring(str.IndexOf('/') + 1);
            str = str.Substring(0, str.IndexOf("/"));
            globalSession.Locale = str;

            globalSession.AuthClient = new();
            if (globalSession.AuthClient.ConnectToAuthServer(login, password, globalSession.Locale))
            {
                string loginTicket = "";
                uint loginTicketExpiry = (uint)(Time.UnixTime + 3600);

                if (loginTicket.IsEmpty() || loginTicketExpiry < Time.UnixTime)
                {
                    byte[] ticket = Array.Empty<byte>().GenerateRandomKey(20);
                    loginTicket = "TC-" + ticket.ToHexString();
                }

                globalSession.LoginTicket = loginTicket;
                globalSession.Username = login;
                globalSession.Password = password;
                Global.AddNewSessionByName(login, globalSession);
                Global.AddNewSessionByTicket(loginTicket, globalSession);

                loginResult.LoginTicket = loginTicket;
                loginResult.AuthenticationState = "DONE";
                SendResponse(HttpCode.Ok, loginResult);
            }
            else
            {
                loginResult.AuthenticationState = "LOGIN";
                loginResult.ErrorCode = "UNABLE_TO_DECODE";
                loginResult.ErrorMessage = "There was an internal error while connecting to Battle.net. Please try again later.";
                SendResponse(HttpCode.BadRequest, loginResult);
            }
        }

        async void SendResponse<T>(HttpCode code, T response)
        {
            await AsyncWrite(HttpHelper.CreateResponse(code, Json.CreateString(response)));
        }

        string CalculateShaPassHash(string name, string password)
        {
            SHA256 sha256 = SHA256.Create();
            var email = sha256.ComputeHash(Encoding.UTF8.GetBytes(name));
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(email.ToHexString() + ":" + password)).ToHexString(true);
        }
    }
}
