using System.ComponentModel.DataAnnotations;

namespace Shared_Classes.Models
{
    public class UserConfirmDTO
    {
        [Required]
        [StringLength(36, MinimumLength = 36)]
        public string Code { get; set; }
        [Required]
        public UserDTO User { get; set; }

        public UserConfirmDTO()
        {
            User = new UserDTO();
        }
    }
}
