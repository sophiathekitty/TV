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
        // games menu
        //----------------------------------------------------------------------
        public class GamesMenu : ScreenMenu
        {
            List<GamesMenuItem> games = new List<GamesMenuItem>();
            ScreenSprite gameBack;
            public GamesMenu(float width, ScreenActionBar actionBar) : base("Games", width, actionBar)
            {
                gameBack = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopCenter, Vector2.Zero, ScreenSprite.MONOSPACE_FONT_SIZE, Vector2.Zero, Color.White, "Monospace","",TextAlignment.CENTER,SpriteType.TEXT);
            }
            void updateBack()
            {
                if (SelectedIndex >= 0 && SelectedIndex < games.Count)
                    gameBack.Data = SceneCollection.GetScene(games[SelectedIndex].game + ".Title.0.Text");
            }
            public override string HandleInput(string argument)
            {
                string action = base.HandleInput(argument);
                updateBack();
                foreach(GamesMenuItem game in games)
                {
                    if(game.name.ToLower() == action)
                    {
                        TV.LaunchGame(game.game);
                        return "";
                    }
                }
                return action;
            }
            public override void AddToScreen(Screen screen)
            {
                List<string> available = SceneCollection.games.ToList();
                menuItems.Clear();
                games.Clear();
                foreach (string game in available)
                {
                    games.Add(new GamesMenuItem(game));
                    AddLabel(games[games.Count - 1].name);
                }
                updateBack();
                actionBar.RemoveFromScreen(screen);
                screen.AddSprite(gameBack);
                base.AddToScreen(screen);
                actionBar.AddToScreen(screen);
            }
            public override void RemoveFromScreen(Screen screen)
            {
                screen.RemoveSprite(gameBack);
                base.RemoveFromScreen(screen);
            }
            public class GamesMenuItem
            {
                public string name;
                public string game;
                public GamesMenuItem(string game)
                {                    
                    this.game = game;
                    string data = SceneCollection.GetScene(game+".Main.0.CustomData");
                    string[] parts = data.Split('║');
                    if(parts.Length > 1)
                    {
                        string[] vars = parts[0].Split(',');
                        foreach(string var in vars)
                        {
                            string[] varParts = var.Split(':');
                            if(varParts.Length == 2 && varParts[0] == "title") name = varParts[1].Trim();
                        }
                    }
                }
            }
        }
    }
}
