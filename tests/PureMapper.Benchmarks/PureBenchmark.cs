#nullable disable
namespace Kritikos.PureMap.Benchmarks
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
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
		public int NumberOfIterations { get; set; } = 50;

		internal Person Person { get; set; }

		internal PersonDto PersonDto { get; set; }

		internal PureMapper PureMapper { get; set; }

		internal IMapper AutoMapper { get; set; }

		internal IConfigurationProvider AutoMapperConfiguration { get; set; }

		public static PersonDto UpdatePerson(string firstName, string lastName, PersonDto parent, PersonDto destination)
		{
			if (destination == null)
			{
				return null;
			}

			destination.Parent = parent;
			destination.FirstName = firstName;
			destination.LastName = lastName;

			return destination;
		}

		[GlobalSetup]
		public void Setup()
		{
			var people = new List<Person>();
			using var file = new StreamReader("RandomNames.txt");
			string line;
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
					25)
				.Map<Person, PersonDto>(
					m => (source, dest) => UpdatePerson(
						source.FirstName,
						source.LastName,
						m.Resolve<Person, PersonDto>().Invoke(source.Parent, dest.Parent),
						dest),
					25);

			PureMapper = new PureMapper(cfg);

			PersonDto = PureMapper.Map<Person, PersonDto>(Person);

			AutoMapperConfiguration = new MapperConfiguration(cfg => cfg
				.CreateMap<Person, PersonDto>()
				.ForMember(x => x.FirstName, o => o.MapFrom(x => x.FirstName))
				.ForMember(x => x.LastName, o => o.MapFrom(x => x.LastName))
				.ForMember(x => x.Parent, o => o.MapFrom(x => x.Parent))
				.ForAllOtherMembers(o => o.Ignore()));

			AutoMapper = AutoMapperConfiguration.CreateMapper();
		}

		/*
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

		[Benchmark]
		public PersonDto PureUpdate()
		{
			for (var i = 0; i < NumberOfIterations; i++)
			{
				var dto = PureMapper.Map(Person, PersonDto);
			}

			return PureMapper.Map<Person, PersonDto>(Person);
		}

		[Benchmark]
		public PersonDto AutoUpdate()
		{
			for (var i = 0; i < NumberOfIterations; i++)
			{
				AutoMapper.Map(Person, PersonDto);
			}

			return AutoMapper.Map<Person, PersonDto>(Person);
		}
		*/

		[Benchmark]
		[ArgumentsSource(nameof(RecursionDepth))]
		public PersonDto UpdateRecursion(int recInlineDepth)
		{
			var cfg = new PureMapperConfig()
				.Map<Person, PersonDto>(
					m => p => new PersonDto
					{
						FirstName = p.FirstName,
						LastName = p.LastName,
						Parent = m.Resolve<Person, PersonDto>().Invoke(p.Parent),
					},
					recInlineDepth)
				.Map<Person, PersonDto>(
					m => (source, dest) => UpdatePerson(
						source.FirstName,
						source.LastName,
						m.Resolve<Person, PersonDto>().Invoke(source.Parent, dest.Parent),
						dest),
					recInlineDepth);

			var mapper = new PureMapper(cfg);

			PersonDto dto = null;

			for (var i = 0; i < NumberOfIterations; i++)
			{
				dto = PureMapper.Map(Person, PersonDto);
			}

			return dto;
		}

		[Benchmark]
		[ArgumentsSource(nameof(RecursionDepth))]
		public PersonDto MapRecursion(int recInlineDepth)
		{
			var cfg = new PureMapperConfig()
				.Map<Person, PersonDto>(
					m => p => new PersonDto
					{
						FirstName = p.FirstName,
						LastName = p.LastName,
						Parent = m.Resolve<Person, PersonDto>().Invoke(p.Parent),
					},
					recInlineDepth)
				.Map<Person, PersonDto>(
					m => (source, dest) => UpdatePerson(
						source.FirstName,
						source.LastName,
						m.Resolve<Person, PersonDto>().Invoke(source.Parent, dest.Parent),
						dest),
					recInlineDepth);

			var mapper = new PureMapper(cfg);

			PersonDto dto = null;

			for (var i = 0; i < NumberOfIterations; i++)
			{
				dto = PureMapper.Map<Person, PersonDto>(Person);
			}

			return dto;
		}

		[SuppressMessage(
			"StyleCop.CSharp.OrderingRules",
			"SA1201:Elements should appear in the correct order",
			Justification = "DotnetBenchmark requirement")]
		[SuppressMessage(
			"Performance",
			"CA1819:Properties should not return arrays",
			Justification = "DotnetBenchmark requirement")]
		public int[] RecursionDepth { get; }
			= Enumerable.Range(0, 100).ToArray();
	}
}
