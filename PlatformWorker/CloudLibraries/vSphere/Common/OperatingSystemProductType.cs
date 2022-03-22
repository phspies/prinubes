using System.Runtime.Serialization;

namespace PlatformWorker.Common
{
    [DataContract]
    public enum OperatingSystemProductType
    {
        [EnumMember] None,
        [EnumMember] Workstation,
        [EnumMember] DomainController,
        [EnumMember] Server,
    }
}
