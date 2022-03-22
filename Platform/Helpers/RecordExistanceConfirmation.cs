using Prinubes.Common.DatabaseModels;
using Prinubes.Platforms.Datamodels;

namespace Prinubes.Platform.Helpers
{
    public class RecordExistanceConfirmation
    {
        static int retries = 60;
        static int waittime = 1000;
        public static async Task OrganizationExistsAsync(Guid orgId, ILogger _logger, PrinubesPlatformDBContext DBContext)
        {
            int count = 0;
            bool found = false;
            while (count < retries)
            {
                if (!DBContext.Organizations.Any(x => x.Id == orgId))
                {
                    _logger.LogDebug($"Group message, organization does not exist yet: {orgId} - retry {count}");
                    await Task.Delay(waittime);
                }
                else
                {
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                _logger.LogDebug($"Group message, organization does not exist, giving up on waiting: {orgId} - retry {count}");
                throw new Exception($"Organization not found: {orgId}");
            }
        }
    }
}
