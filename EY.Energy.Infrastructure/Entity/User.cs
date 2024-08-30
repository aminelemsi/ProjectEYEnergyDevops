using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EY.Energy.Entity
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
       public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public string NameCompany { get; set; } = string.Empty;
        public Role? role { get; set; }
        public bool IsValid { get; set; }
        public bool IsBanned { get; set; }
        public string ResetPasswordToken { get; set; } = string.Empty;
        public DateTime? ResetPasswordTokenExpiration { get; set; }
    }
}
