using InternetSecurityProject.Model;

namespace InternetSecurityProject.Services
{
    public class EmailService
    {
        public static bool SendCertificate(Certificate certificate)
        {
            var certificateViewModel = certificate.MapToModel();
            return true;
        }
        
        public static bool SendToken()
        {
            return true;
        }
        
        
        public static bool SendMail(User user, string title, string content)
        {
            return true;
        }
    }
}