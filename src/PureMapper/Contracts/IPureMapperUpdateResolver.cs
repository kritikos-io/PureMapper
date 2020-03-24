namespace Kritikos.PureMap.Contracts
{
	using System;
	using System.Linq.Expressions;

	public interface IPureMapperUpdateResolver
	{
		/// <summary>
		/// Updates the values of an existing object from a provided source.
		/// </summary>
		/// <typeparam name="TSource">Object to act as souce of values.</typeparam>
		/// <typeparam name="TDestination">Object that will have values updated from source.</typeparam>
		/// <returns><typeparamref name="TDestination"/> object with values updated from <typeparamref name="TSource"/>.</returns>
		Expression<Func<TSource, TDestination, TDestination>> Resolve<TSource, TDestination>()
			where TSource : class
			where TDestination : class;

		/// <summary>
		/// Updates the values of an existing object from a provided source.
		/// </summary>
		/// <typeparam name="TSource">Object to act as souce of values.</typeparam>
		/// <typeparam name="TDestination">Object that will have values updated from source.</typeparam>
		/// <param name="name">Named map to use.</param>
		/// <returns><typeparamref name="TDestination"/> object with values updated from <typeparamref name="TSource"/>.</returns>
		Expression<Func<TSource, TDestination, TDestination>> Resolve<TSource, TDestination>(string name)
			where TSource : class
			where TDestination : class;
	}
}
