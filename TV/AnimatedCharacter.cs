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
        //-----------------------------------------------------------------------
        // animated character
        //-----------------------------------------------------------------------
        public class AnimatedCharacter : ScreenSprite
        {
            public static Dictionary<string, string> CharacterLibrary;
            public static int CharacterWidth = 16;
            public static int CharacterHeight = 16;
            public static int frameRate = 5;
            public static void LoadCharacters(string data)
            {
                string[] parts = data.Split('║');
                foreach(string part in parts)
                {
                    if (part.Contains("type:characters"))
                    {
                        // split , and then : to get width and height
                        string[] info = part.Split(',');
                        foreach(string var in info)
                        {
                            string[] pair = var.Split(':');
                            if (pair[0] == "width") CharacterWidth = int.Parse(pair[1]);
                            else if (pair[0] == "height") CharacterHeight = int.Parse(pair[1]);
                            else if (pair[0] == "frameRate") frameRate = int.Parse(pair[1]);
                        }
                    }
                    else
                    {
                        string[] info = part.Split('═');
                        foreach(string var in info)
                        {
                            if(var.Contains("type:"))
                            {
                                string[] pair = var.Split(':');
                                string type = pair[1];
                                CharacterLibrary.Add(type,part);
                                break;
                            }
                        }
                    }
                }
            }
            Dictionary<string, string[]> spriteData = new Dictionary<string,string[]>();
            string type = "";
            string direction = "down";

            public AnimatedCharacter(string element) : base(ScreenSpriteAnchor.BottomLeft,Vector2.Zero,0.02f,new Vector2(16,16),Color.White,"Monospace","",TextAlignment.LEFT,SpriteType.TEXT)
            {
                string[] parts = element.Split('═');
                foreach(string part in parts)
                {
                    string[] data = part.Split(':');
                    if (data[0] == "type") type = data[1];
                    else
                    {
                        string id = data[0];
                        string[] sprites = data[1].Split(',');
                        spriteData.Add(id,sprites);
                    }
                }
            }
            public void SetDirection(string dir)
            {
                if(spriteData.ContainsKey(dir)) direction = dir;
                else direction = spriteData.Keys.First();
            }
            int frame = 0;
            int frameCount = 0;
            public override MySprite ToMySprite(RectangleF _viewport)
            {
                // get the next sprite frame and loop
                if(frameCount++ > frameRate)
                {
                    frame = (frame + 1) % spriteData[direction].Length;
                    frameCount = 0;
                }
                Data = spriteData[direction][frame];
                return base.ToMySprite(_viewport);
            }
        }
        //-----------------------------------------------------------------------
    }
}
