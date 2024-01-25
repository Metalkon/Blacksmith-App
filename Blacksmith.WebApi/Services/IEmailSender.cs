﻿namespace Blacksmith.WebApi.Services
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(string email, string subject, string message);
    }

}
