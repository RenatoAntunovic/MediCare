using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MediCare.Infrastructure.Database
{
    public class DatabaseContextFactory
        : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();

            optionsBuilder.UseSqlServer(
          "Data Source=DESKTOP-O6GMT7T;" +
          "Initial Catalog=MediCareDb;" +
          "Integrated Security=True;" +
          "TrustServerCertificate=True;" +
          "MultipleActiveResultSets=True"
      );

            return new DatabaseContext(
                optionsBuilder.Options,
                TimeProvider.System
            );
        }
    }
}
