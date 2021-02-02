using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.X509;

namespace InternetSecurityProject
{
    static class Helper{
        public static bool ValidateCertificate(this IFormFile file, string userModelUsername)
        {
            try
            {
                var ms = new MemoryStream();
                file.CopyTo(ms);
                var fileBytes = ms.ToArray();
                var clientCert = new X509Certificate2(fileBytes, "sigurnost");

                if (!Regex.IsMatch(clientCert.Subject, $"CN={userModelUsername}"))
                {
                    return false;
                }

                var caCert =
                    new X509Certificate2(@"C:\Users\stepa\source\repos\InternetSecurityProject\ca_cert\client.p12",
                        "sigurnost");
                var ca = new X509CertificateParser().ReadCertificate(caCert.GetRawCertData());
                var client = new X509CertificateParser().ReadCertificate(clientCert.GetRawCertData());

                try
                {
                    client.Verify(ca.GetPublicKey());
                    Console.WriteLine("Good");
                }
                catch
                {
                    Console.WriteLine("Bad");
                    return false;
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
