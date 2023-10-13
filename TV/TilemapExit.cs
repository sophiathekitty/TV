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
        public class TilemapExit
        {
            public int X;
            public int Y;
            public string Map;
            public int MapX;
            public int MapY;
            public TilemapExit(string element)
            {
                string[] parts = element.Split(',');
                foreach(string part in parts)
                {
                    string[] pair = part.Split(':');
                    if (pair[0] == "x") X = int.Parse(pair[1]);
                    else if (pair[0] == "y") Y = int.Parse(pair[1]);
                    else if (pair[0] == "map") Map = pair[1];
                    else if (pair[0] == "targetX") MapX = int.Parse(pair[1]);
                    else if (pair[0] == "targetY") MapY = int.Parse(pair[1]);
                }
            }
        }
    }
}
