using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.Interfaces;
using Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware.VirtualMachine;
using Prinubes.vCenterSDK;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using ObjectContent = Prinubes.vCenterSDK.ObjectContent;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public class VCService : IVimService
    {
        public enum ConnectionState
        {
            Connected,
            Disconnected,
        }
        protected ConnectionState _connectionState;
        private VimPortType _Service;
        private ServiceContent _ServiceContent;
        private ILogger<VCService> logger;

        private ManagedObjectReference _SvcRef;
        private ManagedObjectReference _PropCol;
        private VimFolderOutsideDC _RootFolder;
        private UserSession _VimUserSession;

        public event ConnectionEventHandler AfterConnect;
        public event ConnectionEventHandler AfterDisconnect;
        public event ConnectionEventHandler BeforeDisconnect;
        public class ConnectionEventArgs : System.EventArgs
        {

        }
        public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs e);

        public void Logon(string url, string userName, string password)
        {
            if (_Service != null)
            {
                Disconnect();
            }
            _SvcRef = new ManagedObjectReference();
            _SvcRef.type = "ServiceInstance";
            _SvcRef.Value = "ServiceInstance";

            if (_ServiceContent != null)
            {
                Disconnect();
            }
            _Service = GetVimService(url, userName, password);
            _ServiceContent = _Service.RetrieveServiceContentAsync(_SvcRef).Result;
            _RootFolder = GetFolderOutsideDC(_ServiceContent.rootFolder);
            if (_ServiceContent.sessionManager != null)
            {
                _VimUserSession = _Service.LoginAsync(_ServiceContent.sessionManager, userName, password, null).Result;
            }
            _connectionState = ConnectionState.Connected;
            if (AfterConnect != null)
            {
                AfterConnect(this, new ConnectionEventArgs());
            }
        }
        private static VimPortType GetVimService(string url, string username = null, string password = null)
        {
            var factory = new ChannelFactory<VimPortType>(GetBinding(), new EndpointAddress(url));
            factory.Credentials.ServiceCertificate.SslCertificateAuthentication = new System.ServiceModel.Security.X509ServiceCertificateAuthentication()
            {
                CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None,
                RevocationMode = X509RevocationMode.NoCheck
            };

            factory.Credentials.UserName.UserName = username;
            factory.Credentials.UserName.Password = password;
            return factory.CreateChannel();
        }
        private static MessageEncodingBindingElement GetWcfEncoding()
        {
            return new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8);
        }
        private static HttpsTransportBindingElement GetWcfTransport()
        {
            HttpsTransportBindingElement transport = new HttpsTransportBindingElement
            {
                RequireClientCertificate = false,
                AuthenticationScheme = AuthenticationSchemes.Basic,
                AllowCookies = true,
                MaxReceivedMessageSize = int.MaxValue
            };
            return transport;
        }
        public static CustomBinding GetBinding()
        {
            return new CustomBinding(GetWcfEncoding(), GetWcfTransport());
        }
        public void Disconnect()
        {
            if (_ServiceContent != null)
            {
                if (BeforeDisconnect != null)
                {
                    BeforeDisconnect(this, new ConnectionEventArgs());
                }
                if (_Service != null)
                    _Service.LogoutAsync(_ServiceContent.sessionManager);
                _Service = null;
                _ServiceContent = null;
                _connectionState = ConnectionState.Disconnected;
                if (AfterDisconnect != null)
                {
                    AfterDisconnect(this, new ConnectionEventArgs());
                }
            }
        }

        public ManagedObjectReference PropertyCollector
        {
            get
            {
                return _PropCol;
            }
        }

        public VimFolderOutsideDC RootFolder
        {
            get
            {
                return _RootFolder;
            }
        }

        public VimPortType Service
        {
            get
            {
                return _Service;
            }
        }

        internal ManagedObjectReference Root
        {
            get
            {
                return _ServiceContent.rootFolder;
            }
        }

        public string FullName
        {
            get
            {
                return _ServiceContent.about.fullName;
            }
        }

        public bool IsVirtualCenter
        {
            get
            {
                return _ServiceContent.about.productLineId.Equals("vpx", StringComparison.OrdinalIgnoreCase);
            }
        }

        public string ApiVersion
        {
            get
            {
                return _ServiceContent.about.apiVersion;
            }
        }

        public ManagedObjectReference FileManager
        {
            get
            {
                return _ServiceContent.fileManager;
            }
        }

        public ManagedObjectReference VirtualDiskManager
        {
            get
            {
                return _ServiceContent.virtualDiskManager;
            }
        }

        public Task<DateTime> CurrentTime
        {
            get
            {
                return _Service.CurrentTimeAsync(_SvcRef);
            }
        }

        internal ManagedObjectReference DVSManager
        {
            get
            {
                return _ServiceContent.dvSwitchManager;
            }
        }

        private ManagedObjectReference CustomizationSpecManager
        {
            get
            {
                return _ServiceContent.customizationSpecManager;
            }
        }






        public async Task HeartbeatAsync()
        {
            await _Service.CurrentTimeAsync(_SvcRef);
        }

        public async Task<IVimHost> SearchHostByIPAsync(string ip, bool retrieveCommonProperties)
        {
            IVimHost vimHost = null;
            IPAddress address = null;
            if (!IPAddress.TryParse(ip, out address))
                return vimHost;
            ManagedObjectReference byIp = await _Service.FindByIpAsync(_ServiceContent.searchIndex, null, ip, false);
            if (byIp != null)
            {
                vimHost = new Host(this, byIp);
                if (retrieveCommonProperties)
                {
                    ServerProperties commonProperties = vimHost.GetCommonProperties();
                    vimHost.Name = commonProperties.Name;
                }
            }
            return vimHost;
        }

        public async Task<IVimHost> SearchHostByDnsNameAsync(string dnsName, bool retrieveCommonProperties)
        {
            ManagedObjectReference byDnsName = await _Service.FindByDnsNameAsync(_ServiceContent.searchIndex, null, dnsName, false);
            IVimHost vimHost = null;
            if (byDnsName != null)
            {
                vimHost = new Host(this, byDnsName);
                if (retrieveCommonProperties)
                {
                    ServerProperties commonProperties = vimHost.GetCommonProperties();
                    vimHost.Name = commonProperties.Name;
                }
            }
            return vimHost;
        }

        public async Task<IVimHost> SearchHostByUuidAsync(string uuid, bool retrieveCommonProperties)
        {
            ManagedObjectReference byUuid = await _Service.FindByUuidAsync(_ServiceContent.searchIndex, null, uuid, false, false);
            IVimHost? vimHost = null;
            if (byUuid != null)
            {
                vimHost = new Host(this, byUuid);
                if (retrieveCommonProperties)
                {
                    ServerProperties commonProperties = vimHost.GetCommonProperties();
                    vimHost.Name = commonProperties.Name;
                }
            }
            return vimHost;
        }

        public async Task<IVimVm> SearchVmByUuidAsync(string uuid)
        {
            return await SearchVmByUuidAsync(uuid, true);
        }

        public async Task<IVimVm> SearchVmByUuidAsync(string uuid, bool retrieveCommonProperties)
        {
            ManagedObjectReference byUuid = await _Service.FindByUuidAsync(_ServiceContent.searchIndex, null, uuid, true, false);
            IVimVm? vimVm = null;
            if (byUuid != null)
            {
                vimVm = new Vm(this, byUuid);
                if (retrieveCommonProperties)
                {
                    VmProperties commonProperties = await vimVm.GetCommonPropertiesAsync();
                    vimVm.Name = commonProperties.Name;
                }
            }
            if (vimVm == null)
                throw new Exception("No such virtual machine: " + uuid);
            return vimVm;
        }

        public async Task<IVimVm> SearchVmByDnsNameAsync(string dnsName)
        {
            return await SearchVmByDnsNameAsync(dnsName, true);
        }

        public async Task<IVimVm> SearchVmByDnsNameAsync(string dnsName, bool retrieveCommonProperties)
        {
            ManagedObjectReference byDnsName = await _Service.FindByDnsNameAsync(_ServiceContent.searchIndex, null, dnsName, true);
            IVimVm? vimVm = null;
            if (byDnsName != null)
            {
                vimVm = new Vm(this, byDnsName);
                if (retrieveCommonProperties)
                {
                    VmProperties commonProperties = await vimVm.GetCommonPropertiesAsync();
                    vimVm.Name = commonProperties.Name;
                }
            }
            return vimVm;
        }

        public IVimDatacenter[] GetDatacenters()
        {
            List<IVimDatacenter> datacenters = new List<IVimDatacenter>();
            getChildrenAsync(RootFolder, datacenters);
            return datacenters.ToArray();
        }

        public async Task<List<IVimDatastore>> GetAllDatastoresAsync()
        {
            ObjectContent[] datastoresObjectContents = await getAllDatastoresObjectContentsAsync();
            List<IVimDatastore> vimDatastoreList = new List<IVimDatastore>();
            if (datastoresObjectContents != null)
            {
                foreach (ObjectContent objectContent in datastoresObjectContents)
                {
                    Dictionary<string, object> dictionary = PropSetToDictionary(objectContent.propSet);
                    IVimDatastore vimDatastore = new Datastore(this, objectContent.obj);
                    vimDatastore.GetCommonPropertiesAsync(dictionary);
                    vimDatastoreList.Add(vimDatastore);
                }
            }
            return vimDatastoreList;
        }

        public async Task<IVimDatastore> GetDatastoreByUrlAsync(string url)
        {
            IVimDatastore vimDatastore1 = null;
            ObjectContent[] datastoresObjectContents = await getAllDatastoresObjectContentsAsync();
            List<IVimDatastore> vimDatastoreList = new List<IVimDatastore>();
            if (datastoresObjectContents != null)
            {
                foreach (ObjectContent objectContent in datastoresObjectContents)
                {
                    Dictionary<string, object> dictionary = PropSetToDictionary(objectContent.propSet);
                    IVimDatastore vimDatastore2 = new Datastore(this, objectContent.obj);
                    vimDatastore2.GetCommonPropertiesAsync(dictionary);
                    if (string.Compare(url, vimDatastore2.DsProperties.Url, true) == 0)
                    {
                        vimDatastore1 = vimDatastore2;
                        break;
                    }
                }
            }
            return vimDatastore1;
        }

        public async Task<IVimDatastore> GetDatastoreByNameAsync(string name)
        {
            IVimDatastore vimDatastore1 = null;
            ObjectContent[] datastoresObjectContents = await getAllDatastoresObjectContentsAsync();
            List<IVimDatastore> vimDatastoreList = new List<IVimDatastore>();
            if (datastoresObjectContents != null)
            {
                foreach (ObjectContent objectContent in datastoresObjectContents)
                {
                    Dictionary<string, object> dictionary = PropSetToDictionary(objectContent.propSet);
                    IVimDatastore vimDatastore2 = new Datastore(this, objectContent.obj);
                    vimDatastore2.GetCommonPropertiesAsync(dictionary);
                    if (string.Compare(name, vimDatastore2.Name, true) == 0)
                    {
                        vimDatastore1 = vimDatastore2;
                        break;
                    }
                }
            }
            return vimDatastore1;
        }

        private async Task<ObjectContent[]> getAllDatastoresObjectContentsAsync()
        {
            PropertySpec propertySpec = new PropertySpec();
            propertySpec.type = "Datastore";
            propertySpec.all = false;
            propertySpec.pathSet = Datastore.VCProperties;
            TraversalSpec traversalSpec1 = new TraversalSpec();
            traversalSpec1.name = "dsFolderSpec";
            traversalSpec1.type = "Folder";
            traversalSpec1.path = "childEntity";
            traversalSpec1.skip = true;
            traversalSpec1.selectSet = new SelectionSpec[1]
            {
        new SelectionSpec()
            };
            traversalSpec1.selectSet[0].name = "dsFolderSpec";
            TraversalSpec traversalSpec2 = new TraversalSpec();
            traversalSpec2.name = "datacenterSpec";
            traversalSpec2.type = "Datacenter";
            traversalSpec2.path = "datastoreFolder";
            traversalSpec2.skip = false;
            traversalSpec2.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec1
            };
            traversalSpec2.selectSet[0].name = "datacenterSpec";
            TraversalSpec traversalSpec3 = new TraversalSpec();
            traversalSpec3.name = "tFolderSpec";
            traversalSpec3.type = "Folder";
            traversalSpec3.path = "childEntity";
            traversalSpec3.skip = true;
            traversalSpec3.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec2
            };
            traversalSpec3.selectSet[0].name = "tFolderSpec";
            return await RetrievePropertiesAsync(new PropertyFilterSpec[1] { new PropertyFilterSpec() { propSet = new PropertySpec[1] { propertySpec }, objectSet = new ObjectSpec[1] { new ObjectSpec() { obj = RootFolder.ManagedObject, skip = true, selectSet = new SelectionSpec[1] { traversalSpec3 } } } } });
        }

        public async Task<IVimHost[]> GetHostsAsync(IVimDatastore[] datastores)
        {
            Dictionary<ManagedObjectReference, Dictionary<string, object>> properties1 = await GetPropertiesAsync(datastores, new string[1] { "host" });
            Dictionary<string, ManagedObjectReference> dictionary1 = new Dictionary<string, ManagedObjectReference>(StringComparer.CurrentCultureIgnoreCase);
            foreach (Dictionary<string, object> dictionary2 in properties1.Values)
            {
                foreach (DatastoreHostMount datastoreHostMount in (DatastoreHostMount[])dictionary2["host"])
                {
                    if (!dictionary1.ContainsKey(datastoreHostMount.key.Value))
                    {
                        dictionary1.Add(datastoreHostMount.key.Value, datastoreHostMount.key);
                    }
                }
            }
            Dictionary<ManagedObjectReference, Dictionary<string, object>> properties2 = await GetPropertiesAsync(Utils.CollectionToArray<ManagedObjectReference>(dictionary1.Values), Host.VCProperties);
            List<IVimHost> vimHostList = new List<IVimHost>();
            foreach (ManagedObjectReference key in properties2.Keys)
            {
                try
                {
                    Dictionary<string, object> hostProperties = properties2[key];
                    IVimHost vimHost = new Host(this, key);
                    vimHost.GetCommonPropertiesAsync(hostProperties);
                    vimHostList.Add(vimHost);
                }
                catch (Exception ex)
                {
                }
            }
            return vimHostList.ToArray();
        }

        public async Task<ObjectContent[]> getObjectContentsAsync(ManagedObjectReference[] managedObjects, string[] properties)
        {
            if (managedObjects == null)
                return null;
            PropertyFilterSpec propertyFilterSpec = new PropertyFilterSpec();
            propertyFilterSpec.propSet = new PropertySpec[1]
            {
        new PropertySpec()
            };
            propertyFilterSpec.propSet[0].all = true;
            propertyFilterSpec.propSet[0].type = managedObjects[0].type;
            propertyFilterSpec.propSet[0].pathSet = properties;
            propertyFilterSpec.objectSet = new ObjectSpec[managedObjects.Length];
            for (int index = 0; index < managedObjects.Length; ++index)
            {
                propertyFilterSpec.objectSet[index] = new ObjectSpec();
                propertyFilterSpec.objectSet[index].skip = false;
                propertyFilterSpec.objectSet[index].obj = managedObjects[index];
            }
            return await RetrievePropertiesAsync(new PropertyFilterSpec[1] { propertyFilterSpec });
        }

        public async Task<Dictionary<ManagedObjectReference, Dictionary<string, object>>> GetPropertiesAsync(ManagedObjectReference[] managedObjects, string[] properties)
        {
            ObjectContent[] objectContents = await getObjectContentsAsync(managedObjects, properties);
            Dictionary<ManagedObjectReference, Dictionary<string, object>> dictionary = new Dictionary<ManagedObjectReference, Dictionary<string, object>>();
            if (objectContents != null)
            {
                foreach (ObjectContent objectContent in objectContents)
                {
                    DynamicProperty[] propSet = objectContent.propSet;
                    ManagedObjectReference equivelentMor = getEquivelentMor(objectContent.obj.Value, managedObjects);
                    dictionary.Add(equivelentMor, new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase));
                    if (propSet != null)
                    {
                        for (int index = 0; index < propSet.Length; ++index)
                        {
                            DynamicProperty dynamicProperty = propSet[index];
                            if (dynamicProperty != null)
                                dictionary[equivelentMor].Add(dynamicProperty.name, dynamicProperty.val);
                        }
                    }
                }
            }
            return dictionary;
        }

        private ManagedObjectReference getEquivelentMor(string morValue, ManagedObjectReference[] managedObjects)
        {
            ManagedObjectReference managedObjectReference = null;
            foreach (ManagedObjectReference managedObject in managedObjects)
            {
                if (morValue == managedObject.Value)
                {
                    managedObjectReference = managedObject;
                    break;
                }
            }
            return managedObjectReference;
        }

        public async Task<Dictionary<ManagedObjectReference, Dictionary<string, object>>?> GetPropertiesAsync(IVimManagedItem[] items, string[] properties)
        {
            if (items == null)
                return null;
            return await GetPropertiesAsync(VCManagedItemsToMors(items), properties);
        }

        public ManagedObjectReference[]? VCManagedItemsToMors(IVimManagedItem[] items)
        {
            if (items == null || items.Length == 0)
                return null;
            ManagedObjectReference[] managedObjectReferenceArray = new ManagedObjectReference[items.Length];
            int index = 0;
            foreach (IVimManagedItem vimManagedItem in items)
            {
                managedObjectReferenceArray[index] = vimManagedItem.ManagedObject;
                ++index;
            }
            return managedObjectReferenceArray;
        }

        public async Task UnregisterVmAsync(IVimVm vm)
        {
            await _Service.UnregisterVMAsync(vm.ManagedObject);
        }

        public async void UnregisterAndDestroyVm(IVimVm vm, VimClientlContext ctx)
        {
            await new VCTask(this, await Service.Destroy_TaskAsync(vm.ManagedObject)).WaitForResultAsync("DeleteVm", ctx);
        }


        public async Task<ObjectContent[]> RetrievePropertiesAsync(PropertyFilterSpec[] pfSpec)
        {
            List<ObjectContent> listobjcontent = new List<ObjectContent>();
            // RetrievePropertiesEx() returns the properties selected from the PropertyFilterSpec

            RetrieveResult rslts = Utils.RetryIf(6, 5000, () =>
            {
                var returnval = Service.RetrievePropertiesExAsync(new RetrievePropertiesExRequest(_ServiceContent.propertyCollector, pfSpec, new RetrieveOptions())).Result;
                return returnval.returnval;
            }, ex =>
            {
                return ex is WebException;
            });

            if (rslts != null && rslts.objects != null && rslts.objects.Length != 0)
            {
                listobjcontent.AddRange(rslts.objects);
            }
            String token = null;
            if (rslts != null && rslts.token != null)
            {
                token = rslts.token;
            }
            while (token != null && token.Length != 0)
            {
                rslts = await Service.ContinueRetrievePropertiesExAsync(_ServiceContent.propertyCollector, token);
                token = null;
                if (rslts != null)
                {
                    token = rslts.token;
                    if (rslts.objects != null && rslts.objects.Length != 0)
                    {
                        listobjcontent.AddRange(rslts.objects);
                    }
                }
            }
            return listobjcontent.ToArray();
        }

        public async Task<IVimHost> GetHostAsync(string name)
        {
            return await GetHostAsync(name, true);
        }

        public async Task<IVimHost> GetHostAsync(string name, bool retrieveCommonProperties)
        {
            IVimHost vimHost1 = await SearchHostByIPAsync(name, retrieveCommonProperties);
            if (vimHost1 != null)
                return vimHost1;
            IVimHost vimHost2 = await SearchHostByDnsNameAsync(name, retrieveCommonProperties);
            if (vimHost2 != null)
                return vimHost2;
            logInformationIfLoggerNotNullGetHost(string.Format("The ESX host: {0} could not be found by IP or DNS search - attempting case insensitive comparison.", name));
            Dictionary<string, IVimHost> allHostsDict = await GetAllHostsDictAsync();
            if (allHostsDict.Count == 0)
            {
                logInformationIfLoggerNotNullGetHost("No hosts were found.");
                return null;
            }
            if (allHostsDict.TryGetValue(name, out vimHost2))
                return vimHost2;
            logInformationIfLoggerNotNullGetHost(string.Format("We still couldn't find the host.  We did find these hosts though: {0}", string.Join(", ", allHostsDict.Keys.ToArray<string>())));
            return null;
        }

        private void logInformationIfLoggerNotNullGetHost(string message)
        {
            logger.LogInformation(message);
        }

        public Task<IVimHost> GetHostWithoutCaseInsensetiveComparison(string name)
        {
            IPAddress address;
            return !IPAddress.TryParse(name, out address) ? SearchHostByDnsNameAsync(name, false) : SearchHostByIPAsync(name, false);
        }

        public async Task<Dictionary<string, IVimHost>> GetAllHostsDictAsync()
        {
            ObjectContent[] hostsObjectContents = await GetAllHostsObjectContentsAsync();
            Dictionary<string, IVimHost> dictionary1 = new Dictionary<string, IVimHost>(StringComparer.CurrentCultureIgnoreCase);
            if (hostsObjectContents != null)
            {
                foreach (ObjectContent objectContent in hostsObjectContents)
                {
                    try
                    {
                        Dictionary<string, object> dictionary2 = PropSetToDictionary(objectContent.propSet);
                        Host host = new Host(this, objectContent.obj);
                        host.GetCommonPropertiesAsync(dictionary2);
                        if (!string.IsNullOrEmpty(host.Name))
                        {
                            if (!dictionary1.ContainsKey(host.Name))
                                dictionary1.Add(host.Name, host);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            return dictionary1;
        }

        public async Task<ObjectContent[]> GetAllHostsObjectContentsAsync()
        {
            TraversalSpec traversalSpec1 = new TraversalSpec();
            traversalSpec1.name = "computeResourceHostTraversalSpec";
            traversalSpec1.type = "ComputeResource";
            traversalSpec1.path = "host";
            traversalSpec1.skip = false;
            TraversalSpec traversalSpec2 = new TraversalSpec();
            traversalSpec2.name = "datacenterHostTraversalSpec";
            traversalSpec2.type = "Datacenter";
            traversalSpec2.path = "hostFolder";
            traversalSpec2.skip = false;
            traversalSpec2.selectSet = new SelectionSpec[1]
            {
        new SelectionSpec()
            };
            traversalSpec2.selectSet[0].name = "folderTraversalSpec";
            TraversalSpec traversalSpec3 = new TraversalSpec();
            traversalSpec3.name = "folderTraversalSpec";
            traversalSpec3.type = "Folder";
            traversalSpec3.path = "childEntity";
            traversalSpec3.skip = false;
            traversalSpec3.selectSet = new SelectionSpec[3]
            {
        new SelectionSpec(),
         traversalSpec2,
         traversalSpec1
            };
            traversalSpec3.selectSet[0].name = "folderTraversalSpec";
            PropertySpec[] propertySpecArray = new PropertySpec[1] { new PropertySpec() };
            propertySpecArray[0].all = false;
            propertySpecArray[0].pathSet = Host.VCProperties;
            propertySpecArray[0].type = "HostSystem";
            PropertyFilterSpec propertyFilterSpec = new PropertyFilterSpec();
            propertyFilterSpec.propSet = propertySpecArray;
            propertyFilterSpec.objectSet = new ObjectSpec[1]
            {
        new ObjectSpec()
            };
            propertyFilterSpec.objectSet[0].obj = RootFolder.ManagedObject;
            propertyFilterSpec.objectSet[0].skip = false;
            propertyFilterSpec.objectSet[0].selectSet = new SelectionSpec[1]
            {
         traversalSpec3
            };
            return await RetrievePropertiesAsync(new PropertyFilterSpec[1] { propertyFilterSpec });
        }

        private async Task getChildrenAsync(VimFolderOutsideDC folder, List<IVimDatacenter> datacenters)
        {
            foreach (IVimManagedItem child in await folder.GetChildrenAsync())
            {
                if (child.ManagedObject.type == "Datacenter")
                {
                    Datacenter datacenter = new Datacenter(this, child.ManagedObject);
                    datacenters.Add((IVimDatacenter)child);
                }
                else
                    await getChildrenAsync((VimFolderOutsideDC)child, datacenters);
            }
        }

        public Dictionary<string, object> PropSetToDictionary(DynamicProperty[] dynamicProperties)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
            if (dynamicProperties != null)
            {
                for (int index = 0; index < dynamicProperties.Length; ++index)
                {
                    DynamicProperty dynamicProperty = dynamicProperties[index];
                    if (dynamicProperty != null)
                        dictionary.Add(dynamicProperty.name, dynamicProperty.val);
                }
            }
            return dictionary;
        }


        public async Task<List<string>> SearchDatastoreSubFolderAsync(string esxHost, string folderName, VimClientlContext ctx)
        {
            return await (await GetHostAsync(esxHost)).SearchDatastoreSubFolderAsync(folderName, ctx);
        }

        public async Task<Dictionary<string, IVimVm>> GetAllVMsDictWithUuidAsync()
        {
            ObjectContent[] vmsObjectContents = await GetAllVMsObjectContentsAsync();
            Dictionary<string, IVimVm> dictionary1 = new Dictionary<string, IVimVm>(StringComparer.CurrentCultureIgnoreCase);
            if (vmsObjectContents != null)
            {
                foreach (ObjectContent objectContent in vmsObjectContents)
                {
                    try
                    {
                        Dictionary<string, object> dictionary2 = PropSetToDictionary(objectContent.propSet);
                        IVimVm vimVm = new Vm(this, objectContent.obj);
                        vimVm.GetCommonProperties(dictionary2);
                        if (!string.IsNullOrEmpty(vimVm.Uuid))
                        {
                            if (!vimVm.VMProperties.IsTemplate)
                            {
                                if (!dictionary1.ContainsKey(vimVm.Uuid))
                                    dictionary1.Add(vimVm.Uuid, vimVm);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return dictionary1;
        }

        public async Task<Dictionary<string, IVimVm>> GetAllVMsDictWithNameAsync()
        {
            ObjectContent[] vmsObjectContents = await GetAllVMsObjectContentsAsync();
            Dictionary<string, IVimVm> dictionary1 = new Dictionary<string, IVimVm>(StringComparer.CurrentCultureIgnoreCase);
            if (vmsObjectContents != null)
            {
                foreach (ObjectContent objectContent in vmsObjectContents)
                {
                    try
                    {
                        Dictionary<string, object> dictionary2 = PropSetToDictionary(objectContent.propSet);
                        IVimVm vimVm = new Vm(this, objectContent.obj);
                        vimVm.GetCommonProperties(dictionary2);
                        if (!string.IsNullOrEmpty(vimVm.Name))
                        {
                            if (!vimVm.VMProperties.IsTemplate)
                            {
                                if (!dictionary1.ContainsKey(vimVm.Name))
                                    dictionary1.Add(vimVm.Name, vimVm);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return dictionary1;
        }
        public async Task<Dictionary<string, IVimVm>> GetAllVMsDictWithNameAsync(ManagedObjectReference _datacenter)
        {
            logger.LogDebug("Retrieving all VMs from the host.");
            ObjectContent[] vmsObjectContents = await GetAllVMsObjectContentsAsync(_datacenter);
            Dictionary<string, IVimVm> dictionary1 = new Dictionary<string, IVimVm>(StringComparer.CurrentCultureIgnoreCase);
            if (vmsObjectContents != null)
            {
                foreach (ObjectContent objectContent in vmsObjectContents)
                {
                    try
                    {
                        Dictionary<string, object> dictionary2 = PropSetToDictionary(objectContent.propSet);
                        IVimVm vimVm = new Vm(this, objectContent.obj);
                        vimVm.GetCommonProperties(dictionary2);
                        if (!string.IsNullOrEmpty(vimVm.Name))
                        {
                            if (!vimVm.VMProperties.IsTemplate)
                            {
                                if (!dictionary1.ContainsKey(vimVm.Name))
                                    dictionary1.Add(vimVm.Name, vimVm);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            logger.LogDebug("Finished retrieving VMs.");
            return dictionary1;
        }


        private async Task<ObjectContent[]> GetAllVMsObjectContentsAsync()
        {
            PropertySpec propertySpec = new PropertySpec();
            propertySpec.type = "VirtualMachine";
            propertySpec.all = false;
            propertySpec.pathSet = Vm.VCProperties;
            TraversalSpec traversalSpec1 = new TraversalSpec();
            traversalSpec1.name = "resourcePoolSpec";
            traversalSpec1.type = "VirtualApp";
            traversalSpec1.path = "vm";
            traversalSpec1.skip = true;
            traversalSpec1.selectSet = new SelectionSpec[1]
            {
        new SelectionSpec()
            };
            traversalSpec1.selectSet[0].name = "resourcePoolSpec";
            TraversalSpec traversalSpec2 = new TraversalSpec();
            traversalSpec2.name = "vAppSpec";
            traversalSpec2.type = "VirtualApp";
            traversalSpec2.path = "vm";
            traversalSpec2.skip = true;
            traversalSpec2.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec1
            };
            traversalSpec2.selectSet[0].name = "vAppSpec";
            TraversalSpec traversalSpec3 = new TraversalSpec();
            traversalSpec3.name = "vmFolderSpec";
            traversalSpec3.type = "Folder";
            traversalSpec3.path = "childEntity";
            traversalSpec3.skip = true;
            traversalSpec3.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec2
            };
            traversalSpec3.selectSet[0].name = "vmFolderSpec";
            TraversalSpec traversalSpec4 = new TraversalSpec();
            traversalSpec4.name = "datacenterSpec";
            traversalSpec4.type = "Datacenter";
            traversalSpec4.path = "vmFolder";
            traversalSpec4.skip = false;
            traversalSpec4.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec3
            };
            traversalSpec4.selectSet[0].name = "datacenterSpec";
            TraversalSpec traversalSpec5 = new TraversalSpec();
            traversalSpec5.name = "tFolderSpec";
            traversalSpec5.type = "Folder";
            traversalSpec5.path = "childEntity";
            traversalSpec5.skip = true;
            traversalSpec5.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec4
            };
            traversalSpec5.selectSet[0].name = "tFolderSpec";
            return await RetrievePropertiesAsync(new PropertyFilterSpec[1] { new PropertyFilterSpec() { propSet = new PropertySpec[1] { propertySpec }, objectSet = new ObjectSpec[1] { new ObjectSpec() { obj = RootFolder.ManagedObject, skip = true, selectSet = new SelectionSpec[1] { traversalSpec5 } } } } });
        }
        private async Task<ObjectContent[]> GetAllVMsObjectContentsAsync(ManagedObjectReference _datacenter)
        {
            PropertySpec propertySpec = new PropertySpec();
            propertySpec.type = "VirtualMachine";
            propertySpec.all = false;
            propertySpec.pathSet = Vm.VCProperties;
            TraversalSpec traversalSpec1 = new TraversalSpec();
            traversalSpec1.name = "resourcePoolSpec";
            traversalSpec1.type = "VirtualApp";
            traversalSpec1.path = "vm";
            traversalSpec1.skip = true;
            traversalSpec1.selectSet = new SelectionSpec[1]
            {
        new SelectionSpec()
            };
            traversalSpec1.selectSet[0].name = "resourcePoolSpec";
            TraversalSpec traversalSpec2 = new TraversalSpec();
            traversalSpec2.name = "vAppSpec";
            traversalSpec2.type = "VirtualApp";
            traversalSpec2.path = "vm";
            traversalSpec2.skip = true;
            traversalSpec2.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec1
            };
            traversalSpec2.selectSet[0].name = "vAppSpec";
            TraversalSpec traversalSpec3 = new TraversalSpec();
            traversalSpec3.name = "vmFolderSpec";
            traversalSpec3.type = "Folder";
            traversalSpec3.path = "childEntity";
            traversalSpec3.skip = true;
            traversalSpec3.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec2
            };
            traversalSpec3.selectSet[0].name = "vmFolderSpec";
            TraversalSpec traversalSpec4 = new TraversalSpec();
            traversalSpec4.name = "datacenterSpec";
            traversalSpec4.type = "Datacenter";
            traversalSpec4.path = "vmFolder";
            traversalSpec4.skip = false;
            traversalSpec4.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec3
            };
            traversalSpec4.selectSet[0].name = "datacenterSpec";
            TraversalSpec traversalSpec5 = new TraversalSpec();
            traversalSpec5.name = "tFolderSpec";
            traversalSpec5.type = "Folder";
            traversalSpec5.path = "childEntity";
            traversalSpec5.skip = true;
            traversalSpec5.selectSet = new SelectionSpec[2]
            {
        new SelectionSpec(),
         traversalSpec4
            };
            traversalSpec5.selectSet[0].name = "tFolderSpec";
            return await RetrievePropertiesAsync(new PropertyFilterSpec[1] { new PropertyFilterSpec() { propSet = new PropertySpec[1] { propertySpec }, objectSet = new ObjectSpec[1] { new ObjectSpec() { obj = _datacenter, skip = true, selectSet = new SelectionSpec[1] { traversalSpec5 } } } } });
        }

        public async Task<IVimVm> GetVmOrVmTemplateAsync(string name)
        {
            IVimVm vimVm1 = null;
            ObjectContent[] vmsObjectContents = await GetAllVMsObjectContentsAsync();
            Dictionary<string, IVimVm> dictionary1 = new Dictionary<string, IVimVm>(StringComparer.CurrentCultureIgnoreCase);
            if (vmsObjectContents != null)
            {
                foreach (ObjectContent objectContent in vmsObjectContents)
                {
                    try
                    {
                        Dictionary<string, object> dictionary2 = PropSetToDictionary(objectContent.propSet);
                        IVimVm vimVm2 = new Vm(this, objectContent.obj);
                        vimVm2.GetCommonProperties(dictionary2);
                        if (string.Compare(vimVm2.Name, name, true) == 0)
                        {
                            vimVm1 = vimVm2;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Error retrieving VM");
                    }
                }
            }
            return vimVm1;
        }
        public async Task<Dictionary<string, InventoryNode>> GetVmInventoryAsync()
        {
            var _object = new BrowseForVmInventory(this);
            return await _object.LoadInventoryAsync();
        }

        public async Task<Dictionary<string, InventoryNode>> GetHostInventoryAsync()
        {
            return await new BrowseForESXInventory(this).LoadInventoryAsync();
        }

        public InventoryNode GetRootFolderOfInventory(Dictionary<string, InventoryNode> inventory)
        {
            foreach (InventoryNode inventoryNode in inventory.Values)
            {
                if (RootFolder.ManagedObject.Value == inventoryNode.ManagedObject.Value)
                    return inventoryNode;
            }
            return null;
        }

        public VimFolderOutsideDC GetFolderOutsideDC(ManagedObjectReference managedObject)
        {
            return new FolderOutsideDC(this, managedObject);
        }

        public IVimFolderInsideDC GetFolderInsideDC(ManagedObjectReference managedObject)
        {
            return new FolderInsideDC(this, managedObject);
        }

        public IVimVm GetVm(ManagedObjectReference managedObject)
        {
            return new Vm(this, managedObject);
        }

        public IVimHost GetHostManagedItem(ManagedObjectReference managedObject)
        {
            return new Host(this, managedObject);
        }

        public IVimDatacenter GetDatacenter(ManagedObjectReference managedObject)
        {
            return new Datacenter(this, managedObject);
        }

        public static string BuildDiskName(string serverName, string diskName)
        {
            return serverName + "_" + diskName + ".vmdk";
        }

        public static string GetVolumeName(string volName)
        {
            return string.IsNullOrEmpty(volName) ? "[Local] " : "[" + volName + "] ";
        }

        public async Task<CustomizationSpecItem> GetCustomizationSpecAsync(string name)
        {
            return await _Service.GetCustomizationSpecAsync(CustomizationSpecManager, name);
        }
    }
}
