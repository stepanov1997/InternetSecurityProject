using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InternetSecurityProject.Model;
using Microsoft.EntityFrameworkCore;

namespace InternetSecurityProject.Services
{
    public static class DefenceService
    {
        public static bool IsXssAttack(string message)
        {
            return Regex.IsMatch(message, @"(<(img.*?src=.*?|script|a.*?href)>?)|(javascipt:)");
        }
        
        public static bool IsSqlInjectionAttack(string message)
        {
            return Regex.IsMatch(message, @"\s*'\s*(''|[^'])*") ||
                   Regex.IsMatch(message, @"\b\s*(ALTER|CREATE|DELETE|DROP|EXEC(UTE){0,1}\s*|\s*INSERT( +INTO){0,1}\s*|MERGE|SELECT|UPDATE|UNION( +ALL){0,1}\s*)\s*\b\s*");
        }
        
        public static async Task<bool> IsDdosAttack(User user)
        {
            Context context = new Context();
            DateTime refTime = DateTime.Now.AddSeconds(-DDOS_SCAN_TIME_IN_SEC);
            bool isDdosAttack = await context.Messages
                .OrderByDescending(msg => msg.DateTimeStamp)
                .Where(msg => msg.Receiver.Id==user.Id && 
                              msg.DateTimeStamp>refTime)
                .CountAsync() > DDOS_NUMBER_OF_REQUESTS;
            return isDdosAttack;
        }

        private static readonly int DDOS_SCAN_TIME_IN_SEC = 10;
        private static readonly int DDOS_NUMBER_OF_REQUESTS = 100;
    }
}