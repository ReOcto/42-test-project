using Cysharp.Threading.Tasks;
using UniRx;
using VContainer;

namespace Application
{
    public interface IConnectorFactory : IFactory<(IPawn pawn, UtilityScriptForConnectorFromTechArtist socket), IConnector> { }
    public sealed class ConnectorFactory : IConnectorFactory
    {
        [Inject] private readonly IDragSystem dragSystem;
        [Inject] private readonly IConnectionFactory connectionFactory;
        [Inject] private readonly IConnectorRecolorSystem connectorRecolorSystem;
        [Inject] private readonly IRegistry<IConnector> connectorRegistry;
        [Inject] private readonly ISelectionSystem selectionSystem;

        public UniTask<IConnector> Create((IPawn pawn, UtilityScriptForConnectorFromTechArtist socket) state)
        {
            Connector connector = new(state.pawn, dragSystem, connectorRegistry, connectorRecolorSystem, connectionFactory, selectionSystem, state.socket);
            connectorRegistry.Register(connector).AddTo(state.socket);

            return UniTask.FromResult((IConnector)connector);
        }
    }
}