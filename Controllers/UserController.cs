using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using InternetSecurityProject.Model;
using InternetSecurityProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InternetSecurityProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EmailSettings _emailSettings;
        private readonly JWTSettings _jwtSettings;
        private readonly ILogger<UserController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(IConfiguration configuration,
            IOptions<JWTSettings> jwtSettings,
            IOptions<EmailSettings> emailSettings,
            ILogger<UserController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _emailSettings = emailSettings.Value;
            _jwtSettings = jwtSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            Context context = new Context();
            return Ok(await context.Users.ToArrayAsync());
        }

        [HttpGet]
        [Route("access_denied")]
        public object AccessDenied()
        {
            return Unauthorized(new { Message = "Access is denied, please login." });
        }

        [HttpGet]
        [Route("login")]
        public object LoginRequired()
        {
            return Unauthorized(new { Message = "Access is denied, please login." });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserViewModel userModel)
        {
            var response = await UserService.RegisterUser(userModel, _jwtSettings);
            return response switch
            {
                string errorMesage => StatusCode(StatusCodes.Status409Conflict, new { message = errorMesage }),
                User user when EmailService.SendCertificate(user, _emailSettings, _httpContextAccessor) => Ok(
                    user.MapToModel()),
                User user => StatusCode(StatusCodes.Status409Conflict,
                    new { message = "Sending mail is not successfully." }),
                _ => Conflict()
            };
        }

        [AllowAnonymous]
        [HttpPost("login_part_one")]
        public async Task<IActionResult> LoginUserPart1([FromForm] UserViewModel userModel)
        {
            if (!userModel.Certificate.ValidateCertificate(userModel.Username))
            {
                return StatusCode(StatusCodes.Status409Conflict, new {message = "Client certificate is not valid"});
            }
            
            /*X509Certificate2 cert2 = await Request.HttpContext.Connection.GetClientCertificateAsync();
            Console.WriteLine(cert2);*/
            var response = await UserService.LoginUserPart1(userModel, _jwtSettings, _emailSettings);

            return response switch
            {
                string errorMesage => StatusCode(StatusCodes.Status409Conflict, new { message = errorMesage }),
                UserViewModel userViewModel => Ok(userViewModel),
                _ => Conflict()
            };
        }

        [AllowAnonymous]
        [HttpPost("login_part_two")]
        public async Task<IActionResult> LoginUserPart2([FromBody] UserViewModel userModel)
        {
            var response = await UserService.LoginUserPart2(userModel, _jwtSettings, _emailSettings);

            return response switch
            {
                string errorMesage => StatusCode(StatusCodes.Status409Conflict, new { message = errorMesage }),
                UserViewModel userViewModel => Ok(userViewModel),
                _ => Conflict()
            };
        }
    }
}