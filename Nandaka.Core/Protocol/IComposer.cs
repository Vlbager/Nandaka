namespace Nandaka.Core.Protocol
{
    public interface IComposer<in TIn, out TOut>
    {
        TOut Compose(TIn message);
    }
}
