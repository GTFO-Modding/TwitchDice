using MTFO.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchDice.Twitch;

namespace TwitchDice.Utilities
{
    public static class PathUtil
    {
        private static string? s_CustomFolder;
        private static string? s_RundownEventFolder;

        public static string CustomFolder
        {
            get
            {
                if (s_CustomFolder == null)
                {
                    s_CustomFolder = Path.Combine(ConfigManager.CustomPath, "TwitchDice");
                    if (!Directory.Exists(s_CustomFolder))
                    {
                        Directory.CreateDirectory(s_CustomFolder);
                    }
                }
                
                return s_CustomFolder;
            }
        }
        public static string RundownEventFolder
        {
            get
            {
                if (s_RundownEventFolder == null)
                {
                    s_RundownEventFolder = Path.Combine(CustomFolder, "Events");
                    if (!Directory.Exists(s_RundownEventFolder))
                    {
                        Directory.CreateDirectory(s_RundownEventFolder);
                    }
                }

                return s_RundownEventFolder;
            }
        }

        public static string GetRundownConfigPath(this IDiceEventConfig config)
        {
            return Path.Combine(RundownEventFolder, config.DiceEvent.EventName + ".json");
        }
    }
}
