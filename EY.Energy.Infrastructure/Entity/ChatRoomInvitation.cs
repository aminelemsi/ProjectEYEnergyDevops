using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace EY.Energy.Infrastructure.Entity
{
    public class ChatRoomInvitation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string RoomName { get; set; } = string.Empty;
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public List<string> Users { get; set; } = new List<string>();
    }
    public class ChatMessage
    {
        public string User { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
    public class UserStatus
    {
        public string Username { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
    }
}
