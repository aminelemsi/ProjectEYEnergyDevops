using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;


namespace EY.Energy.Infrastructure.Entity
{
    public class Review
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty; // New property

        public int Rating { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow; // Nouveau champ pour la date

    }

}
