using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Application
{
    public interface IConnector : IDisposable
    {
        IPawn Pawn { get; }
        List<IConnection> Connections { get; }
        IObservable<Vector3> Position { get; }

        UtilityScriptForConnectorFromTechArtist ConnectorObject { get; }
        (Renderer renderer, Material defaultMaterial) Rend { get; }
        bool ConnectionExists(IConnector other);
    }

    public sealed class Connector : IConnector, ISelectable, IDropZoneOwner
    {
        public IPawn Pawn { get; }
        public List<IConnection> Connections { get; } = new();
        public IObservable<Vector3> Position { get; }
        public UtilityScriptForConnectorFromTechArtist ConnectorObject { get; }
        public (Renderer renderer, Material defaultMaterial) Rend { get; }
        public ISelectableTrigger SelectableTrigger { get; }

        private readonly IConnectorRecolorSystem connectorRecolorSystem;
        private readonly IRegistry<IConnector> connectorRegistry;
        private readonly IConnectionFactory connectionFactory;
        private readonly ISelectionSystem selectionSystem;
        private readonly CompositeDisposable compositeDisposable = new();

        private Material activeMaterial;

        public Connector(IPawn pawn,
            IDragSystem dragSystem,
            IRegistry<IConnector> connectorRegistry,
            IConnectorRecolorSystem connectorRecolorSystem,
            IConnectionFactory connectionFactory,
            ISelectionSystem selectionSystem,
            UtilityScriptForConnectorFromTechArtist connectorObject)
        {
            Pawn = pawn;
            this.connectorRegistry = connectorRegistry;
            this.connectorRecolorSystem = connectorRecolorSystem;
            this.connectionFactory = connectionFactory;
            this.selectionSystem = selectionSystem;
            ConnectorObject = connectorObject;
            Position = ConnectorObject.transform.ObserveEveryValueChanged(x => x.position);
            Renderer renderer = ConnectorObject.GetComponent<Renderer>();
            Rend = (renderer, renderer.material);
            SelectableTrigger = selectionSystem.CreateTrigger(ConnectorObject.gameObject, this);

            dragSystem.CreateDropZone(this, ConnectorObject.gameObject);
            dragSystem.Register(ConnectorObject.gameObject, OnDrag, OnDrop, x => x.Owner is IConnector).AddTo(compositeDisposable);
        }

        public void Dispose() => compositeDisposable.Dispose();

        private void OnDrop(DragData data)
        {
            if (TryGetConnector(data.DropZone, out IConnector otherConnector))
            {
                TryConnect(otherConnector).Forget();
            }

            connectorRecolorSystem.SetAllConnectorMaterials(null, false).Forget();
        }

        private void OnDrag(DragData data)
        {
            connectorRecolorSystem.SetAllConnectorMaterials(this, true).Forget();
        }

        private static bool TryGetConnector(IDropZone otherDropZone, out IConnector connector)
        {
            if (otherDropZone?.Owner is IConnector otherConnector)
            {
                connector = otherConnector;
                return true;
            }

            connector = null;
            return false;
        }

        private async UniTask<bool> TryConnect(IConnector other)
        {
            if (Pawn.Connectors.Contains(other)) return false;
            if (ConnectionExists(other)) return false;

            IConnection connection = await connectionFactory.Create((this, other));
            RegisterConnection(Connections, connection).AddTo(ConnectorObject);
            RegisterConnection(other.Connections, connection).AddTo(other.ConnectorObject);
            return true;

            static IDisposable RegisterConnection(IList<IConnection> target, IConnection connection)
            {
                target.Add(connection);
                return Disposable.CreateWithState((target, connection), static x =>
                {
                    x.target.Remove(x.connection);
                    x.connection.Dispose();
                });
            }
        }

        public bool ConnectionExists(IConnector other)
        {
            foreach (var connection in Connections)
            {
                if (connection.Start == this && connection.End == other)
                {
                    return true;
                }
            }

            foreach (var connection in other.Connections)
            {
                if (connection.Start == other && connection.End == this)
                {
                    return true;
                }
            }

            return false;
        }

        public void OnSelect()
        {
            if (selectionSystem.Previous.Value is IConnector otherConnector)
            {
                TryConnect(otherConnector).Forget();
                selectionSystem.ResetSelection();
            }
        }
    }
}