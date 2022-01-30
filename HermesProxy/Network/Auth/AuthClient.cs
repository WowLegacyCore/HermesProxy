using System.Threading;
using System.Net;
using System.Net.Sockets;

using HermesProxy.Framework.Logging;

namespace HermesProxy.Network.Auth
{
    public class AuthClient
    {
        public const int MAX_RETRIES = 5;
        public AuthSession Session { get; private set; }

        readonly TcpClient _tcpClient;

        public AuthClient()
        {
            _tcpClient = new();
        }

        public bool ConnectToAuthServer(string username, string password)
        {
            Log.Print(LogType.Server, "Connecting to the auth server...");

            var retries = 0;
            while (!_tcpClient.Connected)
            {
                if (retries >= MAX_RETRIES)
                {
                    Log.Print(LogType.Error, $"Failed to connect to {Settings.ServerAddress}:3724 after {MAX_RETRIES}");
                    return false;
                }

                try
                {
                    _tcpClient.Connect(IPAddress.Parse(Settings.ServerAddress), 3724);
                    ++retries;
                }
                catch
                {
                    Log.Print(LogType.Error, $"Failed to connect to {Settings.ServerAddress}:3724, retrying in 500ms");
                    Thread.Sleep(500);
                }
            }

            Session = new(_tcpClient.Client, username, password);
            Session.SendLogonChallenge();

            // @TODO: Add update thread

            return true;
        }
    }
}
