using System.IO;

namespace BPAdvancedVideo
{
    public class Paths
    {
        public static string Folder { get; } = "PluginsConfig/AdvancedVideo";

        public string SettingsFile { get; } = Path.Combine(Folder, "settings.json");

    }
}