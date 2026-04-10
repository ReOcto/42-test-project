using System.Threading;
using CrazyPawn;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using VContainer;

namespace Application
{
    public sealed class AppController : IController
    {
        private const string SETTINGS_KEY = "Assets/Templates/CrazyPawnSettings.asset";
        private readonly AsyncOperationHandle<CrazyPawnSettings> settingsHandle = Addressables.LoadAssetAsync<CrazyPawnSettings>(SETTINGS_KEY);

        [Inject] private readonly IFactory<ICheckerboard> checkerboardFactory;
        [Inject] private readonly IFactory<IPawn> pawnFactory;

        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            await SceneManager.LoadSceneAsync("Scenes/PawnField");
            await checkerboardFactory.Create();
            await SpawnPawns();
        }

        private async UniTask SpawnPawns()
        {
            CrazyPawnSettings settings = await settingsHandle;

            for (int i = 0; i < settings.InitialPawnCount; i++)
            {
                Vector2 pos = Random.insideUnitCircle * settings.InitialZoneRadius;
                IPawn newPawn = await pawnFactory.Create();
                newPawn.Body.transform.position = new Vector3(pos.x, 0, pos.y);
            }
        }
    }
}