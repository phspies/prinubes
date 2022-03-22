using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

internal class Connector
{
    private string[] Ips;
    private int Port;
    private string _FastestConnectedAddress;
    private AutoResetEvent FindFastestAddressEvent;

    private string FastestConnectedAddress
    {
        get
        {
            lock (this)
                return this._FastestConnectedAddress;
        }
    }

    public Connector(string[] ips, int port)
    {
        this.Ips = ips;
        this.Port = port;
    }

    public string FindFastestAddress(int timeoutSeconds)
    {
        string str = (string)null;
        this.FindFastestAddressEvent = new AutoResetEvent(false);
        foreach (string ip in this.Ips)
            new Thread(new ParameterizedThreadStart(this.ConnectionThreadFunc)).Start((object)new IPEndPoint(IPAddress.Parse(ip), this.Port));
        if (this.FindFastestAddressEvent.WaitOne(timeoutSeconds * 1000, false))
            str = this.FastestConnectedAddress;
        this.FindFastestAddressEvent.Close();
        return str;
    }

    private void ConnectionThreadFunc(object data)
    {
        IPEndPoint ipEndPoint = (IPEndPoint)data;
        Socket socket = (Socket)null;
        try
        {
            socket = new Socket(ipEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect((EndPoint)ipEndPoint);
        }
        catch (Exception ex)
        {
        }
        if (socket == null || !socket.Connected)
            return;
        if (this.IsFastest(ipEndPoint.Address.ToString()))
        {
            try
            {
                this.FindFastestAddressEvent.Set();
            }
            catch (Exception ex)
            {
            }
        }
        socket.Close();
    }

    private bool IsFastest(string address)
    {
        bool flag = false;
        lock (this)
        {
            if (this._FastestConnectedAddress == null)
            {
                this._FastestConnectedAddress = address;
                flag = true;
            }
        }
        return flag;
    }
}
