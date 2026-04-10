using System;
using System.Collections.Generic;
using System.Linq;
using CrazyPawn;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

namespace Application
{
    public interface IConnectorRecolorSystem
    {
        UniTask SetAllConnectorMaterials(IConnector currentConnector, bool active);
    }

    public sealed class ConnectorRecolorSystem : IConnectorRecolorSystem, IDisposable
    {
        private const string SETTINGS_KEY = "Assets/Templates/CrazyPawnSettings.asset";
        private readonly AsyncOperationHandle<CrazyPawnSettings> settingsHandle = Addressables.LoadAssetAsync<CrazyPawnSettings>(SETTINGS_KEY);

        private readonly IRegistry<IConnector> connectorRegistry;
        private readonly CompositeDisposable compositeDisposable = new();

        private Material activeMaterial;

        [Inject] public ConnectorRecolorSystem(IRegistry<IConnector> connectorRegistry, ISelectionSystem selectionSystem)
        {
            this.connectorRegistry = connectorRegistry;

            selectionSystem.Current.ToUniTaskAsyncEnumerable()
                .SubscribeAwait(selectable => SetAllConnectorMaterials(selectable as IConnector, selectable is IConnector))
                .AddTo(compositeDisposable);
        }

        public void Dispose() => compositeDisposable.Dispose();

        public async UniTask SetAllConnectorMaterials(IConnector currentConnector, bool active)
        {
            if (activeMaterial == null)
            {
                CrazyPawnSettings settings = await settingsHandle;
                activeMaterial = settings.ActiveConnectorMaterial;
            }

            IReadOnlyList<IConnector> allConnectors = connectorRegistry.GetAll();
            List<IConnector> toHighlight = new();

            foreach (var item in allConnectors)
            {
                if (currentConnector != null && (currentConnector.Pawn.Connectors.Contains(item) || currentConnector.ConnectionExists(item)))
                {
                    continue;
                }

                toHighlight.Add(item);
            }

            foreach (var item in toHighlight)
            {
                item.Rend.renderer.material = active ? activeMaterial : item.Rend.defaultMaterial;
            }            
        }
    }
}