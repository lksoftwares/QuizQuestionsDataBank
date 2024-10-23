using System.Net.Mail;
using System.Net;

namespace Quiz_DataBank.Classes
{
    public class SendEmail
    {


        public void SendConfirmationEmail(string userEmail, string userName, string [] files)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("haseenrajput012@gmail.com", "ngmp xpqr jcas hjrd"),
                    EnableSsl = true
                };

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress("haseenrajput012@gmail.com"),
                    Subject = "Welcome to QuizDatabnk!",
                    Body = $"Dear {userName}, Thanks for register",

                    IsBodyHtml = false

                };

                mailMessage.To.Add(userEmail);

                System.Net.Mail.Attachment _Attach;
                foreach(var file in files)
                {
                    _Attach=   new System.Net.Mail.Attachment(file);
                    mailMessage.Attachments.Add(_Attach);
                }
                // new System.Net.Mail.Attachment(file);

              
//   mailMessage.Attachments.Add(new Attachment(attachmentStream, attachmentName));

smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" email error : {ex.Message}");
            }
        }
    }
}
