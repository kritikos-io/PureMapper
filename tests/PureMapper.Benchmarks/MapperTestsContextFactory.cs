namespace Kritikos.PureMap.Benchmarks
{
	using Kritikos.PureMapper.Persistence;

	using Microsoft.EntityFrameworkCore;
	using Microsoft.EntityFrameworkCore.Design;
	using Microsoft.EntityFrameworkCore.Diagnostics;
	using Microsoft.Extensions.Logging;

	public class MapperTestsContextFactory : IDesignTimeDbContextFactory<MapperTestsContext>
	{
		public MapperTestsContext CreateDbContext(string[] args)
		{
			var options = new DbContextOptionsBuilder<MapperTestsContext>()
				.EnableDetailedErrors()
				.EnableSensitiveDataLogging()
				.ConfigureWarnings(o => o
					.Log((RelationalEventId.CommandExecuting, LogLevel.Information))
					.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning))

				// .UseNpgsql(Environment.GetEnvironmentVariable("PGSQL_DOTNET"));
				.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=Mapper");

			return new MapperTestsContext(options.Options);
		}
	}
}
