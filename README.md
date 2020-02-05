# PureMapper

A simple object to object mapper, based on the awesome work done in [AutoMapper][autoMapper]. This is a rather opinionated derrivative, foregoing the usage of reflection and implicit conventions in favor of verbose expression trees. This allows direct usage by anything that can handle expression trees, from in memory representations to Entity Framework (including projections).

## Usage

Firstly create a new ```IPureMapperConfig``` with all the required mappings:

```csharp
var map = new PureMapperConfig()
  .Map<User, UserDto>(m => u => new UserDto
  {
    NormalizedUsername = u.Username.ToUpperInvariant(),
    HashedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(u.Password)),
    Knows = m.Resolve<Person, PersonDto>().Invoke(u.Knows),
    Parent = m.Resolve<User, UserDto>().Invoke(u.Parent),
  }, recInlineDepth)
  .Map<Person, PersonDto>(m => p => new PersonDto { Name = p.Name, }, recInlineDepth, string.Empty)
  .Map<Person, PersonDto>(m => p => new PersonDto { Name = p.Name.ToUpperInvariant(), }, 0, "upper");
```

Construct a new ```IPureMapper``` using that configuration:

```csharp
var mapper = new PureMapper(map);
```

Then, simply invoke the Map function on the objects needed:

```csharp
var dto = mapper.Map<User, UserDto>(user);
```

## Advanced Features

### Updating Existing Objects

```csharp
var cfg = new PureMapperConfig()
  .Map<Person, PersonDto>(m => (source, dest) => UpdatePerson(source.Name, dest));

  [...]

private static PersonDto UpdatePerson(string Name, PersonDto destination)
  {
    destination.Name = Name.ToUpperInvariant();
    return destination;
  }
```

Use the Map overload allowing for ```Func<TSource,TDestination,TDestination>``` mappings, provide a private function to get around the inability of expression trees to contain assigment statements and you are ready to go:

```csharp
var nick = new Person{Name = "npal"};
var dto = mapper.Map<Person,PersonDto>(nick,"upper");
nick.Name = "Nikos Palladinos";
mapper.Map(nick, dto);
```

The same overloads are supported, as well as the capability to use [named maps](#named-maps).

### Dealing with recursion

```csharp
IPureMapperConfig Map<TSource, TDestination>(
  Func<IPureMapperResolver, Expression<Func<TSource, TDestination>>> map,
  int recInlineDepth = 0,
  string name = "")
```

#### Resolving complex objects

Mapping syntax includes ```IPureMapperResolver``` allowing the usage of other maps in each definition. This allows cascading maps, that change all user created instances to their respective mapped objects in a single trip, and provides additional value during projections.

#### Recursion Depth

Recursive properties are resolved by unrolling, and as such the recursion depth (recInlineDepth) is required in such scenarios. Only one recursion depth can be specified per map, even when multiple recursive properties exist, but in return the generated tree can work even in databases. Use [named maps](#named-maps) to create different profiles if using this feature, since recursion unrolling adds inner joins **even when not included in the query**.

#### Named Maps

PureMapper supports multiple mapping profiles for the same source/destination types by using profile names. As such, on recursive properties, numerous profiles can be defined to limit inner joins for projection, a variety of maps could be used to tailor DTOs to the needs of each view and so on.

## Simplistic Benchmarks

To be taken with a grain of salt...
Benchmarks were calculated with an object containing a recursive property populated for 100 levels. In such extreme scenarios, additional performance can be gained by tweaking the recursion depth, but the golden ratio is dependent on the specific task at hand, so mostly trial and error for now.

``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=3.1.100
  [Host] : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT  [AttachedDebugger]

Runtime=.NET Core 3.1

```

|      Method |      Job |              Toolchain | IterationCount | LaunchCount | WarmupCount | NumberOfIterations |       Mean |      Error |     StdDev |
|------------ |--------- |----------------------- |--------------- |------------ |------------ |------------------- |-----------:|-----------:|-----------:|
| PureMapping | ShortRun | InProcessEmitToolchain |              3 |           1 |           3 |               1000 |   2.237 ms |   6.429 ms |  0.3524 ms |
| AutoMapping | ShortRun | InProcessEmitToolchain |              3 |           1 |           3 |               1000 |  35.935 ms | 172.306 ms |  9.4447 ms |
| PureMapping | ShortRun | InProcessEmitToolchain |              3 |           1 |           3 |              10000 |  17.979 ms |   7.878 ms |  0.4318 ms |
| AutoMapping | ShortRun | InProcessEmitToolchain |              3 |           1 |           3 |              10000 | 283.295 ms | 418.051 ms | 22.9148 ms |

![BenchmarkGraph](bench.png)

## Contributors

* [palladin](https://github.com/palladin), aka [@NickPalladinos](https://twitter.com/NickPalladinos), high priest of the Old Ones, providing tips and guidance in return for blood sacrifice.

[autoMapper]: https://github.com/AutoMapper/AutoMapper
