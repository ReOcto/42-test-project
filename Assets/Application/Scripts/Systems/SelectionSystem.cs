using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Application
{
    public interface ISelectable
    {
        ISelectableTrigger SelectableTrigger { get; }
        void OnSelect();
    }

    public interface ISelectionSystem
    { 
        IReadOnlyReactiveProperty<ISelectable> Previous { get; }
        IReadOnlyReactiveProperty<ISelectable> Current { get; }

        public ISelectableTrigger CreateTrigger(GameObject gameObject, ISelectable owner);
        void ResetSelection();
    }

    public interface ISelectableTrigger
    {
        public ISelectable Owner { get; }
    }
    
    public sealed class SelectionSystem : ISelectionSystem, IDisposable
    {
        private sealed class SelectableTrigger : MonoBehaviour, ISelectableTrigger
        {
            public ISelectable Owner { get; set; }
        }

        public IReadOnlyReactiveProperty<ISelectable> Previous => previous;
        public IReadOnlyReactiveProperty<ISelectable> Current => current;

        private readonly ReactiveProperty<ISelectable> previous = new();
        private readonly ReactiveProperty<ISelectable> current = new();
        private readonly CompositeDisposable compositeDisposable = new();

        [Inject] public SelectionSystem(IDragSystem dragSystem)
        {
            List<RaycastResult> cachedRaycastResults = new();
            Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ =>
                {
                    PointerEventData pointerData = new (EventSystem.current)
                    {
                        position = Input.mousePosition
                    };
                    EventSystem.current.RaycastAll(pointerData, cachedRaycastResults);
                    foreach (var item in cachedRaycastResults)
                    {
                        ISelectableTrigger trigger = item.gameObject.GetComponent<ISelectableTrigger>();
                        if (trigger != null)
                        {
                            OnClick(trigger.Owner);
                            return;
                        }
                    }

                    OnClick(null);
                })
                .AddTo(compositeDisposable);

            dragSystem.OnDragGameObject.Subscribe(_ => ResetSelection()).AddTo(compositeDisposable);
        }

        public void Dispose() => compositeDisposable.Dispose();

        public ISelectableTrigger CreateTrigger(GameObject gameObject, ISelectable owner)
        {
            var trigger = gameObject.AddComponent<SelectableTrigger>();
            trigger.Owner = owner;
            return trigger;
        }

        public void ResetSelection()
        {
            previous.Value = null;
            current.Value = null;
        }

        private void OnClick(ISelectable selectable)
        {
            if (selectable != null)
            {
                previous.Value = Current.Value;
                current.Value = selectable;
                Current.Value.OnSelect();
            }
            else
            {
                current.Value = null;
            }
        }
    }
}