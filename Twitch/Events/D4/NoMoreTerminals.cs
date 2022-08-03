using LevelGeneration;

namespace TwitchDice.Twitch.Events.D4
{
    public class NoMoreTerminals : DiceEvent<NMT>
    {
        public override string EventName => "No More TERMINALS!";
        public override string EventDescription => "Kicks players using the terminal.";
        public override string EventID => "terminalKick";

        protected override DiceTier DiceTier => DiceTier.D4;

        public override bool CanBeTriggered()
        {
            int playersUsingTerminals = 0;
            foreach (LG_ComputerTerminal terminal in LG_ComputerTerminalManager.Current.m_terminals.Values)
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
            TriggerCommon();
        }

        private static void TriggerCommon()
        {
            foreach (LG_ComputerTerminal terminal in LG_ComputerTerminalManager.Current.m_terminals.Values)
            {
                if (terminal.m_hasInteractingPlayer)
                {
                    terminal.DoExitFPSView();
                }
            }
        }

        public override void TriggerHost()
        {
            TriggerCommon();
        }
    }

    public struct NMT
    { }
}
