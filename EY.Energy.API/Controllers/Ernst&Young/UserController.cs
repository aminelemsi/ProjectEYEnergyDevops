using EY.Energy.Application.DTO.User;
using EY.Energy.Application.EmailConfiguration;
using EY.Energy.Application.Services.Users;
using EY.Energy.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace EY.Energy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserServices userServices;
        private readonly CustomerServices customerServices;
        private readonly IEmailService emailService;
        public UserController(UserServices userServices, CustomerServices customerServices, IEmailService emailService)
        {
            this.userServices = userServices;
            this.customerServices = customerServices;
            this.emailService = emailService;
        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfile model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User identifier not found in claims.");
                }
                var existingUser = await userServices.GetUserById(userId);
                if (existingUser == null)
                {
                    return NotFound("User not found");
                }
                if (existingUser == null)
                {
                    return NotFound("User not found");
                }
                existingUser.Phone = model.Phone;
                await userServices.UpdateUser(existingUser);
                return Ok(new { message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = $"An error occurred while updating the user profile: {ex.Message}" });
            }

        }

        [Authorize(Roles = "Manager")]
        [HttpGet("GetAllConsultant")]
        public async Task<IActionResult> GetManagersAndConsultants()
        {
            try
            {
                var managersAndConsultants = await userServices.GetUsersByRole(Role.Consultant);
                return Ok(managersAndConsultants);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [Authorize(Roles = "Manager")]
        [HttpGet("GetAllConsultantAndClient")]
        public async Task<IActionResult> GetAllConsultantAndClient()
        {
            try
            {
                var managersAndConsultants = await userServices.GetUsersByRole(Role.Consultant);
                return Ok(managersAndConsultants);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("GetMyProfile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var userDataCookie = Request.Cookies["UserDataCookie"];
                if (string.IsNullOrEmpty(userDataCookie))
                {
                    return Unauthorized("No user logged in.");
                }
                var userDataBytes = WebEncoders.Base64UrlDecode(userDataCookie);
                var userDataJson = Encoding.UTF8.GetString(userDataBytes);
                var userData = JsonSerializer.Deserialize<UserData>(userDataJson);

                var user = await userServices.GetCurrentUserByUsername(userData!.Username);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"GetMyProfile took {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        [HttpGet("GetProfileCustomer")]
        public async Task<IActionResult> GetProfileClient()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var userDataCookie = Request.Cookies["UserDataCookie2"];


                if (string.IsNullOrEmpty(userDataCookie))
                {
                    return Unauthorized("No user logged in.");
                }

                var userDataBytes = WebEncoders.Base64UrlDecode(userDataCookie);
                var userDataJson = Encoding.UTF8.GetString(userDataBytes);
                var userData = JsonSerializer.Deserialize<UserData>(userDataJson);

                var user = await userServices.GetCurrentUserByUsername(userData!.Username);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"GetMyProfile took {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        [HttpPost("AssignCustomerToConsultant")]
        public async Task<IActionResult> AssignCustomerToConsultant(string customerUsername, string consultantUsername)
        {
            try
            {
                await customerServices.AssignCustomerToConsultant(customerUsername, consultantUsername);
                return Ok("Customer assigned to consultant successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("Customers")]
        public async Task<IActionResult> GetCustomersForConsultant()
        {
            try
            {
                var userDataCookie = Request.Cookies["UserDataCookie"];

                if (string.IsNullOrEmpty(userDataCookie))
                {
                    return Unauthorized("No user logged in.");
                }

                var userDataBytes = WebEncoders.Base64UrlDecode(userDataCookie);
                var userDataJson = Encoding.UTF8.GetString(userDataBytes);
                var userData = JsonSerializer.Deserialize<UserData>(userDataJson);

                var consultantId = User.FindFirstValue(userData!.Username);
                if (string.IsNullOrEmpty(consultantId))
                {
                    throw new UnauthorizedAccessException("Consultant ID not found in claims.");
                }

                var customers = await customerServices.GetCustomersForConsultant(consultantId);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            try
            {
                var user = await userServices.GetUserByEmail(model.Email);
                if (user == null)
                {
                    return NotFound(new { message = "Invalid user with this mail" });
                }

                Random generator = new Random();
                string resetToken = generator.Next(0, 1000000).ToString("D6");

                DateTime expirationTime = DateTime.UtcNow.AddMinutes(2);

                user.ResetPasswordToken = resetToken;
                user.ResetPasswordTokenExpiration = expirationTime;
                await userServices.UpdateUser(user);

                var emailSubject = "Resetting your password";
                var emailMessage = $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    color: #333333;
                }}
                .container {{
                    width: 100%;
                    max-width: 600px;
                    margin: 0 auto;
                    padding: 20px;
                    border: 1px solid #eaeaea;
                    border-radius: 5px;
                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    background-color: #f7f7f7;
                    padding: 10px;
                    text-align: center;
                    border-bottom: 1px solid #eaeaea;
                }}
                .header img {{
                    max-width: 100px;
                }}
                .content {{
                    padding: 20px;
                }}
                .footer {{
                    background-color: #f7f7f7;
                    padding: 10px;
                    text-align: center;
                    border-top: 1px solid #eaeaea;
                    font-size: 12px;
                    color: #777777;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <img src='https://cdn.freebiesupply.com/logos/large/2x/ey-logo-png-transparent.png' alt='EY'>
                </div>
                <div class='content'>
                    <p>Good morning {user.FirstName},</p>
                    <p>You have requested a password reset.</p>
                    <p><strong>Your secret code is: {resetToken}</strong></p>
                    <p>If you haven't requested this reset, just ignore this email.</p>
                </div>
                <div class='footer'>
                    Regards, <br> EY
                </div>
            </div>
        </body>
        </html>";

                await emailService.SendEmailAsync(user.Email, emailSubject, emailMessage); // Pass 'true' to send HTML email

                return Ok(new
                {
                    message = "An email with password reset instructions has been sent to your registered email address.",
                    expiration = user.ResetPasswordTokenExpiration.Value.ToString("o") // ISO 8601 format
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return StatusCode(500, new { message = $"An error occurred while sending password reset instructions: {ex.Message}" });
            }
        }


        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            try
            {
                var user = await userServices.GetUserByResetToken(model.Token);
                if (user == null)
                {
                    return NotFound(new { message = "The password reset token has expired." });
                }
                if (user.ResetPasswordTokenExpiration.HasValue && user.ResetPasswordTokenExpiration.Value < DateTime.UtcNow)
                {
                    return BadRequest(new { message = "The password reset token has expired." });
                }
                string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.Password = newPasswordHash;
                await userServices.UpdateUser(user);
                return Ok(new { message = "Your password has been successfully reset." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return StatusCode(500, new { message = $"An error occurred while resetting your password: {ex.Message}" });
            }
        }
        private string GenerateResetToken()
        {
            return Guid.NewGuid().ToString();
        }

    }
}
