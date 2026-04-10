using System;
using CrazyPawn;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

namespace Application
{
    public sealed class CheckerboardFactory : IFactory<ICheckerboard>, IDisposable
    {
        private const string SETTINGS_KEY = "Assets/Templates/CrazyPawnSettings.asset";
        private const string CELL_MATERIAL_KEY = "Assets/Application/CheckerboardCellMaterial.mat";
        private const float QUAD_SCALE = 1.5f;

        private readonly AsyncOperationHandle<CrazyPawnSettings> settingsHandle = Addressables.LoadAssetAsync<CrazyPawnSettings>(SETTINGS_KEY);
        private readonly AsyncOperationHandle<Material> cellMaterialHandle = Addressables.LoadAssetAsync<Material>(CELL_MATERIAL_KEY);

        [Inject] private readonly IRegistry<ICheckerboard> checkerboardRegistry; 
        
        private Material cachedBlackMaterial;
        private Material cachedWhiteMaterial;

        public async UniTask<ICheckerboard> Create()
        {
            CrazyPawnSettings settings = await settingsHandle;
            GameObject parent = new("Checkerboard");

            Material baseMaterial = await cellMaterialHandle;
            if (cachedBlackMaterial == null)
            {
                cachedBlackMaterial = new Material(baseMaterial)
                {
                    color = settings.BlackCellColor,
                };
            }

            if (cachedWhiteMaterial == null)
            {
                cachedWhiteMaterial = new Material(baseMaterial)
                {
                    color = settings.WhiteCellColor,
                };
            }

            float halfSizeUnits = settings.CheckerboardSize * QUAD_SCALE * .5f;
            for (int x = 0; x < settings.CheckerboardSize; x++)
            {
                for (int y = 0; y < settings.CheckerboardSize; y++)
                {
                    Material material = (x + y) % 2 == 0 ? cachedBlackMaterial : cachedWhiteMaterial;
                    Vector3 position = new Vector3(x * QUAD_SCALE - halfSizeUnits, 0, y * QUAD_SCALE - halfSizeUnits);
                    CreateCell($"{x}:{y}", position, QUAD_SCALE, material);
                }
            }

            Checkerboard checkerboard = parent.AddComponent<Checkerboard>();
            checkerboardRegistry.Register(checkerboard).AddTo(checkerboard);

            return checkerboard;
            
            void CreateCell(string name, Vector3 position, float scale, Material material)
            {
                GameObject newObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                newObj.name = name;
                newObj.transform.SetParent(parent.transform);
                newObj.transform.SetPositionAndRotation(position, Quaternion.LookRotation(Vector3.down));
                newObj.transform.localScale = Vector3.one * scale;
                newObj.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        public void Dispose() => Addressables.Release(settingsHandle);
    }
}