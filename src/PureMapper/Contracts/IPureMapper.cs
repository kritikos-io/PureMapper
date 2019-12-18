namespace Kritikos.PureMapper.Contracts
{
	using System;
	using System.Linq.Expressions;

	public interface IPureMapper
	{
		Expression<Func<TSource, TDestination>> ResolveExpr<TSource, TDestination>();

		Func<TSource, TDestination> ResolveFunc<TSource, TDestination>();
	}
}
