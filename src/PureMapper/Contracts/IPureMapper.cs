namespace Kritikos.PureMapper.Contracts
{
	public interface IPureMapper
	{
		TDestination Map<TSource, TDestination>(TSource source)
			where TSource : class
			where TDestination : class;
	}
}
