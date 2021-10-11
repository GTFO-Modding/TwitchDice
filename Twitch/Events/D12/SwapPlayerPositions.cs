using Player;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D12
{
    public class SwapPlayerPositions : DiceEvent<SwapTargetPosition>
    {
        public override string EventName => "Swap!";

        public override string EventID => "swap";

        protected override DiceTier DiceTier => DiceTier.D12;

        private int offset = 0;

        public override bool CanBeTriggered()
        {
            return PlayerUtil.PlayerCount > 1;
        }

        public override void ReceiveClient(ulong sender, SwapTargetPosition packet)
        {
            PlayerUtil.TeleportToPosition(PlayerUtil.LocalPlayerAgent, new Vector3(packet.x, packet.y, packet.z));
        }

        public override void TriggerHost()
        {
            List<PlayerCard> playerCards = new List<PlayerCard>();
            foreach (var player in PlayerManager.PlayerAgentsInLevel)
            {
                playerCards.Add(new PlayerCard(player, player.IsLocallyOwned, player.Position));
            }

            playerCards.Shuffle();

            for (int i = 1; i <= playerCards.Count; i++)
            {
                if (i != playerCards.Count)
                {
                    playerCards[i].Position = playerCards[i + 1].Position;
                } else
                {
                    playerCards[0].Position = playerCards[i].Position;
                }
            }

            foreach (var player in playerCards)
            {
                if (player.IsHost)
                {
                    PlayerUtil.TeleportToPosition(player.Player, player.Position);
                } else
                {
                    TriggerClient(new SwapTargetPosition(player.Position));
                }
            }
        }

        class PlayerCard
        {
            public PlayerAgent Player;
            public bool IsHost;
            public Vector3 Position;

            public PlayerCard(PlayerAgent player, bool isHost, Vector3 position)
            {
                Player = player;
                IsHost = isHost;
                Position = position;
            }
        }
    }

    public struct SwapTargetPosition
    {
        public SwapTargetPosition(Vector3 pos)
        {
            x = pos.x;
            y = pos.y;
            z = pos.z;
        }
        public float x;
        public float y;
        public float z;
    }
}
