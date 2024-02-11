using System.ComponentModel.DataAnnotations;

namespace Blacksmith.WebApi.Models
{
    /*
    Account Status:
    - "Active", Normal user status
    - "Inactive", Abnormal user status
    - "Suspended", User cannot login for x amount of time
    - "Banned", User is restricted from login

    Login Status:
    - "Awaiting", User cannot attempt login/regiser for x minutes, and separate registration checks (null/existing) with this
    - "Locked", User is restricted from login for x time
    */

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
        public AccountStatus AccountStatus { get; set; }
        [Required]
        public LoginStatus LoginStatus { get; set; }
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
            Role = "User";
            AccountStatus = new AccountStatus();
            LoginStatus = new LoginStatus();
            LoginCode = string.Empty;
            LoginCodeExp = new List<DateTime> { DateTime.UtcNow, DateTime.UtcNow.AddMinutes(15) };
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Update the status after fetching the user from the database
        public async Task<UserModel> UpdateStatus(UserModel user)
        {

            return user;
        }
    }

    public class AccountStatus
    {
        public bool Validated { get; set; }
        public string Status { get; set; }
        public int? Value { get; set; }
        public DateTime? Time { get; set; }
        public AccountStatus()
        {
            Validated = false;
            Status = "Inactive";
        }
    }

    public class LoginStatus
    {
        public string Status { get; set; }
        public int? Value { get; set; }
        public DateTime? Time { get; set; }
        public LoginStatus()
        {
            Status = "Awaiting";
        }
    }
}
