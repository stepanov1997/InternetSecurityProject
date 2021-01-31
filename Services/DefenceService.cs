using System.Security.Cryptography.X509Certificates;
using InternetSecurityProject.Model;

namespace InternetSecurityProject.Services
{
    public static class DefenceService
    {
        public static bool IsXssAttack(string message)
        {
            return false;
        }
        
        public static bool IsSqlInjectionAttack(string message)
        {
            return false;
        }
        
        public static bool IsDdosAttack(User user, string message)
        {
            return false;
        }
    }
}