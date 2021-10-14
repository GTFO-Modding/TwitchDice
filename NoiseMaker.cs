using AIGraph;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TwitchDice
{
    public class NoiseMaker : MonoBehaviour
    {
        public NoiseMaker(IntPtr intPtr) : base(intPtr) 
        {
            Instance = this;
        }
        public static NoiseMaker Instance;
        private readonly Queue<ManagedNoiseData> ManagedNoiseDatas = new Queue<ManagedNoiseData>();

        public void MakeNoise(ManagedNoiseData data)
        {
            ManagedNoiseDatas.Enqueue(data);
        }

        void Update()
        {
            while(ManagedNoiseDatas.Count > 0)
            {
                Noise(ManagedNoiseDatas.Dequeue());
            }
        }

        private void Noise(ManagedNoiseData data)
        {
            NM_NoiseData noiseData = new NM_NoiseData()
            {
                noiseMaker = null,
                position = data.Position,
                radiusMin = 0,
                radiusMax = data.RadiusMax,
                yScale = 1f,
                node = data.Node,
                type = data.Type,
                includeToNeightbourAreas = data.IncludeToNeightbourAreas,
                raycastFirstNode = data.RaycastFirstNode
            };
            noiseData.position = data.Position;
            NoiseManager.MakeNoise(noiseData);
        }
    }

    public struct ManagedNoiseData
    {
        public ManagedNoiseData(Vector3 position, float radiusMin, float radiusMax, float yScale, NM_NoiseType type, bool includeToNeightbourAreas, bool raycastFirstNode, AIG_CourseNode node)
        {
            Position = position;
            RadiusMin = radiusMin;
            RadiusMax = radiusMax;
            YScale = yScale;
            Type = type;
            IncludeToNeightbourAreas = includeToNeightbourAreas;
            RaycastFirstNode = raycastFirstNode;
            Node = node;
        }

        public Vector3 Position { get; set; }
        public float RadiusMin { get; set; }
        public float RadiusMax { get; set; }
        public float YScale { get; set; }
        public NM_NoiseType Type { get; set; }
        public bool IncludeToNeightbourAreas { get; set; }
        public bool RaycastFirstNode { get; set; }
        public AIG_CourseNode Node { get; set; }
    }
}
