using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace Truprogram.Services
{
    public class EmailService : ISendInfo
    {
        public async Task Send(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта", "popa0101@yandex.ru"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using var client = new SmtpClient();

            await client.ConnectAsync("smtp.yandex.ru", 465, true);
            await client.AuthenticateAsync("popa0101@yandex.ru", "szzxoxkljyucdtuf");
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }
    }
}
