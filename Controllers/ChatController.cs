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
using Org.BouncyCastle.Ocsp;

namespace InternetSecurityProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly JWTSettings _jwtSettings;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IConfiguration configuration, IOptions<JWTSettings> jwtSettings,
            ILogger<ChatController> logger)
        {
            _configuration = configuration;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActiveUsers()
        {
            bool isTokenValid = await UserService.IsTokenValid(HttpContext.User.Identity as ClaimsIdentity);
            if (!isTokenValid)
            {
                return Ok(new {status = 401, message = "Token is expired or doesn't exist."});
            }

            return Ok(new
            {
                status = 200,
                data = (await new Context().Users.ToListAsync()).Where(x => x.IsUserActive()).Select(x =>
                    new {username = x.Username})
            });
        }

        [Authorize]
        [HttpGet]
        [Route("inactive")]
        public async Task<IActionResult> GetInactiveUsers()
        {
            bool isTokenValid = await UserService.IsTokenValid(HttpContext.User.Identity as ClaimsIdentity);
            if (!isTokenValid)
            {
                return Ok(new {status = 401, message = "Token is expired or doesn't exist."});
            }

            return Ok(new
            {
                status = 200,
                data = (await new Context().Users.ToListAsync()).Where(x => !x.IsUserActive()).Select(x =>
                    new {username = x.Username})
            });
        }

        [Authorize]
        [HttpPost]
        [Route("messages")]
        public async Task<IActionResult> GetMessages([FromBody] string receiver)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var success = long.TryParse(identity?.Claims.ToList()[0].Value, out var result);
            if (!success || !await UserService.IsUserTokenValid(result))
                return Ok(new {status = 401, message = "Token is expired or doesn't exist."});


            Context context = new Context();

            User senderUser = await context.Users.FirstOrDefaultAsync(elem => elem.Id == result);
            User receiverUser = await context.Users.FirstOrDefaultAsync(elem => elem.Username == receiver);


            var messages = await context.Messages
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
                            (x.Receiver.Username == receiver || x.Receiver.Id == result))
                .ToListAsync();

            bool activeAttacksExists = await context
                .Attacks
                .Where(attack => (attack.Attacker.Id == senderUser.Id && attack.Attacked.Id == receiverUser.Id) ||
                                 (attack.Attacked.Id == senderUser.Id && attack.Attacker.Id == receiverUser.Id))
                .AnyAsync(attack => attack.Type != AttackType.None && !attack.IsRelogged);

            var response = messages.Select(elem => new MessageModel
            {
                Sender = elem.Sender.Username,
                Receiver = elem.Receiver.Username,
                Content = elem.Message.Content
            });

            return activeAttacksExists
                ? Ok(new {status = 503, data = response})
                : Ok(new {status = 200, data = response});
        }


        [Authorize]
        [HttpPost]
        [Route("messages/send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageModel messageModel)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var success = long.TryParse(identity?.Claims.ToList()[0].Value, out var result);
            if (!success || !await UserService.IsUserTokenValid(result))
                return Ok(new {status = 401, message = "Token is expired or doesn't exist."});

            Context context = new Context();
            User receiver =
                await context.Users.FirstOrDefaultAsync(x => x.Username == messageModel.Receiver.ToString());
            User sender = await context.Users.FirstOrDefaultAsync(x => x.Id == result);

            bool activeAttacksExists = await context
                .Attacks
                .Where(attack => (attack.Attacker.Id == sender.Id && attack.Attacked.Id == receiver.Id) ||
                                 (attack.Attacked.Id == sender.Id && attack.Attacker.Id == receiver.Id))
                .AnyAsync(attack => attack.Type != AttackType.None && !attack.IsRelogged);

            if (activeAttacksExists)
            {
                return Ok(new {status = 503, message = "Preventing attack..."});
            }

            if (sender == null || receiver == null)
            {
                return Ok(new {status = 400, message = "Sender or receiver not found in request"});
            }

            string messageContent = messageModel.Content;

            AttackType attackType = AttackType.None;

            if (DefenceService.IsXssAttack(messageContent))
            {
                messageContent = "Potential XSS attack...";
                attackType = AttackType.Xss;
            }
            else if (DefenceService.IsSqlInjectionAttack(messageContent))
            {
                messageContent = "Potential SQL Injection attack...";
                attackType = AttackType.SqlInjection;
            }
            else if (await DefenceService.IsDdosAttack(receiver))
            {
                messageContent = "Potential DOS attack...";
                attackType = AttackType.Ddos;
            }

            if (attackType != AttackType.None)
            {
                Attack attack = new Attack
                {
                    Attacker = sender,
                    Attacked = receiver,
                    DateTime = DateTime.Now,
                    IsRelogged = false,
                    Type = attackType
                };

                await context.Attacks.AddAsync(attack);
            }

            Message message = new Message
            {
                Content = messageContent,
                DateTimeStamp = DateTime.Now,
                Sender = sender,
                Receiver = receiver,
            };
            await context.Messages.AddAsync(message);
            await context.SaveChangesAsync();

            return Ok(new {status = 200, data = message});
        }
    }
}