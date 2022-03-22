using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Xml;
using Prinubes.vCenterSDK;
using System.Threading.Tasks;
using System.ServiceModel.Channels;

namespace PlatformWorker
{


    /// <summary>
    /// Connection Handler for WebService
    /// </summary>
    public class SvcConnection
    {
        public enum ConnectionState
        {
            Connected,
            Disconnected,
        }

        public VimPortType _service;
        protected ConnectionState _state;
        public ServiceContent _sic;
        protected ManagedObjectReference _svcRef;
        public event ConnectionEventHandler AfterConnect;
        public event ConnectionEventHandler AfterDisconnect;
        public event ConnectionEventHandler BeforeDisconnect;
        public UserSession VimUserSession { get; private set; }
     
        private bool _ignoreCert;
        public bool ignoreCert
        {
            get { return _ignoreCert; }
            set
            {
                if (value)
                {
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                }
                _ignoreCert = value;
            }
        }
        /// <summary>
        ///  This method is used to validate remote certificate 
        /// </summary>
        /// <param name="sender">string Array</param>
        /// <param name="certificate">X509Certificate certificate</param>
        /// <param name="chain">X509Chain chain</param>
        /// <param name="policyErrors">SslPolicyErrors policyErrors</param>
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }
        public SvcConnection(string svcRefVal)
        {
            _state = ConnectionState.Disconnected;
            if (ignoreCert)
            {
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
            }
            _svcRef = new ManagedObjectReference();
            _svcRef.type = "ServiceInstance";
            _svcRef.Value = svcRefVal;
        }
        /// <summary>
        /// Creates an instance of the VMA proxy and establishes a connection
        /// </summary>
        /// <param name="url"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public async Task ConnectAsync(string url, string username, string password)
        {
            if (_service != null)
            {
                DisconnectAsync();
            }
            _service = GetVimService(url, username, password);
            _sic = await _service.RetrieveServiceContentAsync(_svcRef);
            if (_sic.sessionManager != null)
            {
                await _service.LoginAsync(_sic.sessionManager, username, password, null);
            }
            _state = ConnectionState.Connected;
            if (AfterConnect != null)
            {
                AfterConnect(this, new ConnectionEventArgs());
            }
        }
        /// <summary>
        /// Establishe a connection using an existing cookie
        /// </summary>
        /// <param name="url">Server Url</param>
        /// <param name="cookie">Cookie used to connect to the server</param>
        public async Task ConnectAsync(string url, Cookie cookie)
        {
            if (_service != null)
            {
                DisconnectAsync();
            }
            _service = GetVimService(url);
            _sic = await _service.RetrieveServiceContentAsync(_svcRef);
            // Add the cookie to the cookie manager
            var cookieManager = ((IContextChannel)_service).GetProperty<IHttpCookieContainerManager>();
            cookieManager.CookieContainer.SetCookies(new Uri(url), cookie.ToString());
            _state = ConnectionState.Connected;
            if (AfterConnect != null)
            {
                AfterConnect(this, new ConnectionEventArgs());
            }
        }
        public void SaveSession(String fileName, String urlString)
        {
            BinaryFormatter bf = new BinaryFormatter();
            Stream s = File.Open(fileName, FileMode.Create);
            var cookieManager = ((IContextChannel)_service).GetProperty<IHttpCookieContainerManager>();
            var cookie = cookieManager.CookieContainer.GetCookies(new Uri(urlString))[0];
            bf.Serialize(s, cookie);
            s.Close();
        }
        public void LoadSession(String fileName, String urlString)
        {
            BinaryFormatter bf = new BinaryFormatter();
            Stream s = File.Open(fileName, FileMode.Open);
            Cookie cookie = bf.Deserialize(s) as Cookie;
            s.Close();
            ConnectAsync(urlString, cookie);
        }
        public VimPortType Service
        {
            get
            {
                return _service;
            }
        }
        public ManagedObjectReference ServiceRef
        {
            get
            {
                return _svcRef;
            }
        }
        public ServiceContent ServiceContent
        {
            get
            {
                return _sic;
            }
        }
        public ManagedObjectReference PropCol
        {
            get
            {
                return _sic.propertyCollector;
            }
        }
        public ManagedObjectReference Root
        {
            get
            {
                return _sic.rootFolder;
            }
        }
        public ConnectionState State
        {
            get
            {
                return _state;
            }
        }
        /// <summary>
        /// Disconnects the Connection
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (_service != null)
            {
                if (BeforeDisconnect != null)
                {
                    BeforeDisconnect(this, new ConnectionEventArgs());
                }
                if (_sic != null)
                {
                    await _service.LogoutAsync(_sic.sessionManager);
                }
                _service = null;
                _sic = null;
                _state = ConnectionState.Disconnected;
                if (AfterDisconnect != null)
                {
                    AfterDisconnect(this, new ConnectionEventArgs());
                }
            }
        }
        private static VimPortType GetVimService(string url, string username = null, string password = null)
        {
            var binding = new BasicHttpBinding();
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

            var factory = new ChannelFactory<VimPortType>(binding, new EndpointAddress(url));
            factory.Credentials.UserName.UserName = username;
            factory.Credentials.UserName.Password = password;
            var service = factory.CreateChannel();
            return service;
        }
    }

    public class ConnectionEventArgs : System.EventArgs
    {
    }
    public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs e);
}
