using Prinubes.Common.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prinubes.Common.Models.Interfaces
{
    public interface ITag
    {
        public List<TaggingModel> Tags { get; set; }
        public Guid OrganizationID { get; set; }
    }
}
