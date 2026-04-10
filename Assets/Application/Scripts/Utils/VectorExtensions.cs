using UnityEngine;

namespace Application
{
    public static class VectorExtensions
    {
        public static Vector3 ToVector3Y0(this in Vector3 vector3) => new(vector3.x, 0, vector3.z);
        public static Vector3 ToVector3Y0(this in Vector2 vector2) => new(vector2.x, 0, vector2.y);
        public static Vector2 ToVector2XZ(this in Vector3 vector2) => new(vector2.x, vector2.z);
    }
}