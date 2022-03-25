using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Datamodels;
using Prinubes.Common.Models;
using Prinubes.Identity.Datamodels;
using Prinubes.Identity.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Prinubes.Identity.Controllers
{
    [Authorize]
    [Route("identity/users")]
    [ApiController]
    public class UserController : ControllerBase, IUserController
    {
        private ILogger<UserController> userControllerlogger;
        private IMapper mapper;
        private ServiceSettings appSettings;
        private IServiceProvider serviceProvider;

        public UserController(IServiceProvider _serviceProvider)
        {
            serviceProvider = _serviceProvider;
            userControllerlogger = serviceProvider.GetRequiredService<ILogger<UserController>>();
            appSettings = serviceProvider.GetRequiredService<ServiceSettings>();
            mapper = serviceProvider.GetRequiredService<IMapper>();
        }
        [PrinubesDescription("List users")]
        [HttpGet]
        public async Task<IActionResult> ListUsersAsync()
        {
            using (var db = new UserDOA(serviceProvider))
            {
                userControllerlogger.LogInformation("Retrieving all users API");
                try
                {
                    return Ok(mapper.Map<List<UserDatabaseModel>, List<UserSimpleDataModel>>(await db.GetListAsync()));
                }
                catch (Exception ex)
                {
                    userControllerlogger.LogDebug(ex, $"Error retrieving users API");
                    return BadRequest("Internal Server Error");
                }
            }
        }
        [PrinubesDescription("Get user by ID")]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetByIDAsync(System.Guid id)
        {
            using (var db = new UserDOA(serviceProvider))
            {
                try
                {
                    userControllerlogger.LogInformation($"Retrieving user API: {id}");
                    if (!await db.UserIdExistsAsync(id))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"User {id} not found"));
                    }
                    else
                    {
                        return Ok(mapper.Map<UserSimpleDataModel>(await db.GetByIDAsync(id)));
                    }
                }
                catch (Exception ex)
                {
                    userControllerlogger.LogDebug(ex, $"Error retrieving user API: {id}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }

            }
        }
        [PrinubesDescription("Get user by Email Address")]
        [HttpGet("byEmail")]
        public async Task<ActionResult> GetByEmailAddressAsync(string emailAddress)
        {
            using (var db = new UserDOA(serviceProvider))
            {
                try
                {
                    userControllerlogger.LogInformation($"Retrieving user API: {emailAddress}");
                    if (!await db.UserEmailAddressExistsAsync(emailAddress))
                    {
                        return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"User {emailAddress} not found"));
                    }
                    else
                    {
                        return Ok(mapper.Map<UserSimpleDataModel>(await db.GetByEmailAddressAsync(emailAddress)));
                    }
                }
                catch (Exception ex)
                {
                    userControllerlogger.LogDebug(ex, $"Error retrieving user API: {emailAddress}");
                    return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, $"Internal server error"));
                }
            }
        }
        [PrinubesDescription("Update user")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(System.Guid id, UserCRUDDataModel user)
        {
            try
            {
                userControllerlogger.LogInformation($"Update user API: {id}:{JsonSerializer.Serialize(user)}");
                using (var db = new UserDOA(serviceProvider))
                {
                    try
                    {
                        if (!await db.UserIdExistsAsync(id))
                        {
                            return Conflict(new ErrorReturnType(HttpStatusCode.Conflict, $"User {user.EmailAddress} already exist"));
                        }
                        else
                        {
                            return Ok(mapper.Map<UserSimpleDataModel>(await db.UpdateAsync(id, user)));
                        }
                    }
                    catch (Exception ex)
                    {
                        userControllerlogger.LogDebug(ex, $"Error updating user API: {id}");
                        return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, "Internal Server Error"));
                    }
                }
            }
            catch (AppException ex)
            {
                userControllerlogger.LogDebug(ex, "Internal error");
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
        [PrinubesDescription("Delete user")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(System.Guid id)
        {
            try
            {
                using (var db = new UserDOA(serviceProvider))
                {
                    userControllerlogger.LogInformation($"Delete user API: {id}");
                    if (id != System.Guid.Empty)
                    {
                        if (!await db.UserIdExistsAsync(id))
                        {
                            return base.NotFound();
                        }
                        try
                        {
                            return base.Ok(db.DeleteAsync(id));
                        }
                        catch (Exception ex)
                        {
                            userControllerlogger.LogDebug(ex, $"Error deleting user API: {id}");
                            return base.BadRequest(new { error = "Internal Server Error" });
                        }
                    }
                    return BadRequest(new ErrorReturnType(HttpStatusCode.NotFound, $"User with id {id} not found"));
                }
            }
            catch (AppException ex)
            {
                userControllerlogger.LogDebug(ex, "Internal error");
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
        [AllowAnonymous]
        [HttpPost("init")]
        public async Task<IActionResult> init()
        {
            try
            {
                UserCRUDDataModel newUser = new UserCRUDDataModel() { EmailAddress = "fspies0@hotmail.com", Firstname = "Phillip", Lastname = "Spies", Password = "c0mp2q" };
                Common.DatabaseModels.OrganizationDatabaseModel NewOrg;
                GroupDatabaseModel NewGroup;
                UserDatabaseModel NewUser;
                using (var userDB = new UserDOA(serviceProvider))
                {
                    NewUser = await userDB.CreateAsync(newUser);
                }

                using (var groupBD = new OrganizationDOA(serviceProvider))
                {
                    NewOrg = await groupBD.CreateAsync(new OrganizationCRUDDataModel() { Organization = "laborg01" });
                }
                using (var groupBD = new GroupDOA(NewOrg.Id, serviceProvider))
                {
                    NewGroup = await groupBD.CreateAsync(new GroupCRUDDataModel() { Group = "labgroup01" });
                    NewGroup = await groupBD.attachUserAsync(NewGroup.Id, NewUser.Id);
                }
                return Ok(mapper.Map<UserSimpleDataModel>(NewUser));
            }


            catch (AppException ex)
            {
                userControllerlogger.LogDebug(ex, "Internal error");
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] AuthenticateModel model)
        {
            try
            {
                using (var db = new UserDOA(serviceProvider))
                {
                    UserDatabaseModel user = await db.AuthenticateAsync(model.Username, model.Password);

                    if (user == null)
                    {
                        return Unauthorized(new ErrorReturnType(HttpStatusCode.Unauthorized, $"Username or password is incorrect!"));
                    }

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(appSettings.JWT_SECRET);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, user.Id.ToString())
                        }),
                        Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(appSettings.JWT_EXPIRE_TIME)),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);

                    // return basic user info and authentication token
                    return Ok(new
                    {
                        Id = user.Id,
                        EmailAddress = user.EmailAddress,
                        FirstName = user.Firstname,
                        LastName = user.Lastname,
                        Token = tokenString
                    });
                }
            }
            catch (AppException ex)
            {
                userControllerlogger.LogDebug(ex, "Internal error");
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserCRUDDataModel user)
        {
            try
            {
                using (var db = new UserDOA(serviceProvider))
                {
                    if (await db.UserEmailAddressExistsAsync(user.EmailAddress))
                    {
                        return Conflict(new ErrorReturnType(HttpStatusCode.Conflict, $"User {user.EmailAddress} already exist"));
                    }
                    else
                    {
                        return Ok(mapper.Map<UserSimpleDataModel>(await db.CreateAsync(user)));

                    }
                }
            }
            catch (AppException ex)
            {
                userControllerlogger.LogDebug(ex, "Internal error");
                return BadRequest(new ErrorReturnType(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}


