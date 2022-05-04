using System.Runtime.Serialization;

namespace Prinubes.PlatformWorker.CloudLibraries.vSphere.Common
{
    [DataContract]
    public enum OperatingSystemArchitecture
    {
        [EnumMember] x86 = 0,
        [EnumMember] ia64 = 6,
        [EnumMember] x64 = 9,
    }
}
