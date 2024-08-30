using EY.Energy.Application.Services.Answers;
using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System.Security.Claims;
using System.Text;

namespace EY.Energy.API.Controllers.Forms
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientResponsesController : ControllerBase
    {

        private readonly ClientResponseService _responseService;
        private readonly GridFSBucket _bucket;
        private readonly CompanyServices companyServices;
        private readonly PdfService _pdfService;
        public ClientResponsesController(ClientResponseService responseService, MongoDBContext dBContext, CompanyServices companyServices, PdfService pdfService)
        {
            _responseService = responseService;
            _bucket = dBContext.Bucket;
            this.companyServices = companyServices;
            _pdfService = pdfService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create([FromForm] ClientResponse response, IFormFile? file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID is not available.");
            }

            response.CustomerId = userId;

            var company = await companyServices.GetCompanyByCustomerIdAsync(userId);
            if (company == null)
            {
                return BadRequest("Company not found for the customer.");
            }

            response.CompanyId = company.Id;

            if (file != null)
            {
                var fileId = ObjectId.GenerateNewId();
                using (var stream = file.OpenReadStream())
                {
                    var options = new GridFSUploadOptions
                    {
                        Metadata = new BsonDocument
                        {
                            { "ContentType", file.ContentType }
                        }
                    };

                    await _bucket.UploadFromStreamAsync(fileId, file.FileName, stream, options);
                }

                response.FileId = fileId.ToString();
            }

            await _responseService.AddResponseAsync(response);

            return Ok();
        }

        [HttpGet("clientResponses")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<List<ClientResponse>>> GetClientResponses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID is not available.");
            }

            var company = await companyServices.GetCompanyByCustomerIdAsync(userId);
            if (company == null)
            {
                return BadRequest("Company not found for the customer.");
            }

            var responses = await _responseService.GetResponsesByCompanyIdAsync(company.Id);
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

                if (!string.IsNullOrEmpty(response.FileId))
                {
                    response.FileId = await _responseService.GetFileNameByIdAsync(response.FileId);
                }
            }

            return Ok(responses);
        }

        [HttpGet("unfinalizedResponses/{formId:length(24)}")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<List<ClientResponse>>> GetUnfinalizedResponses(string formId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID is not available.");
            }

            var responses = await _responseService.GetUnfinalizedResponsesAsync(userId, formId);
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

                if (!string.IsNullOrEmpty(response.FileId))
                {
                    response.FileId = await _responseService.GetFileNameByIdAsync(response.FileId);
                }
            }

            return Ok(responses);
        }

        [HttpPost("finalize/{formId:length(24)}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> FinalizeResponses(string formId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID is not available.");
            }

            await _responseService.FinalizeResponsesAsync(userId, formId);

            return Ok();
        }

        [HttpGet("hasResponded/{formId:length(24)}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> HasClientResponded(string formId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID is not available.");
            }

            var hasResponded = await _responseService.HasClientRespondedToForm(userId, formId);
            return Ok(new { hasResponded });
        }

        [HttpPut("updateFile/{responseId:length(24)}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateResponseFile(string responseId, IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("File is required.");
            }

            var fileId = ObjectId.GenerateNewId();
            using (var stream = file.OpenReadStream())
            {
                var options = new GridFSUploadOptions
                {
                    Metadata = new BsonDocument
                    {
                        { "ContentType", file.ContentType }
                    }
                };

                await _bucket.UploadFromStreamAsync(fileId, file.FileName, stream, options);
            }

            await _responseService.UpdateResponseFileAsync(responseId, fileId.ToString());

            return Ok();
        }



        [HttpGet("exportResponsesByForm/{companyId}")]
        public async Task<IActionResult> ExportResponsesByFormToPdf(string companyId)
        {
            var groupedResponses = await _responseService.GetResponsesByCompanyGroupedByFormAsync(companyId);
            if (groupedResponses == null || groupedResponses.Count == 0)
            {
                return NotFound();
            }

            var zipStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var formResponses in groupedResponses)
                {
                    var formTitle = formResponses.Key;
                    var responses = formResponses.Value;

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

                    var sb = new StringBuilder();
                    sb.Append("<html><head><style>");
                    sb.Append("body { font-family: Arial, sans-serif; }");
                    sb.Append("h1 { color: #000000; }");
                    sb.Append("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
                    sb.Append("table, th, td { border: 1px solid #000; }");
                    sb.Append("th, td { padding: 10px; text-align: left; }");
                    sb.Append("th { background-color: #333; color: #FFF; }");
                    sb.Append("</style></head><body>");
                    sb.Append($"<header><img src='data:image/png;base64," + Convert.ToBase64String(System.IO.File.ReadAllBytes("wwwroot/images/logo.png")) + "' alt='EY Logo' width='110' style='float: left;'/><h1>Responses Report</h1></header>");
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

                    var pdfEntry = archive.CreateEntry($"{formTitle}.pdf", System.IO.Compression.CompressionLevel.Fastest);
                    using (var entryStream = pdfEntry.Open())
                    using (var pdfStream = new MemoryStream(pdfContent))
                    {
                        pdfStream.CopyTo(entryStream);
                    }
                }
            }

            zipStream.Seek(0, SeekOrigin.Begin);
            return File(zipStream, "application/zip", "ResponsesByForm.zip");
        }



        [HttpGet("company/{companyId:length(24)}/groupedByForm")]
        public async Task<ActionResult<Dictionary<string, List<ClientResponse>>>> GetResponsesByCompanyGroupedByForm(string companyId)
        {
            var responses = await _responseService.GetResponsesByCompanyGroupedByFormAsync(companyId);
            if (responses == null || responses.Count == 0)
            {
                return NotFound();
            }
            return Ok(responses);
        }


        [HttpGet("consultant/responses/{companyId:length(24)}")]
        [Authorize(Roles = "Consultant")]
        public async Task<ActionResult<List<ClientResponse>>> GetResponsesByConsultant(string companyId)
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

            var company = companies.FirstOrDefault(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound("Company not found for this consultant.");
            }

            var responses = await _responseService.GetResponsesByCompanyIdsAsync(new List<string> { companyId });
            if (responses == null || !responses.Any())
            {
                return NotFound();
            }

            return Ok(responses);
        }

        [HttpGet("company/{companyId:length(24)}")]
        public async Task<ActionResult<List<ClientResponse>>> GetResponsesByCompanyId(string companyId)
        {
            var responses = await _responseService.GetResponsesByCompanyIdAsync(companyId);
            if (responses == null || !responses.Any())
            {
                return NotFound();
            }
            return responses;
        }

        [HttpPut("{responseId:length(24)}")]
        public async Task<IActionResult> UpdateResponse(string responseId, [FromBody] ClientResponse updatedResponse)
        {
            if (!ObjectId.TryParse(responseId, out _))
            {
                return BadRequest("Invalid response ID.");
            }

            var existingResponse = await _responseService.GetResponseByIdAsync(responseId);
            if (existingResponse == null)
            {
                return NotFound("Response not found.");
            }

            updatedResponse.ResponseId = responseId;
            await _responseService.UpdateResponse(updatedResponse);

            return NoContent();
        }

    }

}
