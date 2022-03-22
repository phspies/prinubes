using System.Text.Json;
using AutoMapper;
using Prinubes.Identity.Datamodels;
using Prinubes.Identity.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Kafka.Producer;
using Prinubes.Common.Datamodels;
using Microsoft.AspNetCore.Identity;
using Prinubes.Common.Models;

namespace Prinubes.Identity.Controllers
{
    [Authorize]
    [Route("identity/{organizationId}/groups")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private ILogger<CredentialController> controllerlogger;
        private IMapper mapper;
        private IServiceProvider serviceProvider;

        public GroupController(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
            controllerlogger = serviceProvider.GetRequiredService<ILogger<CredentialController>>();
            mapper = serviceProvider.GetRequiredService<IMapper>();
        }
        [PrinubesAuthorize]
        [PrinubesDescription("Create identity group in organization")]
        [HttpPost("")]
        public async Task<IActionResult> Create(Guid organizationId, GroupCRUDDataModel group)
        {

            controllerlogger.LogInformation($"Create group API: {JsonSerializer.Serialize(group)}");
            using (var db = new GroupDOA(organizationId, serviceProvider))
            {
                try
                {
                    if (! await db.GroupNameExistsAsync(group.Group))
                    {
                        return Ok(mapper.Map<GroupDisplayDataModel>(await db.CreateAsync(group)));
                    }
                    else
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.Conflict, $"Group {group.Group} already exists in Organization"));
                    }
                }
                catch (Exception ex)
                {
                    controllerlogger.LogDebug(ex, $"Error creating group API: {JsonSerializer.Serialize(group)}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesAuthorize]
        [PrinubesDescription("List identity group in organization")]
        [HttpGet("")]
        public async Task<IActionResult> List(Guid organizationId)
        {
            using (var db = new GroupDOA(organizationId, serviceProvider))
            {
                controllerlogger.LogInformation("Retrieving all groups API");
                try
                {
                    return Ok(mapper.Map<List<GroupDatabaseModel>, List<GroupDisplayUsersDataModel>>(await db.GetListAsync()));
                }
                catch (Exception ex)
                {
                    controllerlogger.LogDebug(ex, $"Error retrieving groups API");
                    return BadRequest("Internal Server Error");
                }
            }
        }
        [PrinubesDescription("Get identity group by ID in organization")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetByID(Guid organizationId, Guid id)
        {
            using (var db = new GroupDOA(organizationId, serviceProvider))
            {
                try
                {
                    controllerlogger.LogInformation($"Retrieving group API: {id}");
                    if (await db.GroupExistsAsync(id))
                    {
                        return Ok(mapper.Map<GroupDisplayDataModel>(await db.GetByIDAsync(id)));
                    }
                    else
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Group {id} not found"));
                    }
                }
                catch (Exception ex)
                {
                    controllerlogger.LogDebug(ex, $"Error retrieving group API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }

            }
        }
        [PrinubesDescription("List users in identity group for organization")]
        [HttpGet("{groupId}/userList")]
        public async Task<IActionResult> listUsers(Guid organizationId, Guid groupId)
        {
            using (var db = new GroupDOA(organizationId, serviceProvider))
            {
                controllerlogger.LogInformation("Retrieving all users for group API");
                try
                {
                    if (await db.GroupExistsAsync(groupId))
                    {
                        return Ok(mapper.Map<GroupDatabaseModel, GroupDisplayUsersDataModel>(await db.GetByIDAsync(groupId)));
                    }
                    else
                    {
                        return NotFound(new ErrorReturnType(HttpStatusCode.NotFound, $"Group {groupId} does not exist"));
                    }
                }
                catch (Exception ex)
                {
                    controllerlogger.LogDebug(ex, $"Error retrieving groups API");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal Server Error"));
                }
            }
        }
        [PrinubesDescription("Attach user to identity group for organization")]
        [HttpPut("{groupID}/attachUser/{userId}")]
        public async Task<IActionResult> attachUser(Guid organizationId, Guid groupID, Guid userId)
        {
            try
            {
                controllerlogger.LogInformation($"Attaching user to group API: {groupID} with {userId}");
                using (var db = new GroupDOA(organizationId, serviceProvider))
                {
                    try
                    {
                        if (!await db.GroupExistsAsync(groupID))
                        {
                            return NotFound(new ErrorReturnType(HttpStatusCode.NotFound, $"Group {groupID} does not exist"));
                        }
                        else if (await db.GroupUserExistsAsync(groupID, userId))
                        {
                            return Conflict(new ErrorReturnType(HttpStatusCode.Conflict, $"User {userId} already associated with group {groupID}"));
                        }
                        else
                        {
                            return Ok(mapper.Map<GroupDatabaseModel, GroupDisplayUsersDataModel>(await db.attachUserAsync(groupID, userId)));
                        }
                    }
                    catch (Exception ex)
                    {
                        controllerlogger.LogDebug(ex, $"Error updating group API: {groupID}");
                        return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));

                    }
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
        [PrinubesDescription("Detach user from identity group for organization")]
        [HttpPut("{groupID}/detachUser/{userId}")]
        public async Task<IActionResult> detachUser(Guid organizationId, Guid groupID, Guid userId)
        {
            try
            {
                controllerlogger.LogInformation($"Attaching user to group API: {groupID} with {userId}");
                using (var db = new GroupDOA(organizationId, serviceProvider))
                {
                    try
                    {
                        if (!await db.GroupExistsAsync(groupID))
                        {
                            return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Group {groupID} does not exist"));
                        }
                        else if (!await db.GroupUserExistsAsync(groupID, userId))
                        {
                            return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"User {userId} is not associated with group {groupID}"));
                        }
                        else
                        {
                            return Ok(mapper.Map<GroupDatabaseModel, GroupDisplayUsersDataModel>(await db.detachUserAsync(groupID, userId)));
                        }
                    }
                    catch (Exception ex)
                    {
                        controllerlogger.LogDebug(ex, $"Error updating group API: {groupID}");
                        return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));

                    }
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
        [PrinubesDescription("Update identity group for organization")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid organizationId, Guid id, [FromBody] GroupCRUDDataModel group)
        {
            try
            {
                controllerlogger.LogInformation($"Update group API: {id}:{JsonSerializer.Serialize(group)}");
                using (var db = new GroupDOA(organizationId, serviceProvider))
                {
                    try
                    {
                        if (!await db.GroupExistsAsync(id))
                        {
                            return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Group {id} does not exist"));
                        }
                        else
                        {
                            return Ok(mapper.Map<GroupDisplayDataModel>(await db.UpdateAsync(id, group)));
                        }
                    }
                    catch (Exception ex)
                    {
                        controllerlogger.LogDebug(ex, $"Error updating group API: {id}");
                        return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                    }
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
        [PrinubesDescription("From identity group from organization")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid organizationId, Guid id)
        {
            try
            {
                using (var db = new GroupDOA(organizationId, serviceProvider))
                {
                    controllerlogger.LogInformation($"Delete group API: {id}");
                    if (id != Guid.Empty)
                    {
                        if (!await db.GroupExistsAsync(id))
                        {
                            return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"Group {id} does not exist"));
                        }
                        try
                        {
                            return Ok(db.DeleteAsync(id));

                        }
                        catch (Exception ex)
                        {
                            controllerlogger.LogDebug(ex, $"Error deleting group API: {id}");
                            return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                        }
                    }
                    return BadRequest(new { error = $"Group with id {id} not found" });
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}


