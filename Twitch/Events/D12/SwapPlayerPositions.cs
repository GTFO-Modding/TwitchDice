using Player;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Extensions;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch.Events.D12
{
    public class SwapPlayerPositions : DiceEvent<JsonVector3>
    {
        public override string EventName => "Swap!";
        public override string EventDescription => "Swaps with another random player.";
        public override string EventID => "swap";

        // broken
        protected override bool ForceDisable => true;
        protected override DiceTier DiceTier => DiceTier.D12;

        public override bool CanBeTriggered()
        {
            return PlayerUtil.PlayerCount > 1;
        }

        public override void ReceiveClient(ulong sender, JsonVector3 packet)
        {
            PlayerUtil.TeleportToPosition(PlayerUtil.LocalPlayerAgent, packet.Convert());
        }

        public override void TriggerHost()
        {
            List<PlayerCard> playerCards = new List<PlayerCard>();
            foreach (PlayerAgent player in PlayerManager.PlayerAgentsInLevel)
            {
                playerCards.Add(new PlayerCard(player, player.IsLocallyOwned, player.Position));
            }

            playerCards.Shuffle();

            try
            {
                for (int i = 1; i <= playerCards.Count; i++)
                {
                    if (i + 1 > playerCards.Count)
                    {
                        playerCards[i].Position = playerCards[i + 1].Position;
                    }
                    else
                    {
                        playerCards[0].Position = playerCards[i].Position;
                    }
                }

                foreach (PlayerCard player in playerCards)
                {
                    if (player.IsHost)
                    {
                        PlayerUtil.TeleportToPosition(player.Player, player.Position);
                    }
                    else
                    {
                        this.TriggerClient(new JsonVector3(player.Position));
                    }
                }
            } 
            catch
            {
                Log.Error("it's still fucked");
            }
        }

        class PlayerCard
        {
            public PlayerAgent Player;
            public bool IsHost;
            public Vector3 Position;

            public PlayerCard(PlayerAgent player, bool isHost, Vector3 position)
            {
                this.Player = player;
                this.IsHost = isHost;
                this.Position = position;
            }
        }
    }
}
