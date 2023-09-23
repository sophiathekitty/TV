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
        public class Subtitles : ScreenSprite
        {
            List<string> subtitles;
            public Subtitles(List<string> data, float scale = 1f) : base(ScreenSpriteAnchor.BottomCenter, new Vector2(0,-30), scale, Vector2.Zero, Color.White, "White", data[0], TextAlignment.CENTER, SpriteType.TEXT)
            {

            }
        }
    }
}
