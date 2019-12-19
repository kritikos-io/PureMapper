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
					Knows = m.ResolveExpr<Person, PersonDto>().Invoke(u.Knows),
					//Parent = m.ResolveExpr<User, UserDto>().Invoke(u.Parent),
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
			var alex = new User { Username = "akritikos", Password = "123test!", Knows = nick };

			var dto = mapper.Map<User,UserDto>(alex);
			Assert.Equal(alex.Username.ToUpperInvariant(),dto.NormalizedUsername);
			Assert.Equal(alex.Knows.Name, dto.Knows.Name);
		}
    }
}
