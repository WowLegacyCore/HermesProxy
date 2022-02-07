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
            if (receivedLength == 0)
            {
                CloseSocket();
                return;
            }

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

            Global.CurrentSessionData.Username = login;
            Global.CurrentSessionData.Password = password;

            if (HermesProxy.Auth.AuthClient.ConnectToAuthServer(login, password))
            {
                string loginTicket = "";
                uint loginTicketExpiry = (uint)(Time.UnixTime + 3600);

                if (loginTicket.IsEmpty() || loginTicketExpiry < Time.UnixTime)
                {
                    byte[] ticket = Array.Empty<byte>().GenerateRandomKey(20);
                    loginTicket = "TC-" + ticket.ToHexString();
                }
                Global.CurrentSessionData.LoginTicket = loginTicket;
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
