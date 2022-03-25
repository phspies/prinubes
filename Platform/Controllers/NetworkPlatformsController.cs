using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Datamodels;
using Prinubes.Common.Helpers;
using Prinubes.Platforms.Datamodels;
using Prinubes.Platforms.Helpers;
using System.Net;
using System.Text.Json;

namespace Prinubes.Platforms.Controllers
{
    [Authorize]
    [Route("platform/{organizationId}/networkplatforms")]
    [ApiController]
    public class NetworkPlatformController : ControllerBase
    {
        private ILogger<NetworkPlatformController> logger;
        private IMapper mapper;
        private IServiceProvider serviceProvider;

        public NetworkPlatformController(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
            logger = serviceProvider.GetRequiredService<ILogger<NetworkPlatformController>>();
            mapper = serviceProvider.GetRequiredService<IMapper>();
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Test networkplatform credentials in organization")]
        [HttpPost("testCredentials")]
        public async Task<IActionResult> TestCredentials(Guid organizationId, NetworkPlatformCRUDDataModel networkplatform)
        {

            logger.LogInformation($"Test networkplatform API: {JsonSerializer.Serialize(networkplatform)}");
            using (var db = new NetworkPlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    if (!await db.CredentialsIdExistsAsync(networkplatform.CredentialID))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Credential {networkplatform.CredentialID} not found"));

                    }
                    return Ok(await db.TestConnectionAsync(networkplatform));
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error creating networkplatform API: {JsonSerializer.Serialize(networkplatform)}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Create identity networkplatform in organization")]
        [HttpPost("")]
        public async Task<IActionResult> Create(Guid organizationId, NetworkPlatformCRUDDataModel networkplatform)
        {

            logger.LogInformation($"Create networkplatform API: {JsonSerializer.Serialize(networkplatform)}");
            using (var db = new NetworkPlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    if (!await db.CredentialsIdExistsAsync(networkplatform.CredentialID))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Credential ID {networkplatform.CredentialID} not found"));
                    }
                    else
                    {
                        return Ok(mapper.Map<NetworkPlatformDisplayDataModel>(await db.CreateAsync(networkplatform)));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error creating networkplatform API: {JsonSerializer.Serialize(networkplatform)}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("List identity networkplatform in organization")]
        [HttpGet("")]
        public async Task<IActionResult> List(Guid organizationId)
        {
            using (var db = new NetworkPlatformDOA(organizationId, serviceProvider))
            {
                logger.LogInformation("Retrieving all networkplatforms API");
                try
                {
                    return Ok(mapper.Map<List<NetworkPlatformDatabaseModel>, List<NetworkPlatformDisplayDataModel>>(await db.GetListAsync()));

                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error retrieving networkplatforms API");
                    return BadRequest("Internal Server Error");
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Get identity networkplatform by ID in organization")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetByID(Guid organizationId, Guid id)
        {
            using (var db = new NetworkPlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    logger.LogInformation($"Retrieving networkplatform API: {id}");
                    var networkplatform = await db.GetByIDAsync(id);
                    if (networkplatform == null)
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"NetworkPlatform {id} not found"));
                    }
                    return Ok(mapper.Map<NetworkPlatformDisplayDataModel>(networkplatform));
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error retrieving networkplatform API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }

            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Update networkplatform for organization")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid organizationId, Guid id, [FromBody] NetworkPlatformCRUDDataModel networkplatform)
        {

            logger.LogInformation($"Update networkplatform API: {id}:{JsonSerializer.Serialize(networkplatform)}");
            using (var db = new NetworkPlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    if (!await db.NetworkPlatformIdExistsAsync(id))
                    {

                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"NetworkPlatform {id} dot not exist"));
                    }
                    else if (!await db.CredentialsIdExistsAsync(networkplatform.CredentialID))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Credential ID {networkplatform.CredentialID} dot not exist"));
                    }
                    else
                    {
                        return Ok(mapper.Map<NetworkPlatformDisplayDataModel>(await db.UpdateAsync(id, networkplatform)));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error updating networkplatform API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("From identity networkplatform from organization")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid organizationId, Guid id)
        {
            using (var db = new NetworkPlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    logger.LogInformation($"Delete networkplatform API: {id}");
                    if (!await db.NetworkPlatformIdExistsAsync(id))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"NetworkPlatform {id} dot not exist"));
                    }
                    else
                    {
                        await db.DeleteAsync(id);
                        return Ok();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error deleting networkplatform API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }

    }
}



