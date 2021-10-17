using Player;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TwitchDice.Components
{
    public class SnowmanAI : MonoBehaviour
    {
        public SnowmanAI(IntPtr intPtr) : base(intPtr) { }

        GameObject Eyebrows;
        GameObject Smile;
        GameObject Frown;

        //public Vector3 SnowmanPosition { get { return transform.position; } }
        public SnowmanState State { get; private set; }
        public float StateTimer { get; private set; }
        public PlayerAgent Target { get; private set; }
        void Awake()
        {
            Smile = transform.Find("Head/smile").gameObject;
            Frown = transform.Find("Head/sad").gameObject;
            Eyebrows = transform.Find("Head/angy").gameObject;
            State = SnowmanState.Idle;
            StateTimer = 0;

            Target = GetClosestPlayer();
            LookAtTarget();
        }

        void Update()
        {
            LookAtTarget();
        }

        PlayerAgent GetClosestPlayer()
        {
            PlayerAgent target = null;
            float lastDist = float.MaxValue;
            foreach (var player in PlayerManager.PlayerAgentsInLevel)
            {
                float dist = Vector3.Distance(player.EyePosition, transform.position);
                if (dist <= lastDist)
                {
                    target = player;
                }
                lastDist = dist;
            }

            return target;
        }

        void LookAtTarget()
        {
            Vector3 relativePos = Target.Position - transform.position;
            relativePos.z = 0;
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = rotation;
        }
    }

    public enum SnowmanState
    {
        Idle
    }
}
