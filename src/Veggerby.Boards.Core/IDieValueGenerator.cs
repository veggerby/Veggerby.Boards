namespace Veggerby.Boards.Core
{
    public interface IDieValueGenerator<T>
    {
        T GetValue();
    }
}