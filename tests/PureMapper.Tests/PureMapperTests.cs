#nullable disable
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
					Knows = m.Resolve<Person, PersonDto>().Invoke(u.Knows),
					Parent = m.Resolve<User, UserDto>().Invoke(u.Parent),
				})
				.Map<Person, PersonDto>(m => p => new PersonDto
				{ 
					Name = p.Name,
				});

        [Fact]
        public void Test1()
        {
			var mapper = new PureMapper(Config);

			var nick = new Person { Name = "npal" };
			var george = new User { Username = "george", Password = "123test!", Knows = nick };
			var alex = new User { Username = "akritikos", Password = "123test!", Knows = nick, Parent = george };

			var dto = mapper.Map<User, UserDto>(alex);
			Assert.Equal(alex.Username.ToUpperInvariant(),dto.NormalizedUsername);
			Assert.Equal(alex.Knows.Name, dto.Knows.Name);
			Assert.Equal(alex.Parent.Username.ToUpperInvariant(), dto.Parent.NormalizedUsername);
		}
    }
}
