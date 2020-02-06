#nullable disable
namespace Kritikos.PureMapper.Persistence
{
	using Kritikos.PureMap.Benchmarks.Entity;

	using Microsoft.EntityFrameworkCore;

	public class MapperTestsContext : DbContext
	{
		public MapperTestsContext(DbContextOptions<MapperTestsContext> options)
			: base(options)
		{
		}

		public DbSet<Person> People { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("Test");
			base.OnModelCreating(modelBuilder);
		}
	}
}
