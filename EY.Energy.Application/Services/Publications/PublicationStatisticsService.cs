using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using EY.Energy.Infrastructure.Entity;
using MongoDB.Driver;


namespace EY.Energy.Application.Services.Publications
{
    public class PublicationStatisticsService
    {
        private readonly IMongoCollection<Publication> _publications;
        private readonly IMongoCollection<User> _users;

        public PublicationStatisticsService(MongoDBContext context)
        {
            _publications = context.Publications;
            _users = context.Users;
        }

        public async Task<Dictionary<string, int>> GetFileTypeStatisticsAsync()
        {
            try
            {
                var publications = await _publications.Find(_ => true).ToListAsync();
                var fileTypeStats = new Dictionary<string, int>
            {
                { "Images", publications.Sum(p => p.ImageIds.Count) },
                { "Videos", publications.Sum(p => p.VideoIds.Count) },
                { "PDFs", publications.Sum(p => p.PdfIds.Count) },
                { "XLS", publications.Sum(p => p.XlsIds.Count) },
                { "Docs", publications.Sum(p => p.DocIds.Count) }
            };

                return fileTypeStats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PublicationReviewStats>> GetPublicationReviewStatsByFileTypeAsync(string fileType)
        {
            try
            {
                var publications = await _publications.Find(_ => true).ToListAsync();
                var publicationReviewStats = new List<PublicationReviewStats>();

                foreach (var publication in publications)
                {
                    int fileCount = fileType switch
                    {
                        "Images" => publication.ImageIds.Count,
                        "Videos" => publication.VideoIds.Count,
                        "PDFs" => publication.PdfIds.Count,
                        "XLS" => publication.XlsIds.Count,
                        "Docs" => publication.DocIds.Count,
                        _ => 0
                    };

                    if (fileCount > 0)
                    {
                        publicationReviewStats.Add(new PublicationReviewStats
                        {
                            Title = publication.Title,
                            ReviewCount = publication.Reviews.Count
                        });
                    }
                }

                return publicationReviewStats.OrderByDescending(p => p.ReviewCount).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, Publication>> GetBestPublicationsAsync()
        {
            try
            {
                var publications = await _publications.Find(_ => true).ToListAsync();
                var bestPublications = new Dictionary<string, Publication>();

                foreach (var fileType in new[] { "Images", "Videos", "PDFs", "XLS", "Docs" })
                {
                    Publication bestPublication = null!;

                    foreach (var publication in publications)
                    {
                        int fileCount = fileType switch
                        {
                            "Images" => publication.ImageIds.Count,
                            "Videos" => publication.VideoIds.Count,
                            "PDFs" => publication.PdfIds.Count,
                            "XLS" => publication.XlsIds.Count,
                            "Docs" => publication.DocIds.Count,
                            _ => 0
                        };

                        if (fileCount > 0)
                        {
                            if (bestPublication == null || publication.Reviews.Count > bestPublication.Reviews.Count)
                            {
                                bestPublication = publication;
                            }
                        }
                    }

                    if (bestPublication != null)
                    {
                        bestPublications[fileType] = bestPublication;
                    }
                }

                return bestPublications;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetBestCustomersAsync()
        {
            try
            {
                var publications = await _publications.Find(_ => true).ToListAsync();
                var bestCustomers = new Dictionary<string, string>();
                var userIds = new HashSet<string>();

                foreach (var fileType in new[] { "Images", "Videos", "PDFs", "XLS", "Docs" })
                {
                    string bestCustomer = null!;
                    int maxReviews = 0;

                    foreach (var publication in publications)
                    {
                        int fileCount = fileType switch
                        {
                            "Images" => publication.ImageIds.Count,
                            "Videos" => publication.VideoIds.Count,
                            "PDFs" => publication.PdfIds.Count,
                            "XLS" => publication.XlsIds.Count,
                            "Docs" => publication.DocIds.Count,
                            _ => 0
                        };

                        if (fileCount > 0)
                        {
                            var customerReviews = publication.Reviews.GroupBy(r => r.UserId)
                                                                     .Select(g => new { CustomerId = g.Key, ReviewCount = g.Count() })
                                                                     .OrderByDescending(x => x.ReviewCount)
                                                                     .FirstOrDefault();

                            if (customerReviews != null && customerReviews.ReviewCount > maxReviews)
                            {
                                maxReviews = customerReviews.ReviewCount;
                                bestCustomer = customerReviews.CustomerId; // Assuming UserId is customer identifier
                                userIds.Add(bestCustomer);
                            }
                        }
                    }

                    if (bestCustomer != null)
                    {
                        bestCustomers[fileType] = bestCustomer;
                    }
                }

                var usernames = await GetUsernamesAsync(userIds);
                return bestCustomers.ToDictionary(kvp => kvp.Key, kvp => usernames[kvp.Value]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

        }


        public async Task<Dictionary<string, string>> GetUsernamesAsync(IEnumerable<string> userIds)
        {
            try
            {
                var filter = Builders<User>.Filter.In(u => u.Id, userIds);
                var users = await _users.Find(filter).ToListAsync();
                return users.ToDictionary(u => u.Id, u => u.Username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public class PublicationReviewStats
        {
            public string Title { get; set; } = string.Empty;
            public int ReviewCount { get; set; }
        }
    }
}