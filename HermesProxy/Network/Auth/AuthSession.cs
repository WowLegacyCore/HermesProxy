using System;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

using HermesProxy.Crypto;
using HermesProxy.Framework.Constants;
using HermesProxy.Framework.IO.Packet;
using HermesProxy.Framework.Logging;
using HermesProxy.Network.Auth.Handler;

namespace HermesProxy.Network.Auth
{
    public class AuthSession
    {
        readonly Socket _socket;
        readonly byte[] _buffer = new byte[4096];
        readonly string _password;

        public string Username { get; private set; }
        public byte[] PasswordHash { get; private set; }
        public byte[] Modulus2 { get; set; }
        public BigInteger Key { get; set; }
        public bool RequestDisconnect { get; set; }
        public bool? HasSucceededLogin { get; set; } = null;

        public AuthSession(Socket socket, string username, string password)
        {
            if (_socket != null)
                throw new InvalidOperationException("There already is a AuthSession instance");

            Username = username;
            _password = password;

            var authstring = $"{Username.ToUpper()}:{_password}";
            PasswordHash = HashAlgorithm.SHA1.Hash(Encoding.ASCII.GetBytes(authstring.ToUpper()));

            _socket = socket;
            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveDataCallback, null);
        }

        private void ReceiveDataCallback(IAsyncResult ar)
        {
            try
            {
                var len = _socket.EndReceive(ar);
                if (len == 0)
                    return;

                var data = new byte[len];
                Buffer.BlockCopy(_buffer, 0, data, 0, len);

                HandlePacket(data);
            }
            catch (Exception ex)
            {
                Log.Print(LogType.Error, $"Receive error: {ex}");
                RequestDisconnect = true;
            }

            _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveDataCallback, null);
        }

        private void HandlePacket(byte[] data)
        {
            using (var reader = new PacketReader(data))
            {
                var command = (AuthCommand)reader.ReadUInt8();

                Log.Print(LogType.Debug, $"Received Opcode: {command} (Size: {data.Length})");

                switch (command)
                {
                    case AuthCommand.LOGON_CHALLENGE:
                        AuthHandler.HandleLogonChallenge(this, reader);
                        break;
                    case AuthCommand.LOGON_PROOF:
                        AuthHandler.HandleLogonProof(this, reader);
                        break;
                    case AuthCommand.REALM_LIST:
                        AuthHandler.HandleRealmlist(this, reader);
                        break;
                    default:
                        Log.Print(LogType.Error, $"Unknown Opcode: {command}");
                        break;
                }
            }
        }

        /// <summary>
        /// Sends the <see cref="AuthCommand.LOGON_CHALLENGE"/> to the <see cref="Socket"/>
        /// </summary>
        public void SendLogonChallenge()
        {
            using (var writer = new PacketWriter())
            {
                writer.WriteUInt8((byte)AuthCommand.LOGON_CHALLENGE);
                writer.WriteUInt8(6);
                writer.WriteUInt16((ushort)(Username.Length + 30));
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
                writer.WriteUInt8((byte)Username.Length);
                writer.WriteBytes(Encoding.ASCII.GetBytes(Username.ToUpper()));

                SendPacket(writer.GetData());
            }
        }

        public void SendPacket(byte[] data)
        {
            try
            {
                _socket.Send(data, 0, data.Length, SocketFlags.None);
            }
            catch
            {
                RequestDisconnect = true;
            }
        }
    }
}
