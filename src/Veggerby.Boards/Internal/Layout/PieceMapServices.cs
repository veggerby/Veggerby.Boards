namespace Veggerby.Boards.Internal.Layout;

/// <summary>
/// Accessor service bundling current snapshot (initial only until incremental update integration).
/// </summary>
internal sealed class PieceMapServices(PieceMapLayout layout, PieceMapSnapshot snapshot)
{
    public PieceMapLayout Layout { get; } = layout;
    public PieceMapSnapshot Snapshot { get; } = snapshot;
}