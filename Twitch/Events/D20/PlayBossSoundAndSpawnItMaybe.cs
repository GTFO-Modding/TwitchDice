using AK;
using Enemies;
using GameData;
using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D20
{
    public class PlayBossSoundAndSpawnItMaybe : DiceEvent<SoundTheseBalls>
    {
        public override string EventName => "Surprise!";

        public override string EventID => "surpriseme";

        protected override DiceTier DiceTier => DiceTier.D20;

        public override void ReceiveClient(ulong sender, SoundTheseBalls packet)
        {
            CellSound.Post(EVENTS.BIRTHERGIVEBIRTH, PlayerUtil.LocalPlayerAgent.Position);
        }

        public override void TriggerHost()
        {
            CellSound.Post(EVENTS.BIRTHERGIVEBIRTH, PlayerUtil.LocalPlayerAgent.Position);

            float chance = UnityEngine.Random.Range(0f, 1f);
            if (chance < 0.5f)
            {
                // we do a little trolling

                if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent randomPlayer))
                {
                    var portals = randomPlayer.CourseNode.m_portals;
                    var randomPortal = portals[UnityEngine.Random.Range(0, portals.Count)];
                    var node = randomPortal.m_nodeB;
                    EnemyAllocator.Current.SpawnEnemy(37U, node, Agents.AgentMode.Agressive, node.Position, Quaternion.identity);
                }
                
            }

            this.TriggerClient();

        }
    }

    public struct SoundTheseBalls
    { }
}
