#nullable disable
namespace Kritikos.PureMap
{
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Reflection;

	using Kritikos.PureMap.Contracts;

	using Nessos.Expressions.Splicer;

	public class PureMapper : IPureMapperResolver, IPureMapper
	{
		private readonly Dictionary<(Type Source, Type Dest, string Name), int> visited =
			new Dictionary<(Type Source, Type Dest, string Name), int>();

		private readonly Dictionary<(Type Source, Type Dest, string Name), MapValue> dict
			= new Dictionary<(Type Source, Type Dest, string Name), MapValue>();

		public PureMapper(IPureMapperConfig cfg)
			=> Map(cfg?.Maps ?? throw new ArgumentNullException(nameof(cfg)));

		public TDestination Map<TSource, TDestination>(TSource source, string name = "")
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!dict.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			return ((Func<TSource, TDestination>)dict[key].SplicedFunc).Invoke(source);
		}

		public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, string name = "")
		{
			throw new NotImplementedException("Under construction");
		}

		public Expression<Func<TSource, TDestination>> Map<TSource, TDestination>(string name = "")
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!dict.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			return (Expression<Func<TSource, TDestination>>)dict[key].SplicedExpr;
		}

		public Expression<Func<TSource, TDestination>> Resolve<TSource, TDestination>()
			where TSource : class
			where TDestination : class
			=> Resolve<TSource, TDestination>(string.Empty);

		public Expression<Func<TSource, TDestination>> Resolve<TSource, TDestination>(string name)
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!dict.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var mapValue = dict[key];

			if (!visited.ContainsKey(key))
			{
				visited.Add(key, 0);
			}

			if (visited[key] > mapValue.RecInlineDepth)
			{
				return (Expression<Func<TSource, TDestination>>)mapValue.Rec;
			}

			visited[key]++;

			var splicer = new Splicer();
			var splicedExpr = (LambdaExpression)splicer.Visit(mapValue.OriginalExpr);

			return (Expression<Func<TSource, TDestination>>)splicedExpr;
		}

#pragma warning disable IDE0051 // Used by reflection
		private Expression<Func<TSource, TDestination>> ResolveExpr<TSource, TDestination>(string name = "")
#pragma warning restore IDE0051 // Remove unused private members
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!dict.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var mapValue = dict[key];
			mapValue.Rec = (Expression<Func<TSource, TDestination>>)(x => null);
			return Resolve<TSource, TDestination>(name);
		}

#pragma warning disable IDE0051 // Used by reflection
		private Expression<Func<TSource, TDestination>> ResolveFunc<TSource, TDestination>(string name = "")
			where TSource : class
			where TDestination : class
		{
			var key = (typeof(TSource), typeof(TDestination), name);
			if (!dict.ContainsKey(key))
			{
				throw new KeyNotFoundException($"{key}");
			}

			var mapValue = dict[key];
			mapValue.Rec =
				(Expression<Func<TSource, TDestination>>)(x => ((Func<TSource, TDestination>)mapValue.SplicedFunc)(x));
			return Resolve<TSource, TDestination>(name);
		}

		private void Map(
			List<(Type Source, Type Dest, string Name, Func<IPureMapperResolver, LambdaExpression> Map, int
				RecInlineDepth)> maps)
		{
			foreach (var (source, destination, name, map, recInlineDepth) in maps)
			{
				var key = (source, destination, name);

				var mapValue = new MapValue { OriginalExpr = map(this), RecInlineDepth = recInlineDepth };

				if (dict.ContainsKey(key))
				{
					dict[key] = mapValue;
				}
				else
				{
					dict.Add(key, mapValue);
				}
			}

			// force resolve
			foreach (var keyValue in dict)
			{
				var (src, dest, name) = keyValue.Key;
				var mapValue = keyValue.Value;

				var resolve = this.GetType().GetTypeInfo().GetDeclaredMethod("ResolveExpr");
				var resolvedGeneric = resolve.MakeGenericMethod(src, dest);
				var lambdaExpression = (LambdaExpression)resolvedGeneric.Invoke(this, new object[] { name });
				mapValue.SplicedExpr = lambdaExpression;
				visited.Clear();

				resolve = this.GetType().GetTypeInfo().GetDeclaredMethod("ResolveFunc");
				resolvedGeneric = resolve.MakeGenericMethod(src, dest);
				lambdaExpression = (LambdaExpression)resolvedGeneric.Invoke(this, new object[] { name });
				mapValue.SplicedFunc = lambdaExpression.Compile();
				visited.Clear();
			}
		}

		private class MapValue
		{
			public LambdaExpression OriginalExpr { get; set; }

			public LambdaExpression SplicedExpr { get; set; }

			public Delegate SplicedFunc { get; set; }

			public LambdaExpression Rec { get; set; }

			public int RecInlineDepth { get; set; }
		}
	}
}
