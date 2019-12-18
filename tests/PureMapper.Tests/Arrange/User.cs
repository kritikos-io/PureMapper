namespace Kritikos.PureMapper.Tests.Arrange
{
	public class User
	{
		public string Username { get; set; } = string.Empty;

		public string Password { get; set; } = string.Empty;

		public User? Knows { get; set; }
	}

	public class UserDto
	{
		public string NormalizedUsername { get; set; } = string.Empty;

		public string HashedPassword { get; set; } = string.Empty;

		public UserDto? Knows { get; set; }
	}
}
