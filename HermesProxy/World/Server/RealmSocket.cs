using System.Net.Sockets;

namespace HermesProxy.World.Server;

public class RealmSocket : WorldSocket
{
    public RealmSocket(Socket socket) : base(socket)
    {
    }
}
