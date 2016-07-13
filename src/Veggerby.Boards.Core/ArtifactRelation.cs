namespace Veggerby.Boards.Core
{
    public abstract class ArtifactRelation<TSource, TDestination> 
        where TSource : Artifact 
        where TDestination : Artifact
    {
        public TSource Source { get; }
        public TDestination Destination { get; }

        public ArtifactRelation(TSource source, TDestination destination)
        {
            Source = source;
            Destination = destination;
        }
    }
}