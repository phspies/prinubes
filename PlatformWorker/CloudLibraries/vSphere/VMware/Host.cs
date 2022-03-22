using PlatformWorker.VMware.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading;
using Prinubes.vCenterSDK;
using System.Threading.Tasks;
using ObjectContent = Prinubes.vCenterSDK.ObjectContent;

namespace PlatformWorker.VMware
{


    internal class Host : VCManagedItem, IVimHost, IVimManagedItem
    {
        public static string[] VCProperties = new string[15] { "name", "hardware.systemInfo.uuid", "summary.config.product.version", "summary.config.product.build", "hardware.systemInfo.vendor", "hardware.systemInfo.model", "summary.hardware.numNics", "summary.hardware.numCpuPkgs", "summary.hardware.numCpuCores", "summary.hardware.numCpuThreads", "summary.hardware.cpuMhz", "summary.hardware.cpuModel", "summary.quickStats.overallCpuUsage", "summary.quickStats.overallMemoryUsage", "summary.hardware.memorySize" };
        private ServerProperties _properties;
        private IVimService _vimService;

        public ServerProperties Properties
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;
            }
        }

        internal Host(IVimService vimService, ManagedObjectReference managedObject)
          : base(vimService, managedObject)
        {
            _vimService = vimService;
        }

        internal Host(IVimService vimService, string name) : base(vimService, null)
        {
            Name = name;
        }

        public async Task<string> GetUuidAsync()
        {
            return (string)await GetPropertyAsync("hardware.systemInfo.uuid");
        }

        public ServerProperties GetCommonProperties()
        {
            if (string.IsNullOrEmpty(_properties.Name))
                GetCommonPropertiesAsync(GetPropertiesAsync(Host.VCProperties).Result).Wait();
            return _properties;
        }

        public async Task<ServerProperties> GetCommonPropertiesAsync(ManagedObjectAndProperties[] managedObjects)
        {
            foreach (ManagedObjectAndProperties managedObject in managedObjects)
            {
                if (managedObject.ManagedObject.Value == ManagedObject.Value)
                {
                    await GetCommonPropertiesAsync(managedObject.Properties);
                    break;
                }
            }
            return _properties;
        }

        public async Task GetCommonPropertiesAsync(Dictionary<string, object> hostProperties)
        {
            for (int index = 0; index < 3 && (hostProperties == null || hostProperties.Count != Host.VCProperties.Length); ++index)
            {
                Thread.Sleep(1000);
                hostProperties = await GetPropertiesAsync(Host.VCProperties);
            }
            if (hostProperties.ContainsKey("name"))
                _properties.Name = (string)hostProperties["name"];
            if (hostProperties.ContainsKey("hardware.systemInfo.uuid"))
                _properties.Uuid = (string)hostProperties["hardware.systemInfo.uuid"];
            if (hostProperties.ContainsKey("summary.config.product.version"))
                _properties.Version = (string)hostProperties["summary.config.product.version"];
            if (hostProperties.ContainsKey("summary.config.product.build"))
                _properties.BuildNum = (string)hostProperties["summary.config.product.build"];
            if (hostProperties.ContainsKey("hardware.systemInfo.vendor"))
                _properties.Vendor = (string)hostProperties["hardware.systemInfo.vendor"];
            if (hostProperties.ContainsKey("hardware.systemInfo.model"))
                _properties.Model = (string)hostProperties["hardware.systemInfo.model"];
            if (hostProperties.ContainsKey("summary.hardware.numCpuCores"))
                _properties.NumOfCpu = (short)hostProperties["summary.hardware.numCpuCores"];
            if (hostProperties.ContainsKey("summary.hardware.numCpuPkgs"))
                _properties.NumOfCpuPkgs = (short)hostProperties["summary.hardware.numCpuPkgs"];
            if (hostProperties.ContainsKey("summary.hardware.numCpuThreads"))
                _properties.NumOfCpuThreads = (short)hostProperties["summary.hardware.numCpuThreads"];
            if (hostProperties.ContainsKey("summary.hardware.cpuMhz"))
                _properties.CpuMHz = (int)hostProperties["summary.hardware.cpuMhz"];
            if (hostProperties.ContainsKey("summary.hardware.cpuModel"))
                _properties.ProcessorType = (string)hostProperties["summary.hardware.cpuModel"];
            if (hostProperties.ContainsKey("summary.hardware.numNics"))
                _properties.NumOfNics = (int)hostProperties["summary.hardware.numNics"];
            if (hostProperties.ContainsKey("summary.quickStats.overallCpuUsage"))
                _properties.CpuUsageMHz = (int)hostProperties["summary.quickStats.overallCpuUsage"];
            if (hostProperties.ContainsKey("summary.quickStats.overallMemoryUsage"))
                _properties.MemoryUsageMB = (long)(int)hostProperties["summary.quickStats.overallMemoryUsage"];
            if (hostProperties.ContainsKey("summary.hardware.memorySize"))
                _properties.MemoryMB = (long)hostProperties["summary.hardware.memorySize"] / 1048576L;
            Name = _properties.Name;
        }

        public override Task<IVimManagedItem[]> GetChildrenAsync()
        {
            return null;
        }

        public IVimVm[] GetVmsAndProperties(ManagedObjectAndProperties[] managedObjectsAndProperties)
        {
            List<IVimVm> vimVmList = new List<IVimVm>();
            foreach (ManagedObjectAndProperties objectsAndProperty in managedObjectsAndProperties)
            {
                if (!(objectsAndProperty.ManagedObject.type != "VirtualMachine"))
                {
                    IVimVm vimVm = (IVimVm)new Vm(VcService, objectsAndProperty.ManagedObject);
                    vimVm.GetCommonProperties(objectsAndProperty.Properties);
                    if (!vimVm.VMProperties.IsTemplate)
                        vimVmList.Add(vimVm);
                }
            }
            return vimVmList.ToArray();
        }

        public async Task<IVimVm[]> GetVmsAndPropertiesAsync()
        {
            return GetVmsAndProperties(await GetManagedObjectAndPropertiesAsync(ManagedObject, "vm", "VirtualMachine", Vm.VCProperties));
        }

        public async Task<IVimDatastore[]> GetDatastoresAndPropertiesAsync()
        {
            return GetDatastoresAndProperties(await GetManagedObjectAndPropertiesAsync(ManagedObject, "datastore", "Datastore", Datastore.VCProperties));
        }

        public IVimDatastore[] GetDatastoresAndProperties(ManagedObjectAndProperties[] managedObjectsAndProperties)
        {
            List<Datastore> datastoreList = new List<Datastore>();
            foreach (ManagedObjectAndProperties objectsAndProperty in managedObjectsAndProperties)
            {
                if (!(objectsAndProperty.ManagedObject.type != "Datastore") && objectsAndProperty.Properties != null)
                {
                    Datastore datastore = new Datastore(VcService, objectsAndProperty.ManagedObject);
                    datastore.GetCommonPropertiesAsync(objectsAndProperty.Properties);
                    if (!string.IsNullOrEmpty(datastore.DsProperties.RemoteId))
                        datastoreList.Add(datastore);
                }
            }
            return (IVimDatastore[])datastoreList.ToArray();
        }

        public async Task<IVimNetwork[]> GetNetworksAsync()
        {
            return await GetNetworksAndPropertiesAsync(await GetManagedObjectAndPropertiesAsync(ManagedObject, "network", "Network", Network.VCProperties));
        }

        public async Task<IVimNetwork[]> GetNetworksAndPropertiesAsync(ManagedObjectAndProperties[] managedObjectsAndProperties)
        {
            List<Network> networkList = new List<Network>();
            Dictionary<string, string> virtualPortgroups = await GetDistributedVirtualPortgroupsAsync();
            foreach (ManagedObjectAndProperties objectsAndProperty in managedObjectsAndProperties)
            {
                if ((!(objectsAndProperty.ManagedObject.type != "Network") || !(objectsAndProperty.ManagedObject.type != "DistributedVirtualPortgroup")) && objectsAndProperty.Properties != null)
                {
                    Network network = new Network(VcService, objectsAndProperty.ManagedObject);
                    await network.GetCommonPropertiesAsync(objectsAndProperty.Properties);
                    if (!network.IsDistributed || virtualPortgroups.ContainsKey(network.PortgroupKey))
                        networkList.Add(network);
                }
            }
            return (IVimNetwork[])networkList.ToArray();
        }

        public async Task<Dictionary<string, string>> GetDistributedVirtualPortgroupsAsync()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            try
            {
                DVSManagerDvsConfigTarget managerDvsConfigTarget = await _vimService.Service.QueryDvsConfigTargetAsync(((VCService)_vimService).DVSManager, ManagedObject, (ManagedObjectReference)null);
                if (managerDvsConfigTarget.distributedVirtualPortgroup != null)
                {
                    foreach (DistributedVirtualPortgroupInfo virtualPortgroupInfo in managerDvsConfigTarget.distributedVirtualPortgroup)
                    {
                        if (!virtualPortgroupInfo.uplinkPortgroup)
                            Utils.AddOrReplace(dictionary, virtualPortgroupInfo.portgroupKey, virtualPortgroupInfo.portgroupName + " (" + virtualPortgroupInfo.switchName + ")");
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dictionary;
        }

        public async Task<Dictionary<string, string>> GetDistributedVirtualSwitchUuidsAsync()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>((IEqualityComparer<string>)StringComparer.CurrentCultureIgnoreCase);
            try
            {
                DVSManagerDvsConfigTarget managerDvsConfigTarget = await _vimService.Service.QueryDvsConfigTargetAsync(((VCService)_vimService).DVSManager, ManagedObject, (ManagedObjectReference)null);
                if (managerDvsConfigTarget.distributedVirtualPortgroup != null)
                {
                    foreach (DistributedVirtualPortgroupInfo virtualPortgroupInfo in managerDvsConfigTarget.distributedVirtualPortgroup)
                    {
                        if (!virtualPortgroupInfo.uplinkPortgroup)
                            Utils.AddOrReplace<string, string>((IDictionary<string, string>)dictionary, virtualPortgroupInfo.portgroupKey, virtualPortgroupInfo.switchUuid);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dictionary;
        }

        public async Task<IVimHost[]> GetHostsAsync()
        {
            SelectionSpec selectionSpec = new SelectionSpec() { name = "recurseFolders" };
            TraversalSpec traversalSpec1 = new TraversalSpec() { type = "Datacenter", path = "hostFolder", skip = true, selectSet = new SelectionSpec[1] { selectionSpec } };
            TraversalSpec traversalSpec2 = new TraversalSpec();
            traversalSpec2.type = "Folder";
            traversalSpec2.name = "recurseFolders";
            traversalSpec2.path = "childEntity";
            traversalSpec2.skip = false;
            traversalSpec2.selectSet = new SelectionSpec[2]
            {
        selectionSpec,
        (SelectionSpec) traversalSpec1
            };
            TraversalSpec traversalSpec3 = traversalSpec2;
            IVimService vcService = VcService;
            PropertyFilterSpec[] pfSpec = new PropertyFilterSpec[1];
            int index1 = 0;
            PropertyFilterSpec propertyFilterSpec1 = new PropertyFilterSpec();
            propertyFilterSpec1.propSet = new PropertySpec[1]
            {
        new PropertySpec()
        {
          all = false,
          type = "Folder",
          pathSet = new string[2]
          {
            "name",
            "childEntity"
          }
        }
            };
            propertyFilterSpec1.objectSet = new ObjectSpec[1]
            {
        new ObjectSpec()
        {
          obj = ((VCService) VcService).Root,
          skip = true,
          selectSet = (SelectionSpec[]) new TraversalSpec[1]
          {
            traversalSpec3
          }
        }
            };
            PropertyFilterSpec propertyFilterSpec2 = propertyFilterSpec1;
            pfSpec[index1] = propertyFilterSpec2;
            ObjectContent[] objectContentArray = await vcService.RetrievePropertiesAsync(pfSpec);
            Host[] hostArray = (Host[])null;
            for (int index2 = 0; index2 < objectContentArray.Length; ++index2)
            {
                ManagedObjectReference managedObjectReference = (ManagedObjectReference)((Array)objectContentArray[index2].propSet[0].val).GetValue(0);
                if (managedObjectReference.type == "ComputeResource" || managedObjectReference.type == "ClusterComputeResource")
                {
                    ManagedObjectReference[] managedObjects = await GetManagedObjectsAsync(new string[1] { "host" });
                    if (managedObjects.Length != 0)
                    {
                        hostArray = new Host[managedObjects.Length];
                        for (int index3 = 0; index3 < managedObjects.Length; ++index3)
                        {
                            ManagedObjectReference managedObject = managedObjects[index3];
                            if (managedObject != null)
                            {
                                hostArray[index3] = new Host(VcService, managedObject);
                                Dictionary<string, object> properties = await hostArray[index3].GetPropertiesAsync(new string[3] { "name", "summary.config.name", "summary.config.product.version" });
                                hostArray[index3].Name = (string)properties["name"];
                            }
                        }
                    }
                }
            }
            return hostArray;
        }

        public async Task<IVimDatacenter> GetDatacenterAndPropertiesAsync()
        {
            IVimDatacenter vimDatacenter = null;
            TraversalSpec traversalSpec1 = new TraversalSpec();
            traversalSpec1.name = "folderTraversalSpec";
            traversalSpec1.type = "Folder";
            traversalSpec1.path = "parent";
            traversalSpec1.skip = true;
            traversalSpec1.selectSet = new SelectionSpec[1]
            {
        new SelectionSpec()
            };
            traversalSpec1.selectSet[0].name = "folderTraversalSpec";
            TraversalSpec traversalSpec2 = new TraversalSpec();
            traversalSpec2.name = "computeResourceTraversalSpec";
            traversalSpec2.type = "ComputeResource";
            traversalSpec2.path = "parent";
            traversalSpec2.skip = true;
            traversalSpec2.selectSet = new SelectionSpec[2]
            {
        (SelectionSpec) traversalSpec1,
        new SelectionSpec()
            };
            traversalSpec2.selectSet[1].name = "computeResourceTraversalSpec";
            TraversalSpec traversalSpec3 = new TraversalSpec();
            traversalSpec3.name = "hostTraversalSpec";
            traversalSpec3.type = "HostSystem";
            traversalSpec3.path = "parent";
            traversalSpec3.skip = true;
            traversalSpec3.selectSet = new SelectionSpec[1]
            {
        (SelectionSpec) traversalSpec2
            };
            PropertySpec[] propertySpecArray = new PropertySpec[1] { new PropertySpec() };
            propertySpecArray[0].all = false;
            propertySpecArray[0].pathSet = Datacenter.VCProperties;
            propertySpecArray[0].type = "Datacenter";
            PropertyFilterSpec propertyFilterSpec = new PropertyFilterSpec();
            propertyFilterSpec.propSet = propertySpecArray;
            propertyFilterSpec.objectSet = new ObjectSpec[1]
            {
        new ObjectSpec()
            };
            propertyFilterSpec.objectSet[0].obj = ManagedObject;
            propertyFilterSpec.objectSet[0].skip = false;
            propertyFilterSpec.objectSet[0].selectSet = new SelectionSpec[1]
            {
        (SelectionSpec) traversalSpec3
            };
            IVimService vcService = VcService;
            PropertyFilterSpec[] pfSpec = new PropertyFilterSpec[1] { propertyFilterSpec };
            foreach (ObjectContent retrieveProperty in await vcService.RetrievePropertiesAsync(pfSpec))
            {
                vimDatacenter = (IVimDatacenter)new Datacenter(VcService, retrieveProperty.obj);
                Dictionary<string, object> dictionary = VcService.PropSetToDictionary(retrieveProperty.propSet);
                vimDatacenter.GetCommonProperties(dictionary);
            }
            return vimDatacenter;
        }

        public async Task<IVimDatastore> GetDatastoreByUrlAsync(string url)
        {
            IVimDatastore[] datastoresAndProperties = await GetDatastoresAndPropertiesAsync();
            IVimDatastore vimDatastore1 = (IVimDatastore)null;
            foreach (IVimDatastore vimDatastore2 in datastoresAndProperties)
            {
                if (string.Compare(url, vimDatastore2.DsProperties.Url, true, CultureInfo.InvariantCulture) == 0)
                {
                    vimDatastore1 = vimDatastore2;
                    break;
                }
            }
            return vimDatastore1;
        }

        public override string GetName()
        {
            return (string)GetPropertyAsync("name").Result;
        }

        public async Task<IVimDatastore> GetDatastoreByNameAsync(string name)
        {
            IVimDatastore[] datastoresAndProperties = await GetDatastoresAndPropertiesAsync();
            IVimDatastore vimDatastore1 = (IVimDatastore)null;
            foreach (IVimDatastore vimDatastore2 in datastoresAndProperties)
            {
                if (string.Compare(name, vimDatastore2.DsProperties.Name, true, CultureInfo.InvariantCulture) == 0)
                {
                    vimDatastore1 = vimDatastore2;
                    break;
                }
            }
            return vimDatastore1;
        }

        public async Task<long> GetDatastoreMaxVmdkSizeMBAsync(string url)
        {
            return GetDatastoreMaxVmdkSizeMB(await GetDatastoreByUrlAsync(url));
        }

        public long GetDatastoreMaxVmdkSizeMB(IVimDatastore ds)
        {
            long num = 0;
            if (ds != null)
            {
                num = ds.DsProperties.MaxFileSize / 1048576L;
                if (ds.DsProperties.Type.Equals("VMFS") && ds.DsProperties.Version.StartsWith("5"))
                    num = _properties.Version.StartsWith("5.0") || _properties.Version.StartsWith("5.1") ? 2097151L : 65011712L;
            }
            if (num == 0L || num == 2097152L)
                num = 2097151L;
            return num;
        }

        public async Task<IVimVm> SearchVmByUuidAsync(string uuid)
        {
            return await VcService.SearchVmByUuidAsync(uuid);
        }

        public async Task<string> GetDatastorePathByUrlAsync(string url)
        {
            IVimDatastore datastoreByUrl = await GetDatastoreByUrlAsync(url);
            if (datastoreByUrl == null)
                return (string)null;
            return datastoreByUrl.GetPath();
        }

        public async Task UnregisterVmAsync(IVimVm vm)
        {
            await VcService.UnregisterVmAsync(vm);
        }

        public async Task<IVimVm> RegisterVmAsync(string dsPath, string resPoolName, VimClientlContext ctx)
        {
            ManagedObjectReference vmFolder = (await GetDatacenterAndPropertiesAsync()).DatacenterProperties.VmFolder;
            if (vmFolder == null)
                throw new Exception("vmFolder is null");
            IVimResourcePool vimResourcePool = (IVimResourcePool)null;
            if (!string.IsNullOrEmpty(resPoolName))
                vimResourcePool = await GetResourcePoolByNameAsync(resPoolName);
            if (vimResourcePool == null)
                vimResourcePool = await GetDefaultResourcePoolAsync();
            ManagedObjectReference managedObject = await VcService.Service.RegisterVM_TaskAsync(vmFolder, dsPath, (string)null, false, vimResourcePool.ManagedObject, ManagedObject);
            ctx.IsRetriableCall = false;
            VCTask task = new VCTask(VcService, managedObject);
            string op = "RegisterVm";
            VimClientlContext rstate = ctx;
            task.WaitForResultAsync(op, rstate);
            string[] properties1 = new string[1] { "info.result" };
            Dictionary<string, object> properties2 = await task.GetPropertiesAsync(properties1);
            if (properties2.ContainsKey("info.result"))
                return (IVimVm)new Vm(VcService, (ManagedObjectReference)properties2["info.result"]);
            throw new Exception("Vm managed object reference does not exist");
        }

        public async Task<ManagedObjectReference> GetComputeResourceAsync()
        {
            return (await GetManagedObjectsAsync(new string[1] { "parent" }))[0];
        }

        public async Task<IVimResourcePool[]> GetAllResourcePoolsAsync()
        {
            ManagedObjectReference computeResource = await GetComputeResourceAsync();
            TraversalSpec traversalSpec1 = new TraversalSpec();
            traversalSpec1.name = "resourcePoolTraversalSpec";
            traversalSpec1.type = "ResourcePool";
            traversalSpec1.path = "resourcePool";
            traversalSpec1.skip = true;
            traversalSpec1.selectSet = new SelectionSpec[1]
            {
        new SelectionSpec()
            };
            traversalSpec1.selectSet[0].name = "resourcePoolTraversalSpec";
            TraversalSpec traversalSpec2 = new TraversalSpec();
            traversalSpec2.name = "computeResourceTraversalSpec";
            traversalSpec2.type = "ComputeResource";
            traversalSpec2.path = "resourcePool";
            traversalSpec2.skip = true;
            traversalSpec2.selectSet = new SelectionSpec[1]
            {
        (SelectionSpec) traversalSpec1
            };
            PropertySpec[] propertySpecArray = new PropertySpec[1] { new PropertySpec() };
            propertySpecArray[0].all = false;
            propertySpecArray[0].pathSet = new string[2]
            {
        "name",
        "parent"
            };
            propertySpecArray[0].type = "ResourcePool";
            PropertyFilterSpec propertyFilterSpec = new PropertyFilterSpec();
            propertyFilterSpec.propSet = propertySpecArray;
            propertyFilterSpec.objectSet = new ObjectSpec[1]
            {
        new ObjectSpec()
            };
            propertyFilterSpec.objectSet[0].obj = computeResource;
            propertyFilterSpec.objectSet[0].skip = false;
            propertyFilterSpec.objectSet[0].selectSet = new SelectionSpec[1]
            {
        (SelectionSpec) traversalSpec2
            };
            ObjectContent[] objectContentArray = await VcService.RetrievePropertiesAsync(new PropertyFilterSpec[1] { propertyFilterSpec });
            List<ResourcePool> resourcePoolList = new List<ResourcePool>();
            foreach (ObjectContent objectContent in objectContentArray)
            {
                ResourcePool resourcePool = new ResourcePool(VcService, objectContent.obj);
                Dictionary<string, object> dictionary = VcService.PropSetToDictionary(objectContent.propSet);
                resourcePool.GetCommonProperties(dictionary);
                resourcePoolList.Add(resourcePool);
            }
            return resourcePoolList.ToArray();
        }

        public async Task<IVimResourcePool> GetDefaultResourcePoolAsync()
        {
            ManagedObjectAndProperties[] objectAndProperties1 = await GetManagedObjectAndPropertiesAsync(await GetComputeResourceAsync(), "resourcePool", "ResourcePool", new string[2] { "name", "parent" });
            ResourcePool resourcePool = null;
            foreach (ManagedObjectAndProperties objectAndProperties2 in objectAndProperties1)
            {
                if (!(objectAndProperties2.ManagedObject.type != "ResourcePool") && objectAndProperties2.Properties != null)
                {
                    resourcePool = new ResourcePool(VcService, objectAndProperties2.ManagedObject);
                    resourcePool.Name = (string)objectAndProperties2.Properties["name"];
                    resourcePool.Parent = (ManagedObjectReference)objectAndProperties2.Properties["parent"];
                    break;
                }
            }
            return resourcePool;
        }

        public async Task<IVimResourcePool> GetResourcePoolByNameAsync(string resPoolName)
        {
            IVimResourcePool vimResourcePool = (IVimResourcePool)null;
            if (!string.IsNullOrEmpty(resPoolName))
            {
                foreach (IVimResourcePool allResourcePool in await GetAllResourcePoolsAsync())
                {
                    if (string.Compare(allResourcePool.Name, resPoolName, true, CultureInfo.InvariantCulture) == 0)
                    {
                        vimResourcePool = allResourcePool;
                        break;
                    }
                }
            }
            return vimResourcePool;
        }

        public async Task MoveVmToResourcePoolAsync(IVimVm vm, string resPoolName)
        {
            IVimResourcePool[] allResourcePools = await GetAllResourcePoolsAsync();
            IVimResourcePool vimResourcePool1 = (IVimResourcePool)null;
            foreach (IVimResourcePool vimResourcePool2 in allResourcePools)
            {
                if (string.Compare(vimResourcePool2.Name, resPoolName, true, CultureInfo.InvariantCulture) == 0)
                {
                    vimResourcePool1 = vimResourcePool2;
                    break;
                }
            }
            if (vimResourcePool1 == null)
                return;
            await VcService.Service.MoveIntoResourcePoolAsync(new MoveIntoResourcePoolRequest(vimResourcePool1.ManagedObject, new ManagedObjectReference[] { vm.ManagedObject }));
        }

        public async Task<string> ContainsVmNameAsync(string vmName)
        {
            string str = (string)null;
            foreach (IVimVm vmsAndProperty in GetVmsAndProperties(await GetManagedObjectAndPropertiesAsync(ManagedObject, "vm", "VirtualMachine", Vm.VCProperties)))
            {
                if (string.Compare(vmsAndProperty.Name, vmName, true, CultureInfo.InvariantCulture) == 0)
                {
                    str = vmsAndProperty.VMProperties.Uuid;
                    break;
                }
            }
            return str;
        }

        public async Task<short> GetNumberCPUAsync()
        {
            return (short)(await GetPropertiesAsync(new string[1] { "summary.hardware.numCpuCores" }))["summary.hardware.numCpuCores"];
        }

        public async Task<short> GetNumberCpuThreadsAsync()
        {
            return (short)(await GetPropertiesAsync(new string[1] { "summary.hardware.numCpuThreads" }))["summary.hardware.numCpuThreads"];
        }

        public async Task<short> GetNumberCpuPackagesAsync()
        {
            return (short)(await GetPropertiesAsync(new string[1] { "summary.hardware.numCpuPkgs" }))["summary.hardware.numCpuPkgs"];
        }

        public async Task<long> GetMemoryAsync()
        {
            return (long)(await GetPropertiesAsync(new string[1] { "summary.hardware.memorySize" }))["summary.hardware.memorySize"];
        }

        public async Task<HostConfiguration> GetConfigurationAsync()
        {
            return new HostConfiguration() { NumCPU = await GetNumberCPUAsync(), NumCpuPkgs = await GetNumberCpuPackagesAsync(), NumCpuThreads = await GetNumberCpuThreadsAsync(), Memory = await GetMemoryAsync(), Networks = await GetNetworksAsync() };
        }

        public async Task<Dictionary<string, string[]>> GetVirtualSwitchAsync()
        {
            HostVirtualSwitch[] property1 = (HostVirtualSwitch[])(await GetPropertiesAsync(new string[1] { "config.network.vswitch" }))["config.network.vswitch"];
            PhysicalNic[] property2 = (PhysicalNic[])(await GetPropertiesAsync(new string[1] { "config.network.pnic" }))["config.network.pnic"];
            Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>((IEqualityComparer<string>)StringComparer.CurrentCultureIgnoreCase);
            List<string> stringList = new List<string>();
            foreach (HostVirtualSwitch hostVirtualSwitch in property1)
            {
                PhysicalNic[] property3 = (PhysicalNic[])(await GetPropertiesAsync(new string[1] { "config.network.vswitch[ \" " + hostVirtualSwitch.key + " \"].pnic" }))["config.network.vswitch[" + hostVirtualSwitch.key + "].pnic"];
                string[] pnic = hostVirtualSwitch.pnic;
                if (pnic != null)
                {
                    foreach (PhysicalNic physicalNic in property2)
                    {
                        foreach (string str in pnic)
                        {
                            if (physicalNic.key == str)
                            {
                                stringList.Add(str);
                                break;
                            }
                        }
                    }
                    string[] array = stringList.ToArray();
                    if (!dictionary.ContainsKey(hostVirtualSwitch.name))
                        dictionary.Add(hostVirtualSwitch.name, array);
                }
                else if (!dictionary.ContainsKey(hostVirtualSwitch.name))
                    dictionary.Add(hostVirtualSwitch.name, new string[1]
                    {
            "No pNic is attached."
                    });
            }
            return dictionary;
        }

        public async Task<IVimVm> GetVmAsync(string name)
        {
            IVimVm vimVm = (IVimVm)null;
            foreach (IVimVm vmsAndProperty in await GetVmsAndPropertiesAsync())
            {
                if (string.Compare(vmsAndProperty.VMProperties.Name, name, true, CultureInfo.InvariantCulture) == 0)
                {
                    vimVm = vmsAndProperty;
                    break;
                }
            }
            return vimVm;
        }

        public async Task<IVimVm> GetVmByUuidAsync(string uuid)
        {
            IVimVm vimVm = (IVimVm)null;
            foreach (IVimVm vmsAndProperty in await GetVmsAndPropertiesAsync())
            {
                if (string.Compare(vmsAndProperty.VMProperties.Uuid, uuid, true, CultureInfo.InvariantCulture) == 0)
                {
                    vimVm = vmsAndProperty;
                    break;
                }
            }
            return vimVm;
        }

        public async Task<IVimVm> CreateVmAsync(VmCreationInfo vmCreationInfo, VimClientlContext ctx)
        {
            VirtualMachineConfigSpec vmConfigSpec = await createVmConfigSpecAsync(vmCreationInfo);
            vmConfigSpec.guestId = vmCreationInfo.GuestId;
            vmConfigSpec.memoryMB = vmCreationInfo.MemoryMB;
            vmConfigSpec.memoryMBSpecified = true;
            vmConfigSpec.name = vmCreationInfo.Name;
            vmConfigSpec.numCPUs = vmCreationInfo.NumCPU;
            vmConfigSpec.numCPUsSpecified = true;
            vmConfigSpec.numCoresPerSocket = Math.Max(1, vmCreationInfo.NumCoresPerProcessor);
            vmConfigSpec.numCoresPerSocketSpecified = true;
            return await CreateVmAsync(vmConfigSpec, ctx);
        }
        public async Task<IVimVm> CreateVmAsync(VmCreationInfo vmCreationInfo, VimClientlContext ctx, string template, ManagedObjectReference chosenDatastore, ResourcePool resourcePool, string templateUUID)
        {
            VirtualMachineCloneSpec virtualMachineCloneSpec = new VirtualMachineCloneSpec();
            virtualMachineCloneSpec.location = new VirtualMachineRelocateSpec();
            virtualMachineCloneSpec.location.datastore = chosenDatastore;
            virtualMachineCloneSpec.location.pool = resourcePool.ManagedObject;
            virtualMachineCloneSpec.powerOn = false;
            virtualMachineCloneSpec.template = false;
            virtualMachineCloneSpec.customization = new CustomizationSpec()
            {
                globalIPSettings = new CustomizationGlobalIPSettings()
                {
                    dnsServerList = new string[] { },
                    dnsSuffixList = new string[] { }
                },
                identity = new CustomizationIdentitySettings()
                {

                },
                nicSettingMap = new CustomizationAdapterMapping[]
                { new CustomizationAdapterMapping
                    {

                        macAddress = "dynamic",
                        adapter = new CustomizationIPSettings()
                        {
                            dnsDomain = ""
                        }
                    }
                },
                options = new CustomizationOptions()
                {

                }
            };
            Vm baseTemplate = (Vm)await SearchVmByUuidAsync(templateUUID);


            VirtualMachineConfigSpec vmConfigSpec = await createVmConfigSpecAsync(vmCreationInfo);
            vmConfigSpec.guestId = vmCreationInfo.GuestId;
            vmConfigSpec.memoryMB = vmCreationInfo.MemoryMB;
            vmConfigSpec.memoryMBSpecified = true;
            vmConfigSpec.name = vmCreationInfo.Name;
            vmConfigSpec.numCPUs = vmCreationInfo.NumCPU;
            vmConfigSpec.numCPUsSpecified = true;
            vmConfigSpec.numCoresPerSocket = Math.Max(1, vmCreationInfo.NumCoresPerProcessor);
            vmConfigSpec.numCoresPerSocketSpecified = true;
            return await CreateVmAsync(vmConfigSpec, ctx);
        }
        // Set up Clone Spec.
        public async Task<IVimVm> CreateVmWithNetworkMappingAsync(VirtualMachineConfigSpec configSpec, Dictionary<string, string> networkMap, VimClientlContext ctx)
        {
            Dictionary<string, IVimNetwork> networksDict = new Dictionary<string, IVimNetwork>((IEqualityComparer<string>)StringComparer.CurrentCultureIgnoreCase);
            ((IEnumerable<IVimNetwork>)GetNetworksAsync()).ToList().ForEach(network =>
            {
                if (networksDict.ContainsKey(network.Name))
                    return;
                networksDict.Add(network.Name, network);
            });
            Dictionary<string, string> dvPortgroupUuids = await GetDistributedVirtualSwitchUuidsAsync();
            ((IEnumerable<VirtualDeviceConfigSpec>)configSpec.deviceChange).Where(vdcs => vdcs.device is VirtualEthernetCard).ToList().ForEach(vdcs =>
            {
                string key;
                IVimNetwork vimNetwork;
                if (!networkMap.TryGetValue(vdcs.device.deviceInfo.summary, out key) || !networksDict.TryGetValue(key, out vimNetwork))
                    throw new Exception(string.Format("Don't know how to map the network connection \"{0}\"", (object)vdcs.device.deviceInfo.summary));
                if (vimNetwork.IsDistributed)
                {
                    string str;
                    if (!dvPortgroupUuids.TryGetValue(vimNetwork.PortgroupKey, out str))
                        return;
                    vdcs.device.backing = (VirtualDeviceBackingInfo)new VirtualEthernetCardDistributedVirtualPortBackingInfo();
                    ((VirtualEthernetCardDistributedVirtualPortBackingInfo)vdcs.device.backing).port = new DistributedVirtualSwitchPortConnection()
                    {
                        switchUuid = str,
                        portgroupKey = vimNetwork.PortgroupKey
                    };
                }
                else
                {
                    vdcs.device.backing = (VirtualDeviceBackingInfo)new VirtualEthernetCardNetworkBackingInfo();
                    ((VirtualEthernetCardNetworkBackingInfo)vdcs.device.backing).network = networksDict[key].ManagedObject;
                    ((VirtualDeviceDeviceBackingInfo)vdcs.device.backing).deviceName = networksDict[key].Name;
                }
            });
            return await CreateVmAsync(configSpec, ctx);
        }

        public async Task<IVimVm> CreateVmAsync(VirtualMachineConfigSpec configSpec, VimClientlContext ctx)
        {
            ManagedObjectReference vmFolder = (await GetDatacenterAndPropertiesAsync()).GetVmFolder();
            IVimResourcePool defaultResourcePool = await GetDefaultResourcePoolAsync();
            long num1 = configSpec.memoryMB % 4L;
            if (num1 != 0L)
                configSpec.memoryMB += 4L - num1;
            VCTask task = new VCTask(VcService, await VcService.Service.CreateVM_TaskAsync(vmFolder, configSpec, defaultResourcePool.ManagedObject, ManagedObject));
            await task.WaitForResultAsync("CreateVm", ctx);
            Dictionary<string, object> dictionary = null;
            TaskInfoState taskInfoState = TaskInfoState.running;
            int num2;
            for (num2 = 0; num2 < 12; ++num2)
            {
                dictionary = await task.GetPropertiesAsync(new string[2] { "info.result", "info.state" });
                if (!dictionary.ContainsKey("info.result"))
                    Thread.Sleep(5000);
                else
                    break;
            }
            if (num2 == 12)
            {
                if (dictionary.ContainsKey("info.state"))
                    throw new Exception("Vm creation timed out. Status returned by CreateVM_Task: (TaskInfoState) " + (object)(TaskInfoState)dictionary["info.state"]);
                throw new Exception("Vm creation timed out. ESX async task issue. Check host performance.");
            }
            if (!dictionary.ContainsKey("info.result"))
                throw new Exception("Vm managed object reference does not exist");
            for (int index = 0; index < 60; ++index)
            {
                taskInfoState = (TaskInfoState)dictionary["info.state"];
                if (taskInfoState != TaskInfoState.success)
                    Thread.Sleep(1000);
                else
                    break;
            }
            if (taskInfoState != TaskInfoState.success)
                throw new Exception("Task State was never set to Success");
            IVimVm vimVm = (IVimVm)new Vm(VcService, (ManagedObjectReference)dictionary["info.result"]);
            for (int index = 0; index < 60; ++index)
            {
                if (!string.IsNullOrEmpty(vimVm.Uuid))
                    return vimVm;
                Thread.Sleep(5000);
            }
            throw new Exception("Vm creation timed out.  Check host performance.");
        }

        private async Task<VirtualDevice[]> getDefaultDevicesAsync(ManagedObjectReference computeResource)
        {
            VirtualMachineConfigOption machineConfigOption = await VcService.Service.QueryConfigOptionAsync((await GetManagedObjectsAsync(computeResource, new string[1] { "environmentBrowser" }))[0], (string)null, ManagedObject);
            VirtualDevice[] virtualDeviceArray = null;
            if (machineConfigOption != null)
                virtualDeviceArray = machineConfigOption.defaultDevice;
            return virtualDeviceArray;
        }

        private async Task<ConfigTarget> getConfigTargetForHostAsync(ManagedObjectReference computeResource)
        {
            return await VcService.Service.QueryConfigTargetAsync((await GetManagedObjectsAsync(computeResource, new string[1] { "environmentBrowser" }))[0], ManagedObject);
        }

        private VirtualDeviceConfigSpec createVirtualDiskConfigSpec(string vmName, VmDiskInfo diskInfo)
        {
            string str = GetVolumeName(diskInfo.Location.Name);
            if (!string.IsNullOrEmpty(diskInfo.File))
                str = str + vmName + "/" + diskInfo.File;
            VirtualDiskFlatVer2BackingInfo flatVer2BackingInfo = new VirtualDiskFlatVer2BackingInfo();
            flatVer2BackingInfo.datastore = diskInfo.Location.ManagedObject;
            flatVer2BackingInfo.fileName = str;
            flatVer2BackingInfo.diskMode = diskInfo.Mode;
            VirtualDisk virtualDisk = new VirtualDisk();
            virtualDisk.key = -1;
            virtualDisk.controllerKey = diskInfo.CtrlKey;
            virtualDisk.controllerKeySpecified = true;
            virtualDisk.unitNumber = diskInfo.UnitNumber;
            virtualDisk.unitNumberSpecified = true;
            virtualDisk.backing = (VirtualDeviceBackingInfo)flatVer2BackingInfo;
            virtualDisk.capacityInKB = diskInfo.SizeMB * 1024L;
            return new VirtualDeviceConfigSpec() { fileOperation = VirtualDeviceConfigSpecFileOperation.create, fileOperationSpecified = true, operation = VirtualDeviceConfigSpecOperation.add, operationSpecified = true, device = (VirtualDevice)virtualDisk };
        }

        private VirtualDeviceConfigSpec addVirtualDiskConfigSpec(VmDiskInfo diskInfo)
        {
            VirtualDiskFlatVer2BackingInfo flatVer2BackingInfo = new VirtualDiskFlatVer2BackingInfo();
            flatVer2BackingInfo.fileName = diskInfo.File;
            flatVer2BackingInfo.diskMode = diskInfo.Mode;
            VirtualDisk virtualDisk = new VirtualDisk();
            virtualDisk.backing = (VirtualDeviceBackingInfo)flatVer2BackingInfo;
            virtualDisk.key = -1;
            virtualDisk.controllerKey = diskInfo.CtrlKey;
            virtualDisk.controllerKeySpecified = true;
            virtualDisk.unitNumber = diskInfo.UnitNumber;
            virtualDisk.unitNumberSpecified = true;
            return new VirtualDeviceConfigSpec() { operation = VirtualDeviceConfigSpecOperation.add, operationSpecified = true, device = (VirtualDevice)virtualDisk };
        }

        private async Task<VirtualMachineConfigSpec> createVmConfigSpecAsync(VmCreationInfo vmCreationInfo)
        {
            ManagedObjectReference computeResource = await GetComputeResourceAsync();
            VirtualMachineConfigSpec machineConfigSpec = new VirtualMachineConfigSpec();
            VirtualDevice[] defaultDevices = await getDefaultDevicesAsync(computeResource);
            await getConfigTargetForHostAsync(computeResource);
            string volumeName = GetVolumeName(vmCreationInfo.Location.Name);
            machineConfigSpec.files = new VirtualMachineFileInfo()
            {
                vmPathName = volumeName
            };
            int num = 1;
            VirtualSCSIController virtualScsiController = vmCreationInfo.ScsiControllerType != ScsiControllerType.LsiLogicSAS ? (vmCreationInfo.ScsiControllerType != ScsiControllerType.LsiLogicParallel ? (VirtualSCSIController)new VirtualBusLogicController() : (VirtualSCSIController)new VirtualLsiLogicController()) : (VirtualSCSIController)new VirtualLsiLogicSASController();
            virtualScsiController.busNumber = 0;
            virtualScsiController.key = num;
            virtualScsiController.sharedBus = VirtualSCSISharing.noSharing;
            VirtualDeviceConfigSpec deviceConfigSpec1 = new VirtualDeviceConfigSpec();
            deviceConfigSpec1.operation = VirtualDeviceConfigSpecOperation.add;
            deviceConfigSpec1.operationSpecified = true;
            deviceConfigSpec1.device = (VirtualDevice)virtualScsiController;
            List<VirtualDeviceConfigSpec> deviceConfigSpecList1 = new List<VirtualDeviceConfigSpec>();
            if (vmCreationInfo.Disks != null)
            {
                foreach (VmDiskInfo disk in vmCreationInfo.Disks)
                {
                    VirtualDeviceConfigSpec deviceConfigSpec2 = !disk.Exists ? createVirtualDiskConfigSpec(vmCreationInfo.Name, disk) : addVirtualDiskConfigSpec(disk);
                    if (deviceConfigSpec2 != null)
                        deviceConfigSpecList1.Add(deviceConfigSpec2);
                }
            }
            VirtualDevice virtualDevice = (VirtualDevice)null;
            for (int index = 0; index < defaultDevices.Length; ++index)
            {
                if (defaultDevices[index] is VirtualIDEController)
                {
                    virtualDevice = defaultDevices[index];
                    break;
                }
            }
            VirtualDeviceConfigSpec deviceConfigSpec3 = (VirtualDeviceConfigSpec)null;
            if (virtualDevice != null)
            {
                VirtualCdromRemotePassthroughBackingInfo passthroughBackingInfo = new VirtualCdromRemotePassthroughBackingInfo();
                passthroughBackingInfo.exclusive = false;
                passthroughBackingInfo.deviceName = "";
                VirtualCdrom virtualCdrom = new VirtualCdrom();
                virtualCdrom.backing = (VirtualDeviceBackingInfo)passthroughBackingInfo;
                virtualCdrom.key = -1;
                virtualCdrom.controllerKey = virtualDevice.key;
                virtualCdrom.controllerKeySpecified = true;
                virtualCdrom.unitNumber = 0;
                virtualCdrom.unitNumberSpecified = true;
                deviceConfigSpec3 = new VirtualDeviceConfigSpec();
                deviceConfigSpec3.operation = VirtualDeviceConfigSpecOperation.add;
                deviceConfigSpec3.operationSpecified = true;
                deviceConfigSpec3.device = (VirtualDevice)virtualCdrom;
            }
            IVimNetwork[] networks = await GetNetworksAsync();
            Dictionary<string, Network> dictionary = new Dictionary<string, Network>((IEqualityComparer<string>)StringComparer.CurrentCultureIgnoreCase);
            foreach (Network network in networks)
            {
                if (!dictionary.ContainsKey(network.Name))
                    dictionary.Add(network.Name, network);
            }
            Dictionary<string, string> virtualSwitchUuids = await GetDistributedVirtualSwitchUuidsAsync();
            List<VirtualDeviceConfigSpec> deviceConfigSpecList2 = new List<VirtualDeviceConfigSpec>();
            VirtualDeviceBackingInfo deviceBackingInfo = (VirtualDeviceBackingInfo)null;
            foreach (string key in vmCreationInfo.NICMapping)
            {
                if (!("---Discard---" == key))
                {
                    if (dictionary.ContainsKey(key))
                    {
                        Network network = dictionary[key];
                        if (network.IsDistributed)
                        {
                            if (virtualSwitchUuids.ContainsKey(network.PortgroupKey))
                            {
                                deviceBackingInfo = (VirtualDeviceBackingInfo)new VirtualEthernetCardDistributedVirtualPortBackingInfo();
                                ((VirtualEthernetCardDistributedVirtualPortBackingInfo)deviceBackingInfo).port = new DistributedVirtualSwitchPortConnection()
                                {
                                    switchUuid = virtualSwitchUuids[network.PortgroupKey],
                                    portgroupKey = network.PortgroupKey
                                };
                            }
                        }
                        else
                        {
                            deviceBackingInfo = (VirtualDeviceBackingInfo)new VirtualEthernetCardNetworkBackingInfo();
                            ((VirtualEthernetCardNetworkBackingInfo)deviceBackingInfo).network = dictionary[key].ManagedObject;
                            ((VirtualDeviceDeviceBackingInfo)deviceBackingInfo).deviceName = dictionary[key].Name;
                        }
                    }
                    else
                        deviceBackingInfo = (VirtualDeviceBackingInfo)new VirtualEthernetCardNetworkBackingInfo();
                    VirtualEthernetCard virtualEthernetCard = vmCreationInfo.NicType != VirtualNicType.Vmxnet3 ? (vmCreationInfo.NicType != VirtualNicType.Vmxnet ? (vmCreationInfo.NicType != VirtualNicType.E1000 ? new VirtualPCNet32() : (VirtualEthernetCard)new VirtualE1000()) : new VirtualVmxnet()) : new VirtualVmxnet3();
                    virtualEthernetCard.addressType = "generated";
                    virtualEthernetCard.backing = deviceBackingInfo;
                    virtualEthernetCard.key = -1;
                    deviceConfigSpecList2.Add(new VirtualDeviceConfigSpec()
                    {
                        operation = VirtualDeviceConfigSpecOperation.add,
                        operationSpecified = true,
                        device = (VirtualDevice)virtualEthernetCard
                    });
                }
            }
            List<VirtualDeviceConfigSpec> deviceConfigSpecList3 = new List<VirtualDeviceConfigSpec>();
            deviceConfigSpecList3.Add(deviceConfigSpec1);
            foreach (VirtualDeviceConfigSpec deviceConfigSpec2 in deviceConfigSpecList1)
                deviceConfigSpecList3.Add(deviceConfigSpec2);
            if (virtualDevice != null)
                deviceConfigSpecList3.Add(deviceConfigSpec3);
            foreach (VirtualDeviceConfigSpec deviceConfigSpec2 in deviceConfigSpecList2)
                deviceConfigSpecList3.Add(deviceConfigSpec2);
            machineConfigSpec.deviceChange = deviceConfigSpecList3.ToArray();
            return machineConfigSpec;
        }

        public async Task<IVimVm[]> GetVmTemplatesAndPropertiesAsync()
        {
            ManagedObjectAndProperties[] objectAndProperties1 = await GetManagedObjectAndPropertiesAsync(ManagedObject, "vm", "VirtualMachine", Vm.VCProperties);
            List<IVimVm> vimVmList = new List<IVimVm>();
            foreach (ManagedObjectAndProperties objectAndProperties2 in objectAndProperties1)
            {
                IVimVm vimVm = (IVimVm)new Vm(VcService, objectAndProperties2.ManagedObject);
                vimVm.GetCommonProperties(objectAndProperties2.Properties);
                if (vimVm.VMProperties.IsTemplate)
                    vimVmList.Add(vimVm);
            }
            return vimVmList.ToArray();
        }

        public async Task<IVimVm> GetRecentlyCreatedVmAsync(ManagedObjectReference task)
        {
            TaskInfoState taskInfoState = TaskInfoState.running;
            do
            {
                ManagedObjectReference managedObject = ManagedObject;
                string path = "recentTask";
                string childType = "Task";
                string[] childProperties = new string[3] { "info.completeTime", "info.descriptionId", "info.state" };
                foreach (ManagedObjectAndProperties objectAndProperty in await GetManagedObjectAndPropertiesAsync(managedObject, path, childType, childProperties))
                {
                    if (objectAndProperty.Properties != null && objectAndProperty.ManagedObject == task)
                    {
                        taskInfoState = (TaskInfoState)objectAndProperty.Properties["info.state"];
                        break;
                    }
                }
                Thread.Sleep(1000);
            }
            while (taskInfoState == TaskInfoState.running);
            ManagedObjectReference[] managedObjects = await GetManagedObjectsAsync(task, new string[1] { "entity" });
            IVimVm? vimVm = null;
            if (managedObjects.Length != 0)
                vimVm = (IVimVm)new Vm(VcService, managedObjects[0]);
            return vimVm;
        }

        public async Task<List<string>> SearchDatastoreSubFolderAsync(string folderName, VimClientlContext ctx)
        {
            List<string> stringList = new List<string>();
            VCTask task = new VCTask(VcService, await VcService.Service.SearchDatastoreSubFolders_TaskAsync((await GetManagedObjectsAsync(new string[1] { "datastoreBrowser" }))[0], "[] " + folderName, new HostDatastoreBrowserSearchSpec() { matchPattern = new string[1] { "*.vmdk" }, searchCaseInsensitive = true, searchCaseInsensitiveSpecified = true }));
            string op = "Browse Datastore";
            VimClientlContext rstate = ctx;
            await task.WaitForResultAsync(op, rstate);
            string[] properties1 = new string[1] { "info.result" };
            Dictionary<string, object> properties2 = await task.GetPropertiesAsync(properties1);
            if (properties2.ContainsKey("info.result"))
            {
                HostDatastoreBrowserSearchResults[] browserSearchResultsArray = (HostDatastoreBrowserSearchResults[])properties2["info.result"];
                if (browserSearchResultsArray != null && browserSearchResultsArray.Length != 0 && browserSearchResultsArray[0].file != null)
                {
                    foreach (Prinubes.vCenterSDK.FileInfo fileInfo in browserSearchResultsArray[0].file)
                    {
                        if (!fileInfo.path.EndsWith("-flat.vmdk", StringComparison.InvariantCultureIgnoreCase) && !fileInfo.path.EndsWith("-delta.vmdk", StringComparison.InvariantCultureIgnoreCase) && !stringList.Contains(fileInfo.path))
                            stringList.Add(fileInfo.path);
                    }
                }
            }
            return stringList;
        }

        public bool UserHasPermissions(out int userRole)
        {
            userRole = 0;
            IVimDatacenter datacenterAndProperties = GetDatacenterAndPropertiesAsync().Result;
            if (datacenterAndProperties.DatacenterProperties.EffectiveRoles == null)
                return false;
            foreach (int effectiveRole in datacenterAndProperties.DatacenterProperties.EffectiveRoles)
            {
                userRole = effectiveRole;
                if (effectiveRole == -1 || effectiveRole == 1 || effectiveRole == 2)
                    return true;
            }
            return false;
        }

        public async Task<ManagedObjectAndProperties[]> GetMangedObjectsAndPropertiesAsync()
        {
            PropertySpec propertySpec1 = new PropertySpec();
            propertySpec1.type = "HostSystem";
            propertySpec1.all = false;
            propertySpec1.pathSet = Host.VCProperties;
            PropertySpec propertySpec2 = new PropertySpec();
            propertySpec2.type = "VirtualMachine";
            propertySpec2.all = false;
            propertySpec2.pathSet = Vm.VCProperties;
            PropertySpec propertySpec3 = new PropertySpec();
            propertySpec3.type = "Datastore";
            propertySpec3.all = false;
            propertySpec3.pathSet = Datastore.VCProperties;
            PropertySpec propertySpec4 = new PropertySpec();
            propertySpec4.type = "Network";
            propertySpec4.all = false;
            propertySpec4.pathSet = new string[1] { "name" };
            ObjectSpec objectSpec = new ObjectSpec();
            objectSpec.obj = ManagedObject;
            objectSpec.skip = false;
            objectSpec.selectSet = new SelectionSpec[3]
            {
        (SelectionSpec) new TraversalSpec()
        {
          type = "HostSystem",
          path = "vm"
        },
        (SelectionSpec) new TraversalSpec()
        {
          type = "HostSystem",
          path = "datastore"
        },
        (SelectionSpec) new TraversalSpec()
        {
          type = "HostSystem",
          path = "network"
        }
            };
            PropertyFilterSpec propertyFilterSpec = new PropertyFilterSpec();
            propertyFilterSpec.propSet = new PropertySpec[4]
            {
        propertySpec1,
        propertySpec2,
        propertySpec3,
        propertySpec4
            };
            propertyFilterSpec.objectSet = new ObjectSpec[1]
            {
        objectSpec
            };
            List<ManagedObjectAndProperties> objectAndPropertiesList = new List<ManagedObjectAndProperties>();
            ObjectContent[] objectContentArray = await VcService.RetrievePropertiesAsync(new PropertyFilterSpec[1] { propertyFilterSpec });
            if (objectContentArray != null)
            {
                foreach (ObjectContent objectContent in objectContentArray)
                    objectAndPropertiesList.Add(new ManagedObjectAndProperties()
                    {
                        ManagedObject = objectContent.obj,
                        Properties = VcService.PropSetToDictionary(objectContent.propSet)
                    });
            }
            return objectAndPropertiesList.ToArray();
        }
    }
}
