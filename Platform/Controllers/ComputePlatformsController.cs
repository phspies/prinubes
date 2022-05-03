using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
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
    [Route("platform/{organizationId}/computeplatforms")]
    [ApiController]
    public class ComputePlatformController : ControllerBase
    {
        private ILogger<ComputePlatformController> logger;
        private IMapper mapper;
        private IServiceProvider serviceProvider;

        public ComputePlatformController(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
            logger = serviceProvider.GetRequiredService<ILogger<ComputePlatformController>>();
            mapper = serviceProvider.GetRequiredService<IMapper>();

        }
        [PrinubesAuthorize]
        [PrinubesDescription("Test computeplatform credentials in organization")]
        [HttpPost("testCredentials")]
        public async Task<IActionResult> TestCredentials(Guid organizationId, ComputePlatformCRUDDataModel computeplatform)
        {

            logger.LogInformation($"Test computeplatform API: {JsonSerializer.Serialize(computeplatform)}");
            using (var db = new ComputePlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    if (!await db.CredentialsIdExistsAsync(computeplatform.CredentialID))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Credential {computeplatform.CredentialID} not found"));

                    }
                    return Ok(await db.TestConnectionAsync(computeplatform));
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error creating computeplatform API: {JsonSerializer.Serialize(computeplatform)}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Create identity computeplatform in organization")]
        [HttpPost("")]
        public async Task<IActionResult> Create(Guid organizationId, ComputePlatformCRUDDataModel computeplatform)
        {

            logger.LogInformation($"Create computeplatform API: {JsonSerializer.Serialize(computeplatform)}");
            using (var db = new ComputePlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    return Ok(mapper.Map<ComputePlatformDisplayDataModel>(await db.CreateAsync(computeplatform)));
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error creating computeplatform API: {JsonSerializer.Serialize(computeplatform)}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("List identity computeplatform in organization")]
        [HttpGet("")]
        public async Task<IActionResult> List(Guid organizationId)
        {
            using (var db = new ComputePlatformDOA(organizationId, serviceProvider))
            {
                logger.LogInformation("Retrieving all computeplatforms API");
                try
                {
                    return Ok(mapper.Map<List<ComputePlatformDatabaseModel>, List<ComputePlatformDisplayDataModel>>(await db.GetListAsync()));

                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error retrieving computeplatforms API");
                    return BadRequest("Internal Server Error");
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Get identity computeplatform by ID in organization")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetByID(Guid organizationId, Guid id)
        {
            using (var db = new ComputePlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    logger.LogInformation($"Retrieving computeplatform API: {id}");
                    var computeplatform = await db.GetByIDAsync(id);
                    if (computeplatform == null)
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"ComputePlatform {id} not found"));
                    }
                    return Ok(mapper.Map<ComputePlatformDisplayDataModel>(computeplatform));
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error retrieving computeplatform API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }

            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Update computeplatform for organization")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid organizationId, Guid id, [FromBody] ComputePlatformCRUDDataModel computeplatform)
        {

            logger.LogInformation($"Update computeplatform API: {id}:{JsonSerializer.Serialize(computeplatform)}");
            using (var db = new ComputePlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    if (!await db.ComputePlatformIdExistsAsync(id))
                    {

                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"ComputePlatform {id} dot not exist"));
                    }
                    else
                    {
                        return Ok(mapper.Map<ComputePlatformDisplayDataModel>(await db.UpdateAsync(id, computeplatform)));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error updating computeplatform API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("From identity computeplatform from organization")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid organizationId, Guid id)
        {
            using (var db = new ComputePlatformDOA(organizationId, serviceProvider))
            {
                try
                {
                    logger.LogInformation($"Delete computeplatform API: {id}");
                    if (!await db.ComputePlatformIdExistsAsync(id))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"ComputePlatform {id} dot not exist"));
                    }
                    else
                    {
                        await db.DeleteAsync(id);
                        return Ok();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error deleting computeplatform API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }

    }
}



