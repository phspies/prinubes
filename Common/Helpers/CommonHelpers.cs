using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Datamodels;
using Prinubes.Common.Models.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Prinubes.Common.Helpers
{
    public class CommonHelpers
    {
        public static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(a1, a2);
        }

        //public static void ReconcileTags(List<TaggingDatabaseModel> sourceList, ITag targetObject, IPrinubesDBContext dbContext)
        //{
        //    if (sourceList != null)
        //    {
        //        foreach (var tag in sourceList)
        //        {
        //            var existingDBTag = dbContext.Tags.FirstOrDefault(x => x.OrganizationID == targetObject.OrganizationID && x.Key == tag.Key && x.Value == tag.Value);
        //            if (targetObject.Tags.Exists(x => x.Key == tag.Key))
        //            {
        //                //check if the key/value pair exists in the DB
        //                if (existingDBTag != null)
        //                {
        //                    targetObject.Tags.Remove(targetObject.Tags.Single(x => x.Key == tag.Key));
        //                    targetObject.Tags.Add(existingDBTag);
        //                }
        //                else
        //                {
        //                    //we have a new tag/value which needs to be created
        //                    var tmptag = targetObject.Tags.Single(x => x.Key == tag.Key);
        //                    tmptag.Value = tag.Value;
        //                    tmptag.OrganizationID = targetObject.OrganizationID;
        //                }
        //            }
        //            else
        //            {
        //                //Check if tag/value pair exists in the db
        //                if (existingDBTag != null)
        //                {
        //                    targetObject.Tags.Add(existingDBTag);
        //                }
        //                else
        //                {
        //                    targetObject.Tags.Add(new TaggingDatabaseModel() { OrganizationID = targetObject.OrganizationID, Key = tag.Key, Value = tag.Value });
        //                }
        //            }
        //        };

        //        foreach (var tag in sourceList)
        //        {
        //            tag.OrganizationID = targetObject.OrganizationID;
        //            if (!targetObject.Tags.Exists(x => x.Key == tag.Key))
        //            {
        //                targetObject.Tags.Remove(targetObject.Tags.Single(x => x.Key == tag.Key));
        //            }
        //        };

        //        //var orphanedTags = targetObject.Tags.Where(x => x.ChilderCount() == 0);
        //        //dbContext.Tags.RemoveRange(orphanedTags);
        //    }
        //}
    }
}
