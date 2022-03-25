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
    [Route("platform/{organizationId}/loadbalancerplatforms")]
    [ApiController]
    public class LoadBalancerPlatformController : ControllerBase
    {
        private ILogger<LoadBalancerPlatformController> logger;
        private IMapper mapper;
        private IServiceProvider serviceProvider;

        public LoadBalancerPlatformController(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
            logger = serviceProvider.GetRequiredService<ILogger<LoadBalancerPlatformController>>();
            mapper = serviceProvider.GetRequiredService<IMapper>();
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Test loadbalancerplatform credentials in organization")]
        [HttpPost("testCredentials")]
        public async Task<IActionResult> TestCredentials(Guid organizationId, LoadBalancerPlatformCRUDDataModel loadbalancerplatform)
        {

            logger.LogInformation($"Test loadbalancerplatform API: {JsonSerializer.Serialize(loadbalancerplatform)}");
            using (var db = new LoadBalancerPlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    if (!await db.CredentialsIdExistsAsync(loadbalancerplatform.CredentialID))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Credential {loadbalancerplatform.CredentialID} not found"));

                    }
                    return Ok(await db.TestConnectionAsync(loadbalancerplatform));
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error creating loadbalancerplatform API: {JsonSerializer.Serialize(loadbalancerplatform)}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Create identity loadbalancerplatform in organization")]
        [HttpPost("")]
        public async Task<IActionResult> Create(Guid organizationId, LoadBalancerPlatformCRUDDataModel loadbalancerplatform)
        {

            logger.LogInformation($"Create loadbalancerplatform API: {JsonSerializer.Serialize(loadbalancerplatform)}");
            using (var db = new LoadBalancerPlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    if (!await db.CredentialsIdExistsAsync(loadbalancerplatform.CredentialID))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Credential ID {loadbalancerplatform.CredentialID} not found"));
                    }
                    else
                    {
                        return Ok(mapper.Map<LoadBalancerPlatformDisplayDataModel>(await db.CreateAsync(loadbalancerplatform)));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error creating loadbalancerplatform API: {JsonSerializer.Serialize(loadbalancerplatform)}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("List identity loadbalancerplatform in organization")]
        [HttpGet("")]
        public async Task<IActionResult> List(Guid organizationId)
        {
            using (var db = new LoadBalancerPlatformDOA(organizationId, serviceProvider))
            {
                logger.LogInformation("Retrieving all loadbalancerplatforms API");
                try
                {
                    return Ok(mapper.Map<List<LoadBalancerPlatformDatabaseModel>, List<LoadBalancerPlatformDisplayDataModel>>(await db.GetListAsync()));

                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error retrieving loadbalancerplatforms API");
                    return BadRequest("Internal Server Error");
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Get identity loadbalancerplatform by ID in organization")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetByID(Guid organizationId, Guid id)
        {
            using (var db = new LoadBalancerPlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    logger.LogInformation($"Retrieving loadbalancerplatform API: {id}");
                    var loadbalancerplatform = await db.GetByIDAsync(id);
                    if (loadbalancerplatform == null)
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"LoadBalancerPlatform {id} not found"));
                    }
                    return Ok(mapper.Map<LoadBalancerPlatformDisplayDataModel>(loadbalancerplatform));
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error retrieving loadbalancerplatform API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }

            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Update loadbalancerplatform for organization")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid organizationId, Guid id, [FromBody] LoadBalancerPlatformCRUDDataModel loadbalancerplatform)
        {

            logger.LogInformation($"Update loadbalancerplatform API: {id}:{JsonSerializer.Serialize(loadbalancerplatform)}");
            using (var db = new LoadBalancerPlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    if (!await db.LoadBalancerPlatformIdExistsAsync(id))
                    {

                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"LoadBalancerPlatform {id} dot not exist"));
                    }
                    else if (!await db.CredentialsIdExistsAsync(loadbalancerplatform.CredentialID))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Credential ID {loadbalancerplatform.CredentialID} dot not exist"));
                    }
                    else
                    {
                        return Ok(mapper.Map<LoadBalancerPlatformDisplayDataModel>(await db.UpdateAsync(id, loadbalancerplatform)));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error updating loadbalancerplatform API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("From identity loadbalancerplatform from organization")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid organizationId, Guid id)
        {
            using (var db = new LoadBalancerPlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    logger.LogInformation($"Delete loadbalancerplatform API: {id}");
                    if (!await db.LoadBalancerPlatformIdExistsAsync(id))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"LoadBalancerPlatform {id} dot not exist"));
                    }
                    else
                    {
                        await db.DeleteAsync(id);
                        return Ok();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error deleting loadbalancerplatform API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }

    }
}



