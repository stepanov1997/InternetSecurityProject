using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using InternetSecurityProject.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InternetSecurityProject.Services
{
    public static class UserService
    {
        public static async Task<object> LoginUser(UserViewModel userModel, JWTSettings jwtSettings)
        {
            if (userModel.Username.Length < 8 || userModel.Password.Length < 8)
            {
                return "Username or password is shorter than 8 characters.";
            }

            Context context = new Context();
            User found = await context.Users.FirstOrDefaultAsync(u => u.Username == userModel.Username);
            if (found==null) return "User with that username does not exists.";

            found.Token = GenerateAccessToken(found.Id, jwtSettings);
            found.TokenCreatedDate = DateTime.Now;
            found.TokenExpires = 15;
            
            await context.SaveChangesAsync();
            
            return found.MapToModel();
        }

        private static string GenerateAccessToken(long userId, JWTSettings jwtSettings)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new []
                {
                    new Claim(ClaimTypes.Name, Convert.ToString(userId))
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public static async Task<object> RegisterUser(UserViewModel userModel, JWTSettings jwtSettings)
        {
            Console.WriteLine(userModel.Username);
            if (userModel.Username.Length < 8 || userModel.Password.Length < 8)
            {
                return "Username or password is shorter than 8 characters.";
            }

            Context context = new Context();
            bool exists = await context.Users.AnyAsync(u => u.Username == userModel.Username);
            if (exists) return "User with that username already exists.";

            User user = userModel.MapToUser();
            user.Token = GenerateAccessToken(user.Id, jwtSettings);
            user.TokenCreatedDate = DateTime.Now;
            user.TokenExpires = 15;
            
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            
            return userModel;
        }

        public static async Task<bool> IsUserTokenValid(long userId)
        {
            Context context = new Context();
            User foundUser = await context.Users.FirstOrDefaultAsync(user => user.Id == userId);
            if (foundUser?.Token == null || foundUser.TokenCreatedDate==null || foundUser.TokenExpires==null)
            {
                return false;
            }

            if (DateTime.Now < foundUser.TokenCreatedDate) return false;
            
            return (DateTime.Now - foundUser.TokenCreatedDate).Minutes < foundUser.TokenExpires;
        }
    }
}