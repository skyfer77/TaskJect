using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Data.DbContexts
{
	public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext(string[] args)
		{
			var currentDir = Directory.GetCurrentDirectory();

			var projectPath = Path.Combine(currentDir, "../TaskJect.Web");

			var configuration = new ConfigurationBuilder()
				.SetBasePath(projectPath)
				.AddJsonFile("appsettings.Development.json", optional: false)
				.Build();

			var connectionString = configuration.GetConnectionString("DefaultConnection");

			var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
			optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("Data"));

			// Для міграцій dispatcher не потрібен
			return new ApplicationDbContext(optionsBuilder.Options, null!);
		}
	}
}

