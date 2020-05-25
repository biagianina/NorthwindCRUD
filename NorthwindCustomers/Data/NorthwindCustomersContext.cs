using Microsoft.EntityFrameworkCore;
using NorthwindCustomers.Models;

namespace NorthwindCustomers.Data
{
    public class NorthwindCustomersContext : DbContext
    {
        public NorthwindCustomersContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
    }
}
