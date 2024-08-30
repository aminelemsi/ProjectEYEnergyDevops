using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using EY.Energy.Infrastructure.Entity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;


namespace EY.Energy.Application.Services.Answers
{
    public class ClientResponseService
    {
        private readonly IMongoCollection<ClientResponse> _responses;
        private readonly IMongoCollection<Question> _questions;
        private readonly GridFSBucket _bucket;
        private readonly IMongoCollection<Form> _form;



        public ClientResponseService(MongoDBContext database)
        {
            _responses = database.ClientResponses;
            _questions = database.Questions;
            _bucket = database.Bucket;
            _form = database.Forms;
        }

        public async Task AddResponseAsync(ClientResponse response)
        {
            try
            {
                await _responses.InsertOneAsync(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public async Task<List<ClientResponse>> GetResponsesByCompanyIdAsync(string companyId)
        {
            try
            {
                var filter = Builders<ClientResponse>.Filter.Eq(response => response.CompanyId, companyId);
                return await _responses.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Question>> GetQuestionsByIdsAsync(List<string> questionIds)
        {
            try
            {
                var filter = Builders<Question>.Filter.In(q => q.QuestionId, questionIds);
                return await _questions.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

        }

/*        public async Task<bool> HasClientRespondedToForm(string customerId, string formId)
        {
            var filter = Builders<ClientResponse>.Filter.And(
                Builders<ClientResponse>.Filter.Eq(response => response.CustomerId, customerId),
                Builders<ClientResponse>.Filter.Eq(response => response.FormId, formId)
            );
            return await _responses.Find(filter).AnyAsync();
        }

        public async Task UpdateResponseFileAsync(string responseId, string newFileId)
        {
            var response = await _responses.Find(r => r.ResponseId == responseId).FirstOrDefaultAsync();
            if (response != null && response.FileId != null)
            {
                response.OldFileIds.Add(response.FileId);
            }

            var filter = Builders<ClientResponse>.Filter.Eq(r => r.ResponseId, responseId);
            var update = Builders<ClientResponse>.Update
                .Set(r => r.FileId, newFileId)
                .Set(r => r.OldFileIds, response!.OldFileIds);

            await _responses.UpdateOneAsync(filter, update);
        }*/


        public async Task<string> GetFileNameByIdAsync(string fileId)
        {
            try
            {
                if (ObjectId.TryParse(fileId, out ObjectId objectId))
                {
                    var filter = Builders<GridFSFileInfo>.Filter.Eq("_id", objectId);
                    var fileInfo = await _bucket.Find(filter).FirstOrDefaultAsync();
                    if (fileInfo != null)
                    {
                        return fileInfo.Filename;
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, List<ClientResponse>>> GetResponsesByCompanyGroupedByFormAsync(string companyId)
        {
            try
            {
                var filter = Builders<ClientResponse>.Filter.Eq(response => response.CompanyId, companyId);
                var responses = await _responses.Find(filter).ToListAsync();

                var formIds = responses.Select(r => r.FormId).Distinct().ToList();
                var forms = await _form.Find(f => formIds.Contains(f.FormId)).ToListAsync();

                var groupedResponses = new Dictionary<string, List<ClientResponse>>();
                foreach (var form in forms)
                {
                    var formResponses = responses.Where(r => r.FormId == form.FormId).ToList();
                    foreach (var response in formResponses)
                    {
                        var question = await _questions.Find(q => q.QuestionId == response.QuestionId).FirstOrDefaultAsync();
                        response.QuestionText = question?.Text;
                    }
                    groupedResponses.Add(form.Title, formResponses);
                }

                return groupedResponses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ClientResponse>> GetResponsesByCompanyIdsAsync(List<string> companyIds)
        {
            try
            {
                var filter = Builders<ClientResponse>.Filter.In(response => response.CompanyId, companyIds);
                var responses = await _responses.Find(filter).ToListAsync();

                var formIds = responses.Select(r => r.FormId).Distinct().ToList();
                var forms = await _form.Find(f => formIds.Contains(f.FormId)).ToListAsync();


                foreach (var form in forms)
                {
                    var formResponses = responses.Where(r => r.FormId == form.FormId).ToList();
                    foreach (var response in formResponses)
                    {
                        var question = await _questions.Find(q => q.QuestionId == response.QuestionId).FirstOrDefaultAsync();
                        response.QuestionText = question?.Text;

                    }
                }

                return responses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ClientResponse>> GetUnfinalizedResponsesAsync(string customerId, string formId)
        {
            try
            {
                var filter = Builders<ClientResponse>.Filter.And(
                    Builders<ClientResponse>.Filter.Eq(response => response.CustomerId, customerId),
                    Builders<ClientResponse>.Filter.Eq(response => response.FormId, formId),
                    Builders<ClientResponse>.Filter.Eq(response => response.IsFinalized, false)
                );
                return await _responses.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> HasClientRespondedToForm(string customerId, string formId)
        {
            try
            {
                var filter = Builders<ClientResponse>.Filter.And(
                    Builders<ClientResponse>.Filter.Eq(response => response.CustomerId, customerId),
                    Builders<ClientResponse>.Filter.Eq(response => response.FormId, formId),
                    Builders<ClientResponse>.Filter.Eq(response => response.IsFinalized, true)
                );
                return await _responses.Find(filter).AnyAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateResponseFileAsync(string responseId, string newFileId)
        {
            try
            {
                var response = await _responses.Find(r => r.ResponseId == responseId).FirstOrDefaultAsync();
                if (response != null && response.FileId != null)
                {
                    response.OldFileIds.Add(response.FileId);
                }

                var filter = Builders<ClientResponse>.Filter.Eq(r => r.ResponseId, responseId);
                var update = Builders<ClientResponse>.Update
                    .Set(r => r.FileId, newFileId)
                    .Set(r => r.OldFileIds, response!.OldFileIds);

                await _responses.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task FinalizeResponsesAsync(string customerId, string formId)
        {
            try
            {
                var filter = Builders<ClientResponse>.Filter.And(
                    Builders<ClientResponse>.Filter.Eq(response => response.CustomerId, customerId),
                    Builders<ClientResponse>.Filter.Eq(response => response.FormId, formId),
                    Builders<ClientResponse>.Filter.Eq(response => response.IsFinalized, false)
                );
                var update = Builders<ClientResponse>.Update.Set(response => response.IsFinalized, true);
                await _responses.UpdateManyAsync(filter, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateResponse(ClientResponse response)
        {
            try
            {
                var filter = Builders<ClientResponse>.Filter.Eq(r => r.ResponseId, response.ResponseId);
                var update = Builders<ClientResponse>.Update
                    .Set(r => r.ResponseText, response.ResponseText)
                    .Set(r => r.OptionId, response.OptionId)
                    .Set(r => r.FileId, response.FileId);

                await _responses.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public async Task<ClientResponse> GetResponseByIdAsync(string responseId)
        {
            try
            {
                return await _responses.Find(r => r.ResponseId == responseId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
