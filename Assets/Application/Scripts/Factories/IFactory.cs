using Cysharp.Threading.Tasks;

namespace Application
{
    public interface IFactory<TResult>
    {
        UniTask<TResult> Create();
    }

    public interface IFactory<in TState, TResult>
    {
        UniTask<TResult> Create(TState state);
    }
}