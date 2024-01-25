using System.ComponentModel.DataAnnotations;

namespace Blacksmith.WebApi.Models
{
    public class UserModel
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(32)]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public string AccountStatus { get; set; }
        [Required]
        public string LoginStatus { get; set; }
        [Required]
        public string LoginCode { get; set; }
        [Required]
        public List<DateTime> LoginCodeExp { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }

        public UserModel()
        {
            Role = "None";
            AccountStatus = "Unconfirmed";
            LoginStatus = string.Empty;
            LoginCode = string.Empty;
            LoginCodeExp = new List<DateTime> { DateTime.UtcNow, DateTime.UtcNow.AddDays(400) };
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
