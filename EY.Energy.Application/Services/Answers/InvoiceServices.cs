using EY.Energy.Entity;
using EY.Energy.Infrastructure.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.Energy.Application.Services.Answers
{
    public class InvoiceServices
    {
        private readonly IMongoCollection<Invoice> _invoiceCollection;

        public InvoiceServices(MongoDBContext dbContextData)
        {
            _invoiceCollection = dbContextData.Invoices;
        }
        public void AddInvoice(Invoice invoice)
        {
            _invoiceCollection.InsertOne(invoice);
        }
    }
}
