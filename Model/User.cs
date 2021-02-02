using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Minutes = System.Int64;

namespace InternetSecurityProject.Model
{
    [Table("user")]
    public class User
    {
        [Key] public long Id { get; set; }
        [Required] public string Username { get; set; }
        [Required] public string Password { get; set; }
        [Required] public string Email { get; set; }
        public string Token { get; set; }
        public DateTime TokenCreatedDate { get; set; } 
        public Minutes TokenExpires { get; set; } = 15;
        
        public UserViewModel MapToModel() => new()
        {
            Username = Username,
            Password = Password,
            Token = Token
        };

        public bool IsUserActive() => Token != null && TokenCreatedDate != null && TokenExpires!=null &&
                                      (DateTime.Now - TokenCreatedDate).Minutes < TokenExpires;
    }

    public class UserViewModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }
        
        public IFormFile Certificate { get; set; } 
        public string Token { get; set; }

        public User MapToUser() => new()
        {
            Username = Username,
            Password = Password,
            Email = Email,
            Token = Token
        };
    }
}