using EY.Energy.Application.Services.Users;
using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using EY.Energy.Infrastructure.Entity;
using MongoDB.Driver;
using ZstdSharp.Unsafe;

namespace EY.Energy.Application.Services.Answers
{
    public class CompanyServices
    {
        private readonly IMongoCollection<Company> _companyCollection;
        private readonly IMongoCollection<User> _customers;
        public CompanyServices(MongoDBContext dbContextData)
        {
            _companyCollection = dbContextData.Companies;
            _customers = dbContextData.Users;
        }
        public async Task AddCompany(Company company)
        {
            try
            {
                await _companyCollection.InsertOneAsync(company);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        public List<Company> GetCompanyList()
        {
            try
            {
                return _companyCollection.Find(company => true).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task AssignConsultantToCompany(string companyId, string consultantId)
        {
            try
            {
                var filter = Builders<Company>.Filter.Eq(c => c.Id, companyId);
                var update = Builders<Company>.Update.Push(c => c.ConsultantIds, consultantId);
                await _companyCollection.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> IsConsultantAssignedToCompany(string companyId, string consultantId)
        {
            try
            {
                var company = await _companyCollection.Find(c => c.Id == companyId).FirstOrDefaultAsync();
                return company?.ConsultantIds?.Contains(consultantId) ?? false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Company?> GetCompanyByCustomerIdAsync(string customerId)
        {
            try
            {
                var filter = Builders<Company>.Filter.Eq(c => c.CustomerId, customerId);
                return await _companyCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

    
        public async Task AddAnswerIdToCompanyAsync(string companyId, string answerId)
        {
            try
            {
                var filter = Builders<Company>.Filter.Eq(c => c.Id, companyId);
                var update = Builders<Company>.Update.AddToSet(c => c.AnswerIds, answerId);
                await _companyCollection.UpdateOneAsync(filter, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetCustomerNamesByIdsAsync(List<string> customerIds)
        {
            try
            {
                var filter = Builders<User>.Filter.In(c => c.Id, customerIds);
                var customers = await _customers.Find(filter).ToListAsync();

                return customers.ToDictionary(c => c.Id, c => c.Username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Company>> GetCompaniesByConsultantIdAsync(string consultantId)
        {
            try
            {
                var filter = Builders<Company>.Filter.AnyEq(c => c.ConsultantIds, consultantId);
                var companies = await _companyCollection.Find(filter).ToListAsync();

                var customerIds = companies.Select(c => c.CustomerId).Distinct().ToList();
                var customerNames = await GetCustomerNamesByIdsAsync(customerIds);

                foreach (var company in companies)
                {
                    if (customerNames.TryGetValue(company.CustomerId, out var customerName))
                    {
                        company.CustomerName = customerName;
                    }
                }

                return companies;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
