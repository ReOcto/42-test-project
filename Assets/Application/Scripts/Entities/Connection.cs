using System;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Application
{
    public interface IConnection : IDisposable
    {
        IConnector Start { get; }
        IConnector End { get; }
    }

    public sealed class Connection : IConnection
    {
        public IConnector Start { get; }
        public IConnector End { get; }

        private LineRenderer LineRenderer { get; }

        public Connection(LineRenderer lineRenderer, IConnector startConnector, IConnector endConnector)
        {
            Start = startConnector;
            End = endConnector;
            LineRenderer = lineRenderer;

            Observable
                .CombineLatest(Start.Position, End.Position, (start, end) => (start: start, end))
                .Subscribe(x =>
                {
                    LineRenderer.positionCount = 2;
                    LineRenderer.SetPosition(0, x.start);
                    LineRenderer.SetPosition(1, x.end);
                })
                .AddTo(LineRenderer);
        }

        public void Dispose()
        {
            if (LineRenderer != null)
            {
                Object.Destroy(LineRenderer.gameObject);
            }
        }
    }
}