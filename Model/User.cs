using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Minutes = System.Int64;

namespace InternetSecurityProject.Model
{
    [Table("user")]
    public class User
    {
        [Key] public long Id { get; set; }
        [Required] public string Username { get; set; }
        [Required] public string Password { get; set; }
        public string Token { get; set; }
        public DateTime TokenCreatedDate { get; set; } 
        public Minutes TokenExpires { get; set; } = 15;
        
        public UserViewModel MapToModel() => new()
        {
            Username = Username,
            Password = Password,
            Token = Token
        };
    }

    public class UserViewModel
    {
        [Required(ErrorMessage = "Please provide username.")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public string Token { get; set; }

        public User MapToUser() => new()
        {
            Username = Username,
            Password = Password,
            Token = Token
        };
    }
}