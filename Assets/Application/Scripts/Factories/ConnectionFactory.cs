using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Application
{
    public interface IConnectionFactory : IFactory<(IConnector start, IConnector end), IConnection> { }

    public sealed class ConnectionFactory : IConnectionFactory
    {
        [Inject] private readonly IFactory<LineRenderer> lineFactory;
        
        public async UniTask<IConnection> Create((IConnector start, IConnector end) state)
        {
            LineRenderer lineRenderer = await lineFactory.Create();
            return new Connection(lineRenderer, state.start, state.end);
        }
    }
}