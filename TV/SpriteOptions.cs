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
        public class SpriteOptions
        {
            public float width = 0f;
            public float height = 0f;
            public float scale = 1f;
            public float x = 0f;
            public float y = 0f;
            public string type = "sprite";
            public bool loop = false;
            public bool random = false;
            public int delay = 10;
            public SpriteOptions(string data)
            {
                string[] options = data.Split(',');
                foreach(string option in options)
                {
                    string[] var = option.Split(':');
                    if (var.Length == 2)
                    {
                        if (var[0] == "width")
                        {
                            width = float.Parse(var[1]);
                        }
                        else if (var[0] == "height")
                        {
                            height = float.Parse(var[1]);
                        }
                        else if (var[0] == "scale")
                        {
                            scale = float.Parse(var[1]);
                        }
                        else if (var[0] == "x")
                        {
                            x = float.Parse(var[1]);
                        }
                        else if (var[0] == "y")
                        {
                            y = float.Parse(var[1]);
                        }
                        else if (var[0] == "type")
                        {
                            type = var[1];
                        }
                        else if (var[0] == "animation" && var[1] == "loop")
                        {
                            loop = true;
                        }
                        else if (var[0] == "animation" && var[1] == "random")
                        {
                            random = true;
                            loop = true;
                        }
                        else if (var[0] == "delay")
                        {
                            delay = int.Parse(var[1]);
                        }
                    }
                }
            }
            public Vector2 position
            {
                get { return new Vector2(x, y); }
                set
                {
                    x = value.X;
                    y = value.Y;
                }
            }
            public Vector2 size
            {
                get { return new Vector2(width, height); }
                set
                {
                    width = value.X;
                    height = value.Y;
                }
            }
        }
    }
}
