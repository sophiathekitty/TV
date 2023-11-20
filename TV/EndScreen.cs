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
        public class EndScreen
        {
            ScreenSprite back;
            ScreenSprite message;
            ScreenActionBar actionBar;
            public EndScreen(string message, ScreenActionBar actionBar)
            {
                back = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, Vector2.Zero, 0, new Vector2(5000, 1000), Color.Black, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                this.message = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopCenter, new Vector2(0, 60), 2f, Vector2.Zero, Color.White, "White", message, TextAlignment.CENTER, SpriteType.TEXT);
                this.actionBar = actionBar;
            }
            public void AddToScreen(Screen screen)
            {
                actionBar.RemoveFromScreen(screen);
                screen.AddSprite(back);
                screen.AddSprite(message);
                actionBar.SetActions("Quit");
                actionBar.AddToScreen(screen);
            }
            public void RemoveFromScreen(Screen screen)
            {
                actionBar.RemoveFromScreen(screen);
                screen.RemoveSprite(message);
                screen.RemoveSprite(back);
            }
        }
    }
}
