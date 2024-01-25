using System.ComponentModel.DataAnnotations;

namespace Shared_Classes.Models
{
    public class UserDTO
    {
        public string Username { get; set; }
        [EmailAddress]
        public string Email { get; set; }
    }
}
