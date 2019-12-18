namespace Kritikos.PureMapper.Tests
{
	using System;
	using System.Text;
	using System.Text.Unicode;
	using Kritikos.PureMapper.Contracts;
	using Kritikos.PureMapper.Tests.Arrange;

	using Nessos.Expressions.Splicer;

	using Xunit;

	public class PureMapperTests
	{
		private static IPureMapperConfig Config
			= new PureMapperConfig()
				.Map<User, UserDto>(m => u => new UserDto
				{
					NormalizedUsername = u.Username.ToUpperInvariant(),
					HashedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(u.Password)),
				});

        [Fact]
        public void Test1()
        {
			var mapper = new PureMapper(Config);

			var nick = new User { Username = "npal", Password = "test123!" };
			var alex = new User { Username = "akritikos", Password = "123test!", Knows = nick };

			var dto = mapper.Map<User,UserDto>(alex);
			Assert.Equal(alex.Username.ToUpperInvariant(),dto.NormalizedUsername);
		}
    }
}
