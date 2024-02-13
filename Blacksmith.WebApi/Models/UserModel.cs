using System.ComponentModel.DataAnnotations;

namespace Blacksmith.WebApi.Models
{
    /*
    Account Status:
    - "Active", Normal user status
    - "Inactive", Abnormal user status
    - "Suspended", User cannot login for x amount of time
    - "Banned", User is restricted from login, refresh token cannot be used

    Login Status:
    - "Active", Normal login status
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
        public List<DateTime> LoginHistory { get; set; }

        public UserModel()
        {
            Role = "User";
            AccountStatus = new AccountStatus();
            LoginStatus = new LoginStatus();
            LoginCode = string.Empty;
            LoginCodeExp = new List<DateTime>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            LoginHistory = new List<DateTime>();
        }

        // Update the user after fetching it from the database
        public async Task<UserModel> UpdateUser(UserModel user)
        {
            user = await UpdateStatus(user);
            // more code here

            return user;
        }

        public async Task<UserModel> UpdateStatus(UserModel user)
        {
            if (user.AccountStatus.Status == "Suspended" && user.AccountStatus.Time <= DateTime.UtcNow)
            {
                user.AccountStatus.Status = "Active";
            }
            if (user.LoginStatus.Status == "Active"
                && user.LoginCodeExp.OrderByDescending(x => x).Count(x => x >= DateTime.UtcNow.AddDays(-30)) >= 3)
            {
                user.LoginStatus.Status = "Locked";
                user.LoginStatus.History.Add(DateTime.UtcNow.AddDays(1));
            }
            if (user.LoginStatus.Status == "Awaiting" && user.LoginStatus.Time <= DateTime.UtcNow)
            {
                user.LoginStatus.Status = "Active";
            }
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
        public List<DateTime> History { get; set; }
        public LoginStatus()
        {
            Status = "Awaiting";
            History = new List<DateTime>();
        }
    }
}
