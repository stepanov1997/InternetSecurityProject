using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        private readonly IConfiguration configuration;
        private readonly JWTSettings jwtSettings;
        private readonly ILogger<UserController> _logger;

        public UserController(IConfiguration configuration, IOptions<JWTSettings> jwtSettings, ILogger<UserController> logger)
        {
            this.configuration = configuration;
            this.jwtSettings = jwtSettings.Value;
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
            return Unauthorized(new { Message = "Access is denied, please login."});
        }
        
        [HttpGet]
        [Route("login")]
        public object LoginRequired()
        {
            return Unauthorized(new { Message = "Access is denied, please login."});
        }
        
        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserViewModel userModel)
        {
            var response = await UserService.RegisterUser(userModel, jwtSettings);
            return response switch
            {
                string errorMesage => StatusCode(StatusCodes.Status409Conflict, new {message = errorMesage}),
                UserViewModel userViewModel => Ok(userViewModel),
                _ => Conflict()
            };
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] UserViewModel userModel)
        {
            string cert = Request.Headers["X-ARR-ClientCert"];;
            Console.WriteLine(cert);
            X509Certificate2 cert2 = await Request.HttpContext.Connection.GetClientCertificateAsync();
            Console.WriteLine(cert2);
            var response = await UserService.LoginUser(userModel, jwtSettings);
            
            return response switch
            {
                string errorMesage => StatusCode(StatusCodes.Status409Conflict, new {message = errorMesage}),
                UserViewModel userViewModel => Ok(userViewModel),
                _ => Conflict()
            };
        }

        [Route("{id}")]
        [HttpGet]
        public IEnumerable<int> GetById(int id) {
            Random random = new Random();
            return Enumerable.Range(0, id).Select(index => random.Next(0, 10));
        }


        private async Task<bool> IsTokenValid()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (!identity.Claims.Any()) return false;
            var success = long.TryParse(identity?.Claims.ToList()[0].Value, out var result);
            return success && await UserService.IsUserTokenValid(result);
        }
    }
}