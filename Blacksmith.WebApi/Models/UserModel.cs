using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blacksmith.WebApi.Models
{
    public class UserModel
    {
        // Basic Properties
        [Key]
        public int Id { get; set; }
        [StringLength(32)]
        public string Username { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Role { get; set; }
        public bool Validated { get; set; } // New
        public AccountStatus AccountStatus { get; set; } // New (Note: Add New Db Table For Banlists)
        public DateTime AccountStatusExp { get; set; } // New
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int GameDataId { get; set; }
        [ForeignKey("GameDataId")]
        public GameData GameData { get; set; }

        // Login/Register Properties
        public string LoginCode { get; set; }
        public DateTime LoginCodeExp { get; set; }
        public LoginStatus LoginStatus { get; set; }
        public string LoginStatusCode { get; set; }
        public DateTime LoginStatusCodeExp { get; set; }
        public int LoginAttempts { get; set; }

        public UserModel()
        {
            Role = "User";
            Validated = false;
            AccountStatus = AccountStatus.Inactive;
            AccountStatusExp = DateTime.UtcNow;
            LoginStatus = LoginStatus.Awaiting;
            LoginStatusCode = string.Empty;
            LoginAttempts = 1;
            LoginCode = string.Empty;
            LoginCodeExp = DateTime.UtcNow.AddMinutes(15);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            GameData = new GameData();
        }

        // Update the user after fetching it from the database
        public async Task<UserModel> UpdateUser(UserModel user)
        {
            user = await UpdateStatus(user);
            return user;
        }

        public async Task<UserModel> UpdateStatus(UserModel user)
        {
            // AccountStatus
            if (user.AccountStatus == AccountStatus.Suspended && user.AccountStatusExp <= DateTime.UtcNow)
            {
                user.AccountStatus = AccountStatus.Active;
                user.LoginAttempts = 0;
            }
            // LoginStatus
            if (user.LoginStatus == LoginStatus.Awaiting && user.LoginCodeExp <= DateTime.UtcNow)
            {
                user.LoginStatus = LoginStatus.Awaiting;
            }
            if (user.LoginAttempts >= 3 && user.LoginCodeExp <= DateTime.UtcNow && user.LoginStatus != LoginStatus.Locked)
            {
                user.LoginStatus = LoginStatus.LockedAwaiting;
            }
            return user;
        }
    }

    public enum AccountStatus
    {
        Active,
        Inactive,
        Suspended,
        Banned
    }

    public enum LoginStatus
    {
        Active,
        Awaiting,
        Locked,
        LockedAwaiting
    }
}