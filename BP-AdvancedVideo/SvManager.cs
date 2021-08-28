using BrokeProtocol.API;
using BrokeProtocol.Entities;
using BrokeProtocol.Managers;
using System;

namespace BPAdvancedVideo.RegisteredEvents
{
    public class OnStarted : IScript
    {
        [Target(GameSourceEvent.ManagerStart, ExecutionMode.Event)]
        public void OnEvent(SvManager svManager)
        {
            Core.Instance.SvManager = svManager;

            CommandHandler.RegisterCommand("ForceApi", new Action<ShPlayer>(Command1));

        }


        // Any optional parameters here will be optional with in-game commands too
        public void Command1(ShPlayer player)
        {
            Core.Instance.LinkResolver.BinaryFound = !Core.Instance.LinkResolver.BinaryFound;
        }
    }
}