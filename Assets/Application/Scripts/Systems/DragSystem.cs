using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Application
{
    public sealed class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public IObservable<PointerEventData> Drag => drag;
        public IObservable<PointerEventData> Drop => drop;

        private readonly Subject<PointerEventData> drag = new();
        private readonly Subject<PointerEventData> drop = new();
        
        public void OnBeginDrag(PointerEventData eventData) => drag.OnNext(eventData);
        public void OnDrag(PointerEventData eventData) => drag.OnNext(eventData);
        public void OnEndDrag(PointerEventData eventData) => drop.OnNext(eventData);
    }

    public interface IDropZoneOwner { }
    public interface IDropZone
    {
        IDropZoneOwner Owner { get; }
    } 

    public readonly struct DragData
    {
        public readonly Vector3 Position;
        public readonly Vector2 Delta;
        public readonly IDropZone DropZone;

        public DragData(Vector3 position, Vector2 delta, IDropZone dropZone)
        {
            Position = position;
            Delta = delta;
            DropZone = dropZone;
        }
    }

    public interface IDragSystem
    {
        IObservable<GameObject> OnDragGameObject { get; }

        IDisposable Register(GameObject draggable, Action<DragData> onDrag, Action<DragData> onDrop, Predicate<IDropZone> filter = null);
        IDropZone CreateDropZone(IDropZoneOwner owner, GameObject dropZoneObject);
    }

    public sealed class DragSystem : IDragSystem
    {
        private sealed class DropZone : MonoBehaviour, IDropZone
        {
            public IDropZoneOwner Owner { get; set; }
        }

        private static readonly RaycastHit[] CachedRaycastResults = new RaycastHit[10];

        public IObservable<GameObject> OnDragGameObject => onDragGameObject;
        private readonly Subject<GameObject> onDragGameObject = new();

        private Camera camera;

        public IDisposable Register(GameObject draggable, Action<DragData> onDrag, Action<DragData> onDrop, Predicate<IDropZone> filter = null)
        {
            CompositeDisposable disposables = new ();

            Draggable d = draggable.AddComponent<Draggable>();
            Disposable.CreateWithState(d, static x => Object.Destroy(x)).AddTo(disposables);
            d.Drag.Where(e => e.button == PointerEventData.InputButton.Left)
                .Subscribe(e => onDrag?.Invoke(ConvertToDragArgs(e, filter)))
                .AddTo(disposables);
            d.Drop.Where(e => e.button == PointerEventData.InputButton.Left)
                .Subscribe(e => onDrop?.Invoke(ConvertToDragArgs(e, filter)))
                .AddTo(disposables);

            return disposables;
        }

        public IDropZone CreateDropZone(IDropZoneOwner owner, GameObject dropZoneObject)
        {
            DropZone dz = dropZoneObject.AddComponent<DropZone>();
            dz.Owner = owner;
            return dz;
        }

        private DragData ConvertToDragArgs(PointerEventData eventData, Predicate<IDropZone> filter = null)
        {
            onDragGameObject.OnNext(eventData.pointerDrag);

            if (camera == null)
            {
                camera = Camera.main;
            }

            Ray ray = camera!.ScreenPointToRay(Input.mousePosition);
            int hits = Physics.RaycastNonAlloc(ray, CachedRaycastResults);
            for (int i = 0; i < hits; i++)
            {
                IDropZone dropZone = CachedRaycastResults[i].transform.GetComponentInParent<IDropZone>();
                if (dropZone != null && filter?.Invoke(dropZone) != false)
                {
                    Vector3 worldPosition = CachedRaycastResults[i].point.ToVector3Y0();
                    return new DragData(worldPosition, eventData.delta, dropZone);
                }
            }
            
            Plane p = new(Vector3.up, 0);
            if (p.Raycast(ray, out float distance))
            {
                return new DragData(ray.GetPoint(distance).ToVector3Y0(), eventData.delta, null);
            }
            
            return new DragData(eventData.pointerCurrentRaycast.worldPosition.ToVector3Y0(), eventData.delta, null);
        }
    }
}