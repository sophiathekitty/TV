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
            string saveTag = "";
            TextGridKeyboard keyboard;
            Screen screen;
            List<string> saveNames = new List<string>();
            public string MenuAction { get { return menu.menuScrollActions; } }
            public GameTitleScreen(string titleText,string name, string save_tag, float width, ScreenActionBar actionBar,string back = "")
            {
                gameName = name;
                saveTag = save_tag;
                background = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopCenter, Vector2.Zero, ScreenSprite.MONOSPACE_FONT_SIZE, Vector2.Zero, Color.White, "Monospace", back, TextAlignment.CENTER, SpriteType.TEXT);
                title = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopCenter, new Vector2(0, 75), 3.66f, Vector2.Zero, Color.White, "White", titleText, TextAlignment.CENTER, SpriteType.TEXT);
                string game_saves = SceneCollection.GetScene(name + ".Main.0.Text");
                string[] save_data = game_saves.Split('║');
                for(int i = 1; i < save_data.Length; i++)
                {
                    string[] save = save_data[i].Split('═');
                    //GridInfo.Echo("GameTitleScreen:save data parse:1: "+save.Length);
                    if (save.Length < 1) continue;
                    string[] vars = save[0].Split(':');
                    //GridInfo.Echo("GameTitleScreen:save data parse:2:"+vars.Length);
                    if (vars.Length < 2) continue;
                    //GridInfo.Echo("GameTitleScreen:save name: "+vars[1]);
                    saveNames.Add(vars[1]);
                }
                menu = new StartMenu("",name, saveNames, width, actionBar, false);
            }
            public string HandleInput(string input)
            {
                string action = "";
                if(keyboard == null) action = menu.HandleInput(input);
                else action = keyboard.HandleInput(input);
                //GridInfo.Echo("GameTitleScreen:HandleInput:action: "+action);
                if (saveNames.Any(name=>name.Equals(action,StringComparison.OrdinalIgnoreCase)))
                {
                    string LoadGame = saveTag.ToLower() + " "+(menu.SelectedIndex+1);
                    //GridInfo.Echo("GameTitleScreen:HandleInput:LoadGame: " + LoadGame);
                    if(LoadGame == action)
                    {
                        //GridInfo.Echo("GameTitleScreen:HandleInput:LoadGame: New Game! Show Keyboard");
                        // new game show keyboard
                        keyboard = new TextGridKeyboard(menu.actionBar,"Name:", "NewGameName");
                        keyboard.AddToScreen(screen);
                        return "";
                    } else return LoadGame;
                }
                if(action.StartsWith("text_results║"))
                {
                    return saveTag.ToLower()+" "+(menu.SelectedIndex+1);
                }
                return action;
            }
            public void AddToScreen(Screen screen)
            {
                this.screen = screen;
                screen.AddSprite(background);
                screen.AddSprite(title);
                menu.AddToScreen(screen);
            }
            public void RemoveFromScreen(Screen screen)
            {
                screen.RemoveSprite(background);
                screen.RemoveSprite(title);
                menu.RemoveFromScreen(screen);
                if (keyboard != null)
                {
                    keyboard.RemoveFromScreen(screen);
                    keyboard = null;
                }
            }
        }
    }
}
