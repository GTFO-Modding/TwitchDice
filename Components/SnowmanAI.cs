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
using Gear;

namespace TwitchDice.Components
{
    [Il2cppInterface(typeof(iResourcePackReceiver))]
    public class SnowmanAI : MonoBehaviour
    {
        public SnowmanAI(IntPtr intPtr) : base(intPtr) { }

        const float MaxHealth = 50;
        const float DistFromPlayer = 2;
        const int TeleportCooldown = 5;
        const float BaseAttackDamage = 1.5f;
        const float AttackMulti = 1.5f;

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
        public bool IsAngry { get => HealthPercent <= 0.2; }
        public float AttackDamage { get; private set; }


        GameObject Eyebrows;
        GameObject Smile;
        GameObject Frown;
        GameObject Interaction;
        private SnowmanState _state;
        private PlayerAgent _target;
        private float nextClientUpdate;

        void Awake()
        {
            Smile = transform.Find("Head/smile").gameObject;
            Frown = transform.Find("Head/sad").gameObject;
            Eyebrows = transform.Find("Head/angy").gameObject;
            Interaction = transform.Find("Interaction").gameObject;
            gameObject.AddComponent<DestroyOnCleanUp>();
            AttackDamage = BaseAttackDamage;

            NetworkingManager.RegisterEvent<PSnowmanState>(typeof(PSnowmanState).Name, OnClientStateUpdate);
            NetworkingManager.RegisterEvent<PSnowmanHeal>(typeof(PSnowmanHeal).Name, OnClientHeal);
            NetworkingManager.RegisterEvent<PSnowmanDamage>(typeof(PSnowmanDamage).Name, OnClientDamage);
            SetupInteraction();
            SetupDamage();
            State = SnowmanState.Client;

            if (!SNet.IsMaster) return;
            State = SnowmanState.Idle;
            StateTimer = 0;
            Target = GetClosestPlayer();
            UpdateRotation();
        }

        void SetupDamage()
        {
            Damage = gameObject.AddComponent<GenericDamageComponent>();
            Damage.add_OnGenericDamageTaken((Il2CppSystem.Action<float>)OnDamage);
            Health = MaxHealth;
        }

        void SetupInteraction()
        {
            Interaction.layer = LayerManager.LAYER_INTERACTION;
            //Interact = Interaction.AddComponent<Interact_Timed>();
            //Interact.m_colliderToOwn = Interaction.GetComponent<Collider>();
            //Interact.InteractDuration = 5;
            //Interact.InteractionMessage = "";
            //Interact.SFXInteractCancel = TEVENTS.PLAY_VINEBOOM;
            //Interact.SFXInteractEnd = EVENTS.EXPEDITION_FAILED_SCREEN_JUMP_SCARE;
            //Interact.OnlyActiveWhenLookingStraightAt = false;
            //Interact.AbortOnDotOrDistanceDiff = false;
            //Func<PlayerAgent, bool> CanInteract = CanPlayerInteract;
            //Interact.ExternalPlayerCanInteract = CanInteract;
            //Interact.add_OnInteractionTriggered((Il2CppSystem.Action<PlayerAgent>)OnInteractDone);
        }

        public void GiveAmmoRel(float ammoStandardRel, float ammoSpecialRel, float ammoClassRel) { }

        public void GiveDisinfection(float disinfection) { }

        public void GiveHealth(float health) 
        {
            if (SNet.IsMaster)
                HealSnowman();
            else
                NetworkingManager.InvokeEvent(typeof(PSnowmanHeal).Name, new PSnowmanHeal());
        }

        public bool NeedDisinfection() { return false; }

        public bool NeedHealth() { return Health < MaxHealth; }

        public bool NeedToolAmmo() { return false; }

        public bool NeedWeaponAmmo() { return false; }

        public string InteractionName { get { return "Snowfriend :)"; } }

        public bool IsLocallyOwned { get { return false; } }

        void OnClientStateUpdate(ulong sender, PSnowmanState state)
        {
            transform.rotation = state.Rotation;
            transform.position = state.Position;
            Health = state.Health;
            Log.Debug($"State Update\n------\nRot: {state.Rotation}, Pos: {state.Position}, Health {Health}");
        }

        void OnClientHeal(ulong sender, PSnowmanHeal hela)
        {
            HealSnowman();
            Log.Debug("Client Heal");
        }

        void OnClientDamage(ulong sender, PSnowmanDamage damage)
        {
            if (!SNet.IsMaster) return;
            Health -= damage.amount;
            if (Health < 0) Health = 0;
            UpdateClientState();
        }

        void OnDamage(float damage)
        {
            if (SNet.IsMaster)
            {
                Health -= damage;
                if (Health < 0) Health = 0;
                UpdateClientState();
            } else
            {
                NetworkingManager.InvokeEvent(typeof(PSnowmanDamage).Name, new PSnowmanDamage() { amount = damage });
            }
        }

        void HealSnowman()
        {
            Health += 20;
            if (Health > MaxHealth) Health = MaxHealth;
            AttackDamage /= AttackMulti;
        }

        void Update()
        {
            UpdateVisuals();

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
                    if (IsAngry)
                    {
                        if (SNet.IsMaster)
                        {
                            Log.Debug($"Try damage for {AttackDamage}");
                            Target.Damage.ParasiteDamage(AttackDamage);
                            Target.Locomotion.AddExternalPushForce(Vector3.forward * 10);
                            Target.Sound.Post(EVENTS.EXPEDITION_FAILED_SCREEN_JUMP_SCARE);
                            AttackDamage *= AttackMulti;
                        }
                        transform.position = Target.Position;
                        State = SnowmanState.Teleport;
                        break;
                    }

                    List<AIG_INode> validNodes = new List<AIG_INode>();
                    foreach (var node in Target.CourseNode.m_nodeCluster.m_nodes)
                    {
                        bool validNode = CanPositionBeSeen(node.Position);
                        if (validNode)
                        {
                            validNodes.Add(node);
                        }
                    }

                    if (validNodes.Count > 0)
                    {
                        var targetNode = validNodes.GetRandomElement<AIG_INode>();
                        transform.position = targetNode.Position;
                        UpdateRotation();
                        State = SnowmanState.Teleport;
                    }
                    break;

                case SnowmanState.Teleport:
                    State = SnowmanState.TeleportCooldown;
                    StateTimer = Clock.Time + (TeleportCooldown * AttackDamage);
                    if (!Target.Alive)
                    {
                        if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent newTarget, true, null, true))
                        {
                            Target = newTarget;
                        }
                        else State = SnowmanState.NoTarget;
                    }
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
        NoTarget,
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

    public struct PSnowmanDamage
    {
        public float amount;
    }
}
