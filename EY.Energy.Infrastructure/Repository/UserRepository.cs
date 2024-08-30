using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;     
using System.Threading.Tasks;          

namespace EY.Energy.Infrastructure.Repository
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _collection;

        public UserRepository(MongoDBContext dbContext)
        {
            _collection = dbContext.Users;
            var indexKeysDefinition = Builders<User>.IndexKeys.Combine(
                Builders<User>.IndexKeys.Ascending(x => x.Username),
                Builders<User>.IndexKeys.Ascending(x => x.Email)
            );
            var indexOptions = new CreateIndexOptions { Unique = true };
            var model = new CreateIndexModel<User>(indexKeysDefinition, indexOptions);
            _collection.Indexes.CreateOne(model);

        }
    }
}
