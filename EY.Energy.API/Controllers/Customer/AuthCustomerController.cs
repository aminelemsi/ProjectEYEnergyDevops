using EY.Energy.Entity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EY.Energy.Application.Services.Users;
using EY.Energy.Application.DTO.User;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;

namespace EY.Energy.API.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthCustomerController : ControllerBase
    {
        private readonly AuthenticationServices authenticationServices;
        public AuthCustomerController(AuthenticationServices authenticationServices)
        {
            this.authenticationServices = authenticationServices;
        }

        //api Registre customer [/api/AuthCustomer/registre]

        [HttpPost("register")]
        public async Task<IActionResult> SignupCustomer([FromBody] CreateCustomerModel model)
        {
            try
            {
                var newCustomer = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.Username,
                    Password = model.Password,
                    Phone = model.Phone,
                    NameCompany = model.NameCompany,
                    role = Role.Customer,
                    IsValid = false,
                };

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(newCustomer.Password);
                newCustomer.Password = passwordHash;

                var result = await authenticationServices.CreateUser(newCustomer);

                if (!result.success)
                {
                    return StatusCode(500, new { message = result.errorMessage });
                }

                return Ok(new { message = "Your account has been created successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                return StatusCode(500, new { message = $"An error occurred while creating the user account: {ex.Message}" });
            }
        }

        //api Login customer [/api/AuthCustomer/login]

        [HttpPost("login")]
        public async Task<IActionResult> LoginCustomer([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await authenticationServices.Authenticate(model.Username);

                if (user == null)
                {
                    return Unauthorized("User not found");
                }

                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    throw new UnauthorizedAccessException("Incorrect password");
                }

                if (user.IsValid == false)
                {
                    return Unauthorized("Is Not valid");
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
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.role.ToString()!
                };

                var userDataJson = JsonSerializer.Serialize(userData);
                var cookieValueBytes = Encoding.UTF8.GetBytes(userDataJson);
                var cookieValueEncoded = WebEncoders.Base64UrlEncode(cookieValueBytes);

                DateTimeOffset? expires = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : (DateTimeOffset?)null;
                Response.Cookies.Append("UserDataCookie2", cookieValueEncoded, new CookieOptions
                {
                    Expires = expires,
                    HttpOnly = false

                });

                return Ok(userData);
            }
            else { return BadRequest(); }
        }

    }
}
