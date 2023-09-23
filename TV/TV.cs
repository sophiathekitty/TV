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
        public class TV : Screen
        {
            public TV() : base(GridBlocks.TV)
            {
                // setup tv stuff?
                SetScene(GridBlocks.GetSurfaceCustomData("TV"));
            }
            public void SetScene(string data)
            {
                // load the scene from the data
                // scene background monospace font image
                // length of the scene
                // animated sprite 1
                // - the start position and end position
                // - monospace font images for each frame (max like 3)
                // animated sprite 2
                // - the start position and end position
                // - monospace font images for each frame (max like 3)
            }
            public void Play()
            {
                // apply animations
                Draw();
            }
            public void Idle()
            {
                // show idle display
                Draw();
            }
        }
    }
}
