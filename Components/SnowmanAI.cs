using AIGraph;
using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using TwitchDice.Utilities;
using UnityEngine;
using AK;
using Gear;
using GTFO.API;
using TwitchDice.Extensions;

namespace TwitchDice.Components
{
    [Il2cppInterface(typeof(iResourcePackReceiver))]
    public class SnowmanAI : MonoBehaviour
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SnowmanAI(IntPtr intPtr) : base(intPtr)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        { }

        const float MAX_HEALTH = 50;
        const float DistFromPlayer = 2;
        const int TeleportCooldown = 5;
        const float BaseAttackDamage = 1.5f;
        const float AttackMulti = 1.5f;

        public SnowmanState State 
        { 
            get => this._state;
            private set
            {
                Log.Debug($"Snowman state change {this._state} -> {value}");
                this._state = value;
            }
        }

        public float HealthPercent => this.Health / this.MaxHealth;

        public float StateTimer { get; private set; }
        public PlayerAgent Target 
        { 
            get
            {
                if (this._target == null)
                {
                    this._target = this.GetClosestPlayer();
                }

                return this._target;
            }
            private set => this._target = value;
        }
        public bool IsSeen { get; private set; }
        public GenericDamageComponent Damage { get; private set; }
        public Interact_Timed Interact { get; private set; }
        public float MaxHealth { get; private set; }
        public bool IsAngry => this.HealthPercent <= 0.2;
        public float AttackDamage { get; private set; }
        public float Health
        {
            get => this._health;
            private set => this._health = Mathf.Clamp(value, 0, this.MaxHealth);
        }


        private GameObject Eyebrows;
        private GameObject Smile;
        private GameObject Frown;
        private GameObject Interaction;
        private SnowmanState _state;
        private PlayerAgent _target;
        private float _health;
        private float nextClientUpdate;

        private void Awake()
        {
            this.Smile = this.transform.Find("Head/smile").gameObject;
            this.Frown = this.transform.Find("Head/sad").gameObject;
            this.Eyebrows = this.transform.Find("Head/angy").gameObject;
            this.Interaction = this.transform.Find("Interaction").gameObject;
            this.MaxHealth = MAX_HEALTH;
            this.gameObject.AddComponent<DestroyOnCleanUp>();
            this.AttackDamage = BaseAttackDamage;

            NetworkAPI.RegisterEvent<PSnowmanState>(typeof(PSnowmanState).Name, this.OnClientStateUpdate);
            NetworkAPI.RegisterEvent<PSnowmanHeal>(typeof(PSnowmanHeal).Name, this.OnClientHeal);
            NetworkAPI.RegisterEvent<PSnowmanDamage>(typeof(PSnowmanDamage).Name, this.OnClientDamage);
            this.SetupInteraction();
            this.SetupDamage();
            this.State = SnowmanState.Client;

            if (!SNet.IsMaster)
            {
                return;
            }

            this.State = SnowmanState.Idle;
            this.StateTimer = 0;
            this.Target = this.GetClosestPlayer();
            this.UpdateRotation();
        }

        private void SetupDamage()
        {
            this.Damage = this.gameObject.AddComponent<GenericDamageComponent>();
            this.Damage.add_OnGenericDamageTaken((Il2CppSystem.Action<float>)this.OnDamage);
            this.Health = this.MaxHealth;
        }

        private void SetupInteraction()
        {
            this.Interaction.layer = LayerManager.LAYER_INTERACTION;
        }

        public void GiveAmmoRel(float ammoStandardRel, float ammoSpecialRel, float ammoClassRel) { }

        public void GiveDisinfection(float disinfection) { }

        public void GiveHealth(float health) 
        {
            if (SNet.IsMaster)
            {
                this.HealSnowman();
            }
            else
            {
                NetworkAPI.InvokeEvent(typeof(PSnowmanHeal).Name, new PSnowmanHeal());
            }
        }

        public bool NeedDisinfection() => false;

        public bool NeedHealth() => this.Health < this.MaxHealth;

        public bool NeedToolAmmo() => false;

        public bool NeedWeaponAmmo() => false;

        public string InteractionName => "Snowfriend :)";

        public bool IsLocallyOwned => false;

        private void OnClientStateUpdate(ulong sender, PSnowmanState state)
        {
            this.transform.rotation = state.Rotation;
            this.transform.position = state.Position;
            this.Health = state.Health;
            Log.Debug($"State Update\n------\nRot: {state.Rotation}, Pos: {state.Position}, Health {this.Health}");
        }

        private void OnClientHeal(ulong sender, PSnowmanHeal hela)
        {
            this.HealSnowman();
            Log.Debug("Client Heal");
        }

        private void OnClientDamage(ulong sender, PSnowmanDamage damage)
        {
            if (!SNet.IsMaster)
            {
                return;
            }

            this.Health -= damage.amount;

            this.UpdateClientState();
        }

        private void OnDamage(float damage)
        {
            if (SNet.IsMaster)
            {
                this.Health -= damage;

                this.UpdateClientState();
            }
            else
            {
                NetworkAPI.InvokeEvent(typeof(PSnowmanDamage).Name, new PSnowmanDamage() { amount = damage });
            }
        }

        private void HealSnowman()
        {
            this.Health += 20;
            this.Health += 20;
            this.AttackDamage /= AttackMulti;
        }

        private void Update()
        {
            this.UpdateVisuals();
            if (!SNet.IsMaster)
            {
                return;
            }

            if (this.Target == null)
            {
                return;
            }

            this.IsSeen = this.IsBeingLookedAt();
            if (!this.IsSeen)
            {
                this.UpdateRotation();
            }

            switch (this.State)
            {
                case SnowmanState.Idle:
                    if (!this.IsSeen)
                    {
                        this.State = SnowmanState.LookingForNodes;
                    }
                    break;

                case SnowmanState.LookingForNodes:
                    if (this.IsSeen)
                    {
                        this.State = SnowmanState.Idle;
                    }
                    if (this.IsAngry)
                    {
                        if (SNet.IsMaster)
                        {
                            this.Target.Damage.ParasiteDamage(this.AttackDamage);
                            this.Target.Locomotion.AddExternalPushForce(Vector3.forward * 10);
                            this.Target.FPSCamera.AddHitReact(this.AttackDamage / (BaseAttackDamage * 2), Vector3.up, 1, true, true);
                            this.Target.Sound.Post(EVENTS.EXPEDITION_FAILED_SCREEN_JUMP_SCARE);
                            this.AttackDamage *= AttackMulti;
                        }
                        this.transform.position = this.Target.Position;
                        this.State = SnowmanState.Teleport;
                        break;
                    }

                    List<AIG_INode> validNodes = new();
                    foreach (AIG_INode node in this.Target.CourseNode.m_nodeCluster.m_nodes)
                    {
                        bool validNode = this.CanPositionBeSeen(node.Position);
                        if (validNode)
                        {
                            validNodes.Add(node);
                        }
                    }

                    if (validNodes.Count > 0)
                    {
                        AIG_INode targetNode = validNodes.GetRandomElement<AIG_INode>();
                        this.transform.position = targetNode.Position;
                        this.UpdateRotation();
                        this.State = SnowmanState.Teleport;
                    }
                    break;

                case SnowmanState.Teleport:
                    this.State = SnowmanState.TeleportCooldown;
                    this.StateTimer = Clock.Time + (TeleportCooldown * this.AttackDamage);
                    if (!this.Target.Alive)
                    {
                        if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent newTarget, true, null, true))
                        {
                            this.Target = newTarget;
                        }
                        else
                        {
                            this.State = SnowmanState.NoTarget;
                        }
                    }
                    this.UpdateClientState();
                    break;

                case SnowmanState.TeleportCooldown:
                    if (this.StateTimer > Clock.Time)
                    {
                        break;
                    }

                    this.State = SnowmanState.Idle;
                    break;
            }

            if (this.nextClientUpdate < Clock.Time)
            {
                this.UpdateClientState();
            }
        }

        private PlayerAgent GetClosestPlayer()
        {
            PlayerAgent? target = null;
            float lastDist = float.MaxValue;
            foreach (PlayerAgent player in PlayerManager.PlayerAgentsInLevel)
            {
                if (!player.Alive)
                {
                    continue;
                }

                float dist = Vector3.Distance(player.EyePosition, this.transform.position);
                if (dist <= lastDist)
                {
                    target = player;
                }
                lastDist = dist;
            }

            if (target == null)
            {
                throw new Exception("No target player could be found!");
            }

            return target;
        }

        private bool IsBeingLookedAt() => this.CanPositionBeSeen(this.transform.position);

        private bool CanPositionBeSeen(Vector3 position)
        {
            foreach (PlayerAgent player in PlayerManager.PlayerAgentsInLevel)
            {
                Vector3 directionToTarget = player.Position - position;
                float dis = Vector3.Distance(player.Position, position);
                float angle = Vector3.Angle(player.transform.forward, directionToTarget);
                if (angle > 80 || dis < DistFromPlayer)
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateRotation()
        {
            Vector3 relativePos = this.Target.Position - this.transform.position;
            relativePos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            this.transform.rotation = rotation;
        }

        private void UpdateVisuals()
        {
            bool smile = true;
            bool frown = false;
            bool brows = false;
            if (this.HealthPercent < 0.5)
            {
                smile = false;
                frown = true;
            }

            if (this.HealthPercent < 0.2)
            {
                brows = true;
            }

            this.Smile.SetActive(smile);
            this.Frown.SetActive(frown);
            this.Eyebrows.SetActive(brows);
        }

        private void UpdateClientState()
        {
            PSnowmanState state = new PSnowmanState()
            {
                Health = this.Health,
                Position = this.transform.position,
                Rotation = this.transform.rotation
            };
            NetworkAPI.InvokeEvent(typeof(PSnowmanState).Name, state);
            this.nextClientUpdate = Clock.Time + 1;
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
        public PSnowmanDamage(float amount)
        {
            this.amount = amount;
        }
    }
}
