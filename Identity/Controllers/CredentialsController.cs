using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Datamodels;
using Prinubes.Identity.Datamodels;
using Prinubes.Identity.Helpers;
using System.Net;
using System.Text.Json;

namespace Prinubes.Identity.Controllers
{
    [Authorize]
    [Route("identity/{organizationId}/credentials")]
    [ApiController]
    public class CredentialController : ControllerBase
    {
        private ILogger<CredentialController> controllerlogger;
        private IMapper mapper;
        private IServiceProvider serviceProvider;

        public CredentialController(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
            controllerlogger = serviceProvider.GetRequiredService<ILogger<CredentialController>>();
            mapper = serviceProvider.GetRequiredService<IMapper>();
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Create identity credential in organization")]
        [HttpPost("")]
        public async Task<IActionResult> Create(Guid organizationId, CredentialCRUDDataModel credential)
        {
            try
            {
                controllerlogger.LogInformation($"Create credential API: {JsonSerializer.Serialize(credential)}");
                using (var db = new CredentialDOA(organizationId, serviceProvider))
                {
                    if (credential.Password == null)
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotAcceptable, $"Password field cannot be empty"));
                    }
                    try
                    {
                        return Ok(mapper.Map<CredentialDatabaseModel, CredentialDisplayDataModel>(await db.CreateAsync(credential)));
                    }
                    catch (Exception ex)
                    {
                        controllerlogger.LogDebug(ex, $"Error creating credential API: {JsonSerializer.Serialize(credential)}");
                        return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                    }
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("List identity credential in organization")]
        [HttpGet("")]
        public async Task<IActionResult> List(Guid organizationId)
        {
            using (var db = new CredentialDOA(organizationId, serviceProvider))
            {
                controllerlogger.LogInformation("Retrieving all credentials API");
                try
                {
                    return Ok(mapper.Map<List<CredentialDatabaseModel>, List<CredentialDisplayDataModel>>(await db.GetListAsync()));
                }
                catch (Exception ex)
                {
                    controllerlogger.LogDebug(ex, $"Error retrieving credentials API");
                    return BadRequest("Internal Server Error");
                }
            }
        }
        [PrinubesDescription("Get identity credential by ID in organization")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetByID(Guid organizationId, Guid id)
        {
            using (var db = new CredentialDOA(organizationId, serviceProvider))
            {
                try
                {
                    if (!await db.CredentialExistsAsync(id))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Credential {id} not found"));
                    }
                    else
                    {
                        return Ok(mapper.Map<CredentialDisplayDataModel>(await db.GetByID(id)));
                    }
                }
                catch (Exception ex)
                {
                    controllerlogger.LogDebug(ex, $"Error retrieving credential API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }

            }
        }

        [PrinubesDescription("Update identity credential for organization")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid organizationId, Guid id, [FromBody] CredentialCRUDDataModel credential)
        {
            try
            {
                controllerlogger.LogInformation($"Update credential API: {id}:{JsonSerializer.Serialize(credential)}");
                using (var db = new CredentialDOA(organizationId, serviceProvider))
                {
                    try
                    {
                        if (!await db.CredentialExistsAsync(id))
                        {
                            return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Credential {id} not found"));
                        }
                        else
                        {
                            return Ok(mapper.Map<CredentialDisplayDataModel>(await db.UpdateAsync(id, credential)));
                        }
                    }
                    catch (Exception ex)
                    {
                        controllerlogger.LogDebug(ex, $"Error updating credential API: {id}");
                        return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                    }
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
        [PrinubesDescription("From identity credential from organization")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid organizationId, Guid id)
        {
            try
            {
                controllerlogger.LogInformation($"Delete credential API: {id}");
                using (var db = new CredentialDOA(organizationId, serviceProvider))
                {

                    try
                    {
                        if (!await db.CredentialExistsAsync(id))
                        {
                            return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Credential {id} not found"));
                        }
                        else
                        {
                            await db.DeleteAsync(id);
                            return Ok();
                        }
                    }
                    catch (Exception ex)
                    {
                        controllerlogger.LogDebug(ex, $"Error updating credential API: {id}");
                        return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                    }
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}


