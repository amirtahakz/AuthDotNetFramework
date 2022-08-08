using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Ui.Api.Models;
using Ui.Core.Repositories;
using Ui.Core.ViewModels;
using Ui.Data.Entities;

namespace Ui.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        #region Connections

        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private IExampleService _exampleService;
        private IEmailService _emailService;
        private ITokenGeneratorService _tokenGeneratorService;

        public AccountController(IExampleService exampleService, IEmailService emailService , ITokenGeneratorService tokenGeneratorService , ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            _exampleService = exampleService;
            _emailService = emailService;
            _tokenGeneratorService = tokenGeneratorService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #endregion

        #region Controllers


        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterVm model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var email = await _userManager.FindByEmailAsync(model.Email);
            if (email != null) return BadRequest("Email already exists!");


            var newUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (!result.Succeeded) return BadRequest("User creation failed! Please check user details and try again.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (!await _roleManager.RoleExistsAsync(UserRolesVm.Admin))
                await _roleManager.CreateAsync(new IdentityRole() { Name = UserRolesVm.Admin });

            if (!await _roleManager.RoleExistsAsync(UserRolesVm.User))
                await _roleManager.CreateAsync(new IdentityRole() { Name = UserRolesVm.User });

            if (await _roleManager.RoleExistsAsync(UserRolesVm.User))
                await _userManager.AddToRoleAsync(user.Id, UserRolesVm.User);

            // Send Email Confirmation Code
            //var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user.Id, user.Email);
            //await _emailService.SendEmailAsync(new EmailModel(user.Email, "Email confirmation", "Your security code is" + code));

            return Ok("User created successfully! please confirm your email");
        }


        [HttpPost]
        [Route("RegisterAdmin")]
        [Authorize(Roles = UserRolesVm.Admin)]
        public async Task<IHttpActionResult> RegisterAdmin(RegisterVm model)
        {

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var email = await _userManager.FindByEmailAsync(model.Email);

            if (email != null) return BadRequest("User already exists!");

            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email
            };
            var result = await _userManager.CreateAsync(user, model.Password);


            if (!result.Succeeded)
                return BadRequest("User creation failed! Please check user details and try again.");


            if (!await _roleManager.RoleExistsAsync(UserRolesVm.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRolesVm.Admin));

            if (!await _roleManager.RoleExistsAsync(UserRolesVm.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRolesVm.User));

            if (await _roleManager.RoleExistsAsync(UserRolesVm.Admin))
                await _userManager.AddToRoleAsync(user.Id, UserRolesVm.Admin);

            return Ok("Admin created successfully!");
        }


        [AllowAnonymous]
        [Route("Login")]
        public async Task<IHttpActionResult> Login(LoginVm model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                if (!await _userManager.IsEmailConfirmedAsync(user.Id)) return BadRequest("Please confirm your email");

                var userRoles = await _userManager.GetRolesAsync(user.Id);

                string accessToken = _tokenGeneratorService.GenerateToken(user, userRoles);
                string refreshToken = _tokenGeneratorService.GenerateRefreshToken();
                var res = _tokenGeneratorService.GetByUserId(user.Id);
                if (res.Result != null)
                    await _tokenGeneratorService.DeleteRefreshToken(res.Result.Id);

                UserRefreshToken item = new UserRefreshToken()
                {
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                };
                await _tokenGeneratorService.CreateRefreshToken(item);

                return Ok(new { accessToken, refreshToken });
            }
            return Unauthorized();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("RefreshToken")]
        public async Task<IHttpActionResult> RefreshToken(RefreshTokenVm model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            bool isValidRefreshToken = _tokenGeneratorService.ValidateRefreshToken(model.RefreshToken);
            if (!isValidRefreshToken) return BadRequest("Invalid refresh token");


            UserRefreshToken res = await _tokenGeneratorService.GetByRefreshToken(model.RefreshToken);
            if (res == null) return BadRequest("Invalid refresh token");


            var user = await _userManager.FindByIdAsync(res.UserId);
            if (user == null) return BadRequest("User not found");



            await _tokenGeneratorService.DeleteRefreshToken(res.Id);

            var userRoles = await _userManager.GetRolesAsync(user.Id);
            string accessToken = _tokenGeneratorService.GenerateToken(user, userRoles);
            string refreshToken = _tokenGeneratorService.GenerateRefreshToken();

            UserRefreshToken item = new UserRefreshToken()
            {
                RefreshToken = refreshToken,
                UserId = user.Id,
            };
            await _tokenGeneratorService.CreateRefreshToken(item);

            return Ok(new { accessToken, refreshToken });

        }


        [HttpPost]
        [AllowAnonymous]
        [Route("SendEmailCode")]
        public async Task<IHttpActionResult> SendEmailCode(SendEmailCodeVm model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null) return BadRequest("User not found");


            if (user.EmailConfirmed) return BadRequest("User email confirmed before");


            var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user.Id, user.Email);
            await _emailService.SendEmailAsync(new EmailModel(user.Email, "Confirm email", "Your security code is" + code));

            return Ok("Confirmation email sent successfully!");
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("ConfirmEmailCode")]
        public async Task<IHttpActionResult> ConfirmEmailCode(ConfirmEmailCodeVm model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = _userManager.Users.SingleOrDefault(u => u.Email == model.Email);

            if (user == null) return BadRequest("User not found");

            var result = await _userManager.ChangePhoneNumberAsync(user.Id, model.Email, model.Code);

            if (!result.Succeeded) return BadRequest("Email is not confirmed");

            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = false;
            await _userManager.UpdateAsync(user);

            return Ok("Your email confirmed successfully!");
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordVm model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest("User is not exists!");

            if (!await _userManager.IsEmailConfirmedAsync(user.Id)) return BadRequest("Please confirm your email");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
            //var callBackUrl = Url.Route("ResetPassword", new { Email = user.Email, Token = code });
            var callBackUrl = $"https://localhost:44305/Account/ResetPassword?userId= { user.Id }&code={ code }";
            await _emailService.SendEmailAsync(new EmailModel(user.Email, "Reset Password", "Please reset your password by clicking <a href=\"" + callBackUrl + "\">here</a>"));
            return Ok("Forgot password email sent successfully!");
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(ResetPasswordVm model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest("User is not exists!");

            if (!await _userManager.IsEmailConfirmedAsync(user.Id)) return BadRequest("Please confirm your email");

            
            var result = await _userManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (!result.Succeeded) return BadRequest("Something is wrong");

            return Ok("Password Changed successfully!");
        }


        public IHttpActionResult Privacy()
        {
            return Ok();
        }


        #endregion

        #region Helpers

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}
