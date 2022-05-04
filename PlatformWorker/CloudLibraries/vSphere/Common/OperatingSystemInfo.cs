using System.Runtime.Serialization;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.Common
{
    [DataContract]
    public class OperatingSystemInfo : IExtensibleDataObject
    {
        [DataMember]
        public string VersionString { get; set; }

        [DataMember]
        public string ServicePack { get; set; }

        [DataMember]
        public int CSDVersion { get; set; }

        [DataMember]
        public OperatingSystemVersion Version { get; set; }

        [DataMember]
        public OperatingSystemArchitecture Architecture { get; set; }

        [DataMember]
        public OperatingSystemProductType ProductType { get; set; }

        [DataMember]
        public int ProductSuite { get; set; }

        [DataMember]
        public bool HasBCDTemplate { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
