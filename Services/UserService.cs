using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using InternetSecurityProject.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;

namespace InternetSecurityProject.Services
{
    public static class UserService
    {
        public static async Task<object> LoginUserPart1(UserViewModel userModel, JWTSettings jwtSettings, EmailSettings emailSettings)
        {
            if (userModel.Username.Length < 8 || userModel.Password.Length < 8)
            {
                return "Username or password is shorter than 8 characters.";
            }

            Context context = new Context();
            User found = (await context.Users.ToListAsync()).FirstOrDefault(u => u.Username == userModel.Username && BCrypt.Net.BCrypt.Verify(userModel.Password, u.Password));
            if (found==null) return "User with that username does not exists.";

            found.Token = GenerateAccessToken(found.Id, jwtSettings);
            found.TokenCreatedDate = DateTime.Now;
            found.TokenExpires = 15;
            await context.SaveChangesAsync();

            Tfa tfa = await context.Tfas.FirstOrDefaultAsync(tfa => tfa.User.Username == found.Username);
            if (tfa == null)
            {
                tfa = new Tfa {
                    User = found,
                    FirstFactorTime = DateTime.Now,
                    IsCertificateOk = true,
                    IsPasswordOk = true,
                    IsTokenOk = false
                };
                await context.Tfas.AddAsync(tfa);
            }
            else
            {
                tfa.IsCertificateOk = true;
                tfa.IsPasswordOk = true;
                tfa.IsTokenOk = false;
            }

            await context.SaveChangesAsync();
            
            if (!EmailService.SendToken(found, emailSettings))
            {
                return "User doesn't have valid email adress.";
            }
            return found.MapToModel();
        }

        public static async Task<object> LoginUserPart2(UserViewModel userModel, JWTSettings jwtSettings, EmailSettings emailSettings)
        {
            Context context = new Context();

            User found = await context.Users.FirstOrDefaultAsync(u => u.Token == userModel.Token);
            if (found==null) return "Invalid token.";

            await context
                .Attacks
                .Where(attack => attack.Attacked.Token == userModel.Token)
                .ForEachAsync(attack =>
                {
                    attack.IsRelogged = true;
                });
            
            Tfa tfa = await context.Tfas.FirstOrDefaultAsync(tfa => tfa.User.Username == found.Username);
            if (tfa == null || !(tfa.IsPasswordOk && tfa.IsCertificateOk))
            {
                return "Log with username and password first";
            }
            context.Tfas.Remove(tfa);
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
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Token = GenerateAccessToken(user.Id, jwtSettings);
            user.TokenCreatedDate = DateTime.Now;
            user.TokenExpires = 15;
            
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            
            return user;
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
        
        
        public static async Task<bool> IsTokenValid(ClaimsIdentity identity)
        {
            if (!identity.Claims.Any()) return false;
            var success = long.TryParse(identity?.Claims.ToList()[0].Value, out var result);
            return success && await UserService.IsUserTokenValid(result);
        }
    }
}