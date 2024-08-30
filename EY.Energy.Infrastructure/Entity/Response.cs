using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EY.Energy.Entity
{
    public class ClientResponse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ResponseId { get; set; } = ObjectId.GenerateNewId().ToString();
        public string FormId { get; set; } = string.Empty;
        public string QuestionId { get; set; } = string.Empty;
        public string? OptionId { get; set; } = null;
        public string ResponseText { get; set; } = string.Empty;
        public string? FileId { get; set; } = null;
        public List<string> OldFileIds { get; set; } = new List<string>();
        public string CustomerId { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty; 
        public string? QuestionText { get; set; } = string.Empty;
        public bool IsFinalized { get; set; } = false;
    }
}
