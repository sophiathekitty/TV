using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class DownloadMenu : ScreenMenu
        {
            public DownloadMenu(float width, ScreenActionBar actionBar) : base("Available", width, actionBar)
            {
            }
            public override void AddToScreen(Screen screen)
            {
                GridInfo.IGC.SendBroadcastMessage("ReportShows", "");
                menuItems.Clear();
                foreach (var show in SceneCollection.remoteShows)
                {
                    if (show.Value.blocks < SceneCollection.unused.Count)
                    {
                        AddLabel(show.Value.name);
                    }
                }
                base.AddToScreen(screen);
            }
            public override string HandleInput(string argument)
            {
                string action = base.HandleInput(argument);
                if (SceneCollection.remoteShows.ContainsKey(action))
                {
                    GridInfo.IGC.SendUnicastMessage(SceneCollection.remoteShows[action].availableFrom, "Download", SceneCollection.remoteShows[action].name);
                    return "back";
                }
                return action;
            }
        }
    }
}
