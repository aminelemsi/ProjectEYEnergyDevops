using EY.Energy.Application.Services.Publications;
using EY.Energy.Infrastructure.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EY.Energy.API.Controllers.Publications
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewService;
        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost("{publicationId}/reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddOrUpdateReview(string publicationId, [FromBody] Review review)
        {
            var result = await _reviewService.AddOrUpdateReviewAsync(publicationId, review);

            if (result)
            {
                return Ok();
            }

            return NotFound();
        }


        [HttpGet("{publicationId}/review")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReviewsByPublicationId(string publicationId)
        {
            var reviews = await _reviewService.GetReviewsByPublicationIdAsync(publicationId);
            if (reviews == null)
            {
                return NotFound();
            }
            return Ok(reviews);
        }
    }
}
