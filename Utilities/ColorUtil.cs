using TwitchDice.Twitch;

namespace TwitchDice.Utilities
{
    public static class ColorUtil
    {
        public static string GetDiceColorForTier(DiceTier tier)
        {
            return tier switch
            {
                DiceTier.D3 => Main.COLOR_D3,
                DiceTier.D4 => Main.COLOR_D4,
                DiceTier.D6 => Main.COLOR_D6,
                DiceTier.D8 => Main.COLOR_D8,
                DiceTier.D12 => Main.COLOR_D12,
                DiceTier.D20 => Main.COLOR_D20,
                DiceTier.D50 => Main.COLOR_D50,
                DiceTier.D100 => Main.COLOR_D100,
                _ => "",
            };
        }
    }
}
