﻿using Sandbox.Game.EntityComponents;
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
        // BackgroundImage - encapsulates a background image
        //----------------------------------------------------------------------
        public class BackgroundImage : ScreenSprite
        {
            public BackgroundImage(string data, Vector2 size, float scale = 1f) : base(ScreenSpriteAnchor.TopLeft,Vector2.Zero,scale,size,Color.White,"Monospace",data,TextAlignment.LEFT,SpriteType.TEXT)
            {
                
            }
            public BackgroundImage(string element) : base(ScreenSpriteAnchor.TopLeft, Vector2.Zero, MONOSPACE_FONT_SIZE, Vector2.Zero, Color.White, "Monospace", "", TextAlignment.LEFT, SpriteType.TEXT)
            {
                string[] data = element.Split('═');
                SpriteOptions options = new SpriteOptions(data[0]);
                Data = data[1];
                Position = options.position;
                Size = options.size;
            }
        }
    }
}
