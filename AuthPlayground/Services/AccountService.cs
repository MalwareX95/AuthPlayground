using AuthPlayground.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthPlayground.Services
{


    public class AccountService
    {
        public static User[] Users = new[]
        {
            new User { Name = "Kamil", Password = "password" }
        };
        private readonly AppSettings appSettings;

        public AccountService(IOptions<AppSettings> options)
        {
            this.appSettings = options.Value;
        }

        public Task<JsonWebToken> SignInAsync(string username, string password)
        {
            var user = Users.FirstOrDefault(x => x.Name == username);
            if (user?.Password == password)
            {
                return Task.FromResult(CreateAccessToken(user));
            }
            else
            {
                throw new Exception();
            }
        }

        private RefreshToken GenerateRefreshToken(User user)
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                Expiration = DateTime.UtcNow.AddMinutes(35) // Make this configurable
            };

            user.RefreshTokens.Add(refreshToken);
            //await userManager.UpdateAsync(user);

            // Update the user along with the new refresh token
            //UserRepository.Update(user);

            return refreshToken;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private void ValidateRefreshToken(User user, string hash)
        {
            if (user is null) throw new ArgumentNullException(paramName: nameof(user));
            var token = user.RefreshTokens.FirstOrDefault(x => x.Token == hash);

            if (token is null || DateTime.UtcNow > token.Expiration)
            {
                throw new SecurityTokenException("Invalid token!");
            }
        }

        private string GetNameClaimFromAccessToken(string accessToken)
        {
            var tokenValidationParamters = new TokenValidationParameters
            {
                ValidateAudience = false, // You might need to validate this one depending on your case
                ValidateIssuer = false,
                ValidateActor = false,
                ValidateLifetime = false, // Do not validate lifetime here
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(appSettings.Secret)
                    )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParamters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token!");
            }

            var userId = principal.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new SecurityTokenException($"Missing claim: {ClaimTypes.Name}!");
            }

            return userId;
        }

        public JsonWebToken RefreshAccessToken(string accessToken, string refreshToken)
        {
            try
            {
                var nameClaim = GetNameClaimFromAccessToken(accessToken);
                var user = Users.FirstOrDefault(x => x.Name == nameClaim);
                ValidateRefreshToken(user, refreshToken);
                return CreateAccessToken(user);
            }
            catch
            {
                return null;
            }
        }

        private JsonWebToken CreateAccessToken(User user)
        {
            var authClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Secret));

            var token = new JwtSecurityToken(
                issuer: "http://dotnetdetail.net",
                audience: "http://dotnetdetail.net",
                expires: DateTime.UtcNow.AddMinutes(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return new JsonWebToken
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = GenerateRefreshToken(user),
                Expiration = token.ValidTo
            };
        }
    }
}
