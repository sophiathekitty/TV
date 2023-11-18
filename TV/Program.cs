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
using System.Text.RegularExpressions;
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
        // TV - main program
        TV tv;
        ProgramScreen screen;
        // constructor
        public Program()
        {
            GridInfo.Init("TV",GridTerminalSystem,IGC,Me,Echo);
            if(Storage != "") GridInfo.Load(Storage);
            SceneCollection.Init();
            tv = new TV();
            screen = new ProgramScreen(Me.GetSurface(0));
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            Echo("TV Booted");
        }
        // save data
        public void Save()
        {
            Storage = GridInfo.Save();
        }
        // main loop
        public void Main(string argument, UpdateType updateSource)
        {
            if (argument != "") tv.HandleInput(argument);
            else if (GridBlocks.Couch.IsUnderControl || GridBlocks.Keyboard.IsUnderControl) tv.Play();
            else tv.Idle();
            if (argument == "")
            {
                SceneCollection.Network();
                screen.Draw();
            }
        }
        //=======================================================================
    }
}
