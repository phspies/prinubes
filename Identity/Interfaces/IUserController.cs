using Microsoft.AspNetCore.Mvc;
using Prinubes.Common.DatabaseModels;
using Prinubes.Common.Datamodels;

namespace Prinubes.Identity.Controllers
{
    public interface IUserController
    {
        Task<IActionResult> AuthenticateAsync([FromBody] AuthenticateModel model);
        Task<ActionResult> Delete(Guid id);
        Task<ActionResult> GetByEmailAddressAsync(string emailAddress);
        Task<ActionResult> GetByIDAsync(Guid id);
        Task<IActionResult> init();
        Task<IActionResult> ListUsersAsync();
        Task<IActionResult> RegisterAsync([FromBody] UserCRUDDataModel user);
        Task<IActionResult> UpdateAsync(Guid id, UserCRUDDataModel user);
    }
}