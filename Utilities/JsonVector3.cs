using UnityEngine;

namespace TwitchDice.Utilities
{
    public struct JsonVector3
    {
        public float x;
        public float y;
        public float z;

        public JsonVector3(Vector3 vector3)
        {
            this.x = vector3.x;
            this.y = vector3.y;
            this.z = vector3.z;
        }
    }

    public static class JsonVector3Extensions
    {
        public static Vector3 Convert(this JsonVector3 jsonVector)
        {
            return new Vector3(jsonVector.x, jsonVector.y, jsonVector.z);
        }
    }
}
