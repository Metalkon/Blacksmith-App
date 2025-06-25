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
        public int LoginAttempts { get; set; }
        public string LockedCode { get; set; }

        public UserModel()
        {
            Role = "None";
            Validated = false;
            AccountStatus = AccountStatus.Inactive;
            AccountStatusExp = DateTime.UtcNow;
            LoginStatus = LoginStatus.Awaiting;
            LoginCode = string.Empty;
            LoginAttempts = 0;
            LockedCode = string.Empty;
            LoginCodeExp = DateTime.UtcNow.AddMinutes(15);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            GameData = new GameData();
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