using EY.Energy.Application.DTO;
using EY.Energy.Entity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using EY.Energy.Application.Services.Users;
using EY.Energy.Application.DTO.User;

namespace EY.Energy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationServices _userService;
        public AuthenticationController(AuthenticationServices userService)
        {
            _userService = userService;

        }

        //api Login EY user (admin , manager , Consultant ) [/api/AuthCustomer/login]

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userService.Authenticate(model.Username);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("Incorrect Username");
                }

                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    throw new UnauthorizedAccessException("Incorrect Username or Password");
                }

                if (user.IsBanned == true)
                {
                    return Unauthorized("Your account is banned");
                }

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.role.ToString()!)
        };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                };


                await HttpContext.SignInAsync(
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   new ClaimsPrincipal(claimsIdentity),
                   authProperties);

                var userData = new UserData
                {
                    UserId = user.Id.ToString(),
                    Username = user.Username.ToString(),
                    Email = user.Email,
                    Role = user.role.ToString()!
                };

                var userDataJson = JsonSerializer.Serialize(userData);
                var cookieValueBytes = Encoding.UTF8.GetBytes(userDataJson);
                var cookieValueEncoded = WebEncoders.Base64UrlEncode(cookieValueBytes);

                DateTimeOffset? expires = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : (DateTimeOffset?)null;
                Response.Cookies.Append("UserDataCookie", cookieValueEncoded, new CookieOptions
                {
                    Expires = expires,
                    HttpOnly = false

                });

                return Ok(userData);
            }
            else { return BadRequest(); }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("UserDataCookie");
            Response.Cookies.Delete("UserDataCookie2");
            return Ok();
        }
    }
}
