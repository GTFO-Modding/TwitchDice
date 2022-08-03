using Player;
using System.Collections.Generic;
using UnityEngine;

namespace TwitchDice.Utilities
{
    public static class SpawnUtil
    {
        public static GameObject CreateEmpty(Vector3 position)
        {
            var go = new GameObject();
            go.transform.position = position;
            return go;
        }

        public static List<RaycastHit> GetRandomScatterAround(Vector3 org, int count)
        {
            var list = new List<RaycastHit>();
            for (int i = 0; i < count; i++)
            {
                Vector3 direction = Random.insideUnitSphere.normalized;
                Ray ray = new Ray(org, direction);
                RaycastHit hit;
                int iterations = 0;
                while (!Physics.Raycast(ray, out hit, 100000, LayerManager.MASK_CAMERA_RAY) && iterations < 100)
                {
                    direction = Random.insideUnitSphere.normalized;
                    ray = new Ray(org, direction);
                    iterations++;
                }
                list.Add(hit);
            }

            return list;
        }

        public static RaycastHit GetRandomPointAround(Vector3 org)
        {
            Vector3 direction = Random.insideUnitSphere.normalized;
            Ray ray = new Ray(org, direction);
            RaycastHit hit;
            int iterations = 0;
            while (!Physics.Raycast(ray, out hit, 100000, LayerManager.MASK_CAMERA_RAY) && iterations < 100)
            {
                direction = Random.insideUnitSphere.normalized;
                ray = new Ray(org, direction);
                iterations++;
            }
            return hit;
        }

        public static void ThrowConsumable(Vector3 org, Vector3 target, PlayerAgent source, pItemData data, int throwForce)
        {
            Vector3 direction = (target - source.EyePosition).normalized;
            var rot = Quaternion.LookRotation(direction, Vector3.up);
            ItemReplicationManager.ThrowItem(data, null, ItemMode.Instance, org, rot, direction * throwForce, source.EyePosition, source.CourseNode, source);
        }
    }
}
