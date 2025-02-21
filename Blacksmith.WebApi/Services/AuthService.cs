using Blacksmith.WebApi.Data;
using Blacksmith.WebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared_Classes.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Sockets;
using System;

namespace Blacksmith.WebApi.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly EmailSender _emailSender;
        private readonly TokenService _tokenService;

        public AuthService(ApplicationDbContext db, EmailSender emailSender, TokenService tokenService)
        {
            _db = db;
            _emailSender = emailSender;
            _tokenService = tokenService;
        }

        public async Task<(int statusCode, string message)> ConfirmUser(UserModel user, UserDTO loginRequest)
        {
            if (user == null)
                return (404, "User Doesn't Exist");

            if (user.Username != loginRequest.Username || user.Email != loginRequest.Email)
                return (400, "Invalid Username or Email");

            if (user.Validated == false)
                return (400, "Access Denied: Account not validated");

            return (200, string.Empty);
        }

        public async Task<(int statusCode, string message)> CheckUserStatus(UserModel user)
        {
            if (user.AccountStatus == AccountStatus.Banned)
                return (400, "Access Denied: Account has been Banned");

            if (user.AccountStatus == AccountStatus.Suspended)
                return (400, "Access Denied: Account has been Suspended");

            if (user.LoginStatus == LoginStatus.Locked)
                return (400, "Access Denied: Login with this email address has been locked due to too many failed attempts. To unlock the account, you will need to confirm ownership by using the 'Unlock' URL sent in the most recently sent email.");

            if (user.LoginStatus == LoginStatus.Awaiting)
                return (400, "Your account is already awaiting confirmation to login");


            return (200, string.Empty);
        }
        public async Task<UserModel> UpdateStatus(UserModel user)
        {
            if (user.AccountStatus == AccountStatus.Suspended && user.AccountStatusExp <= DateTime.UtcNow)
            {
                user.AccountStatus = AccountStatus.Active;
                user.LoginAttempts = 0;
            }

            if (user.LoginStatus == LoginStatus.Awaiting && user.LoginCodeExp <= DateTime.UtcNow)
                user.LoginStatus = LoginStatus.Active;

            if (user.LoginAttempts == 4 && user.LoginStatus != LoginStatus.Locked)
                user.LoginStatus = LoginStatus.LockedAwaiting;

            if (user.LoginAttempts >= 5)
                user.LoginStatus = LoginStatus.Locked;

            return user;
        }

        public async Task<(int statusCode, string message)> LockedEmail(UserModel user)
        {
            if (user.LoginStatus == LoginStatus.LockedAwaiting)
            {
                user.LoginStatus = LoginStatus.Locked;
                user.LockedCode = Guid.NewGuid().ToString();
                return await SendEmailLocked(user);
            }
            return (200, string.Empty);
        }



























        public async Task<(int statusCode, string message)> SendEmailLogin(UserModel user)
        {
            var subject = "Blacksmith App - Login Verification";
            var message = $"Welcome to Blacksmith Web App!\n\n" +
                          $"To complete your login, click the link below (valid for 15 minutes):\n" +
                          $"https://localhost:8001/confirmation?confirmType=Login&username={user.Username}&email={user.Email}&code={user.LoginCode}";
            bool sentEmail = await _emailSender.SendEmailAsync(user.Email, subject, message);
            if (sentEmail == false)
                return (500, "Failed to send the email. Please try again later");

            return (200, $"An Email to complete your login has been sent to {user.Email}");
        }

        public async Task<(int statusCode, string message)> SendEmailLocked(UserModel user)
        {
            var subject = "Blacksmith App - Account Has Been Locked";
            var message = $"Login with this email has been locked due to too many failed attempts, if you wish to unlock it and attempt again then click the link below (no expiry time while valid):\n" +
                          $"https://localhost:8001/confirmation?confirmType=UnlockEmail&username={user.Username}&email={user.Email}&code={user.LockedCode}";
            bool sentEmail = await _emailSender.SendEmailAsync(user.Email, subject, message);
            if (sentEmail == false)
                return (500, "Failed to send the email. Please try again later");

            return (400, $"Access Denied: Login with this email address has been locked due to too many failed attempts. An email to unlock this account has been sent to {user.Email}.");
        }
    }
}