using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternetSecurityProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;

        public ChatController(ILogger<ChatController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<int> Get()
        {
            return Enumerable.Range(1, 5).Select(index => index*2).ToArray();
        }

        [Route("{id}")]
        [HttpGet]
        public IEnumerable<int> GetById(int id) {
            Random random = new Random();
            return Enumerable.Range(0, id).Select(index => random.Next(0, 10));
        }

    }
}
