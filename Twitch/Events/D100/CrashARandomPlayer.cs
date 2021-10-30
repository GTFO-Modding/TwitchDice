using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using Player;
using System.Threading.Tasks;
using UnityEngine.Diagnostics;
using UnhollowerBaseLib;

namespace TwitchDice.Twitch.Events.D100
{
    public class Crash : DiceEvent<CrashPlayer>
    {
        public override string EventName => "Unity Moment";

        public override string EventID => "crashPlayer";

        protected override DiceTier DiceTier => DiceTier.D100;

        private bool Activated = true;

        public override bool CanBeTriggered() => !Activated && PlayerUtil.PlayerCount > 1;

        public override void TriggerHost()
        {
            PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player, false);
            Activated = true;
            TriggerClient(new CrashPlayer() { Slot = player.PlayerSlotIndex });
        }

        private void CrashPlayer()
        {
            unsafe
            {
                PlayerUtil.LocalPlayerAgent.Damage.FallDamage(100000000);
                PlayerUtil.LocalPlayerAgent.Damage.FallDamage(100000000);
                PlayerUtil.LocalPlayerAgent.Damage.FallDamage(100000000);
                PlayerUtil.LocalPlayerAgent.Damage.FallDamage(100000000);
                PlayerUtil.LocalPlayerAgent.Damage.FallDamage(100000000);
                Log.Error("error");
                for (int i = 0; i < 10000000;)
                {
                    Log.Error("error");
                    Log.Error("error");
                    Log.Error("error");
                    Log.Error("error");
                    Log.Error("error");
                    Log.Error("error");
                    Log.Error("error");
                    Log.Error("error");
                    Task.Run(() =>
                    {
                        Task.Delay(700);
                        Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
                        IL2CPP.il2cpp_field_get_offset(
                            IL2CPP.il2cpp_class_get_field_from_name(
                                Il2CppClassPointerStore<Dam_EnemyDamageBase>.NativeClassPtr,
                                "hotfix deez nuts"
                            )
                        );
                    });
                }
            }
        }

        public override void ReceiveClient(ulong sender, CrashPlayer packet)
        {
            CrashPlayer();
        }
    }

    public struct CrashPlayer
    {
        public int Slot;
    }

    /*    public class CrashARandomPlayer : global::DiceEvent<PCrashARandomPlayer>
        {
            private static bool CrashedPlayer = false;
            public override bool RequireNetworking => true;

            public override bool HasNetworkData => true;

            public override string EventName => "Crash Player";

            public override string EventId => "d100_crashPlayer";

            public override DiceTier Tier => DiceTier.D100;

            public override bool CanBeTriggered()
            {
                if (PlayerUtil.PlayerCount > 1 && CrashedPlayer == false)
                {
                    CrashedPlayer = true;
                    return true;
                }
                return false;
            }

            protected override void TriggerClient(PCrashARandomPlayer NetworkInfo)
            {
                if (NetworkInfo.PlayerSlot == LocalPlayer.PlayerSlotIndex)
                {
                    //LOL
                    UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.Abort);
                }
            }

            protected override PCrashARandomPlayer TriggerHost()
            {
                if (PlayerUtil.TryGetRandomPlayerAgent(out PlayerAgent player, false))
                {
                    return new PCrashARandomPlayer() { PlayerSlot = player.PlayerSlotIndex  };
                }

                return new PCrashARandomPlayer();
            }
        }

        public struct PCrashARandomPlayer
        {
            public int PlayerSlot;
        }*/
}
