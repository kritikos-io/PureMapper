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
				.Map<Person, PersonDto>(m => p => new PersonDto { Name = p.Name, }, recInlineDepth, string.Empty)
				.Map<Person, PersonDto>(m => p => new PersonDto { Name = p.Name.ToUpperInvariant(), }, 0, "upper")
				.Map<Person, PersonDto>(m => (source, dest) => UpdatePerson(source.Name, dest))
				.Map<User, UserDto>(m => (source, dest) =>
					UpdateUser(source.Username, source.Password,
						m.Resolve<User, UserDto>().Invoke(source.Parent, dest.Parent), dest)
				);

		public static UserDto UpdateUser(string Name, string Pass, UserDto Parent, UserDto destination)
		{
			destination.NormalizedUsername = Name.ToUpperInvariant();
			destination.Parent = Parent;
			destination.HashedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(Pass));
			return destination;
		}

		public static PersonDto UpdatePerson(string Name, PersonDto destination)
		{
			destination.Name = Name.ToUpperInvariant();
			return destination;
		}

		[Fact]
		public void TestRecMapping()
		{
			var mapper = new PureMapper(Config());

			var nick = new Person { Name = "npal" };
			var george = new User { Username = "george", Password = "123test!", Knows = nick };
			var alex = new User { Username = "akritikos", Password = "123test!", Knows = nick, Parent = george };

			var dto = mapper.Map<User, UserDto>(alex);
			Assert.Equal(alex.Username.ToUpperInvariant(), dto.NormalizedUsername);
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
		public void TestNamedMap()
		{
			var mapper = new PureMapper(Config());
			var nick = new Person { Name = "npal" };
			var dtoUnamed = mapper.Map<Person, PersonDto>(nick);
			var dtoUpper = mapper.Map<Person, PersonDto>(nick, "upper");

			Assert.Equal(nick.Name, dtoUnamed.Name);
			Assert.Equal(nick.Name.ToUpperInvariant(), dtoUpper.Name);
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

		[Fact]
		public void TestUpdateValues()
		{
			var mapper = new PureMapper(Config(2));

			var nick = new Person { Name = "npal" };

			var dto = mapper.Map<Person, PersonDto>(nick, "upper");
			Assert.Equal(nick.Name.ToUpperInvariant(), dto.Name);

			nick.Name = "Nikos Palladinos";
			mapper.Map(nick, dto);

			Assert.Equal(nick.Name.ToUpperInvariant(), dto.Name);
		}

		[Fact]
		public void TestUpdateRecursion()
		{
			var mapper = new PureMapper(Config(2));
			var alex = new User { Username = "Alex", Password = "test" };

			var nick = new User { Parent = alex, Username = "Nick", Password = "testing", };

			var nickDto = mapper.Map<User, UserDto>(nick);

			alex.Username = "Alexandros";
			alex.Password = "Testing";

			nick.Username = "Nikos";
			nick.Password = "sample";

			mapper.Map(nick, nickDto);

			Assert.Equal(nickDto.NormalizedUsername,nick.Username.ToUpperInvariant());
			Assert.Equal(nickDto.HashedPassword, Convert.ToBase64String(Encoding.UTF8.GetBytes(nick.Password)));

			Assert.Equal(nickDto.Parent.NormalizedUsername, alex.Username.ToUpperInvariant());
			Assert.Equal(nickDto.Parent.HashedPassword, Convert.ToBase64String(Encoding.UTF8.GetBytes(alex.Password)));
		}
	}
}
