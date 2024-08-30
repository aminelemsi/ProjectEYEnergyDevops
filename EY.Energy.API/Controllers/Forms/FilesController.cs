using EY.Energy.Application.Services.Answers;
using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace EY.Energy.API.Controllers.Forms
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly GridFSBucket _bucket;

        public FilesController(MongoDBContext dbContext)
        {
            _bucket = dbContext.Bucket; // Use the GridFS bucket from the context
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

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

            return Ok(new { fileId = fileId.ToString() });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetFile(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId fileId))
            {
                return BadRequest("Invalid file ID format");
            }

            try
            {
                var stream = await _bucket.OpenDownloadStreamAsync(fileId);
                return File(stream, stream.FileInfo.Metadata["ContentType"].AsString, stream.FileInfo.Filename);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error retrieving file: {ex.Message}");
                return NotFound($"File not found: {id}");
            }
        }
    }
}