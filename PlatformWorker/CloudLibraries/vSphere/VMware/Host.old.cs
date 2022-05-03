using Prinubes.vCenterSDK;
using ObjectContent = Prinubes.vCenterSDK.ObjectContent;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.VMware
{
    internal class HostOld

    {
        public static async Task<ObjectContent[]> GetAllAsync(ManagedObjectReference dcMoRef, ServiceContent _sic, VimPortType _service)
        {
            PropertySpec pSpec = new PropertySpec();
            pSpec.type = "HostSystem";
            pSpec.pathSet = new String[] { "network", "name",
                "summary.hardware", "runtime.connectionState",
                "summary.overallStatus", "summary.quickStats" };


            TraversalSpec computeResource2HostTraversal = new TraversalSpec();
            computeResource2HostTraversal.type = "ComputeResource";
            computeResource2HostTraversal.path = "host";
            computeResource2HostTraversal.name = "computeResource2HostTraversal";

            SelectionSpec recurseFolders = new SelectionSpec();
            recurseFolders.name = "folder2childEntity";

            TraversalSpec folder2childEntity = new TraversalSpec();
            folder2childEntity.type = "Folder";
            folder2childEntity.path = "childEntity";
            folder2childEntity.name = recurseFolders.name;
            folder2childEntity.selectSet = new SelectionSpec[] { recurseFolders, computeResource2HostTraversal };

            TraversalSpec dc2HostFolderTraversal = new TraversalSpec();
            dc2HostFolderTraversal.type = "Datacenter";
            dc2HostFolderTraversal.path = "hostFolder";
            dc2HostFolderTraversal.name = "dc2HostFolderTraversal";
            dc2HostFolderTraversal.selectSet = new SelectionSpec[] { folder2childEntity };

            ObjectSpec oSpec = new ObjectSpec();
            oSpec.obj = dcMoRef;
            oSpec.skipSpecified = true;
            oSpec.selectSet = new SelectionSpec[] { dc2HostFolderTraversal };

            PropertyFilterSpec pfSpec = new PropertyFilterSpec();
            pfSpec.propSet = new PropertySpec[] { pSpec };
            pfSpec.objectSet = new ObjectSpec[] { oSpec };


            List<ObjectContent> listobjcontent = await Retrieve.AllPropertiesAsync(pfSpec, _sic, _service);
            return listobjcontent.ToArray();


            // PropertySpec specifies what properties to
            //// retrieve from what type of Managed Object
            //PropertySpec pSpec = new PropertySpec();
            //pSpec.type = "HostSystem";
            //pSpec.pathSet = new String[] { "network", "name",
            //    "summary.hardware", "runtime.connectionState",
            //    "summary.overallStatus", "summary.quickStats" };
            ////SelectionSpec recurseFolders = new SelectionSpec();
            ////recurseFolders.name = "folder2childEntity";
            ////TraversalSpec computeResource2host = new TraversalSpec();
            ////computeResource2host.type = "ComputeResource";
            ////computeResource2host.path = "host";
            ////TraversalSpec folder2childEntity = new TraversalSpec();
            ////folder2childEntity.type = "Folder";
            ////folder2childEntity.path = "childEntity";
            ////folder2childEntity.name = recurseFolders.name;
            ////// Add BOTH of the specifications to this specification
            ////folder2childEntity.selectSet = new SelectionSpec[] { recurseFolders };
            ////// Traverse from a Datacenter through
            ////// the 'hostFolder' property
            //TraversalSpec dc2hostFolder = new TraversalSpec();
            //dc2hostFolder.type = "Datacenter";
            //dc2hostFolder.path = "hostFolder";
            ////dc2hostFolder.selectSet = new SelectionSpec[] { folder2childEntity };
            //ObjectSpec oSpec = new ObjectSpec();
            //oSpec.obj = dcMoRef;
            //oSpec.skip = true;
            //oSpec.selectSet = new SelectionSpec[] { dc2hostFolder };
            //PropertyFilterSpec pfSpec = new PropertyFilterSpec();
            //pfSpec.propSet = new PropertySpec[] { pSpec };
            //pfSpec.objectSet = new ObjectSpec[] { oSpec };

        }
    }
}
