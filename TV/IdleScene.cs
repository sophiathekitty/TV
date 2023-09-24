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
            int delay = 200;
            int currentDelay = 0;
            ScreenSprite sprite;
            public IdleScene(Vector2 size)
            {
                sprite = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center,Vector2.Zero,0f,size,Color.White,"","",TextAlignment.CENTER,SpriteType.TEXTURE);
                sprites.Add(sprite);
                images.Add("LCD_HI_Poster1_Landscape");
                images.Add("LCD_HI_Poster2_Landscape");
                images.Add("LCD_HI_Poster3_Landscape");
                images.Add("LCD_SoF_BrightFuture_Landscape");
                images.Add("LCD_SoF_CosmicTeam_Landscape");
                images.Add("LCD_SoF_Exploration_Landscape");
                images.Add("LCD_SoF_SpaceTravel_Landscape");
                images.Add("LCD_SoF_ThunderFleet_Landscape");
            }
            public override void Update()
            {
                if (GridInfo.GetVarAs<bool>("ShowSlideshow", false))
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
                }
            }
        }
        //----------------------------------------------------------------------
    }
}
