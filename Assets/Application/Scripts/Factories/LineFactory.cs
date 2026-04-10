using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Application
{
    public sealed class LineFactory : IFactory<LineRenderer>, IDisposable
    {
        private const string LINE_KEY = "Assets/Application/Line.prefab";
        private readonly AsyncOperationHandle<GameObject> lineHandle = Addressables.LoadAssetAsync<GameObject>(LINE_KEY);

        public async UniTask<LineRenderer> Create()
        {
            GameObject prefab = await lineHandle;
            GameObject instance = Object.Instantiate(prefab);
            return instance.GetComponent<LineRenderer>();
        }

        public void Dispose() => Addressables.Release(lineHandle);
    }
}