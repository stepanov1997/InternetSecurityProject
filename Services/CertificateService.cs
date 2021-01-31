using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using InternetSecurityProject.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Hpc.Scheduler.Store;
using X509KeyUsageFlags = System.Security.Cryptography.X509Certificates.X509KeyUsageFlags;

namespace InternetSecurityProject.Services
{
    public class CertificateService
    {
        public static async Task<List<Certificate>> GetAllCertificates()
        {
            Context context = new Context();
            return await context.Certificates.ToListAsync();
        }

        public static async Task<Certificate> GenerateCertificateForUser(string username, CertsSettings certsSettings)
        {
            Context context = new Context();
            User user = await context.Users.FirstOrDefaultAsync(elem => elem.Username == username);
            
            // Check if certificate exists 
            Certificate existingCertificate = await context.Certificates
                .FirstOrDefaultAsync(cert => cert.User.Id == user.Id);

            if (existingCertificate != null)
            {
                return existingCertificate;
            }

            // Do if not exists
            X509Certificate2 userCertificate = CertCreateNew(user.Username,
                certsSettings.CA_cert, certsSettings.CA_password, certsSettings.ClientPassword);

            byte[] p12 = userCertificate.Export(X509ContentType.Pfx, certsSettings.ClientPassword);
            string path = $@"{certsSettings.ClientFolder}\{user.Username}.p12";
            await File.WriteAllBytesAsync(path, p12);

            Certificate certificate = new Certificate {Path = path, User = user};

            await context.Certificates.AddAsync(certificate);
            await context.SaveChangesAsync();

            return certificate;
        }

        public static X509Certificate2 CertCreateNew(string subjectName, string caPath, string caPassword,
            string clientPass)
        {
            X509Certificate2 caCert = new X509Certificate2(caPath, caPassword, X509KeyStorageFlags.MachineKeySet |
                                                                               X509KeyStorageFlags.PersistKeySet |
                                                                               X509KeyStorageFlags.Exportable);
            RSA rsa = RSA.Create(2048);

            CertificateRequest req = new CertificateRequest(
                $@"CN={subjectName}",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            req.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(false, false, 0, false));

            req.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation,
                    false));

            req.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection
                    {
                        new Oid("1.3.6.1.5.5.7.3.2")
                    },
                    true));

            req.CertificateExtensions.Add(
                new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

            X509Certificate2 cert = req.Create(
                caCert,
                DateTimeOffset.UtcNow,
                caCert.NotAfter,
                new byte[] {1, 2, 3, 4});

            return cert;
        }
    }
}