using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Truprogram.Services
{
    public class EmailService
    {
        public async Task Send(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта", "popa0101@yandex.ru"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(TextFormat.Html)
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