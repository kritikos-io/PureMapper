#nullable disable
namespace Kritikos.PureMap.Tests
{
	using System;
	using System.Linq;
	using System.Text;

	using Kritikos.PureMap;
	using Kritikos.PureMap.Contracts;
	using Kritikos.PureMap.Tests.Arrange;

	using Nessos.Expressions.Splicer;

	using Xunit;

	public class PureMapperTests
	{
		private static IPureMapperConfig Config(int recInlineDepth = 0)
			=> new PureMapperConfig()
				.Map<User, UserDto>(m => u => new UserDto
				{
					NormalizedUsername = u.Username.ToUpperInvariant(),
					HashedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(u.Password)),
					Knows = m.Resolve<Person, PersonDto>().Invoke(u.Knows),
					Parent = m.Resolve<User, UserDto>().Invoke(u.Parent),
				}, recInlineDepth)
				.Map<Person, PersonDto>(m => p => new PersonDto
				{ 
					Name = p.Name,
				}, recInlineDepth);

        [Fact]
        public void TestRecMapping()
        {
			var mapper = new PureMapper(Config());

			var nick = new Person { Name = "npal" };
			var george = new User { Username = "george", Password = "123test!", Knows = nick };
			var alex = new User { Username = "akritikos", Password = "123test!", Knows = nick, Parent = george };

			var dto = mapper.Map<User, UserDto>(alex);
			Assert.Equal(alex.Username.ToUpperInvariant(),dto.NormalizedUsername);
			Assert.Equal(alex.Knows.Name, dto.Knows.Name);
			Assert.Equal(alex.Parent.Username.ToUpperInvariant(), dto.Parent.NormalizedUsername);
		}

		[Fact]
		public void TestRecMappingWithDepth()
		{
			var mapper = new PureMapper(Config(2));

			var nick = new Person { Name = "npal" };
			var john = new User { Username = "john", Password = "123test!", Knows = nick };
			var george = new User { Username = "george", Password = "123test!", Knows = nick, Parent = john };
			var alex = new User { Username = "akritikos", Password = "123test!", Knows = nick, Parent = george };

			var dto = mapper.Map<User, UserDto>(alex);
			Assert.Equal(alex.Username.ToUpperInvariant(), dto.NormalizedUsername);
			Assert.Equal(alex.Knows.Name, dto.Knows.Name);
			Assert.Equal(alex.Parent.Username.ToUpperInvariant(), dto.Parent.NormalizedUsername);
			Assert.Equal(alex.Parent.Parent.Username.ToUpperInvariant(), dto.Parent.Parent.NormalizedUsername);
		}

		[Fact]
		public void TestProject()
		{
			var mapper = new PureMapper(Config(2));

			var nick = new Person { Name = "npal" };
			var john = new User { Username = "john", Password = "123test!", Knows = nick };
			var george = new User { Username = "george", Password = "123test!", Knows = nick, Parent = john };
			var alex = new User { Username = "akritikos", Password = "123test!", Knows = nick, Parent = george };

			var users = new[] { alex, george, john };

			var dtoUsers = users.AsQueryable().Project<User, UserDto>(mapper).ToArray();
			Assert.Equal(users.Length, dtoUsers.Length);
		}
	}
}
