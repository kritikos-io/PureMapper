namespace Kritikos.PureMap.Contracts
{
	using System;
	using System.Linq.Expressions;

	public interface IPureMapperResolver
	{
		/// <summary>
		/// Maps a source object to the requested type.
		/// </summary>
		/// <typeparam name="TSource"><see cref="Type"/> to map from.</typeparam>
		/// <typeparam name="TDestination"><see cref="Type"/> to map to.</typeparam>
		/// <returns>New instance of <typeparamref name="TDestination"/> with values mapped from <typeparamref name="TSource"/>.</returns>
		Expression<Func<TSource, TDestination>> Resolve<TSource, TDestination>()
			where TSource : class
			where TDestination : class;

		/// /// <summary>
		/// Maps a source object to the requested type.
		/// </summary>
		/// <typeparam name="TSource"><see cref="Type"/> to map from.</typeparam>
		/// <typeparam name="TDestination"><see cref="Type"/> to map to.</typeparam>
		/// <param name="name">Name of map registration to use.</param>
		/// <returns>New instance of <typeparamref name="TDestination"/> with values mapped from <typeparamref name="TSource"/>.</returns>
		Expression<Func<TSource, TDestination>> Resolve<TSource, TDestination>(string name)
			where TSource : class
			where TDestination : class;
	}
}
