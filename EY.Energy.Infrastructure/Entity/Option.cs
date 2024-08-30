using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace EY.Energy.Entity
{
        public class Option
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string OptionId { get; set; } = ObjectId.GenerateNewId().ToString();
            public string Text { get; set; } = string.Empty;
            public List<Question>? SubQuestions { get; set; } = new List<Question>();
        }

}
