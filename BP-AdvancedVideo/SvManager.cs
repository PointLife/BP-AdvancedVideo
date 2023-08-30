using BrokeProtocol.API;
using BrokeProtocol.Entities;
using BrokeProtocol.Managers;
using System;

namespace BPAdvancedVideo.RegisteredEvents
{
    public class OnStarted : IScript
    {
        [Target(GameSourceEvent.ManagerStart, ExecutionMode.Additive)]
        public void OnEvent()
        {
            Core.Instance.SvManager = SvManager.Instance ;

            CommandHandler.RegisterCommand("ForceApi", new Action<ShPlayer>(Command1));

        }


        public void Command1(ShPlayer player)
        {
            Core.Instance.LinkResolver.BinaryFound = !Core.Instance.LinkResolver.BinaryFound;
        }
    }
}