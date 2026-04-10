using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;
using UniRx;
using Object = UnityEngine.Object;

namespace Application
{
    public sealed class PawnFactory : IFactory<IPawn>, IDisposable
    {
        private const string PAWN_KEY = "Assets/Templates/Pawn.prefab";
        private readonly AsyncOperationHandle<GameObject> pawnHandle = Addressables.LoadAssetAsync<GameObject>(PAWN_KEY);

        [Inject] private readonly IDragSystem dragSystem;
        [Inject] private readonly IConnectorFactory connectorFactory;
        [Inject] private readonly IRegistry<IPawn> pawnRegistry;

        public async UniTask<IPawn> Create()
        {
            GameObject prefab = await pawnHandle;
            GameObject instance = Object.Instantiate(prefab);
            Pawn pawn = new Pawn(dragSystem, connectorFactory, instance);
            pawnRegistry.Register(pawn).AddTo(instance);
            return pawn;
        }

        public void Dispose() => Addressables.Release(pawnHandle);
    }
}