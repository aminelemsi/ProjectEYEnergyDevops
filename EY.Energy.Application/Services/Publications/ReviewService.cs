using EY.Energy.Infrastructure.Configuration;
using EY.Energy.Infrastructure.Entity;
using MongoDB.Driver;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EY.Energy.Application.Services.Publications
{
    public class ReviewService
    {
        private readonly IMongoCollection<Publication> _publications;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMongoCollection<Review> _reviews;
        public ReviewService(MongoDBContext context, IHttpContextAccessor httpContextAccessor)
        {
            _reviews = context.Reviews;
            _publications = context.Publications;
            _httpContextAccessor = httpContextAccessor;

        }
        public async Task<bool> AddOrUpdateReviewAsync(string publicationId, Review review)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = _httpContextAccessor.HttpContext.User.Identity.Name;

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
                {
                    return false;
                }

                var filter = Builders<Publication>.Filter.Eq(p => p.Id, publicationId);
                var update = Builders<Publication>.Update.PullFilter(p => p.Reviews, r => r.UserId == userId);

                await _publications.UpdateOneAsync(filter, update);

                review.UserId = userId;
                review.Username = username;
                update = Builders<Publication>.Update.AddToSet(p => p.Reviews, review);

                var result = await _publications.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }



        public async Task<List<Review>> GetReviewsByPublicationIdAsync(string publicationId)
        {
            try
            {
                var publication = await _publications.Find(p => p.Id == publicationId).FirstOrDefaultAsync();
                return publication?.Reviews ?? new List<Review>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
