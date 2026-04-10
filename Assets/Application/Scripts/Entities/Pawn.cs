using System;
using System.Collections.Generic;
using System.Linq;
using CrazyPawn;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Application
{
    public interface IPawn : IDisposable
    {
        UnilityScriptForPawnFormTechArtists Body { get; }
        IReadOnlyList<IConnector> Connectors { get; }
    }

    public sealed class Pawn : IPawn
    {
        private const string SETTINGS_KEY = "Assets/Templates/CrazyPawnSettings.asset";
        private readonly AsyncOperationHandle<CrazyPawnSettings> settingsHandle = Addressables.LoadAssetAsync<CrazyPawnSettings>(SETTINGS_KEY);

        public UnilityScriptForPawnFormTechArtists Body { get; }
        public IReadOnlyList<IConnector> Connectors => connectors;

        private (Renderer renderer, Material defaultMaterial)[] AllRenderers { get; }
        private readonly List<IConnector> connectors = new();
        private readonly CompositeDisposable compositeDisposable = new();

        public Pawn(IDragSystem dragSystem, IConnectorFactory connectorFactory, GameObject instance)
        {
            Body = instance.GetComponent<UnilityScriptForPawnFormTechArtists>();
            Body.OnDestroyAsObservable().Subscribe(_ => Dispose()).AddTo(compositeDisposable);
            AllRenderers = instance.GetComponentsInChildren<Renderer>().Select(x => (x, x.material)).ToArray();

            dragSystem.Register(Body.gameObject, OnDrag, OnDrop, FilterDropZone).AddTo(Body);

            InitializeConnectors().Forget();
            async UniTask InitializeConnectors()
            {
                UtilityScriptForConnectorFromTechArtist[] children = instance.GetComponentsInChildren<UtilityScriptForConnectorFromTechArtist>();
                foreach (var child in children)
                {
                    IConnector connector = await connectorFactory.Create((this, child));
                    connector.AddTo(compositeDisposable);
                    connectors.Add(connector);
                }
            }
        }

        public void Dispose() => compositeDisposable.Dispose();

        private static bool FilterDropZone(IDropZone dropZone) => dropZone.Owner is ICheckerboard;

        private void OnDrag(DragData data)
        {
            if (data.Position.sqrMagnitude > 0)
            {
                Body.transform.position = data.Position;
            }

            foreach ((Renderer renderer, Material defaultMaterial) rend in AllRenderers)
            {
                Material material = data.DropZone == null && settingsHandle.IsDone
                    ? settingsHandle.Result.DeleteMaterial
                    : rend.defaultMaterial;
                rend.renderer.material = material;
            }
        }

        private void OnDrop(DragData data)
        {
            if (data.Position.sqrMagnitude > 0)
            {
                Body.transform.position = data.Position;
            }

            if (data.DropZone == null)
            {
                Object.Destroy(Body.gameObject);
            }
        }
    }
}