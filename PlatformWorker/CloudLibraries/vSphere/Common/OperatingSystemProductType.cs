using System.Runtime.Serialization;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.Common
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
