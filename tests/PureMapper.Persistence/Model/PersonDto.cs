namespace Kritikos.PureMap.Benchmarks.Model
{
	public class PersonDto
	{
		public long Id { get; set; }

		public string FirstName { get; set; } = string.Empty;

		public string LastName { get; set; } = string.Empty;

		public PersonDto? Parent { get; set; }
	}
}
