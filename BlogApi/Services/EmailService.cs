using BlogApi.Models;
using BlogApi.Settings;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BlogApi.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfig _emailConfig;
        private readonly SmtpClient _smtpClient;

        public EmailService(IOptionsMonitor<EmailConfig> emailConfig, SmtpClient smtpClient)
        {
            _emailConfig = emailConfig.CurrentValue;
            _smtpClient = smtpClient;
        }

        public async Task Send(EmailModel emailModel)
        {
            MailAddress from = new(_emailConfig.UserName, _emailConfig.DisplayName);
            MailMessage message = new(_emailConfig.UserName, emailModel.To, emailModel.Subject, emailModel.Body);
            message.From = from;
            message.IsBodyHtml = true;

            await _smtpClient.SendMailAsync(message);
        }

        public string GetTemplate(string type)
        {
            string FileName = type + ".html";
            string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", FileName);
            StreamReader str = new(FilePath);
            string templateBody = str.ReadToEnd();
            str.Close();

            return templateBody;
        }
    }
}
