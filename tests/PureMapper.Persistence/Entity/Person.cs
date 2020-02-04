namespace Kritikos.PureMap.Benchmarks.Entity
{
	using System.ComponentModel.DataAnnotations.Schema;

	public class Person
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }

		public string FirstName { get; set; } = string.Empty;

		public string LastName { get; set; } = string.Empty;

		public Person? Parent { get; set; }
	}
}
