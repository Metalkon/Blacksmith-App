using System.ComponentModel.DataAnnotations;

namespace Shared_Classes.Models
{
    public class UserDTO
    {
        [Required]
        [StringLength(64)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(128)]
        public string Email { get; set; }
    }
}
