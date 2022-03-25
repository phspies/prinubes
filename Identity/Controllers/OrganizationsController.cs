using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Datamodels;
using Prinubes.Common.Kafka.Producer;
using Prinubes.Common.Models;
using Prinubes.Identity.Datamodels;
using Prinubes.Identity.Helpers;
using System.Net;
using System.Text.Json;

namespace Prinubes.Identity.Controllers
{
    [Authorize]
    [Route("identity/organizations")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private PrinubesIdentityDBContext organizationDBContext;
        private ILogger<OrganizationsController> logger;
        private IMessageProducer kafkaProducer;
        private IMapper mapper;
        private readonly ServiceSettings appSettings;

        private IServiceProvider serviceProvider;

        public OrganizationsController(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
            logger = serviceProvider.GetRequiredService<ILogger<OrganizationsController>>();
            mapper = serviceProvider.GetRequiredService<IMapper>();
        }

        [PrinubesDescription("Create organization")]
        [HttpPost("")]
        public async Task<IActionResult> CreateOrganization(OrganizationCRUDDataModel organization)
        {
            try
            {
                logger.LogInformation($"Create organization API: {JsonSerializer.Serialize(organization)}");
                using (var db = new OrganizationDOA(serviceProvider))
                {
                    try
                    {
                        return base.Ok(mapper.Map<Common.DatabaseModels.OrganizationDatabaseModel>(await db.CreateAsync(organization)));
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug(ex, $"Error creating organization API: {JsonSerializer.Serialize(organization)}");
                        return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                    }
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
        [PrinubesAuthorizeAttribute]
        [PrinubesDescription("List all organizations")]
        [HttpGet]
        public async Task<IActionResult> ListOrgaizations()
        {
            using (var db = new OrganizationDOA(serviceProvider))
            {
                logger.LogInformation("Retrieving all organizations API");
                try
                {
                    return base.Ok(mapper.Map<List<Common.DatabaseModels.OrganizationDatabaseModel>, List<OrganizationDisplayDataModel>>(await db.GetListAsync()));
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error retrieving organizations API");
                    return BadRequest("Internal Server Error");
                }
            }
        }
        [PrinubesDescription("Get organization by ID")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetOrganizationByID(System.Guid id)
        {
            using (var db = new OrganizationDOA(serviceProvider))
            {
                try
                {
                    logger.LogInformation($"Retrieving organization API: {id}");
                    var organization = await db.GetByIDAsync(id);
                    if (organization == null)
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Organization {id} not found"));
                    }
                    return Ok(mapper.Map<OrganizationDisplayDataModel>(organization));
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, $"Error retrieving organization API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }

            }
        }
        [PrinubesDescription("Update organization")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganization(System.Guid id, OrganizationCRUDDataModel organization)
        {
            try
            {
                logger.LogInformation($"Update organization API: {id}:{JsonSerializer.Serialize(organization)}");
                using (var db = new OrganizationDOA(serviceProvider))
                {
                    try
                    {
                        if (!await db.OrganizationExistsAsync(id))
                        {
                            return Conflict(new ErrorReturnType(HttpStatusCode.Conflict, $"Organization {id} already exist"));
                        }
                        else
                        {
                            return Ok(await db.UpdateAsync(id, organization));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug(ex, $"Error updating organization API: {id}");
                        return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                    }
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
        [PrinubesDescription("Delete organization")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteOrganization(System.Guid id)
        {
            try
            {
                using (var db = new OrganizationDOA(serviceProvider))
                {
                    logger.LogInformation($"Delete organization API: {id}");
                    if (id != System.Guid.Empty)
                    {
                        if (!await db.OrganizationExistsAsync(id))
                        {
                            return base.NotFound();
                        }
                        try
                        {
                            return base.Ok(db.DeleteAsync(id));

                        }
                        catch (Exception ex)
                        {
                            logger.LogDebug(ex, $"Error deleting organization API: {id}");
                            return base.BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                        }
                    }
                    return BadRequest(new { error = $"Organization with id {id} not found" });
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}


