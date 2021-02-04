using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using InternetSecurityProject.Model;
using InternetSecurityProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace InternetSecurityProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CertificateController : Controller
    {
        private readonly CertsSettings _certsSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CertificateController(IOptions<CertsSettings> certsSettings, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _certsSettings = certsSettings.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCertificates()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var success = long.TryParse(identity?.Claims.ToList()[0].Value, out var result);
            if (!success || !await UserService.IsUserTokenValid(result))
                return Ok(new {status = 401, message = "Token is expired or doesn't exist."});

            Context context = new Context();

            var user = context.Users.FirstOrDefaultAsync(u => u.Id == result);
            if (user == null)
            {
                return Ok(new {status = 401, message = "Token is expired or doesn't exist."});
            }

            string host = _httpContextAccessor.HttpContext.Request.Host.Value;
            string dateTimeFormat = "dd.MM.yyyy. HH:mm:ss";
            List<dynamic> certificateModelList = new List<dynamic>();
            foreach (var certificate in await CertificateService.GetAllCertificates())
            {
                X509Certificate2 cert = new X509Certificate2(certificate.Path, _certsSettings.ClientPassword);
                if (certificate.User.Id != result)
                {
                    certificateModelList.Add(new
                    {
                        Username = certificate.User.Username,
                        ValidFrom = cert.NotBefore.ToString(dateTimeFormat),
                        ValidTo = cert.NotAfter.ToString(dateTimeFormat)
                    });
                }
                else
                {
                    certificateModelList.Add(new
                    {
                        Username = certificate.User.Username,
                        ValidFrom = cert.NotBefore.ToString(dateTimeFormat),
                        ValidTo = cert.NotAfter.ToString(dateTimeFormat),
                        Download = $"https://{host}/certificate/{certificate.User.Username}"
                    });
                }
            }

            return Ok(new {status = 200, data = certificateModelList});
        }

        [HttpGet]
        [Route("{username}")]
        public async Task<IActionResult> GetCertificateByUsername(string username)
        {
            var cert = await CertificateService.GenerateCertificateForUser(username, _certsSettings);
            return new FileStreamResult(new FileStream(cert.Path, FileMode.Open), "application/octet-stream")
            {
                FileDownloadName = Path.GetFileName(cert.Path)
            };
        }
    }
}