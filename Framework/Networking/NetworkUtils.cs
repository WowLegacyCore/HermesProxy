using System.Linq;
using System.Net;

namespace Framework.Networking;

public static class NetworkUtils
{
    public static IPAddress ResolveOrDirectIp(string hostOrIpaddress)
    {
        IPAddress result;
        if (IPAddress.TryParse(hostOrIpaddress, out result))
        {
            if (IPAddress.IsLoopback(result))
                return IPAddress.Loopback;

            return result;
        }

        return Dns.GetHostAddresses(hostOrIpaddress).First();
    }
}
