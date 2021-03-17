namespace Kritikos.PureMap.Benchmarks
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Threading.Tasks;

	using AutoMapper.QueryableExtensions;

	using BenchmarkDotNet.Columns;
	using BenchmarkDotNet.Configs;
	using BenchmarkDotNet.Exporters.Csv;
	using BenchmarkDotNet.Horology;
	using BenchmarkDotNet.Jobs;
	using BenchmarkDotNet.Reports;
	using BenchmarkDotNet.Running;
	using BenchmarkDotNet.Toolchains.InProcess.Emit;

	using Kritikos.PureMap.Benchmarks.Entity;
	using Kritikos.PureMap.Benchmarks.Model;
	using Kritikos.PureMapper.Persistence;

	using Microsoft.EntityFrameworkCore;
	using Microsoft.EntityFrameworkCore.Diagnostics;
	using Microsoft.Extensions.Logging;

	using Serilog;
	using Serilog.Events;
	using Serilog.Exceptions;
	using Serilog.Exceptions.Core;
	using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
	using Serilog.Extensions.Logging;
	using Serilog.Sinks.SystemConsole.Themes;

	public sealed class Program
	{
		private static ILoggerFactory? loggerFactory;

#pragma warning disable 1998
		public static async Task Main()
#pragma warning restore 1998
		{
			var logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.MinimumLevel.Override("System", LogEventLevel.Error)
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
				.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
					.WithDefaultDestructurers()
					.WithRootName("Exception")
					.WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }))
				.WriteTo.Console(theme: AnsiConsoleTheme.Code);

			Log.Logger = logger.CreateLogger();
			loggerFactory = new SerilogLoggerFactory(Log.Logger);

			var config = ManualConfig.Create(DefaultConfig.Instance);
			config.Add(new CsvExporter(
				CsvSeparator.CurrentCulture,
				new SummaryStyle(true, SizeUnit.KB, TimeUnit.Millisecond, false, false, 20)));
			config.Add(Job.ShortRun.With(InProcessEmitToolchain.Instance));

			_ = BenchmarkRunner.Run<PureBenchmark>(config);

			/*
			try
			{
				await DatabaseTest();
			}
			catch (Exception e)
			{
				Log.Fatal(e, "ERROR");
				throw;
			}
			*/
		}

		[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Test method")]
		[SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Test method")]
		private static async Task DatabaseTest()
		{
			var bench = new PureBenchmark();
			bench.Setup();
			var db = Environment.GetEnvironmentVariable("PGSQL_DOTNET");
			var postgres = DefaultContextBuilder()
				.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=Mapper");
#pragma warning disable CA2000 // await using takes care of it
			await using var ctx = new MapperTestsContext(postgres.Options);
#pragma warning restore CA2000 // Dispose objects before losing scope

			var pureMapper = await ctx.People
				.OrderByDescending(x => x.Id)
				.Project<Person, PersonDto>(bench.PureMapper)
				.FirstOrDefaultAsync();

			var autoMapper = await ctx.People
				.Include(x => x.Parent)
				.OrderByDescending(x => x.Id)
				.ProjectTo<PersonDto>(bench.AutoMapperConfiguration)
				.FirstOrDefaultAsync();
		}

		private static DbContextOptionsBuilder<MapperTestsContext> DefaultContextBuilder() =>
			new DbContextOptionsBuilder<MapperTestsContext>()
				.EnableDetailedErrors()
				.EnableSensitiveDataLogging()
				.UseLoggerFactory(loggerFactory)
				.ConfigureWarnings(o => o
					.Log((RelationalEventId.CommandExecuting, LogLevel.Information))
					.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning));
	}
}
