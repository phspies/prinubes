using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

public class IPHelper
{
    private static Regex _ipv4Regex;
    private static Regex _ipv6Regex;

    private static Regex _IPv4Regex
    {
        get
        {
            if (IPHelper._ipv4Regex == null)
                IPHelper._ipv4Regex = new Regex("((25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|\\d)\\.){3}(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]\\d|\\d)");
            return IPHelper._ipv4Regex;
        }
    }

    private static Regex _IPv6Regex
    {
        get
        {
            if (IPHelper._ipv6Regex == null)
                IPHelper._ipv6Regex = new Regex("^((([0-9A-Fa-f]{1,4}:){7}[0-9A-Fa-f]{1,4})|(([0-9A-Fa-f]{1,4}:){6}:[0-9A-Fa-f]{1,4})|(([0-9A-Fa-f]{1,4}:){5}:([0-9A-Fa-f]{1,4}:)?[0-9A-Fa-f]{1,4})|(([0-9A-Fa-f]{1,4}:){4}:([0-9A-Fa-f]{1,4}:){0,2}[0-9A-Fa-f]{1,4})|(([0-9A-Fa-f]{1,4}:){3}:([0-9A-Fa-f]{1,4}:){0,3}[0-9A-Fa-f]{1,4})|(([0-9A-Fa-f]{1,4}:){2}:([0-9A-Fa-f]{1,4}:){0,4}[0-9A-Fa-f]{1,4})|(([0-9A-Fa-f]{1,4}:){6}((\b((25[0-5])|(1\\d{2})|(2[0-4]\\d)|(\\d{1,2}))\b)\\.){3}(\b((25[0-5])|(1\\d{2})|(2[0-4]\\d)|(\\d{1,2}))\b))|(([0-9A-Fa-f]{1,4}:){0,5}:((\b((25[0-5])|(1\\d{2})|(2[0-4]\\d)|(\\d{1,2}))\b)\\.){3}(\b((25[0-5])|(1\\d{2})|(2[0-4]\\d)|(\\d{1,2}))\b))|(::([0-9A-Fa-f]{1,4}:){0,5}((\b((25[0-5])|(1\\d{2})|(2[0-4]\\d)|(\\d{1,2}))\b)\\.){3}(\b((25[0-5])|(1\\d{2})|(2[0-4]\\d)|(\\d{1,2}))\b))|([0-9A-Fa-f]{1,4}::([0-9A-Fa-f]{1,4}:){0,5}[0-9A-Fa-f]{1,4})|(::([0-9A-Fa-f]{1,4}:){0,6}[0-9A-Fa-f]{1,4})|(([0-9A-Fa-f]{1,4}:){1,7}:))$");
            return IPHelper._ipv6Regex;
        }
    }

    private static List<string> GetExternalUnicastAddresses(IPInterfaceProperties adapterProperties)
    {
        List<string> stringList = new List<string>();
        UnicastIPAddressInformationCollection unicastAddresses = adapterProperties.UnicastAddresses;
        if (unicastAddresses != null)
        {
            foreach (UnicastIPAddressInformation addressInformation in unicastAddresses)
            {
                if (!addressInformation.Address.IsIPv6LinkLocal && !addressInformation.Address.IsIPv6SiteLocal)
                    stringList.Add(addressInformation.Address.ToString());
            }
        }
        return stringList;
    }

    private static List<string> GetIPAddresses(IPInterfaceProperties adapterProperties)
    {
        List<string> stringList = new List<string>();
        UnicastIPAddressInformationCollection unicastAddresses = adapterProperties.UnicastAddresses;
        if (unicastAddresses != null)
        {
            foreach (UnicastIPAddressInformation addressInformation in unicastAddresses)
                stringList.Add(addressInformation.Address.ToString());
        }
        MulticastIPAddressInformationCollection multicastAddresses = adapterProperties.MulticastAddresses;
        if (multicastAddresses != null)
        {
            foreach (MulticastIPAddressInformation addressInformation in multicastAddresses)
                stringList.Add(addressInformation.Address.ToString());
        }
        return stringList;
    }

    public static List<string> GetAllIPAddresses()
    {
        List<string> stringList = new List<string>();
        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        if (networkInterfaces != null)
        {
            foreach (NetworkInterface networkInterface in networkInterfaces)
                stringList.AddRange(IPHelper.GetIPAddresses(networkInterface.GetIPProperties()));
        }
        return stringList;
    }

    public static string GetFastestAddress(string[] ips, int port, int timeoutSeconds)
    {
        if (ips == null || ips.Length == 0)
            return null;
        if (ips.Length == 1)
            return ips[0];
        return new Connector(ips, port).FindFastestAddress(timeoutSeconds);
    }

    public static string IpBracketed(string ipString)
    {
        if (IPHelper.IsBracketed(ipString) || !IPHelper.IsIPv6(ipString))
            return ipString;
        return "[" + ipString + "]";
    }

    public static bool IsBracketed(string ipString)
    {
        if (ipString.StartsWith("["))
            return ipString.EndsWith("]");
        return false;
    }

    public static string GetWNetServerString(string server)
    {
        IPAddress address = null;
        string str;
        if (IPAddress.TryParse(server, out address))
        {
            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                str = "\\\\" + server.Replace(":", "-") + ".ipv6-literal.net";
            }
            else
            {
                if (address.AddressFamily != AddressFamily.InterNetwork)
                    throw new NotSupportedException(address.AddressFamily.ToString());
                str = "\\\\" + server;
            }
        }
        else
            str = "\\\\" + server;
        return str;
    }

    public static bool IsIPAddress(string address)
    {
        IPAddress address1;
        return IPAddress.TryParse(address, out address1);
    }

    public static bool IsLoopback(string address)
    {
        bool flag = false;
        IPAddress address1;
        if (IPAddress.TryParse(address, out address1))
        {
            if (address1.Equals(IPAddress.IPv6Loopback) || address1.Equals(IPAddress.Loopback))
                flag = true;
        }
        else if (string.Compare(address, "localhost", true, CultureInfo.InvariantCulture) == 0)
            flag = true;
        return flag;
    }

    public static bool IsLocalServer(string server)
    {
        if (IPHelper.IsLoopback(server))
            return true;
        return IPHelper.GetAllIPAddresses().Contains(Dns.GetHostAddresses(server)[0].ToString());
    }

    public static bool IsIPv4(string address)
    {
        bool flag = false;
        IPAddress address1;
        if (IPAddress.TryParse(address, out address1) && address1.AddressFamily == AddressFamily.InterNetwork)
            flag = true;
        return flag;
    }

    public static bool IsIPv6(string address)
    {
        bool flag = false;
        IPAddress address1;
        if (IPAddress.TryParse(address, out address1) && address1.AddressFamily == AddressFamily.InterNetworkV6)
            flag = true;
        return flag;
    }

    public static string GetLocalAddress(string remoteAddress, int port)
    {
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(remoteAddress), port);
        Socket socket = new Socket(ipEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ipEndPoint);
        string str = ((IPEndPoint)socket.LocalEndPoint).Address.ToString();
        socket.Close();
        return str;
    }

    public static List<string> GetIPv4(List<string> ipAddresses)
    {
        List<string> stringList = new List<string>();
        foreach (string ipAddress in ipAddresses)
        {
            IPAddress address = null;
            IPAddress.TryParse(ipAddress, out address);
            if (address != null && address.AddressFamily == AddressFamily.InterNetwork)
                stringList.Add(ipAddress);
        }
        return stringList;
    }

    public static List<string> GetIPv6(List<string> ipAddresses)
    {
        List<string> stringList = new List<string>();
        foreach (string ipAddress in ipAddresses)
        {
            IPAddress address = null;
            IPAddress.TryParse(ipAddress, out address);
            if (address != null && address.AddressFamily == AddressFamily.InterNetworkV6)
                stringList.Add(ipAddress);
        }
        return stringList;
    }

    public static bool IsIPv4Enabled(List<string> ipAddresses)
    {
        return IPHelper.GetIPv4(ipAddresses).Count > 0;
    }

    public static bool IsIPv6Enabled(List<string> ipAddresses)
    {
        return IPHelper.GetIPv6(ipAddresses).Count > 0;
    }

    public static bool IsValidIP(string address)
    {
        IPAddress address1;
        bool flag;
        if (IPAddress.TryParse(address, out address1))
        {
            string empty = string.Empty;
            flag = address1.AddressFamily != AddressFamily.InterNetwork ? IPHelper._IPv6Regex.IsMatch(address) : IPHelper._IPv4Regex.IsMatch(address);
            if (flag && (IPHelper.IsLoopback(address) || string.Compare(address, "0.0.0.0") == 0 || string.Compare(address, "::0") == 0))
                flag = false;
        }
        else
            flag = false;
        return flag;
    }

    public static string GetPreferredIPAddress(List<string> listIPs)
    {
        if (listIPs == null || listIPs.Count == 0)
            return string.Empty;
        string listIp = listIPs[0];
        List<string> ipv4 = IPHelper.GetIPv4(listIPs);
        if (ipv4.Count > 0)
            listIp = ipv4[0];
        return listIp;
    }
}
