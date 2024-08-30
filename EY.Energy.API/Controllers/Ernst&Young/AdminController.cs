using EY.Energy.Application.DTO.User;
using EY.Energy.Application.EmailConfiguration;
using EY.Energy.Application.Services.Answers;
using EY.Energy.Application.Services.Users;
using EY.Energy.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EY.Energy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AuthenticationServices authenticationServices;
        private readonly IEmailService emailService;
        private readonly UserServices userServices;
        private readonly CustomerServices customerServices;
        private readonly CompanyServices companyService;
        public AdminController(AuthenticationServices authenticationServices, IEmailService emailService, UserServices userServices, CustomerServices customerServices, CompanyServices companyService)
        {
            this.authenticationServices = authenticationServices;
            this.emailService = emailService;
            this.userServices = userServices;
            this.customerServices = customerServices;
            this.companyService = companyService;
        }


        //Create compte User EY by admin [/api/Admin/create-user]
        [Authorize(Roles = "Admin")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserModel model)
        {
            try
            {
                var newUser = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.LastName + model.FirstName,
                    Password = GenerateRandomPassword(),
                    Phone = model.Phone,
                    IsBanned = false,
                    role = null
                };

                string temporaryPassword = newUser.Password;


                string passwordHash = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
                newUser.Password = passwordHash;


                /*          if (!newUser.Email.EndsWith("@tn.ey.com"))
                          {
                              return BadRequest(new { message = "Emails for Consultant and Manager roles must end with @tn.ey.com" });
                          }*/

                var result = await authenticationServices.CreateUser(newUser);

                if (!result.success)
                {
                    return StatusCode(500, new { message = result.errorMessage });
                }

                var emailSubject = "Your new account has been created successfully";
                var emailMessage = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            color: #333333;
            padding: 20px;
        }}
        .container {{
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            padding: 20px;
            margin: 20px auto;
            max-width: 600px;
        }}
        .header {{
            text-align: center;
            border-bottom: 1px solid #dddddd;
            padding-bottom: 10px;
        }}
        .header img {{
            max-width: 150px;
        }}
        .content {{
            padding: 20px;
        }}
        .footer {{
            text-align: center;
            font-size: 12px;
            color: #999999;
            margin-top: 20px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://geomarvel.com/wp-content/uploads/2021/06/ernst-young-EY-logo-800x389.png' alt='EY Logo' />
        </div>
        <div class='content'>
            <h1>Welcome to EY</h1>
            <p>Hi {newUser.Username},</p>
            <p>Your account has been successfully created.</p>
            <p><strong>Username:</strong> {newUser.Username}</p>
            <p><strong>Password:</strong> {temporaryPassword}</p>
            <p>Please use the following link to sign in:</p>
            <p><a href='http://localhost:4200/signin'>Sign In</a></p>
            <p>Best regards,<br>EY</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 EY. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

                await emailService.SendEmailAsync(newUser.Email, emailSubject, emailMessage);

                await emailService.SendEmailAsync(newUser.Email, emailSubject, emailMessage);

                return Ok(new { message = "The user account has been created successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                return StatusCode(500, new { message = $"An error occurred while creating the user account: {ex.Message}" });
            }
        }

        //Assign role to user by Admin [/api/Admin/assign-role]
        [Authorize(Roles = "Admin")]
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleModel model)
        {
            var existingUser = await userServices.GetUserByUsername(model.Username);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (Enum.TryParse(model.Role, true, out Role role))
            {
                existingUser.role = role;
                await userServices.UpdateUser(existingUser);

                return Ok(new { message = "The role has been successfully assigned to the user." });
            }
            else
            {
                return BadRequest(new { message = "The specified role is invalid." });
            }
        }

        //Display all user of EY by Admin (Manager and Consultant) [/api/Admin/getallUserForEY]
        [Authorize(Roles = "Admin")]
        [HttpGet("getallUserForEY")]
        public async Task<IActionResult> GetManagersAndConsultants()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var managersAndConsultants = await userServices.GetUsersByRole(Role.Manager, Role.Consultant, null);
                return Ok(managersAndConsultants);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"GetManagersAndConsultants took {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        //Display all user Blocked of EY by Admin (Manager and Consultant) [/api/Admin/getallUserBlocked]
        [Authorize(Roles = "Admin")]
        [HttpGet("getallUserBlocked")]
        public async Task<IActionResult> GetUserBlocked()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var managersAndConsultants = await userServices.GetUsersBlock(Role.Manager, Role.Consultant, null);
                return Ok(managersAndConsultants);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"GetUserBlocked took {stopwatch.ElapsedMilliseconds} ms");
            }
        }

        //Display all Costumer no validate by Admin [/api/Admin/customers]
        [Authorize(Roles = "Admin")]
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var customers = await customerServices.GetNonValidatedCustomers();
                return Ok(customers);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"GetUserBlocked took {stopwatch.ElapsedMilliseconds} ms");
            }

        }
        //Display all Costumer valide by Admin [/api/Admin/Validcustomers]

        [Authorize(Roles = "Admin")]
        [HttpGet("Validcustomers")]
        public async Task<IActionResult> GetCustomersValid()
        {
            try
            {
                var customers = await customerServices.GetValidatedCustomers();
                return Ok(customers);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }


        // Blocked user EY (manager and consultant ) By admin [/api/Admin/BanedUser]

        [Authorize(Roles = "Admin")]
        [HttpPost("BanedUser")]
        public async Task<IActionResult> BanedUser([FromBody] BanedOrValidateUser model)
        {
            var existingUser = await userServices.GetUserByUsername(model.Username);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found." });
            }

            existingUser.IsBanned = true;

            await userServices.UpdateUser(existingUser);

            var emailSubject = "Information";
            var emailMessage = $"Hi {existingUser.FirstName},\n\n: Your account is banned by admin \n\nBest regards,\nEY";

            await emailService.SendEmailAsync(existingUser.Email, emailSubject, emailMessage);

            return Ok(new { message = "User is Banned" });
        }

        // Allowed user of EY (manager and consultant ) By admin [/api/Admin/AllowedUser]

        [Authorize(Roles = "Admin")]
        [HttpPost("AllowedUser")]
        public async Task<IActionResult> AdmitUser([FromBody] BanedOrValidateUser model)
        {
            var existingUser = await userServices.GetUserByUsername(model.Username);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found." });
            }

            existingUser.IsBanned = false;

            await userServices.UpdateUser(existingUser);

            var emailSubject = "Information";
            var emailMessage = $"Hi {existingUser.FirstName},\n\n: Your account is allowed by admin \n\nBest regards,\nEY";

            await emailService.SendEmailAsync(existingUser.Email, emailSubject, emailMessage);

            return Ok(new { message = "User is allowed" });
        }


        //Validate costumer by Admin and creation of your own business declared in authentication  [/api/Admin/ManageCustomer]

        [Authorize(Roles = "Admin")]
        [HttpPost("ManageCustomer")]
        public async Task<IActionResult> ValidUser([FromBody] BanedOrValidateUser model)
        {
            var existingUser = await userServices.GetUserByUsername(model.Username);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found." });
            }

            existingUser.IsValid = true;

            await userServices.UpdateUser(existingUser);

            var emailSubject = "Your new account has been created successfully";
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
                <p>Hi {existingUser.FirstName},</p>
                <p>Your account is active. You can connect now.</p>
            </div>
            <div class='footer'>
                Best regards, <br> EY
            </div>
        </div>
    </body>
    </html>";

            await emailService.SendEmailAsync(existingUser.Email, emailSubject, emailMessage); // Pass 'true' to send HTML email

            // Create and assign a company to the user
            var company = new Company
            {
                CustomerId = existingUser.Id,
                Name = existingUser.NameCompany,
                AnswerIds = new List<string>(),
                ConsultantIds = new List<string>()
            };

            await companyService.AddCompany(company);

            return Ok(new { message = "ok", companyId = company.Id });
        }


        //Auto generate password 
        public static string GenerateRandomPassword(PasswordOptions opts = null!)
        {
            if (opts == null) opts = new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        //Display User by id [/api/Admin/{userId})]
        [HttpGet("{userId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            try
            {
                var user = await userServices.GetUserById(userId);
                if (user == null)
                {
                    return NotFound();
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //Display by username [/api/Admin/search-by-username})]
        [Authorize(Roles = "Admin")]
        [HttpGet("search-by-username")]
        public async Task<IActionResult> SearchUsersByUsername(string username)
        {
            var users = await userServices.SearchUsersByUsernameAsync(username);
            return Ok(users);
        }

        //Display by Roles [/api/Admin/search-by-role})]
        [Authorize(Roles = "Admin")]
        [HttpGet("search-by-role")]
        public async Task<IActionResult> SearchUsersByRole(Role? role)
        {
            var users = await userServices.SearchUsersByRoleAsync(role);
            return Ok(users);
        }

    }
}

