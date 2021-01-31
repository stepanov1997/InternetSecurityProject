using System.IO;
using System.Threading.Tasks;
using InternetSecurityProject.Model;
using InternetSecurityProject.Services;
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

        public CertificateController(IOptions<CertsSettings> certsSettings)
        {
            _certsSettings = certsSettings.Value;
        }

        /*
        // GET
        public async Task<IActionResult> GetAllCertificates()
        {
            return Ok(await CertificateService.GetAllCertificates());
        }*/
        
        [HttpGet]
        [Route("{username}")]
        public async Task<IActionResult> GetCertificateByUsername(string username)
        {
            var cert = await CertificateService.GenerateCertificateForUser(username, _certsSettings);
            return new FileStreamResult(new FileStream(cert.Path,FileMode.Open), "application/octet-stream")
            {
                FileDownloadName = Path.GetFileName(cert.Path)
            };
        }
    }
}