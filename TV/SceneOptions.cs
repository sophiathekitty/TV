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
        //----------------------------------------------------------------------
        // SceneOptions - encapsulates the options for a scene
        //----------------------------------------------------------------------
        public class SceneOptions
        {
            public string type;
            public int length;
            public bool loop = false;
            public string nextScene = "";
            // constructor
            public SceneOptions(string data)
            {
                string[] options = data.Split(',');
                foreach(string option in options)
                {
                    string[] var = option.Split(':');
                    if (var.Length == 2)
                    {
                        if (var[0] == "type")
                        {
                            type = var[1];
                        }
                        else if (var[0] == "length")
                        {
                            int.TryParse(var[1], out length);
                        }
                        else if (var[0] == "animation" && var[1] == "loop")
                        {
                            loop = true;
                        }
                        else if (var[0] == "nextScene")
                        {
                            nextScene = var[1];
                        }
                    }
                }
            }
        }
        //----------------------------------------------------------------------
    }
}
