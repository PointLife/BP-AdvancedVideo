using BPAdvancedVideo;
using NetChalker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace YouTube_DL_Handler
{
    public class LinkResolver
    {

        public static Chalker ChalkerInstance { get; set; }

        public bool BinaryFound { get; internal set; } = false;

        public LinkResolver(bool forceApi = false )
        {
            ChalkerInstance = new Chalker();
            ChalkerInstance.WriteMessage("Starting up Link Resolver");
            if(!forceApi)
            {
                BinaryCheck();
            }
        }

        public void BinaryCheck()
        {
            ChalkerInstance.WriteMessage("Checking for youtube-dl execution!");

            try
            {

                var processInfo = new ProcessStartInfo("youtube-dl");
                ChalkerInstance.WriteMessage(ConsoleColor.Yellow, "RUN", $"{processInfo.FileName} {processInfo.Arguments}");

                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;

                var process = Process.Start(processInfo);

                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    Console.WriteLine("output>> " + e.Data);
                process.BeginOutputReadLine();

                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    Console.WriteLine("error>> " + e.Data);
                process.BeginErrorReadLine();

                process.WaitForExit();

                ChalkerInstance.WriteMessage(ConsoleColor.DarkYellow, "EXIT", "ExitCode: " + process.ExitCode);
                process.Close();
                BinaryFound = true;

            }

            catch (Win32Exception ex)
            {
                ChalkerInstance.WriteError("Binary not Found!");
                ChalkerInstance.WriteMessage("Falling back to API");

            }
            catch (Exception ex)
            {
                ChalkerInstance.WriteError(ex.ToString());
                ChalkerInstance.WriteError(ex.GetType().ToString());

            }
            finally
            {
                ChalkerInstance.WriteMessage("Binary Existance Check Complete");
            }
        }

        public void GetDirectURL(string url, System.Action<string> callback)
        {
            if (BinaryFound)
            {
                Core.Instance.SvManager.StartCoroutine(YoutubeDLGetURL(url, callback));
            }
            else
            {
                Core.Instance.SvManager.StartCoroutine(ApiFetch(url, callback));
            }
        }


        private IEnumerator ApiFetch(string url, System.Action<string> callback)
        {
            ChalkerInstance.WriteMessage("API");
            var uri = $"http://sv1.pointlife.net:3050/v1/video?url={url}&schema=url&schema=entries";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                ChalkerInstance.WriteMessage(ConsoleColor.DarkMagenta, "REQUEST", "TO: " + webRequest.uri);

                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.Success:
                        callback(HandleJson(webRequest.downloadHandler.text));

                        yield break;
                    default:
                        ChalkerInstance.WriteMessage(webRequest.error);
                        ChalkerInstance.WriteMessage(webRequest.responseCode.ToString());
                        yield break;

                }
            }
        }

        private IEnumerator YoutubeDLGetURL(string url, Action<string> callback)
        {

            var processInfo = new ProcessStartInfo($"youtube-dl", $"-f best --verbose --dump-single-json \"{url}\"");
            ChalkerInstance.WriteMessage(ConsoleColor.Yellow, "RUN", $"{processInfo.FileName} {processInfo.Arguments}");
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;

            var process = Process.Start(processInfo);
            var output = "";

            var waitItem = new WaitUntil(() => process.HasExited);


#if DEBUG
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
               Console.WriteLine("output>> " + e.Data);
#endif
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                output += e.Data;
            process.BeginOutputReadLine();
#if DEBUG

            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => 
                Console.WriteLine("error>> " + e.Data);
#endif
            process.BeginErrorReadLine();

            yield return waitItem;

            ChalkerInstance.WriteMessage(ConsoleColor.DarkYellow, "EXIT", "ExitCode: " + process.ExitCode);

            int exitcode = process.ExitCode;
            process.Close();
            if (exitcode == 0)
            {
                callback(HandleJson(output));
                yield break;
            }
            ChalkerInstance.WriteError("Failed to fetch Data!");
            callback(url);
        }

        internal void PlayDirectURL(string url, Action<string> svStartCustomVideo)
        {
            if (BinaryFound)
            {
                svStartCustomVideo("https://pointlife.net/sv1/AdvancedVideo/loading.mp4");
            }
            else
            {
                svStartCustomVideo("https://pointlife.net/sv1/AdvancedVideo/loading_api.mp4");
            }
            GetDirectURL(url, svStartCustomVideo);
        }

        private string HandleJson(string output)
        {
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<YtDlAPIResponse>(output);
            if (json.url != null)
            {
                var responseString = Newtonsoft.Json.JsonConvert.SerializeObject(json);

                ChalkerInstance.WriteMessage(ConsoleColor.Magenta, "RESPONSE", "DATA: " + responseString.Substring(0, 50) + (responseString.Length > 50 ? "..." : ""));

                return json.url;
            }
            else if (json.entries != null)
            {
                var responseString = Newtonsoft.Json.JsonConvert.SerializeObject(json.entries[0]);

                ChalkerInstance.WriteWarning("Fetching First Entry from Playlist");
                foreach (var entry in json.entries)
                {
                    ChalkerInstance.WriteMessage(ConsoleColor.DarkBlue, "INTEL", entry.title);
                }

                ChalkerInstance.WriteMessage(ConsoleColor.Magenta, "RESPONSE", "DATA: " + responseString.Substring(0, 50) + (responseString.Length > 50 ? "..." : ""));

                return json.entries[0].url;
            }
            return "";
        }
    }

    internal class YtDlAPIResponse
    {
        public string url;
        public string title;
        public YtDlAPIResponse[] entries;
    }
}

// for anyone reading this, i hope our pain and suffering satisfy your needs - random