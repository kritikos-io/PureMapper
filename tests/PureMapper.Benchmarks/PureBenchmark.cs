namespace Kritikos.PureMap.Benchmarks
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using AutoMapper;

	using BenchmarkDotNet.Attributes;
	using BenchmarkDotNet.Jobs;

	using Kritikos.PureMap.Benchmarks.Entity;
	using Kritikos.PureMap.Benchmarks.Model;

	using Nessos.Expressions.Splicer;

	[SimpleJob(RuntimeMoniker.NetCoreApp31)]
	public class PureBenchmark
	{
		[Params(1000, 10000)]
		public int NumberOfIterations;

		internal Person Person;

		internal PureMapper PureMapper;

		internal IMapper? AutoMapper;
		internal IConfigurationProvider AutoMapperConfiguration;

		[GlobalSetup]
		public void Setup()
		{
			var people = new List<Person>();
			using var file = new StreamReader("RandomNames.txt");
			string? line;
			while ((line = file.ReadLine()) != null)
			{
				var split = line.Split(' ');
				people.Add(new Person { FirstName = split[0], LastName = split[1], });
			}

			Person = people.First();

			foreach (var person in people)
			{
				var i = people.IndexOf(person) + 1;
				if (i < people.Count)
				{
					person.Parent = people[i];
				}
			}

			var cfg = new PureMapperConfig()
				.Map<Person, PersonDto>(
					m => p => new PersonDto
					{
						FirstName = p.FirstName,
						LastName = p.LastName,
						Parent = m.Resolve<Person, PersonDto>().Invoke(p.Parent),
					},
					100);

			PureMapper = new PureMapper(cfg);

			AutoMapperConfiguration = new MapperConfiguration(cfg => cfg
				.CreateMap<Person, PersonDto>()
				.ForMember(x => x.FirstName, o => o.MapFrom(x => x.FirstName))
				.ForMember(x => x.LastName, o => o.MapFrom(x => x.LastName))
				.ForMember(x => x.Parent, o => o.MapFrom(x => x.Parent))
				.ForAllOtherMembers(o => o.Ignore()));

			AutoMapper = AutoMapperConfiguration.CreateMapper();
		}

		[Benchmark]
		public PersonDto PureMapping()
		{
			for (var i = 0; i < NumberOfIterations; i++)
			{
				PureMapper.Map<Person, PersonDto>(Person);
			}

			return PureMapper.Map<Person, PersonDto>(Person);
		}

		[Benchmark]
		public PersonDto AutoMapping()
		{
			for (var i = 0; i < NumberOfIterations; i++)
			{
				AutoMapper.Map<Person, PersonDto>(Person);
			}

			return AutoMapper.Map<Person, PersonDto>(Person);
		}
	}
}
