using EY.Energy.Application.DTO;
using EY.Energy.Application.Services.Claim;
using EY.Energy.Infrastructure.Entity;
using Microsoft.AspNetCore.Mvc;

namespace EY.Energy.API.Controllers.Customer
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {

        private readonly ClaimServices claimServices;

        public ContactController(ClaimServices claimServices)
        {
            this.claimServices = claimServices;
        }

        //Send reclamation to administrator [/api/Contact/SendMessage]
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendContactMessage([FromBody] ContactMessage message)
        {
            try
            {
                await claimServices.AddClaim(message);
                return Ok(new { message = "Profile updated successfully." });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

                return StatusCode(500, new { message = $"An error occurred while creating the user account: {ex.Message}" });

            }

        }

        //Get All reclamation to administrator [/api/Contact/GetAllClaims]
        [HttpGet("GetAllClaims")]
        public async Task<IActionResult> GetAllClaims()
        {
            try
            {
                var claims = await claimServices.GetAllClaims();
                return Ok(claims);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        //Validation reclamation to administrator [/api/ValidateClaim/{claimId}]
        [HttpPost("ValidateClaim/{claimId}")]
        public async Task<IActionResult> ValidateClaim(string claimId, [FromBody] ValidationPayload payload)
        {
            try
            {
                await claimServices.ValidateClaim(claimId, payload.ValidationMessage);
                return Ok(new { message = "Claim validated and email sent." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

    }
}
