using EY.Energy.Entity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Infrastructure.Configuration.Validation
{
    public class ValidationServices
    {
        private readonly IMongoCollection<User> _users;

        public ValidationServices(MongoDBContext context)
        {
            _users = context.Users;
        }
        public async Task<bool> IsEmailUnique(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            return user == null;
        }

        public async Task<bool> IsUsernameUnique(string username)
        {
            var user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
            return user == null;
        }
        public async Task<bool> IsPhoneUnique(string phone)
        {
            var user = await _users.Find(u => u.Phone == phone).FirstOrDefaultAsync();
            return user == null;
        }

    }
}
