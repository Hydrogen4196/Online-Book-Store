using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DummyProject.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configration;
        private EmailSettings _emailSettings { get; }
        //private readonly EmailSettings _emailSettings;(not the same !!!
        public EmailSender(IConfiguration configration, IOptions<EmailSettings> emailSettings)
        {
            _configration = configration;
            _emailSettings = emailSettings.Value;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)//use to send the method 
        {
            Execute(email, subject, htmlMessage).Wait();//call execute method down bellow and send the email using (email/subject/htmlmessage) and wait (means it runs directily rather waiting
            return Task.FromResult(0);//finish the task
        }
        public async Task Execute (string email, string subject, string message)
        {
            try
            {
                //mail format***************************
                string toEmail = string.IsNullOrEmpty(email) ? _emailSettings.ToEmail : email;
                //if we give email this line will take that if we don't it will take json's(ToEmail
                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.UsernameEmail, "Book Shopping Project")
                };
                //new mail address is usernameemail from emailsettings (appseting.json// "display name" 
                mailMessage.To.Add(toEmail);// this means mail will go to user
                mailMessage.To.Add(_emailSettings.CcEmail);//mail will go to server as copy(carbon copy)
                mailMessage.Subject = "shopping App : " + subject;//this will give the email a title that tells the user what it's about
                mailMessage.Body = message;//this is mail message
                mailMessage.IsBodyHtml = true;//formating and better dezine
                mailMessage.Priority = MailPriority.High;//this ensure mail got attension
                //**********************
                //real code*******************
                using(SmtpClient smtpClient = new SmtpClient(_emailSettings.PrimaryDomain,_emailSettings.PrimaryPort))//set up a connection to email server and port number for email configration
                {
                    smtpClient.Credentials = new NetworkCredential(_emailSettings.UsernameEmail,_emailSettings.UsernamePassword);//create cradential object and passed username/password
                    smtpClient.EnableSsl = true;//secures the communication channel for sending emails
                    smtpClient.Send(mailMessage);//send the email defined in the (mailMessage) object through the SMTP server configured in the smtpClient
                };    
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }
    }
}
