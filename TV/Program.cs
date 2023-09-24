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
    partial class Program : MyGridProgram
    {
        //=======================================================================
        TV tv;

        public Program()
        {
            GridInfo.Init("TV",GridTerminalSystem,IGC,Me,Echo);
            if(Storage != "") GridInfo.Load(Storage);
            tv = new TV();
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {
            Storage = GridInfo.Save();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            //Echo("TV: Main");
            if(GridBlocks.Couch.IsUnderControl)
            {
                //Echo("TV: Main: Couch is under control");
                tv.Play();
            }
            else
            {
                //Echo("TV: Main: Couch is not under control");
                tv.Idle();
            }
        }
        //=======================================================================
    }
}
