using System;
using System.IO;

namespace EmailTemplateExample
{
    public class ContactUsTemplate
    {
        static void Main(string[] args)
        {
            string contactUsEmailTemplate = File.ReadAllText("FutureOfLations.Web/Content/ContactUsEmailTemplate.html");
            string contactUsWithEmail = contactUsEmailTemplate.Replace("{email}", "erictilton18@gmail.com"); //ContactUsAddRequest.Email
            string contactUsWithName = contactUsEmailTemplate.Replace("{name}", "Eric"); //ContactUsAddRequest.Name
            string contactUsWithQuestion = contactUsEmailTemplate.Replace("{question}", "whats up?"); //ContactUsAddRequest.Question
            Console.Read();
        }
    }
}
