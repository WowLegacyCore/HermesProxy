using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using HermesProxy.Framework.IO.Packet;
using HermesProxy.Framework.Constants;
using HermesProxy.Framework.Logging;

namespace HermesProxy.Network.Auth
{
    public class AuthClient
    {
        readonly TcpClient _tcpClient;
        AuthSession _session;

        public AuthClient() => _tcpClient = new();

        public void ConnectToAuthServer()
        {
            Log.Print(LogType.Server, "Connecting to the auth server...");

            while (!_tcpClient.Connected)
            {
                try
                {
                    _tcpClient.Connect(IPAddress.Parse(Settings.ServerAddress), 3724);
                }
                catch
                {
                    Log.Print(LogType.Error, $"Failed to connect to {Settings.ServerAddress}:3724, retrying in 500ms");
                    Thread.Sleep(500);
                }
            }

            _session = new(_tcpClient.Client);

            // @TODO: Add update thread

            SendLogonChallenge();
        }

        private void SendLogonChallenge()
        {
            using (var writer = new PacketWriter())
            {
                writer.WriteUInt8((byte)AuthCommand.LOGON_CHALLENGE);
                writer.WriteUInt8(6);
                writer.WriteUInt16((ushort)(Settings.ServerUsername.Length + 30));
                writer.WriteBytes(Encoding.ASCII.GetBytes("WoW"));
                writer.WriteUInt8(0);
                writer.WriteUInt8(Settings.GetServerExpansionVersion());
                writer.WriteUInt8(Settings.GetServerMajorPatchVersion());
                writer.WriteUInt8(Settings.GetServerMinorPatchVersion());
                writer.WriteUInt16((ushort)Settings.ServerBuild);
                writer.WriteBytes(Encoding.ASCII.GetBytes("68x"));
                writer.WriteUInt8(0);
                writer.WriteBytes(Encoding.ASCII.GetBytes("niW"));
                writer.WriteUInt8(0);
                writer.WriteBytes(Encoding.ASCII.GetBytes("SUne"));
                writer.WriteUInt32(0x3C);
                writer.WriteUInt32(0); // IP
                writer.WriteUInt8((byte)Settings.ServerUsername.Length);
                writer.WriteBytes(Encoding.ASCII.GetBytes(Settings.ServerUsername.ToUpper()));

                _session.SendPacket(writer.GetData());
            }
        }
    }
}
