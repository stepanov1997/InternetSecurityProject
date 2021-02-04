using System;
using InternetSecurityProject.Model;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace InternetSecurityProject.Services
{
    public class EmailService
    {
        public static bool SendCertificate(User user, EmailSettings emailSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            string subject = "InternetSecurityProject - sertifikat";
            string host = httpContextAccessor.HttpContext.Request.Host.Value;
            string body = @$"
                <h3>Pozdrav, {user.Username}!</h3>
                <br>
                <p>Uspješno ste se registrovali.</p>
                <p>U nastavku možete preuzeti vaš klijentski sertifikat koji ćete koristiti za svako prijavljivanje na sajtu.</p>
                <br>
                <br>
                Link: <a href='https://{host}/certificate/{user.Username}'>Preuzmi sertifikat</a>
                <br>
                <h3>Admin</h3>
                ";
            try
            {
                return SendMail(user, subject, body, emailSettings);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SendToken(User user, EmailSettings emailSettings)
        {
            string subject = "InternetSecurityProject - token";
            string body = @$"
                <h1>Pozdrav, {user.Username}!</h1><br>
                <br>
                <p>Sertifikat je uspješno prošao provjeru.
                <p>U nastavku dobijate token za prijavu:<br>
                Token: {user.Token}
                <br>
                <h1>Admin</h1>
                ";
            try
            {
                return SendMail(user, subject, body, emailSettings);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool SendMail(User user, string subject, string body, EmailSettings emailSettings)
        {
            try
            {
                var fromAddress = new MailAddress(emailSettings.AdminEmail, "InternetSecurityProject");
                var toAddress = new MailAddress(user.Email, user.Username);
                string fromPassword = emailSettings.AdminPassword;

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                    {Subject = subject, Body = body, IsBodyHtml = true})
                {
                    smtp.Send(message);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}