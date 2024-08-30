using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using EY.Energy.Infrastructure.Configuration.Validation;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Application.Services.Users
{
    public class AuthenticationServices
    {
        private readonly IMongoCollection<User> _users;

        private readonly ValidationServices validationServices;
        public AuthenticationServices(MongoDBContext context, ValidationServices validationServices)
        {
            _users = context.Users;
            this.validationServices = validationServices;

        }

        public async Task<User> Authenticate(string username)
        {
            try
            {
                var user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool success, string errorMessage)> CreateUser(User user)
        {
            try
            {
                bool isEmailUnique = await validationServices.IsEmailUnique(user.Email);
                if (!isEmailUnique)
                {
                    return (false, "Email already exists.");
                }

                bool isUsernameUnique = await validationServices.IsUsernameUnique(user.Username);
                if (!isUsernameUnique)
                {
                    return (false, "Username already exists.");
                }

                bool isPhoneUnique = await validationServices.IsPhoneUnique(user.Phone!);
                if (!isPhoneUnique)
                {
                    return (false, "Phone number already exists.");
                }

                await _users.InsertOneAsync(user);

                return (true, null!);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create user: {ex.Message}");
            }
        }

    }
}