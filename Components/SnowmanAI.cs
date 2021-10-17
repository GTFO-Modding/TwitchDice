using AIGraph;
using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;
using UnhollowerBaseLib;
using TwitchDice.CustomSounds.TAK;
using AK;
using Nidhogg.Managers;

namespace TwitchDice.Components
{
    public class SnowmanAI : MonoBehaviour
    {
        public SnowmanAI(IntPtr intPtr) : base(intPtr) { }

        const float MaxHealth = 50;
        const float DistFromPlayer = 2;

        public SnowmanState State 
        { 
            get => _state;
            private set
            {
                Log.Debug($"Snowman state change {_state} -> {value}");
                _state = value;
            }
        }

        public float HealthPercent
        {
            get
            {
                return Health / MaxHealth;
            }
        }

        public float StateTimer { get; private set; }
        public PlayerAgent Target 
        { 
            get
            {
                if (_target != null) return _target;
                _target = GetClosestPlayer();
                return _target;
            }
            private set 
            {
                _target = value;
            } 
        }
        public bool IsSeen { get; private set; }
        public GenericDamageComponent Damage { get; private set; }
        public Interact_Timed Interact { get; private set; }
        public float Health { get; private set; }


        GameObject Eyebrows;
        GameObject Smile;
        GameObject Frown;
        GameObject Interaction;
        private SnowmanState _state;
        private PlayerAgent _target;
        private float nextClientUpdate;
        Vector3 teleportPosition;

        void Awake()
        {
            Smile = transform.Find("Head/smile").gameObject;
            Frown = transform.Find("Head/sad").gameObject;
            Eyebrows = transform.Find("Head/angy").gameObject;
            Interaction = transform.Find("Interaction").gameObject;
            gameObject.AddComponent<DestroyOnCleanUp>();

            NetworkingManager.RegisterEvent<PSnowmanState>(typeof(PSnowmanState).Name, OnClientStateUpdate);
            NetworkingManager.RegisterEvent<PSnowmanHeal>(typeof(PSnowmanHeal).Name, OnClientHeal);
            SetupInteraction();
            State = SnowmanState.Client;

            if (!SNet.IsMaster) return;
            SetupDamage();
            State = SnowmanState.Idle;
            StateTimer = 0;
            Target = GetClosestPlayer();
            UpdateRotation();
        }

        void SetupInteraction()
        {
            Damage = gameObject.AddComponent<GenericDamageComponent>();
            Damage.add_OnGenericDamageTaken((Il2CppSystem.Action<float>)OnDamage);
            Health = MaxHealth;
        }

        void SetupDamage()
        {
            Interaction.layer = LayerManager.LAYER_INTERACTION;
            Interact = Interaction.AddComponent<Interact_Timed>();
            Interact.m_colliderToOwn = Interaction.GetComponent<Collider>();
            Interact.InteractDuration = 30;
            Interact.InteractionMessage = "Heal Friend";
            Interact.SFXInteractCancel = TEVENTS.PLAY_VINEBOOM;
            Interact.SFXInteractEnd = EVENTS.EXPEDITION_FAILED_SCREEN_JUMP_SCARE;
            Interact.OnlyActiveWhenLookingStraightAt = false;
            Interact.AbortOnDotOrDistanceDiff = false;
            Interact.add_OnInteractionTriggered((Il2CppSystem.Action<PlayerAgent>)OnInteractDone);
        }

        void OnClientStateUpdate(ulong sender, PSnowmanState state)
        {
            transform.rotation = state.Rotation;
            transform.position = state.Position;
            Health = state.Health;
            Log.Debug($"State Update\n------\nRot: {state.Rotation}, Pos: {state.Position}, Health {Health}");
        }

        void OnClientHeal(ulong sender, PSnowmanHeal hela)
        {
            Health = MaxHealth;
            Log.Debug("Client Heal");
        }

        void OnDamage(float damage)
        {
            if (!SNet.IsMaster) return;
            Health -= damage;
            if (Health < 0) Health = 0;
            UpdateClientState();
        }

        void OnInteractDone(PlayerAgent agent)
        {
            if (SNet.IsMaster) 
                Health = MaxHealth;
            else
                NetworkingManager.InvokeEvent(typeof(PSnowmanHeal).Name, new PSnowmanHeal());
            Log.Debug("Interact Done");
        }

        void Update()
        {
            UpdateVisuals();
            Interact.SetActive(Health != MaxHealth);

            if (!SNet.IsMaster) return;
            IsSeen = IsBeingLookedAt();
            if (!IsSeen)
            {
                UpdateRotation();
            }

            switch (State)
            {
                case SnowmanState.Idle:
                    if (!IsSeen)
                    {
                        State = SnowmanState.LookingForNodes;
                    }
                    break;

                case SnowmanState.LookingForNodes:
                    if (IsSeen)
                    {
                        State = SnowmanState.Idle;
                    }
                    foreach (var node in Target.CourseNode.m_nodeCluster.m_nodes)
                    {
                        bool validNode = CanPositionBeSeen(node.Position);
                        teleportPosition = node.Position;
                        if (validNode && Vector3.Distance(Target.Position, node.Position) > DistFromPlayer)
                        {
                            State = SnowmanState.Teleport;
                            break;
                        }
                    }
                    break;

                case SnowmanState.Teleport:
                    transform.position = teleportPosition;
                    State = SnowmanState.TeleportCooldown;
                    StateTimer = Clock.Time + 15;
                    UpdateClientState();
                    break;

                case SnowmanState.TeleportCooldown:
                    if (StateTimer > Clock.Time) break;
                    State = SnowmanState.Idle;
                    break;
            }

            if (nextClientUpdate < Clock.Time)
            {
                UpdateClientState();
            }
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

        bool IsBeingLookedAt()
        {
            return CanPositionBeSeen(transform.position);
        }

        bool CanPositionBeSeen(Vector3 position)
        {
            foreach (var player in PlayerManager.PlayerAgentsInLevel)
            {
                Vector3 directionToTarget = player.Position - position;
                float dis = Vector3.Distance(player.Position, position);
                float angle = Vector3.Angle(player.transform.forward, directionToTarget);
                if (angle > 80 || dis < DistFromPlayer) return true;
            }
            return false;
        }

        void UpdateRotation()
        {
            Vector3 relativePos = Target.Position - transform.position;
            relativePos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            transform.rotation = rotation;
        }

        void UpdateVisuals()
        {
            bool smile = true;
            bool frown = false;
            bool brows = false;
            if (HealthPercent < 0.5)
            {
                smile = false;
                frown = true;
            }

            if (HealthPercent < 0.2)
            {
                brows = true;
            }

            Smile.SetActive(smile);
            Frown.SetActive(frown);
            Eyebrows.SetActive(brows);
        }

        void UpdateClientState()
        {
            PSnowmanState state = new PSnowmanState()
            {
                Health = Health,
                Position = transform.position,
                Rotation = transform.rotation
            };
            NetworkingManager.InvokeEvent(typeof(PSnowmanState).Name, state);
            nextClientUpdate = Clock.Time + 1;
        }
    }

    public enum SnowmanState
    {
        Idle,
        LookingForNodes,
        Teleport,
        TeleportCooldown,
        Client
    }

    public struct PSnowmanState
    {
        public int Instance;
        public float Health;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public struct PSnowmanHeal
    {

    }
}
