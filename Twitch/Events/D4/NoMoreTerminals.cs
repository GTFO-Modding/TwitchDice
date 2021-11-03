using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;

namespace TwitchDice.Twitch.Events.D4
{
    public class NoMoreTerminals : DiceEvent<NMT>
    {
        public override string EventName => "No More TERMINALS!";

        public override string EventID => "terminalKick";

        protected override DiceTier DiceTier => DiceTier.D4;

        public override bool CanBeTriggered()
        {
            int playersUsingTerminals = 0;
            foreach (var terminal in LG_ComputerTerminalManager.Current.m_terminals.Values)
            {
                if (terminal.m_hasInteractingPlayer)
                {
                    playersUsingTerminals++;
                }
            }
            return playersUsingTerminals > 0;
        }

        public override void ReceiveClient(ulong sender, NMT packet)
        {
            this.TriggerCommon();
        }

        private void TriggerCommon()
        {
            foreach (var terminal in LG_ComputerTerminalManager.Current.m_terminals.Values)
            {
                if (terminal.m_hasInteractingPlayer)
                {
                    terminal.DoExitFPSView();
                }
            }
        }

        public override void TriggerHost()
        {
            this.TriggerCommon();
        }
    }

    public struct NMT
    { }
}
