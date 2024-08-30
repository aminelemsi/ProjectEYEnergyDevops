using EY.Energy.Entity;
using EY.Energy.Infrastructure.Entity;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace EY.Energy.Infrastructure.Configuration
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;
        private readonly GridFSBucket _bucket;
        public MongoDBContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            _bucket = new GridFSBucket(_database);
        }
        public IMongoCollection<Form> Forms => _database.GetCollection<Form>("Forms");
        public IMongoCollection<Company> Companies => _database.GetCollection<Company>("Companies");
        public IMongoCollection<Invoice> Invoices => _database.GetCollection<Invoice>("Invoices");
        public IMongoCollection<Option> Options => _database.GetCollection<Option>("Options");
        public IMongoCollection<ContactMessage> ContactMessages => _database.GetCollection<ContactMessage>("ContactMessages");
        public IMongoCollection<Question> Questions => _database.GetCollection<Question>("Questions");
        public IMongoCollection<ClientResponse> ClientResponses => _database.GetCollection<ClientResponse>("ClientResponses");
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<ChatRoomInvitation> Invitations => _database.GetCollection<ChatRoomInvitation>("Invitations");
        public IMongoCollection<Publication> Publications => _database.GetCollection<Publication>("Publications");
        public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("Reviews");
        public GridFSBucket Bucket => _bucket;
    }
}
