using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Entity
{
    public class Invoice
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Type_energy { get; set; } = string.Empty;
        public DateTime date { get; set; }
        public float amount { get; set; } 

    }

}
