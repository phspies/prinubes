using System.Runtime.Serialization;

namespace PlatformWorker.Common
{
    [DataContract]
    public class OperatingSystemVersion : IExtensibleDataObject
    {
        [DataMember]
        public int Major { get; set; }

        [DataMember]
        public int Minor { get; set; }

        [DataMember]
        public int Build { get; set; }

        [DataMember]
        public int Revision { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
