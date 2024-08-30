using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;


namespace EY.Energy.Application.Services.Users
{
    public class UserServices
    {
        private readonly IMongoCollection<User> _users;

        public UserServices(MongoDBContext context)
        {
            _users = context.Users;
        }

        public async Task UpdateUser(User user)
        {
            try
            {
                await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public async Task<List<User>> GetUsersByRole(params Role?[] roles)
        {
            try
            {
                var users = await _users.Find(u => (roles.Contains(u.role) || u.role == null) && u.IsBanned == false).ToListAsync();
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<User>> GetUsersBlock(params Role?[] roles)
        {
            try
            {
                var users = await _users.Find(u => (roles.Contains(u.role) || u.role == null) && u.IsBanned == true).ToListAsync();
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<User> GetUserById(string userId)
        {
            try
            {
                ObjectId objectId;
                if (!ObjectId.TryParse(userId, out objectId))
                {
                    return null!;
                }
                var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var user = await _users.Find(filter).FirstOrDefaultAsync();
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        public async Task<User> GetUserByUsername(string username)
        {
            try
            {
                return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public async Task<List<User>> SearchUsersByUsernameAsync(string username)
        {
            try
            {
                var filterBuilder = Builders<User>.Filter;
                var usernameFilter = filterBuilder.Regex(u => u.Username, new BsonRegularExpression($"^{username}", "i"));
                var roleFilter = filterBuilder.In(u => u.role, new List<Role?> { Role.Manager, Role.Consultant });

                var combinedFilter = filterBuilder.And(usernameFilter, roleFilter);

                return await _users.Find(combinedFilter).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<User>> SearchUsersByRoleAsync(Role? role)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.role, role);
                return await _users.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<User> GetCurrentUserByUsername(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username))
                {
                    throw new ArgumentException("Username cannot be null or empty", nameof(username));
                }

                var currentUser = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();

                if (currentUser == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                return currentUser;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        public async Task<User> GetUserByResetToken(string resetToken)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.ResetPasswordToken, resetToken);
                var user = await _users.Find(filter).FirstOrDefaultAsync();

                if (user != null && user.ResetPasswordTokenExpiration.HasValue && user.ResetPasswordTokenExpiration.Value > DateTime.UtcNow)
                {
                    return user;
                }

                return null!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<User> GetUserByEmail(string email)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Email, email);
                return await _users.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        public async Task<List<User>> GetAllConsultants()
        {
            try
            {
                var consultants = await _users.Find(u => u.role == Role.Consultant && u.IsBanned == false).ToListAsync();
                return consultants;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

    }
}
