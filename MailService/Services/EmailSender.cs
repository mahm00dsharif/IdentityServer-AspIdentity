using MailKit.Net.Smtp;
using MailService.Configuration;
using MailService.Models;
using MimeKit;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MailService.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message);
    }

    public class EmailSender : IEmailSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;

        public EmailSender(EmailConfiguration emailConfig
            , IRazorViewToStringRenderer razorViewToStringRenderer)
        {
            _emailConfig = emailConfig;
            _razorViewToStringRenderer = razorViewToStringRenderer;
        }

        public async Task SendEmailAsync(Message message)
        {
            var mailMessage = await CreateEmailMessage(message);

            await SendAsync(mailMessage);
        }

        private async Task<MimeMessage> CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            var confirmAccountModel = new ConfirmAccountEmailViewModel(message.Content, message.CompanyName);
            string body = await _razorViewToStringRenderer.RenderViewToStringAsync(
                message.EmailTemplate, confirmAccountModel);


            var bodyBuilder = new BodyBuilder { HtmlBody = body };

            if (message.Attachments != null && message.Attachments.Any())
            {
                byte[] fileBytes;
                foreach (var attachment in message.Attachments)
                {
                    using (var ms = new MemoryStream())
                    {
                        attachment.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }

                    bodyBuilder.Attachments.Add(attachment.FileName, fileBytes, ContentType.Parse(attachment.ContentType));
                }
            }

            emailMessage.Body = bodyBuilder.ToMessageBody();
            return emailMessage;
        }

        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

                    await client.SendAsync(mailMessage);
                }
                catch
                {
                    //log an error message or throw an exception, or both.
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}
