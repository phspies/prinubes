using PlatformWorker;
using PlatformWorker.VMware;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        VCService vc = new VCService("192.168.0.253", "administrator@vsphere.local", "VMware1!");
        vc.Logon();
        var ds = await vc.GetAllDatastoresAsync();
        var dc = vc.GetDatacenters();
       
        var vms = await vc.GetAllVMsDictWithNameAsync();
        var test = vms["edge01"].GetResourcePoolAsync();
        var vms2 = vc.GetVmInventoryAsync();
        //_serviceInstance.Connect("https://192.168.0.253/sdk", "administrator@vsphere.local", "VMware1!");

        //ServiceContent _sic = _serviceInstance._sic;
        //VimPortType _service = _serviceInstance._service;

        //var dcs = Datacenter.GetAll(_sic, _service);
        //var vms = VM.GetAll(dcs[0].obj, _sic, _service);
        //var hosts = Host.GetAll(dcs[0].obj, _sic, _service);
        //var networks = Network.GetAll(dcs[0].obj,_sic, _service);

        var datacenterName = "Tanzu";
       // ManagedObjectReference datacenterRef = _service.FindByInventoryPath(_sic.searchIndex, datacenterName);

        //if (datacenterRef == null)
        //{
        //    Console.WriteLine("The specified datacenter is not found");
        //}



        //// Get a list of Windows VM's

        //var vmlist = Client.FindEntityViews(typeof(VirtualMachine), null, null, null);

        ////Get a list of ESXi hosts
        //var hostlist = Client.FindEntityViews(typeof(HostSystem), null, null, null);
        ////Get a list of Clusters

        //NameValueCollection filter = new NameValueCollection();
        //filter.Add("Name", "Tanzu01");
        //var clusterlist = Client.FindEntityViews(typeof(ClusterComputeResource), null, filter, null);
        //clusterlist[0].
        //var test = Client.ServiceContent;

        //var test2 = Client.ServiceContent.RootFolder;
        //NameValueCollection filter = new NameValueCollection();
        //filter.Add("Name", "Tanzu01")
        //Client.FindEntityView(typeof(ClusterComputeResource), null, )

        Console.WriteLine("test");
    }
}