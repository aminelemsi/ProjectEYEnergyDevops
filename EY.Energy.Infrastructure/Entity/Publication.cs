using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Infrastructure.Entity
{
    public class Publication
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> ImageIds { get; set; } = new List<string>();
        public List<string> VideoIds { get; set; } = new List<string>();
        public List<string> PdfIds { get; set; } = new List<string>();
        public List<string> XlsIds { get; set; } = new List<string>();
        public List<string> DocIds { get; set; } = new List<string>();
        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}
