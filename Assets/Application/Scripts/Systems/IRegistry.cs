using System;
using System.Collections.Generic;
using UniRx;

namespace Application
{
    public interface IRegistry<T>
    {
        IReadOnlyList<T> GetAll();

        bool Add(T value);
        bool Remove(T value);

        IDisposable Register(T value)
        {
            if (Add(value))
            {
                return Disposable.CreateWithState((self: this, value), static x => x.self.Remove(x.value));
            }
            
            return Disposable.Empty;
        }
    }

    public abstract class DefaultRegistryImplementation<T>: IRegistry<T>
    {
        private readonly List<T> list = new();

        public IReadOnlyList<T> GetAll() => list.AsReadOnly();

        public bool Add(T value)
        {
            if (list.Contains(value)) return false;
            list.Add(value);
            return true;
        }

        public bool Remove(T value) => list.Remove(value);
    }
    
    public sealed class PawnRegistry : DefaultRegistryImplementation<IPawn> { }
    public sealed class ConnectorsRegistry : DefaultRegistryImplementation<IConnector> { }
    public sealed class SelectableRegistry : DefaultRegistryImplementation<ISelectable> { }
    public sealed class CheckerboardRegistry : DefaultRegistryImplementation<ICheckerboard> { }
}