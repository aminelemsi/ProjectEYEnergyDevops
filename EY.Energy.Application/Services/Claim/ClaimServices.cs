using EY.Energy.Application.EmailConfiguration;
using EY.Energy.Infrastructure.Configuration;
using EY.Energy.Infrastructure.Entity;
using MongoDB.Driver;


namespace EY.Energy.Application.Services.Claim
{
    public class ClaimServices
    {
        private readonly IEmailService _emailService;
        private readonly IMongoCollection<ContactMessage> _contactMessage;
        public ClaimServices(IEmailService emailService, MongoDBContext mongoDB)
        {
            _emailService = emailService;
            _contactMessage = mongoDB.ContactMessages;
        }
        public async Task AddClaim(ContactMessage contactMessage)
        {
            try
            {
                contactMessage.IsProcessed = false;
                _contactMessage.InsertOne(contactMessage);
                await _emailService.SendEmailAsync("mohamed.ouni@esprit.tn", contactMessage.Subject, contactMessage.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ContactMessage>> GetAllClaims()
        {
            try
            {
                return await _contactMessage.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task ValidateClaim(string claimId, string validationMessage)
        {
            try
            {
                var filter = Builders<ContactMessage>.Filter.Eq(c => c.Id, claimId);
                var update = Builders<ContactMessage>.Update.Set(c => c.IsProcessed, true).Set(c => c.IsProcessed, true);
                await _contactMessage.UpdateOneAsync(filter, update);
                var claim = await _contactMessage.Find(filter).FirstOrDefaultAsync();
                if (claim != null)
                {
                    await _emailService.SendEmailAsync(claim.Email, "Claim Validation", validationMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

        }
    }
}
