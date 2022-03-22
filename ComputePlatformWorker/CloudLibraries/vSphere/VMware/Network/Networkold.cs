using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prinubes.vCenterSDK;

namespace PlatformWorker.VMware
{
    internal class Networkold
    { 
        public static async Task<ObjectContent[]> GetAllAsync(ManagedObjectReference dcMoRef, ServiceContent _sic, VimPortType _service)
        {
            // PropertySpec specifies what properties to
            // retrieve from what type of Managed Object
            // This spec selects the Network name
            PropertySpec networkPropSpec = new PropertySpec();
            networkPropSpec.type = "Network";
            networkPropSpec.pathSet = new String[] { "name" };
            // This spec selects HostSystem information
    
            TraversalSpec network2host = new TraversalSpec();
            network2host.type = "Network";
            network2host.path = "host";
            // c. Traverse from a Network through the 'vm' property
            TraversalSpec network2vm = new TraversalSpec();
            network2vm.type = "Network";
            network2vm.path = "vm";
            // a. Traverse from a Datacenter through
            // the 'network' property
            TraversalSpec dc2network = new TraversalSpec();
            dc2network.type = "Datacenter";
            dc2network.path = "network";
            dc2network.selectSet = new SelectionSpec[] {
                // Add b. traversal
                network2host
                // Add c. traversal
                 };
            // ObjectSpec specifies the starting object and
            // any TraversalSpecs used to specify other objects
            // for consideration
            ObjectSpec oSpec = new ObjectSpec();
            oSpec.obj = dcMoRef;
            oSpec.skip = true;
            oSpec.selectSet = new SelectionSpec[] { dc2network };
            // PropertyFilterSpec is used to hold the ObjectSpec and
            // PropertySpec for the call
            PropertyFilterSpec pfSpec = new PropertyFilterSpec();
            pfSpec.propSet = new PropertySpec[] { networkPropSpec };
            pfSpec.objectSet = new ObjectSpec[] { oSpec };
            // RetrieveProperties() returns the properties
            // selected from the PropertyFilterSpec
            List<ObjectContent> listobjcontent = await Retrieve.AllPropertiesAsync(pfSpec, _sic, _service);
            return listobjcontent.ToArray();
        }
    }
}
