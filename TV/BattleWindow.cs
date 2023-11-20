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
        public class BattleWindow
        {
            ScreenSprite _background;
            ScreenSprite _enemy;
            public BattleWindow(string back, string enemy, float x, float y)
            {
                if(back.Length > 0)
                {
                    //GridInfo.Echo("BattleWindow: back:0: " + back.Length);
                    string[] lines = back.Split('\n');
                    //float width = lines[0].Length/Tilemap.fontSize;
                    float height = lines.Length/Tilemap.fontSize;
                    //_background = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, new Vector2(width, height), Tilemap.fontSize, Vector2.Zero, Color.White, "Monospace", back, TextAlignment.CENTER, SpriteType.TEXT);
                    _background = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, new Vector2(0, (-height/2) - 60), Tilemap.fontSize, Vector2.Zero, Color.White, "Monospace", back, TextAlignment.CENTER, SpriteType.TEXT);
                    //GridInfo.Echo("BattleWindow: back:1: " + width + ", " +height + " | " +_background.Data.Length);
                }
                else
                {
                    // sold black background
                    _background = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, Vector2.Zero, 0, new Vector2(5000,1000), Color.Black, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                }
                _enemy = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, new Vector2(x,y-60), Tilemap.fontSize, Vector2.Zero, Color.White, "Monospace", enemy, TextAlignment.CENTER, SpriteType.TEXT);
            }
            public void AddToScreen(Screen screen)
            {
                //GridInfo.Echo("BattleWindow: add: " + _background.Data.Length);
                screen.AddSprite(_background);
                //GridInfo.Echo("BattleWindow: add: " + _enemy.Data.Length);
                screen.AddSprite(_enemy);
            }
            public void RemoveFromScreen(Screen screen)
            {
                screen.RemoveSprite(_background);
                screen.RemoveSprite(_enemy);
            }
            public void HideEnemy()
            {
                _enemy.Visible = false;
            }
        }
    }
}
