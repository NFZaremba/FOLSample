using FutureOfLatinos.Models.Requests;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FutureOfLatinos.Web.helpers
{
    public class ContactUsEmail
    {
        public static async Task<Response> SendEmail(ContactUsRequest contactusRequest)
        {
            var client = new SendGridClient(contactusRequest.SendGridKey); //sendgrid key
            //locates and reads html template
            var path = contactusRequest.ContactUsEmailTemplate;
            string contactUsTemplate = File.ReadAllText(path);
            // finds keywords and replace them with user email 
            string contactUsWithEmail = contactUsTemplate.Replace("{email}", contactusRequest.FromEmail); //ContactUsRequest.Email
            string contactUsWithName = contactUsWithEmail.Replace("{name}", contactusRequest.FromName); //ContactUsRequest.FromName
            string contactUsWithQuestion = contactUsWithName.Replace("{question}", contactusRequest.Question); //ContactUsRequest.Question
            //uses items from email request to populate email
            var from = new EmailAddress(contactusRequest.FromEmail, contactusRequest.FromName);
            var subject = contactusRequest.Subject;
            var to = new EmailAddress(contactusRequest.ToEmail);
            var plainTextContent = contactusRequest.Question;
            var htmlContent = contactUsWithQuestion;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            //sends message
            return await client.SendEmailAsync(msg);
        }
    }
}