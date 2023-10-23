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
            public static Dictionary<string, string> CharacterLibrary = new Dictionary<string, string>();
            public static int CharacterWidth = 16;
            public static int CharacterHeight = 16;
            public static int frameRate = 5;
            public static void LoadCharacters(string data)
            {
                //GridInfo.Echo("Loading characters: "+data.Length);
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
                            if (pair.Length != 2) { GridInfo.Echo(var); continue; }
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
                                if (pair.Length != 2) { GridInfo.Echo(var); continue; }
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
            public string Direction { get { return direction; } set { SetDirection(value); } }
            public int X;
            public int Y;
            public AnimatedCharacter(string element) : base(ScreenSpriteAnchor.TopLeft,Vector2.Zero,Tilemap.fontSize,new Vector2(16,16),Color.White,"Monospace","",TextAlignment.LEFT,SpriteType.TEXT)
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
                        //GridInfo.Echo("animated character: constructor: "+ type+": " + id + ": " + sprites.Length);
                        spriteData.Add(id,sprites);
                    }
                }
            }
            public int DirectionCount()
            {
                return spriteData.Count;
            }
            public void SetDirection(string dir, bool useDefault = true)
            {
                if(spriteData.ContainsKey(dir)) direction = dir;
                else if(useDefault) direction = spriteData.Keys.First();
                //Data = spriteData[direction][0];
            }
            int frame = 0;
            int frameCount = 0;
            public override MySprite ToMySprite(RectangleF _viewport)
            {
                //GridInfo.Echo("npc: ToMySprite: "+frameCount);
                // get the next sprite frame and loop
                if(frameCount++ > frameRate)
                {
                    frame = (frame + 1) % spriteData[direction].Length;
                    frameCount = 0;
                }
                //GridInfo.Echo("npc: ToMySprite: " + direction + ": "+frame);
                Data = spriteData[direction][frame];
                //GridInfo.Echo("npc: ToMySprite: " + Data);
                return base.ToMySprite(_viewport);
            }
        }
        //-----------------------------------------------------------------------
    }
}
