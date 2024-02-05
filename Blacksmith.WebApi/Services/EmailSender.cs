using System.Net;
using System.Net.Mail;

namespace Blacksmith.WebApi.Services
{
    public class EmailSender
    {
        private IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string message)
        {
            var mail = _config["EmailSettings:Email"];
            var pw = _config["EmailSettings:Password"];

            var client = new SmtpClient("smtp-mail.outlook.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw)
            };

            var mailMessage = new MailMessage(from: mail, to: email, subject, message);

            try
            {
                await client.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                // handle the exception here, e.g. log the error
                return false;
            }
        }
    }
}
