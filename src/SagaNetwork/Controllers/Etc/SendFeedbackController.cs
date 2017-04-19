using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.IO;

namespace SagaNetwork.Controllers
{
    public class SendFeedbackController : Controller
    {
        [HttpPost]
        public JToken Post ()
        {
            if (Request.Body.CanSeek)
                Request.Body.Position = 0;

            var fileStream = new StreamReader(Request.Body).ReadToEnd();

            var smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("palecolonlord@gmail.com", "sagapower!2012");
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = true;

            var mail = new MailMessage();
            mail.From = new MailAddress("palecolonlord@gmail.com", "Palecolon Lord");
            mail.To.Add(new MailAddress("support@warholdthegame.com"));
            mail.Subject = "Feedback Report";
            mail.Body = "The feedback report is in the attachment.";
            mail.Attachments.Add(new Attachment(fileStream, "feedback.txt"));

            smtpClient.Send(mail);

            return JStatus.Ok;
        }
    }
}
