using UnityEngine;

namespace Application
{
    public interface ICheckerboard : IDropZone, IDropZoneOwner
    {
        public GameObject GameObject { get; }
    }
    
    public sealed class Checkerboard : MonoBehaviour, ICheckerboard
    {
        public GameObject GameObject => gameObject;
        public IDropZoneOwner Owner => this;
    }
}