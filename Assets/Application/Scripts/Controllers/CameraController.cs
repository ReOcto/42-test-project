using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniRx;
using UnityEngine;
using VContainer;

namespace Application
{
    public sealed class CameraController : IController, IDisposable
    {
        [Inject] private readonly IRegistry<ICheckerboard> checkerboardRegistry;
        [Inject] private readonly IDragSystem dragSystem;

        private readonly CompositeDisposable compositeDisposable = new();

        private Camera camera;

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            ICheckerboard checkerboard = await checkerboardRegistry
                .ObserveEveryValueChanged(x => x.GetAll().SingleOrDefault())
                .ToUniTaskAsyncEnumerable()
                .FirstOrDefaultAsync(x => x != null, cancellation);

            dragSystem.Register(checkerboard.GameObject, OnDrag, OnDrop).AddTo(compositeDisposable);

            Observable.EveryUpdate()
                .Select(_ => Input.mouseScrollDelta.y)
                .Where(scroll => Mathf.Approximately(scroll, 0) == false)
                .Subscribe(OnScroll)
                .AddTo(compositeDisposable);
        }

        public void Dispose() => compositeDisposable.Dispose();

        private static void OnDrop(DragData obj) { }
        private void OnDrag(DragData obj)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }
            camera!.transform.position -= obj.Delta.ToVector3Y0() * Time.deltaTime;
        }

        private void OnScroll(float delta)
        {
            if (camera == null)
            {
                camera = Camera.main;
            }

            Vector3 nextPosition = camera!.transform.position;
            nextPosition.y -= delta * Time.deltaTime;

            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            float zoomDistance = delta * Time.deltaTime;
            nextPosition += (ray.direction * zoomDistance).ToVector3Y0();

            camera.transform.position = nextPosition;
        }
    }
}