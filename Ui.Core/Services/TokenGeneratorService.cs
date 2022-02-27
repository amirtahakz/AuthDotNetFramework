using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Ui.Core.Repositories;
using Ui.Data.Entities;
using System.Configuration;
using Ui.Data.Context;

namespace Ui.Core.Services
{
    public class TokenGeneratorService : BaseService<UserRefreshToken>, ITokenGeneratorService
    {
        #region Connections

        public TokenGeneratorService(ApplicationDbContext context) : base(context) { }

        #endregion

        #region Methods

        public string GenerateToken(ApplicationUser user, IList<string> userRoles)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationSettings.AppSettings["AccessTokenSecret"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //Create a List of Claims, Keep claims name short    
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            //Create Security Token object by giving required parameters    
            var token = new JwtSecurityToken(
                            ConfigurationSettings.AppSettings["Issuer"], //Issure    
                            ConfigurationSettings.AppSettings["Audience"],  //Audience    
                            authClaims, // Claims
                            expires: DateTime.Now.AddMinutes(double.Parse(ConfigurationSettings.AppSettings["AccessTokenExpirationMinutes"])),
                            signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationSettings.AppSettings["RefreshTokenSecret"]));

            var token = new JwtSecurityToken(
                issuer: ConfigurationSettings.AppSettings["Issuer"],
                audience: ConfigurationSettings.AppSettings["Audience"],
                expires: DateTime.Now.AddMinutes(double.Parse(ConfigurationSettings.AppSettings["RefreshTokenExpirationMinutes"])),
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateRefreshToken(string refreshToken)
        {
            TokenValidationParameters validationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationSettings.AppSettings["RefreshTokenSecret"])),
                ValidIssuer = ConfigurationSettings.AppSettings["Issuer"],
                ValidAudience = ConfigurationSettings.AppSettings["Audience"],
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ClockSkew = TimeSpan.Zero
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task CreateRefreshToken(UserRefreshToken refreshToken)
        {
            await AddAsync(refreshToken);
        }

        public Task<UserRefreshToken> GetByRefreshToken(string refreshToken)
        {
            try
            {
                UserRefreshToken result = Table().FirstOrDefault(r => r.RefreshToken == refreshToken);

                return Task.FromResult(result);
            }
            catch (Exception)
            {

                return null;
            }
        }

        public async Task DeleteRefreshToken(Guid id)
        {
            await DeleteAsync(id);
        }

        public async Task<UserRefreshToken> GetByUserId(string userId)
        {
            try
            {
                return Table().FirstOrDefault(r => r.UserId == userId);
            }
            catch (Exception)
            {

                return null;
            }
        }

        #endregion
    }
}
