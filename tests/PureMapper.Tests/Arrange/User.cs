namespace Kritikos.PureMapper.Tests.Arrange
{
	public class User
	{
		public string Username { get; set; } = string.Empty;

		public string Password { get; set; } = string.Empty;

		public Person Knows { get; set; }

		public User Parent { get; set; }
	}

	public class Person
	{
		public string Name { get; set; }
	}

	public class PersonDto
	{
		public string Name { get; set; }
	}

	public class UserDto
	{
		public string NormalizedUsername { get; set; } = string.Empty;

		public string HashedPassword { get; set; } = string.Empty;

		public PersonDto Knows { get; set; }
		public UserDto Parent { get; set; }
	}
}
