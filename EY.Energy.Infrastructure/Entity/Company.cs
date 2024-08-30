using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace EY.Energy.Entity
{
    public class Company
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string Name { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public List<string>? AnswerIds { get; set; }
        public List<string>? ConsultantIds { get; set; }

        [BsonIgnore]
        public string CustomerName { get; set; } = string.Empty;
    }

}

