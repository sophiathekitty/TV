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
        public class StartMenu : ScreenMenu
        {
            public StartMenu(string title,string save_tag, float width, ScreenActionBar actionBar, bool background = true) : base(title, width, actionBar)
            {
                if(background) SetBackgroundColor(Color.Black);
                for(int i = 1; i < 4; i++) AddLabel(save_tag + " " + i);
                AddLabel("Quit Game");
            }
        }
    }
}
