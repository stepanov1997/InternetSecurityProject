using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using InternetSecurityProject.Model;
using InternetSecurityProject.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InternetSecurityProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly JWTSettings jwtSettings;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IConfiguration configuration, IOptions<JWTSettings> jwtSettings,
            ILogger<ChatController> logger)
        {
            this.configuration = configuration;
            this.jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActiveUsers() =>
            Ok((await new Context().Users.ToListAsync()).Where(x => x.IsUserActive()).Select(x => new
            {
                username = x.Username
            }));

        [Authorize]
        [HttpGet]
        [Route("inactive")]
        public async Task<IActionResult> GetInactiveUsers() =>
            Ok((await new Context().Users.ToListAsync()).Where(x => !x.IsUserActive()).Select(x => new
            {
                username = x.Username
            }));

        
        [Authorize]
        [HttpPost]
        [Route("messages")]
        public async Task<IActionResult> GetMessages([FromBody] string receiver)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var success = long.TryParse(identity?.Claims.ToList()[0].Value, out var result);
            if(!success || !await UserService.IsUserTokenValid(result))
                return Unauthorized(new {Message = "Token is expired or doesn't exist."});
            
            
            Context context = new Context();
            var messages = (await context.Messages
                .Join(context.Users,
                    m => m.Sender.Id,
                    u => u.Id,
                    (message, user) => new
                    {
                        Message = message, User = user
                    })
                .Join(context.Users,
                    m => m.Message.Receiver.Id,
                    u => u.Id,
                    (message, user) => new
                    {
                        Message = message.Message, Sender = message.User, Receiver = user
                    })
                .Where(x => (x.Sender.Id == result || x.Sender.Username == receiver) && 
                                            (x.Receiver.Username == receiver || x.Receiver.Id==result))
                .ToListAsync())
                .Select(x=>x.Message.MapToModel());
            return Ok(messages);
        }
        
        
        [Authorize]
        [HttpPost]
        [Route("messages/send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageModel messageModel)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var success = long.TryParse(identity?.Claims.ToList()[0].Value, out var result);
            if(!success || !await UserService.IsUserTokenValid(result))
                return Unauthorized(new {Message = "Token is expired or doesn't exist."});
            
            Context context = new Context();
            User receiver = await context.Users.FirstOrDefaultAsync(x => x.Username == messageModel.Receiver.ToString());
            User sender = await context.Users.FirstOrDefaultAsync(x => x.Id == result);

            if (sender == null || receiver == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new {Message = "Sender or receiver not found in request"});
            }
            Message message = new Message
            {
                Content = messageModel.Content,
                DateTimeStamp = DateTime.Now,
                Sender = sender,
                Receiver = receiver
            };

            await context.Messages.AddAsync(message);
            await context.SaveChangesAsync();
            
            return Ok(message);
        }

        [Route("{id}")]
        [HttpGet]
        public IEnumerable<int> GetById(int id)
        {
            Random random = new Random();
            return Enumerable.Range(0, id).Select(index => random.Next(0, 10));
        }


        private async Task<bool> IsTokenValid()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var success = long.TryParse(identity?.Claims.ToList()[0].Value, out var result);
            return success && await UserService.IsUserTokenValid(result);
        }
    }
}