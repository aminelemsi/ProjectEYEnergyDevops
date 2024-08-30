using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using MongoDB.Driver;


namespace EY.Energy.Application.Services.Users
{
    public class CustomerServices
    {
        private readonly IMongoCollection<User> _users;
        public CustomerServices(MongoDBContext context)
        {
            _users = context.Users;
        }
        public async Task<List<User>> GetNonValidatedCustomers()
        {
            try
            {
                return await _users.Find(u => u.role == Role.Customer && u.IsValid == false).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        public async Task<List<User>> GetValidatedCustomers()
        {
            try
            {
                return await _users.Find(u => u.role == Role.Customer && u.IsValid == true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
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

        public async Task AssignCustomerToConsultant(string customerUsername, string consultantUsername)
        {
            try
            {
                if (string.IsNullOrEmpty(customerUsername) || string.IsNullOrEmpty(consultantUsername))
                {
                    throw new ArgumentException("Customer username and Consultant username must be provided.");
                }

                var customer = await _users.Find(u => u.Username == customerUsername && u.role == Role.Customer).FirstOrDefaultAsync();
                if (customer == null)
                {
                    throw new KeyNotFoundException("Customer not found.");
                }

                var consultant = await _users.Find(u => u.Username == consultantUsername && u.role == Role.Consultant).FirstOrDefaultAsync();
                if (consultant == null)
                {
                    throw new KeyNotFoundException("Consultant not found or is not a valid consultant.");
                }

                customer.Id = consultant.Id;
                await UpdateUser(customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<User>> GetCustomersForConsultant(string usernameConsultant)
        {
            try
            {
                var customers = await _users.Find(u => u.Username == usernameConsultant && u.role == Role.Customer).ToListAsync();
                return customers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
