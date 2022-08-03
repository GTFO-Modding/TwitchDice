using Enemies;
using System.Collections.Generic;
using TwitchDice.Extensions;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D4
{
    public class RandomPatrolEnemy : DiceEvent<RPE>
    {
        public override string EventName => "What the Sleeper Doing?";
        public override string EventDescription => "Makes a random enemy a patrolling enemy.";
        public override string EventID => "randomPatrol";

        protected override bool ForceDisable => true;

        protected override DiceTier DiceTier => DiceTier.D4;

        public override void ReceiveClient(ulong sender, RPE packet)
        {
            foreach (EnemyAgent enemy in GameObject.FindObjectsOfType<EnemyAgent>())
            {
                if (enemy.GlobalID == packet.EnemyID)
                {
                    this.TriggerCommon(enemy);
                    return;
                }
            }

            Log.Error($"Failed to find enemy with global id '{packet.EnemyID}'");
        }

        private void TriggerCommon(EnemyAgent enemy)
        {
            //enemy.AI.m_group.m_stateMachine.ChangeState((int)EGS.PatrolMove);
            //enemy.AI.Mode = Agents.AgentMode.Patrolling;
            //enemy.AI.m_locomotion.ChangeState(ES_StateEnum.PathMove);
            enemy.AI.m_navMeshAgent.enabled = true;
            enemy.AI.m_navMeshAgent.SetDestination(PlayerUtil.LocalPlayerAgent.Position);
            // old code [doesn't work]
            //enemy.AI.m_navMeshAgent.Move(PlayerUtil.LocalPlayerAgent.Position);
        }

        public override void TriggerHost()
        {
            List<EnemyAgent> enemies = new List<EnemyAgent>(GameObject.FindObjectsOfType<EnemyAgent>())
                .Filter((enemy) => enemy.AI.m_behaviour.m_currentStateName == EB_States.Hibernating);

            EnemyAgent randomEnemy = enemies.GetRandomElement<EnemyAgent>();

            this.TriggerCommon(randomEnemy);
            this.TriggerClient(new RPE(randomEnemy));

            //randomEnemy.AI.m_behaviour.ChangeState(EB_States.FollowingGroup);
        }
    }

    public struct RPE
    {
        public ushort EnemyID;

        public RPE(EnemyAgent agent)
        {
            this.EnemyID = agent.GlobalID;
        }
    }
}
