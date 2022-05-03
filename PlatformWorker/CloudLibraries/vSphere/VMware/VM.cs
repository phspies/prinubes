using Prinubes.vCenterSDK;
using ObjectContent = Prinubes.vCenterSDK.ObjectContent;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    public class VM
    {
        public static async Task<ObjectContent[]> GetAllAsync(ManagedObjectReference dcMoRef, ServiceContent _sic, VimPortType _service)
        {
            // The PropertySpec object specifies what properties
            // retrieve from what type of Managed Object
            PropertySpec pSpec = new PropertySpec();
            pSpec.type = "VirtualMachine";
            pSpec.pathSet = new String[] {
              "name",
              "config.guestFullName",
              "config.hardware.memoryMB",
              "config.hardware.numCPU",
              "guest.toolsStatus",
              "guestHeartbeatStatus",
              "guest.ipAddress",
              "guest.hostName",
              "runtime.powerState",
              "summary.quickStats.overallCpuUsage",
              "summary.quickStats.hostMemoryUsage",
              "summary.quickStats.guestMemoryUsage", };

            SelectionSpec recurseFolders = new SelectionSpec();
            recurseFolders.name = "folder2childEntity";
            TraversalSpec folder2childEntity = new TraversalSpec();
            folder2childEntity.type = "Folder";
            folder2childEntity.path = "childEntity";
            folder2childEntity.name = recurseFolders.name;
            folder2childEntity.selectSet = new SelectionSpec[] { recurseFolders };
            // Traverse from a Datacenter through the 'vmFolder' property
            TraversalSpec dc2vmFolder = new TraversalSpec();
            dc2vmFolder.type = "Datacenter";
            dc2vmFolder.path = "vmFolder";
            dc2vmFolder.selectSet = new SelectionSpec[] { folder2childEntity };
            ObjectSpec oSpec = new ObjectSpec();
            oSpec.obj = dcMoRef;
            oSpec.skip = true;
            oSpec.selectSet = new SelectionSpec[] { dc2vmFolder };
            PropertyFilterSpec pfSpec = new PropertyFilterSpec();
            pfSpec.propSet = new PropertySpec[] { pSpec };
            pfSpec.objectSet = new ObjectSpec[] { oSpec };
            List<ObjectContent> listobjcontent = await Retrieve.AllPropertiesAsync(pfSpec, _sic, _service);
            return listobjcontent.ToArray();
        }
        private static async Task<ObjectContent[]> VMInfoAsync(ManagedObjectReference vmMoRef, ServiceContent _sic, VimPortType _service)
        {
            // This spec selects VirtualMachine information
            PropertySpec vmPropSpec = new PropertySpec();
            vmPropSpec.type = "VirtualMachine";
            vmPropSpec.pathSet = new String[] {
              "name",
              "config.guestFullName",
              "config.hardware.memoryMB",
              "config.hardware.numCPU",
              "guest.toolsStatus",
              "guestHeartbeatStatus",
              "guest.ipAddress",
              "guest.hostName",
              "runtime.powerState",
              "summary.quickStats.overallCpuUsage",
              "summary.quickStats.hostMemoryUsage",
              "summary.quickStats.guestMemoryUsage", };
            PropertySpec hostPropSpec = new PropertySpec();
            hostPropSpec.type = "HostSystem";
            hostPropSpec.pathSet = new String[] { "name" };
            PropertySpec taskPropSpec = new PropertySpec();
            taskPropSpec.type = "Task";
            taskPropSpec.pathSet = new String[] { "info.name", "info.completeTime" };
            PropertySpec datastorePropSpec = new PropertySpec();
            datastorePropSpec.type = "Datastore";
            datastorePropSpec.pathSet = new String[] { "info" };
            PropertySpec networkPropSpec = new PropertySpec();
            networkPropSpec.type = "Network";
            networkPropSpec.pathSet = new String[] { "name" };
            TraversalSpec hostTraversalSpec = new TraversalSpec();
            hostTraversalSpec.type = "VirtualMachine";
            hostTraversalSpec.path = "runtime.host";
            TraversalSpec taskTravesalSpec = new TraversalSpec();
            taskTravesalSpec.type = "VirtualMachine";
            taskTravesalSpec.path = "recentTask";
            TraversalSpec datastoreTraversalSpec = new TraversalSpec();
            datastoreTraversalSpec.type = "VirtualMachine";
            datastoreTraversalSpec.path = "datastore";
            TraversalSpec networkTraversalSpec = new TraversalSpec();
            networkTraversalSpec.type = "VirtualMachine";
            networkTraversalSpec.path = "network";
            // ObjectSpec specifies the starting object and
            // any TraversalSpecs used to specify other objects
            // for consideration
            ObjectSpec oSpec = new ObjectSpec();
            oSpec.obj = vmMoRef;
            // Add the TraversalSpec objects to the ObjectSpec
            // This specifies what additional object we want to
            // consider during the process.
            oSpec.selectSet = new SelectionSpec[] {
            hostTraversalSpec,
            taskTravesalSpec,
            datastoreTraversalSpec,
            networkTraversalSpec };
            PropertyFilterSpec pfSpec = new PropertyFilterSpec();
            // Add the PropertySpec objects to the PropertyFilterSpec
            // This specifies what data we want to collect while
            // processing the found objects from the ObjectSpec
            pfSpec.propSet = new PropertySpec[] {
            vmPropSpec,
            hostPropSpec,
            taskPropSpec,
            datastorePropSpec,
            networkPropSpec };
            pfSpec.objectSet = new ObjectSpec[] { oSpec };
            List<ObjectContent> listobjcontent = await Retrieve.AllPropertiesAsync(pfSpec, _sic, _service);
            return listobjcontent.ToArray();
        }
    }
}
