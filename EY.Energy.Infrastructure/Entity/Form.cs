using EY.Energy.Entity;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Infrastructure.Entity
{
    public class Form
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string FormId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        public List<Question> Questions { get; set; } = new List<Question>();
    }

}
