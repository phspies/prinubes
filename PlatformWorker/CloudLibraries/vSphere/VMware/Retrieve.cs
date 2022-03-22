using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prinubes.vCenterSDK;
using ObjectContent = Prinubes.vCenterSDK.ObjectContent;

namespace PlatformWorker.VMware
{
    internal class Retrieve
    {

        public static async Task<List<ObjectContent>> AllPropertiesAsync(PropertyFilterSpec pfSpec, ServiceContent _sic, VimPortType _service)
        {
            List<ObjectContent> listobjcontent = new List<ObjectContent>();
            // RetrievePropertiesEx() returns the properties selected from the PropertyFilterSpec
            RetrieveResult rslts = (await _service.RetrievePropertiesExAsync(new RetrievePropertiesExRequest(_sic.propertyCollector,new PropertyFilterSpec[] { pfSpec },new RetrieveOptions()))).returnval;
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
                rslts = await _service.ContinueRetrievePropertiesExAsync(_sic.propertyCollector, token);
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
            return listobjcontent;
        }
    }
}
