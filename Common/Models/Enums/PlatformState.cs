using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Prinubes.Common.Models.Enums
{
    public class PlatformState
    {
        [JsonProperty("platform_state_comment")]
        public string? Comment { get; set; }

        [JsonProperty("platform_state")]
        public PlatformStateEnum? State { get; set; }


    }
    public enum PlatformStateEnum
    {
        [EnumMember(Value = "Initializing")]
        Initializing,
        [EnumMember(Value = "Ready")]
        Ready,
        [EnumMember(Value = "Busy")]
        Busy,
        [EnumMember(Value = "Error")]
        Error,
        [EnumMember(Value = "Deleting")]
        Deleting
    }
}
