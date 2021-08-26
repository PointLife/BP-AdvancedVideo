using System.Collections.Generic;

namespace BPAdvancedVideo.Models.SettingsModel
{
    public class Settings
    {
        public General General { get; set; }
    }

    public class General
    {
        public string SettingsVersion { get; set; }

    }

}