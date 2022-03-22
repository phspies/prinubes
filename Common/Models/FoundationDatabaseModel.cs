using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Prinubes.Common.DatabaseModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class FoundationDatabaseModel : RowVersionModel
    {
        [JsonRequired]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("create_timestamp")]
        public DateTime CreateTimeStamp { get; set; }

        [JsonProperty("update_timestamp")]
        public DateTime UpdateTimeStamp { get; set; }
    }

    public interface IRowVersionModel
    {
        byte[]? RowVersion { get; set; }
    }

    public abstract class RowVersionModel : IRowVersionModel
    {
        [JsonProperty("row_version")]
        public byte[]? RowVersion { get; set; }
    }

    public enum ActionEnum
    {
        create, update, delete, detach, attach, test
    }
}
