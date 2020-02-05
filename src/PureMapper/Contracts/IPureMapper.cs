namespace Kritikos.PureMap.Contracts
{
	using System;
	using System.Diagnostics;
	using System.Linq.Expressions;

	public interface IPureMapper
	{
		TDestination Map<TSource, TDestination>(TSource source, string name = "")
			where TSource : class
			where TDestination : class;

		Expression<Func<TSource, TDestination>> Map<TSource, TDestination>(string name = "")
			where TSource : class
			where TDestination : class;

		TDestination Map<TSource, TDestination>(TSource source, TDestination destination, string name = "");
	}
}
