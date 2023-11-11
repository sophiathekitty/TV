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
        // idle scene for the TV
        //----------------------------------------------------------------------
        public class IdleScene : ScreenScene
        {
            List<string> images = new List<string>();
            int index = 0;
            int delay = 300;//2200;
            int currentDelay = 0;
            ScreenSprite sprite;
            ScreenSprite timeOverlay;
            public IdleScene(Vector2 size)
            {
                sprite = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center,Vector2.Zero,0f,size,Color.White,"", "LCD_HI_Poster1_Landscape", TextAlignment.CENTER,SpriteType.TEXTURE);
                sprites.Add(sprite);
                // add time overlay sprite to the bottom left corner
                timeOverlay = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.BottomRight, new Vector2(-30,-30*4), 3f, Vector2.Zero, Color.White, "White", "Hello?", TextAlignment.RIGHT, SpriteType.TEXT);
                sprites.Add(timeOverlay);

                // add images to the list
                List<string> s = new List<string>();
                GridInfo.Me.GetSurface(0).GetSprites(s);
                //GridInfo.Echo("Surface Sprite Count: " + s.Count);
                if (s.Contains("Ambient1"))
                {
                    GridInfo.Echo("Ambient1");
                    sprite.Data = "Ambient1";
                    for(int i= 1; i <= 16; i++) images.Add("Ambient"+i);
                    //images.Add("Ambient3");
                    //images.Add("Ambient6");
                    //images.Add("Ambient10");
                    //images.Add("Ambient11");
                    //images.Add("Ambient12");
                    //images.Add("Ambient13");
                    //images.Add("Ambient16");
                }
                else
                {
                    //GridInfo.Echo("LCD");
                    images.Add("LCD_HI_Poster1_Landscape");
                    images.Add("LCD_SoF_SpaceTravel_Landscape");
                    images.Add("LCD_SoF_ThunderFleet_Landscape");
                }
            }
            // get the current in game time of day where every 2 hours is 1 day
            DateTime GameTime
            {
                get
                {
                    // get the current time since midnight in seconds
                    double time = DateTime.Now.TimeOfDay.TotalSeconds;
                    // scale the time so that 2 hours real time is 1 day in game so 1am real time is 12pm in game
                    // if time = 3600 then time = 43200
                    time = time * 12;
                    double hour = (time / 3600) % 24;
                    double minute = (time / 60) % 60;
                    double second = time % 60;
                    return new DateTime(1, 1, 1, (int)hour, (int)minute, (int)second);
                }
            }
            public override void Update()
            {
                if (GridInfo.GetVarAs<bool>("ShowSlideshow", true))
                {
                    currentDelay++;
                    if (currentDelay >= delay)
                    {
                        currentDelay = 0;
                        index++;
                        if (index >= images.Count)
                        {
                            index = 0;
                        }
                        sprite.Data = images[index];
                    }
                    // show time in 12 hour format with am/pm
                    //timeOverlay.Data = DateTime.Now.ToString("h:mm tt");
                    // show remapped time in 12 hour format with am/pm
                    timeOverlay.Data = GameTime.ToString("h:mm tt");
                }
                else
                {
                    sprite.Data = "";
                    timeOverlay.Data = "";
                }
            }
        }
        //----------------------------------------------------------------------
    }
}
