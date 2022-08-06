using System;
using System.Collections.Generic;
using System.Text;

namespace TwitchDice
{
    public static class Log
    {
        public static void Verbose(object msg)
        {
            if (Main.Verbose) Main.log.LogDebug(msg);
        }
        public static void Debug(object msg)
        {
            Main.log.LogDebug(msg);
        }

        public static void Error(object msg)
        {
            Main.log.LogError(msg);
        }

        public static void Warning(object msg)
        {
            Main.log.LogWarning(msg);
        }

        public static void Message(object msg)
        {
            Main.log.LogMessage(msg);
        }
    }
}
