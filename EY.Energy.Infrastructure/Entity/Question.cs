using EY.Energy.Infrastructure.Entity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace EY.Energy.Entity
{
    public class Question
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string QuestionId { get; set; } = ObjectId.GenerateNewId().ToString();
        public string Text { get; set; } = string.Empty;
        public TypeQuestion TypeQuestion { get; set; }
        public List<Option>? Options { get; set; } = new List<Option>();
        public string? NextQuestionId { get; set; }  // ID de la prochaine question si ce n'est pas une question avec options
        public bool AllowsFileResponse { get; set; } = false; // Add this field

    }

}
