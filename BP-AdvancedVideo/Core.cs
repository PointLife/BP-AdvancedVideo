using BPAdvancedVideo.Models.SettingsModel;
using BPCoreLib.Interfaces;
using BPCoreLib.PlayerFactory;
using BPCoreLib.Util;
using BrokeProtocol.API;
using BrokeProtocol.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YouTube_DL_Handler;

namespace BPAdvancedVideo
{
    public class Core : Plugin
    {
        public static Core Instance { get; internal set; }

        public static string Version { get; } = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

        public static string Git { get; } = "https://github.com/Pointlife/BP-AdvancedVideo";

        public static string[] Authors { get; } = { "PLASMA_chicken", "RandomowyTyp" };

        public ILogger Logger { get; } = new Logger();

        public Paths Paths { get; } = new Paths();

        public IReader<Settings> SettingsReader { get; } = new Reader<Settings>();

        public Settings Settings => SettingsReader.Parsed;

        public I18n I18n { get; set; }

        public SvManager SvManager { get; set; }

        public LinkResolver LinkResolver { get; set; }

        public Core()
        {
            Instance = this;
            Info = new PluginInfo("BP-AdvancedVideo", "bpadvvideo")
            {
                Description = "Video Handler by PointLife Development Team \n Authors: [ PLASMA_chicken and RandmowyTyp ]",
                Website = "Yes"
            };
            OnReloadRequestAsync();
            SetCustomData();

            LinkResolver = new LinkResolver();

            EventsHandler.Add("bpe:reload", new Action(OnReloadRequestAsync));
            EventsHandler.Add("bpe:version", new Action<string>(OnVersionRequest));
            Logger.LogInfo($"BP-AdvancedVideo {(IsDevelopmentBuild() ? "[DEVELOPMENT-BUILD] " : "")}v{Version} loaded in successfully!");
        }

        private void SetCustomData()
        {
            CustomData.AddOrUpdate("version", Version);
            CustomData.AddOrUpdate("devbuild", IsDevelopmentBuild());
        }

        public static bool IsDevelopmentBuild()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public void SetConfigurationFilePaths()
        {
            SettingsReader.Path = Paths.SettingsFile;
        }

        public void ReadConfigurationFiles()
        {
            SettingsReader.ReadAndParse();
        }

        public async void OnReloadRequestAsync()
        {
            SetConfigurationFilePaths();
            await FileChecker.CheckFiles();
            ReadConfigurationFiles();
        }

        public void OnVersionRequest(string callback)
        {
            if (callback.StartsWith(Core.Instance.Info.GroupNamespace + ":"))
            {
                return;
            }
            EventsHandler.Exec(callback, Version, IsDevelopmentBuild());
        }
    }
}
