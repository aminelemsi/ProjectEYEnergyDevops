using EY.Energy.Application.Services.Publications;
using EY.Energy.Infrastructure.Entity;
using Microsoft.AspNetCore.Mvc;

namespace EY.Energy.API.Controllers.Publications
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicationController : ControllerBase
    {
        private readonly PublicationService _publicationService;
        private readonly PublicationStatisticsService _statisticsService;

        public PublicationController(PublicationService publicationService, PublicationStatisticsService statisticsService)
        {
            _publicationService = publicationService;
            _statisticsService = statisticsService;
        }

        [HttpPost("CreatePublication")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePublication([FromForm] string title, [FromForm] string content, [FromForm] IFormFileCollection files)
        {
            var publication = new Publication
            {
                Title = title,
                Content = content
            };

            var createdPublication = await _publicationService.CreatePublicationAsync(publication, files);
            return Ok(createdPublication);
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPublications()
        {
            var publications = await _publicationService.GetAllPublicationsAsync();
            return Ok(publications);
        }

 
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPublicationById(string id)
        {
            var publication = await _publicationService.GetPublicationByIdAsync(id);
            if (publication == null)
            {
                return NotFound();
            }
            return Ok(publication);
        }

        [HttpGet("files/{id}")]
        public async Task<IActionResult> GetFile(string id)
        {
            var fileStream = await _publicationService.GetFileStreamByIdAsync(id);
            if (fileStream == null)
            {
                return NotFound();
            }

            var fileName = await _publicationService.GetFileNameByIdAsync(id);
            return File(fileStream, "application/octet-stream", fileName);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePublication(string id, [FromForm] string title, [FromForm] string content, [FromForm] IFormFileCollection? files)
        {
            var updatedPublication = new Publication
            {
                Title = title,
                Content = content
            };

            var result = await _publicationService.UpdatePublicationAsync(id, updatedPublication, files);

            if (result)
            {
                return Ok(updatedPublication);
            }

            return NotFound();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePublication(string id)
        {
            var existingPublication = await _publicationService.GetPublicationByIdAsync(id);
            if (existingPublication == null)
            {
                return NotFound();
            }

            var result = await _publicationService.DeletePublicationAsync(id);

            if (result)
            {
                return Ok();
            }

            return StatusCode(500, "An error occurred while deleting the publication.");
        }

        [HttpGet("file-type-stats")]
        public async Task<IActionResult> GetFileTypeStatistics()
        {
            var stats = await _statisticsService.GetFileTypeStatisticsAsync();
            return Ok(stats);
        }

        [HttpGet("publication-review-stats/{fileType}")]
        public async Task<IActionResult> GetPublicationReviewStatsByFileType(string fileType)
        {
            var stats = await _statisticsService.GetPublicationReviewStatsByFileTypeAsync(fileType);
            return Ok(stats);
        }

        [HttpGet("best-publications")]
        public async Task<IActionResult> GetBestPublications()
        {
            var publications = await _statisticsService.GetBestPublicationsAsync();
            return Ok(publications);
        }
        [HttpGet("best-customers")]
        public async Task<IActionResult> GetBestCustomers()
        {
            var customers = await _statisticsService.GetBestCustomersAsync();
            return Ok(customers);
        }
    }
}
