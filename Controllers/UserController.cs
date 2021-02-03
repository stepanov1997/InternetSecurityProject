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
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserViewModel userModel)
        {
            var response = await UserService.RegisterUser(userModel, _jwtSettings);
            return response switch
            {
                string errorMesage => Ok(new {status = 409, message = errorMesage}),
                User user when EmailService.SendCertificate(user, _emailSettings, _httpContextAccessor) => Ok(
                    user.MapToModel()),
                User user => Ok(new {status = 409, message = "Sending mail is not successfully."}),
                _ => Ok(new {status = 409, message = "Error"})
            };
        }

        [AllowAnonymous]
        [HttpPost("login_part_one")]
        public async Task<IActionResult> LoginUserPart1([FromForm] UserViewModel userModel)
        {
            if (!userModel.Certificate.ValidateCertificate(userModel.Username))
            {
                return Ok(new {status = 409, message = "Client certificate is not valid"});
            }

            var response = await UserService.LoginUserPart1(userModel, _jwtSettings, _emailSettings);

            return response switch
            {
                string errorMesage => Ok(new {status = 409, message = errorMesage}),
                UserViewModel userViewModel => Ok(new {status=200, data=userViewModel}),
                _ => Ok(new {status = 409, message = "Error"})
            };
        }

        [AllowAnonymous]
        [HttpPost("login_part_two")]
        public async Task<IActionResult> LoginUserPart2([FromBody] UserViewModel userModel)
        {
            var response = await UserService.LoginUserPart2(userModel, _jwtSettings, _emailSettings);

            return response switch
            {
                string errorMesage => Ok(new {status = 409, message = errorMesage}),
                UserViewModel userViewModel => Ok(new {status=200, data=userViewModel}),
                _ => Ok(new {status = 409, message = "Error"})
            };
        }
    }
}