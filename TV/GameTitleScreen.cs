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
        public class GameTitleScreen
        {
            ScreenSprite background;
            ScreenSprite title;
            StartMenu menu;
            string gameName = "";
            public string MenuAction { get { return menu.menuScrollActions; } }
            public GameTitleScreen(string titleText,string name, string save_tag, float width, ScreenActionBar actionBar,string back = "")
            {
                gameName = name;
                background = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopCenter, Vector2.Zero, ScreenSprite.MONOSPACE_FONT_SIZE, Vector2.Zero, Color.White, "Monospace", back, TextAlignment.CENTER, SpriteType.TEXT);
                title = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopCenter, new Vector2(0, 75), 3.66f, Vector2.Zero, Color.White, "White", titleText, TextAlignment.CENTER, SpriteType.TEXT);
                menu = new StartMenu("", save_tag, width, actionBar, false);
            }
            public string HandleInput(string input)
            {
                return menu.HandleInput(input);
            }
            public void AddToScreen(Screen screen)
            {
                screen.AddSprite(background);
                screen.AddSprite(title);
                menu.AddToScreen(screen);
            }
            public void RemoveFromScreen(Screen screen)
            {
                screen.RemoveSprite(background);
                screen.RemoveSprite(title);
                menu.RemoveFromScreen(screen);
            }
        }
    }
}
