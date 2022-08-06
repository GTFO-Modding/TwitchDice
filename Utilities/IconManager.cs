using AssetShards;
using System.Collections.Generic;
using System.Reflection;
using TwitchDice.Twitch;
using UnityEngine;

namespace TwitchDice.Utilities
{
    public static class IconManager
    {
        private static readonly IconInfo[] s_iconPaths = new IconInfo[]
        {
            new("Icons.D3.png", 430, 430),
            new("Icons.D4.png", 430, 430),
            new("Icons.D6.png", 430, 430),
            new("Icons.D8.png", 430, 430),
            new("Icons.D12.png", 430, 430),
            new("Icons.D20.png", 430, 430),
            new("Icons.D50.png", 430, 430),
            new("Icons.D100.png", 430, 430)
        };
        private static readonly Dictionary<string, Sprite> s_iconMap = new();

        internal static void Init()
        {
            AssetShardManager.add_OnStartupAssetsLoaded((System.Action)OnStartupAssetsLoaded);
        }

        private static void OnStartupAssetsLoaded()
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (IconInfo iconInfo in s_iconPaths)
            {
                byte[] result;
                using (System.IO.Stream stream = assembly.GetManifestResourceStream($"TwitchDice.Assets.{iconInfo.path}")!)
                {
                    result = new byte[stream.Length - stream.Position];
                    stream.Read(result);
                }

                Texture2D texture = new Texture2D(iconInfo.width, iconInfo.height);
                if (!texture.LoadImage(result))
                {
                    Log.Error($"Failed to load icon '{iconInfo.path}'");
                    continue;
                }

                texture.Apply();
                Sprite? sprite = Sprite.Create(texture, new Rect(0, 0, iconInfo.width, iconInfo.height), Vector2.one * 0.5f, 100f);
                if (sprite == null)
                {
                    Log.Error($"Failed to create sprite for icon '{iconInfo.path}'");
                    continue;
                }

                s_iconMap.Add(iconInfo.path, sprite);
            }
        }

        public static Sprite? GetDiceIconForTier(DiceTier tier)
        {
            string iconName = $"Icons.{tier}.png";
            return s_iconMap.TryGetValue(iconName, out Sprite? sprite) ? sprite : null;
        }

        private struct IconInfo
        {
            public int width;
            public int height;
            public string path;

            public IconInfo(string path, int width, int height)
            {
                this.path = path;
                this.width = width;
                this.height = height;
            }
        }
    }
}
