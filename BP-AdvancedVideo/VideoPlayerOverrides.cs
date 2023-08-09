using BrokeProtocol.API;
using BrokeProtocol.Collections;
using BrokeProtocol.Entities;
using BrokeProtocol.Required;
using BrokeProtocol.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace BPAdvancedVideo
{
    class VideoPlayerOverrides
    {

        private const string videoPanel = "videoPanel";
        private const string customVideo = "customVideo";
        private const string searchVideo = "searchVideo";

        private const string stopVideo = "stopVideo";



        [Target(GameSourceEvent.PlayerVideoPanel, ExecutionMode.Override)]
        public void OnVideoPanel(ShPlayer player, ShEntity videoEntity)
        {
            var options = new List<LabelID>();

            if (VideoPermission(player, videoEntity, PermEnum.VideoCustom))
            {
                options.Add(new LabelID("&6Search for YoutubeVideo", searchVideo));
            }

            if (VideoPermission(player, videoEntity, PermEnum.VideoCustom))
            {
                options.Add(new LabelID("&6Custom Video URL", customVideo));
            }

            if (VideoPermission(player, videoEntity, PermEnum.VideoStop))
            {
                options.Add(new LabelID("&4Stop Video", stopVideo));
            }

            if (VideoPermission(player, videoEntity, PermEnum.VideoDefault))
            {
                int index = 0;
                foreach (var option in player.manager.svManager.videoOptions)
                {
                    options.Add(new LabelID(option.label, index.ToString()));
                    index++;
                }
            }

            string title = "&7Video Panel";
            player.svPlayer.SendOptionMenu(title, videoEntity.ID, videoPanel, options.ToArray(), new LabelID[] { new LabelID("Select", string.Empty) });
        }

        private bool VideoPermission(ShPlayer player, ShEntity videoPlayer, PermEnum permission) {

            if(videoPlayer)
            {
                return player.svPlayer.HasPermission($"{Core.Instance.Info.GroupNamespace}.{(player.InOwnApartment ? "apartment" : "external")}.{permission.ToString()}");
            }
            return false;

        }


        [Target(GameSourceEvent.PlayerOptionAction, ExecutionMode.Test)]
        public bool OnOptionAction(ShPlayer player, int targetID, string menuID, string optionID, string actionID)
        {
            switch (menuID)
            {
                case videoPanel:
                    var videoEntity = EntityCollections.FindByID(targetID);

                    if (optionID == customVideo && VideoPermission(player, videoEntity, PermEnum.VideoCustom))
                    {
                        player.svPlayer.SendGameMessage("BP-AdvancedVideo will try to resolve any URL!");
                        player.svPlayer.SendInputMenu("Video Link", targetID, customVideo, InputField.ContentType.Standard, 128);
                    }
                    else if (optionID == searchVideo && VideoPermission(player, videoEntity, PermEnum.VideoCustom))
                    {
                        player.svPlayer.SendInputMenu("Search for Youtube Video", targetID, searchVideo, InputField.ContentType.Standard, 128);
                    }
                    else if (optionID == stopVideo && VideoPermission(player, videoEntity, PermEnum.VideoStop))
                    {
                        videoEntity.svEntity.SvStopVideo();
                        player.svPlayer.DestroyMenu(videoPanel);
                    }
                    else if (VideoPermission(player, videoEntity, PermEnum.VideoDefault) && int.TryParse(optionID, out var index))
                    {
                        videoEntity.svEntity.SvStartDefaultVideo(index);
                        player.svPlayer.DestroyMenu(videoPanel);
                    }
                    return false;
                default:
                    return true;
            }
        }

        [Target(GameSourceEvent.PlayerSubmitInput, ExecutionMode.Test)]
        public bool OnSubmitInput(ShPlayer player, int targetID, string menuID, string input)
        {
            switch (menuID)
            {
                case customVideo:
                    var videoEntity = EntityCollections.FindByID(targetID);

                    if (VideoPermission(player, videoEntity, PermEnum.VideoCustom) && input.StartsWith("https://"))
                    {

                        Core.Instance.LinkResolver.PlayDirectURL(input, videoEntity.svEntity.SvStartCustomVideo);

                    }
                    else
                    {
                        player.svPlayer.SendGameMessage("Must have permission and start with 'https://'");
                    }
                    return false;
                case searchVideo:
                    var videoEntity2 = EntityCollections.FindByID(targetID);

                    if (VideoPermission(player, videoEntity2, PermEnum.VideoCustom))
                    {
                        Core.Instance.LinkResolver.PlayDirectURL("ytsearch:" + input, videoEntity2.svEntity.SvStartCustomVideo);

                    }
                    else
                    {
                        player.svPlayer.SendGameMessage("Must have permission!'");
                    }
                    return false;
                default:
                    return true;

            }
        } // it just decided to suicide

     //    IEnumerator ResolveVideo(ShPlayer player, ShEntity videoEntity, string url) // ok so what is this feetus // gogole aitn taht depe // ill try to read and see if i understand anything
     // 
     //    {
     // 
     //        yield return null;
     // 
     //    } // so basically a lot of people callnig eachother back and forth becaues yes till it works
     // 

    }
}
