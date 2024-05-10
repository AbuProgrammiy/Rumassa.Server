using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Rumassa.Application.UseCases.AuthService;
using Rumassa.Domain.Entities.Auth;
using Rumassa.Domain.Entities.DTOs;
using Serilog;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Rumassa.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IAuthService _authService;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO register)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception();
            }

            var user = new User()
            {
                UserName = register.Name + register.Surname,
                Name = register.Name,
                Surname = register.Surname,
                Email = register.Email,
                Password = register.Password,
                PhoneNumber = register.PhoneNumber,
                Role = "User"
            };

            var result = await _userManager.CreateAsync(user, register.Password);

            if (!result.Succeeded)
                throw new Exception();

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new ResponseModel()
            {
                IsSuccess = true,
                Message = "Successfully created",
                StatusCode = 201
            });

        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception();
            }
            var user = await _userManager.FindByEmailAsync(login.Email);

            if (user == null)
            {
                return BadRequest(new TokenDTO()
                {
                    Message = "Email Not Found!",
                    isSuccess = false,
                    Token = ""
                });
            }

            var checker = await _userManager.CheckPasswordAsync(user, login.Password);
            if (!checker)
            {
                return BadRequest(new TokenDTO()
                {
                    Message = "Password do not match!",
                    isSuccess = false,
                    Token = ""
                });
            }

            var token = _authService.GenerateToken(user);

            return Ok(new TokenDTO()
            {
                Token = token,
                isSuccess = true,
                Message = "Success"
            });

        }

        [HttpGet]
        public async Task<List<User>> GetAll()
        {
            var result = await _userManager.Users.ToListAsync();

            return result;
        }



        [HttpGet]
        public async Task<User> GetById(Guid id)
        {
            var result = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (result == null)
                throw new Exception();

            return result;
        }


        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { ReturnUrl = returnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string remoteError)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                throw new Exception("Remote Error!");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                throw new Exception("Error loading external login information!");
            }

            // If the user already has a login (i.e., if there is a record in AspNetUserLogins table)
            // then sign-in the user with this external login provider
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);

                var user = await _userManager.FindByEmailAsync(userEmail!);

                var token = _authService.GenerateToken(user!);

                HttpContext.Response.Cookies.Append("token", token);

                return Ok(200);
            }

            else
            {
                var userEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (userEmail != null)
                {
                    var user = await _userManager.FindByEmailAsync(userEmail);

                    if (user == null)
                    {
                        var name = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                        var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);
                        var phoneNumber = info.Principal.FindFirstValue(ClaimTypes.MobilePhone);

                        user = new User()
                        {
                            UserName = name,
                            Name = name!,
                            Surname = surname!,
                            Email = userEmail,
                            PhoneNumber = phoneNumber,
                            Role = "User"
                        };

                        await _userManager.CreateAsync(user);

                        await _userManager.AddToRoleAsync(user, "User");
                    }

                    await _userManager.AddLoginAsync(user, info);

                    await _signInManager.SignInAsync(user, false);

                    var token = _authService.GenerateToken(user);

                    HttpContext.Response.Cookies.Append("token", token);

                    return Ok(200);
                }

            }

            throw new Exception("Something went wrong!");
        }
    }
}
