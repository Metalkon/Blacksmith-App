﻿using System.ComponentModel.DataAnnotations;

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
        public DateTime LoginCodeExp { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }
        public GameData Data { get; set; }

        public UserModel()
        {
            Role = "User";
            AccountStatus = new AccountStatus();
            LoginStatus = new LoginStatus();
            LoginCode = string.Empty;
            LoginCodeExp = DateTime.UtcNow.AddMinutes(15);
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Data = new GameData();
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
            if (user.AccountStatus.Status == "Suspended" && user.AccountStatus.StatusExp <= DateTime.UtcNow)
            {
                user.AccountStatus.Status = "Active";
                user.LoginStatus.LoginAttempts = 0;
            }
            // LoginStatus
            if (user.LoginStatus.Status == "Awaiting" && user.LoginCodeExp <= DateTime.UtcNow)
            {
                user.LoginStatus.Status = "Active";
            }
            if (user.LoginStatus.LoginAttempts >= 3 && user.LoginCodeExp <= DateTime.UtcNow && user.LoginStatus.Status.Contains("Locked") == false)
            {
                user.LoginStatus.Status = "Locked/Awaiting";
            }
            return user;
        }
    }

    public class AccountStatus
    {
        public bool Validated { get; set; }
        public string Status { get; set; }
        public DateTime? StatusExp { get; set; }
        public AccountStatus()
        {
            Validated = false;
            Status = "Inactive";
        }
    }

    public class LoginStatus
    {
        public string Status { get; set; }
        public string? StatusCode { get; set; }
        public int LoginAttempts { get; set; }
        public LoginStatus()
        {
            Status = "Awaiting";
            LoginAttempts = 1;
        }
    }
}
