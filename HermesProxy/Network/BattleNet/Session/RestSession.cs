using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using HermesProxy.Framework.Logging;
using HermesProxy.Framework.Util;
using HermesProxy.Network.Auth;
using HermesProxy.Network.BattleNet.REST;

namespace HermesProxy.Network.BattleNet.Session
{
    public class RestSession
    {
        readonly byte[] _buffer = new byte[4096];
        readonly Socket _socket;
        readonly X509Certificate2 _cert;

        SslStream _sslStream;

        public RestSession(Socket socket, X509Certificate2 certificate)
        {
            if (_socket != null)
                throw new InvalidOperationException("There already is a RestSession instance");

            _socket = socket;
            _cert = certificate;

            Log.Print(LogType.Network, $"Received new connection on RestSession ({_socket.RemoteEndPoint})");
        }

        /// <summary>
        /// Handles the incoming rest session connection
        /// </summary>
        public async Task HandleIncomingConnection()
        {
            _sslStream = new(new NetworkStream(_socket), false);
            await _sslStream.AuthenticateAsServerAsync(_cert, false, SslProtocols.Tls, false);
            _sslStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveDataCallback, null);
        }

        private async void ReceiveDataCallback(IAsyncResult ar)
        {
            try
            {
                var receivedLen = _sslStream.EndRead(ar);
                if (receivedLen == 0)
                    return;

                var data = new byte[receivedLen];
                Buffer.BlockCopy(_buffer, 0, data, 0, receivedLen);

                var httpRequest = HttpHelper.ParseRequest(_buffer, receivedLen);
                if (httpRequest == null)
                    return;

                // Log.Print(LogType.Debug, $"Got HttpRequest Method: {httpRequest.Method} Content: {httpRequest.Content}");

                switch (httpRequest.Method)
                {
                    case "POST":
                        await HandleLogin(httpRequest);
                        break;
                    case "GET":
                        await SendResponse(HttpCode.Ok, Forms.GetFormInput());
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Print(LogType.Error, ex);
                CloseSocket();

                return;
            }

            _sslStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveDataCallback, null);
        }

        private async Task HandleLogin(HttpHeader request)
        {
            var logonRequest = JSON.CreateObject<LogonData>(request.Content);
            if (logonRequest == null)
            {
                await SendErrorResponseCode(HttpCode.BadRequest, new());
                return;
            }

            var username = logonRequest.GetDataFromId("account_name");
            var password = logonRequest.GetDataFromId("password");

            var authClient = new AuthClient();
            if (!authClient.ConnectToAuthServer(username, password))
            {
                await SendErrorResponseCode(HttpCode.InternalServerError, new());
                return;
            }

            while (authClient.Session.HasSucceededLogin == null) { }

            if (authClient.Session.HasSucceededLogin.Value)
            {
                var response = new LogonResult()
                {
                    LoginTicket = $"Proxy-{RandomNumberGenerator.GetBytes(20).ToHexString()}",
                    AuthenticationState = "DONE"
                };
                await SendResponse(HttpCode.Ok, response);
            }
            else
                await SendErrorResponseCode(HttpCode.InternalServerError, new());
        }

        private async Task SendErrorResponseCode(HttpCode code, LogonResult result)
        {
            result.AuthenticationState = "LOGIN";
            result.ErrorCode = "UNABLE_TO_DECODE";
            result.ErrorMessage = "There was an internal error while connecting to Battle.net. Please try again later.";

            await SendResponse(code, result);
        }

        private async Task SendResponse<T>(HttpCode code, T response)
        {
            var jsonData = JSON.CreateString(response);
            await SendData(HttpHelper.CreateResponse(code, jsonData));
        }

        private async Task SendData(byte[] data) => await _sslStream.WriteAsync(data);

        private void CloseSocket()
        {
            _sslStream.Close();
            _socket.Disconnect(false);
        }
    }
}
