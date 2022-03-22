using System.ComponentModel.DataAnnotations;

namespace Prinubes.Common.Datamodels
{
    public class AuthenticateModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
