using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InternetSecurityProject.Model;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InternetSecurityProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet]
        public Task<User[]> Get()
        {
            Context context = new Context();
            return context.Users.ToArrayAsync();
        }

        [Route("{id}")]
        [HttpGet]
        public IEnumerable<int> GetById(int id) {
            Random random = new Random();
            return Enumerable.Range(0, id).Select(index => random.Next(0, 10));
        }

    }
}