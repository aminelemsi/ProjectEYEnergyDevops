using EY.Energy.Application.DTO.User;
using EY.Energy.Application.Services.Answers;
using EY.Energy.Application.Services.Users;
using EY.Energy.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.GridFS;
using System.Security.Claims;
using System.Text;

namespace EY.Energy.API.Controllers.Forms
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly CompanyServices companyServices;
        private readonly UserServices userServices;
        private readonly ILogger<CompanyController> _logger;
        private readonly ClientResponseService _responseService;
        private readonly PdfService _pdfService;

        public CompanyController(CompanyServices companyServices, UserServices userServices, ILogger<CompanyController> logger ,PdfService pdfService, ClientResponseService clientResponseService )
        {
            this.companyServices = companyServices;
            this.userServices = userServices;
            _logger = logger;
            _responseService = clientResponseService;
            _pdfService = pdfService;
        }

        [HttpGet("ListCompany")]
        [Authorize(Roles = "Admin,Manager")]
        public Task<IActionResult> GetCompany()
        {
            try
            {
                var companies = companyServices.GetCompanyList();
                return Task.FromResult<IActionResult>(Ok(companies));
            }
            catch (Exception)
            {
                return Task.FromResult<IActionResult>(StatusCode(500, "Internal server error"));
            }
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("consultants")]
        public async Task<IActionResult> GetConsultants()
        {
            try
            {
                var consultants = await userServices.GetAllConsultants();
                return Ok(consultants);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost("assign-consultant")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignConsultant([FromBody] AssignConsultantModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.CompanyId) || string.IsNullOrEmpty(model.ConsultantId))
            {
                _logger.LogWarning("Invalid data: {Model}", model); 
                return BadRequest("Invalid data.");
            }

            try
            {
                _logger.LogInformation("Assigning consultant: {ConsultantId} to company: {CompanyId}", model.ConsultantId, model.CompanyId); // Log for debugging

                var isAssigned = await companyServices.IsConsultantAssignedToCompany(model.CompanyId, model.ConsultantId);
                if (isAssigned)
                {
                    _logger.LogWarning("Consultant is already assigned to this company: {CompanyId}", model.CompanyId); // Log for debugging
                    return BadRequest(new { message = "Consultant is already assigned to this company." });
                }

                await companyServices.AssignConsultantToCompany(model.CompanyId, model.ConsultantId);
                return Ok(new { message = "Consultant assigned successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning consultant");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("consultant/companies")]
        [Authorize(Roles = "Consultant")]
        public async Task<ActionResult<List<Company>>> GetCompaniesByConsultant()
        {
            var consultantId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (consultantId == null)
            {
                return Unauthorized("Consultant ID is not available.");
            }

            var companies = await companyServices.GetCompaniesByConsultantIdAsync(consultantId);
            if (companies == null || !companies.Any())
            {
                return NotFound();
            }

            return Ok(companies);
        }



        [HttpGet("exportResponses/{companyId}")]
        public async Task<IActionResult> ExportResponsesToPdf(string companyId)
        {
            var responses = await _responseService.GetResponsesByCompanyIdAsync(companyId);
            if (responses == null || !responses.Any())
            {
                return NotFound();
            }

            var questionIds = responses.Select(r => r.QuestionId).Distinct().ToList();
            var questions = await _responseService.GetQuestionsByIdsAsync(questionIds);

            foreach (var response in responses)
            {
                var question = questions.FirstOrDefault(q => q.QuestionId == response.QuestionId);
                if (question != null)
                {
                    response.QuestionText = question.Text;
                }
            }

            // Génération du contenu HTML pour le PDF
            var sb = new StringBuilder();
            sb.Append("<html><head><style>");
            sb.Append("body { font-family: Arial, sans-serif; }");
            sb.Append("h1 { color: #000000; }");
            sb.Append("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.Append("table, th, td { border: 1px solid #000; }");
            sb.Append("th, td { padding: 10px; text-align: left; }");
            sb.Append("th { background-color: #333; color: #FFF; }");
            sb.Append("</style></head><body>");
            sb.Append("<header><img src='data:image/png;base64," + Convert.ToBase64String(System.IO.File.ReadAllBytes("wwwroot/images/logo.png")) + "' alt='EY Logo' width='110' style='float: left;'/><h1>Responses Report</h1></header>");
            sb.Append("<table><thead><tr><th>Question</th><th>Response</th><th>File</th></tr></thead><tbody>");

            foreach (var response in responses)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{response.QuestionText}</td>");
                sb.Append($"<td>{response.ResponseText}</td>");
                sb.Append($"<td>{(response.FileId != null ? "Yes" : "No")}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            sb.Append("<footer style='position: fixed; bottom: 0; width: 100%; text-align: center; font-size: small; color: #777;'>Generated by EY Energy Management System</footer>");
            sb.Append("</body></html>");

            var pdfContent = _pdfService.CreatePdf(sb.ToString());

            return File(pdfContent, "application/pdf", "responses.pdf");
        }
    }

}
