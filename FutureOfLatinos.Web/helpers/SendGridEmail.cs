using FutureOfLatinos.Models.Requests;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IO;
using System.Threading.Tasks;

namespace FutureOfLatinos.Web.helpers
{
    public class SendGridEmail
    {
        public static async Task<Response> SendEmail(EmailRequest emailRequest)
        {
            var client = new SendGridClient(emailRequest.SendGridKey);
            //locates and reads html template
            var path = emailRequest.EmailTemplate;
            string emailTemplate = File.ReadAllText(path);
            // finds keywords and replace them with user email and unique link
            string templateWithEmail = emailTemplate.Replace("{email}", emailRequest.ToEmail); //RegistrationAddRequest.Email
            string templateWithLink = templateWithEmail.Replace("{confirmationLink}", emailRequest.ConfirmationLink);
            //uses items from email request to populate email
            var from = new EmailAddress(emailRequest.FromEmail, emailRequest.FromName);
            var subject = emailRequest.Subject;
            var to = new EmailAddress(emailRequest.ToEmail);
            var plainTextContent = emailRequest.TextBody;
            var htmlContent = templateWithLink; //templateWithLink;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            //sends mesage
            return await client.SendEmailAsync(msg);
        }
    }
}