using EY.Energy.Infrastructure.Configuration;
using EY.Energy.Infrastructure.Entity;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using MongoDB.Driver;


namespace EY.Energy.Application.Services.Publications
{
    public class PublicationService
    {

        private readonly IMongoCollection<Publication> _publications;
        private readonly GridFSBucket _bucket;

        public PublicationService(MongoDBContext context)
        {
            _publications = context.Publications;
            _bucket = context.Bucket;
        }

        public async Task<Publication> CreatePublicationAsync(Publication publication, IFormFileCollection files)
        {
            try
            {
                foreach (var file in files)
                {
                    var uploadOptions = new GridFSUploadOptions
                    {
                        Metadata = new BsonDocument
                {
                    { "fileName", file.FileName },
                    { "contentType", file.ContentType }
                }
                    };

                    using var stream = file.OpenReadStream();
                    var fileId = await _bucket.UploadFromStreamAsync(file.FileName, stream, uploadOptions);

                    if (file.ContentType.StartsWith("image/"))
                    {
                        publication.ImageIds.Add(fileId.ToString());
                    }
                    else if (file.ContentType.StartsWith("video/"))
                    {
                        publication.VideoIds.Add(fileId.ToString());
                    }
                    else if (file.ContentType == "application/pdf")
                    {
                        publication.PdfIds.Add(fileId.ToString());
                    }
                    else if (file.ContentType == "application/vnd.ms-excel" || file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        publication.XlsIds.Add(fileId.ToString());
                    }

                    else if (file.ContentType == "application/msword" || file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                    {
                        publication.DocIds.Add(fileId.ToString());
                    }
                }

                await _publications.InsertOneAsync(publication);
                return publication;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Publication>> GetAllPublicationsAsync()
        {
            try
            {
                return await _publications.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Publication> GetPublicationByIdAsync(string id)
        {
            try
            {
                return await _publications.Find(p => p.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Stream> GetFileStreamByIdAsync(string id)
        {
            try
            {
                return await _bucket.OpenDownloadStreamAsync(new ObjectId(id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<string?> GetFileNameByIdAsync(string id)
        {
            try
            {
                var fileInfo = await _bucket.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", new ObjectId(id))).FirstOrDefaultAsync();
                return fileInfo?.Filename;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public async Task<bool> UpdatePublicationAsync(string id, Publication updatedPublication, IFormFileCollection? files)
        {
            try
            {
                var existingPublication = await _publications.Find(p => p.Id == id).FirstOrDefaultAsync();
                if (existingPublication == null)
                {
                    return false;
                }

                if (files != null && files.Count > 0)
                {
                    updatedPublication.ImageIds = new List<string>();
                    updatedPublication.VideoIds = new List<string>();
                    updatedPublication.PdfIds = new List<string>();
                    updatedPublication.XlsIds = new List<string>();
                    updatedPublication.DocIds = new List<string>();

                    foreach (var file in files)
                    {
                        var uploadOptions = new GridFSUploadOptions
                        {
                            Metadata = new BsonDocument
                {
                    { "fileName", file.FileName },
                    { "contentType", file.ContentType }
                }
                        };

                        using var stream = file.OpenReadStream();
                        var fileId = await _bucket.UploadFromStreamAsync(file.FileName, stream, uploadOptions);

                        if (file.ContentType.StartsWith("image/"))
                        {
                            updatedPublication.ImageIds.Add(fileId.ToString());
                        }
                        else if (file.ContentType.StartsWith("video/"))
                        {
                            updatedPublication.VideoIds.Add(fileId.ToString());
                        }
                        else if (file.ContentType == "application/pdf")
                        {
                            updatedPublication.PdfIds.Add(fileId.ToString());
                        }
                        else if (file.ContentType == "application/vnd.ms-excel" || file.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                        {
                            updatedPublication.XlsIds.Add(fileId.ToString());
                        }
                        else if (file.ContentType == "application/msword" || file.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                        {
                            updatedPublication.DocIds.Add(fileId.ToString());
                        }
                    }
                }
                else
                {
                    updatedPublication.ImageIds = existingPublication.ImageIds;
                    updatedPublication.VideoIds = existingPublication.VideoIds;
                    updatedPublication.PdfIds = existingPublication.PdfIds;
                    updatedPublication.XlsIds = existingPublication.XlsIds;
                    updatedPublication.DocIds = existingPublication.DocIds;
                }

                updatedPublication.Id = id;
                var result = await _publications.ReplaceOneAsync(p => p.Id == id, updatedPublication);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeletePublicationAsync(string id)
        {
            try
            {
                var result = await _publications.DeleteOneAsync(p => p.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

    }
}
