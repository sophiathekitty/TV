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
        // a very simple RPG game based on dragon warrior for the NES
        //-----------------------------------------------------------------------
        public class GameRPG
        {
            public static Action<string> Say;
            public static Action<string,int,int> Go;
            public static Action<string,npc,string> Ask;
            npc promptNPC = null;
            string promptTag = "";
            string title;
            string name;
            Tilemap map;
            int playerX = 0;
            int playerY = 0;
            string currentMap = "";
            AnimatedCharacter player;
            public static Dictionary<string,double> playerStats = new Dictionary<string,double>();
            public static Dictionary<string,int> playerMaxStats = new Dictionary<string,int>();
            public static Dictionary<string,string> playerGear = new Dictionary<string,string>();
            public static Dictionary<string, int> playerInventory = new Dictionary<string, int>();
            public static Dictionary<string, int> enemyStats = new Dictionary<string, int>();
            public static Dictionary<string,Dictionary<string,string>> itemStats = new Dictionary<string,Dictionary<string,string>>();
            public static Dictionary<string, bool> gameBools = new Dictionary<string, bool>();
            public static Dictionary<string, int> gameInts = new Dictionary<string, int>();
            public static Dictionary<string, string> maps = new Dictionary<string, string>();
            string playerHP = "hp";
            Screen tv;
            ScreenActionBar actionBar;
            GameActionMenu gameActionMenu;
            PlayerStatsWindow playerStatsWindow;
            DialogWindow dialogWindow;
            string controls = "< v ^ > Menu";
            // get a subset of the item library
            Dictionary<string, Dictionary<string, string>> ItemsOfType(string type)
            {
                Dictionary<string, Dictionary<string, string>> items = new Dictionary<string, Dictionary<string, string>>();
                foreach(KeyValuePair<string,Dictionary<string,string>> item in itemStats)
                {
                    if (item.Value["item_type"] == type) items.Add(item.Key,item.Value);
                }
                return items;
            }
            //-------------------------------------------------------------------
            // load a game from a string
            //-------------------------------------------------------------------
            public GameRPG(string game, ScreenActionBar actionBar)
            {
                playerGear.Clear();
                playerInventory.Clear();
                playerStats.Clear();
                playerMaxStats.Clear();
                enemyStats.Clear();
                gameBools.Clear();
                gameInts.Clear();
                maps.Clear();
                itemStats.Clear();
                AnimatedCharacter.CharacterLibrary.Clear();
                GridInfo.Echo("Loading game: " + game);
                this.actionBar = actionBar;
                actionBar.SetActions(controls);
                name = game;
                string element = SceneCollection.GetScene(game + ".Main.0.CustomData");
                string[] parts = element.Split('║');
                foreach(string part in parts)
                {
                    if (part.Contains("type:player")) parsePlayer(part);
                    else if(part.Contains("type:game")) parseInfo(part);
                    else if(part.Contains("type:item")) parseItems(part);
                }
                loadGraphics();                
                loadMapsList();
                player = new AnimatedCharacter(AnimatedCharacter.CharacterLibrary["player"]);
                Say = ShowDialog;
                Ask = ShowDialogPrompt;
                Go = LoadMap;
            }
            // parse game info
            void parseInfo(string data)
            {
                GridInfo.Echo("parseInfo:");
                string[] vars = data.Split(',');
                foreach(string var in vars)
                {
                    string[] pair = var.Split(':');
                    if (pair[0] == "title") title = pair[1];
                    else if (pair[0] == "name") name = pair[1];
                }
            }
            // parse player info (create the default player)
            void parsePlayer(string data)
            {
                GridInfo.Echo("parsePlayer:");
                string[] parts = data.Split('═');
                if(parts.Length > 1) parsePlayerStats(parts[1]);
                if(parts.Length > 2) parsePlayerGear(parts[2]);
                if(parts.Length > 3) parsePlayerLocation(parts[3]);
            }
            // parse player stats
            void parsePlayerStats(string data)
            {
                GridInfo.Echo("parsePlayerStats:");
                string[] parts = data.Split(',');
                foreach(string part in parts)
                {
                    string[] pair = part.Split(':');
                    playerStats.Add(pair[0], int.Parse(pair[1]));
                    playerMaxStats.Add(pair[0], int.Parse(pair[1]));
                }
            }
            // parse player gear
            void parsePlayerGear(string data)
            {
                GridInfo.Echo("parsePlayerGear:");
                string[] parts = data.Split(',');
                foreach(string part in parts)
                {
                    string[] pair = part.Split(':');
                    playerGear.Add(pair[0], pair[1]);
                }
            }
            // parse player location
            void parsePlayerLocation(string data)
            {
                GridInfo.Echo("parsePlayerLocation:");
                string[] parts = data.Split(',');
                foreach(string part in parts)
                {
                    string[] vars = part.Split(':');
                    if(vars.Length != 2) continue;
                    if (vars[0] == "map") currentMap = vars[1];
                    else if (vars[0] == "x") playerX = int.Parse(vars[1]);
                    else if (vars[0] == "y") playerY = int.Parse(vars[1]);
                }
            }
            // parse a group of items
            void parseItems(string data)
            {
                string[] parts = data.Split('═');
                string item_type = "";
                if(parts.Length > 1 && parts[0].Contains("item_type"))
                {
                    string[] vars = parts[0].Split(',');
                    foreach(string var in vars)
                    {
                        string[] pair = var.Split(':');
                        if (pair[0] == "item_type") item_type = pair[1];
                    }
                }
                for(int i = 1; i < parts.Length; i++)
                {
                    Dictionary<string,string> stats = new Dictionary<string,string>();
                    if(item_type != "") stats.Add("item_type",item_type);
                    string[] vars = parts[i].Split(',');
                    string item_name = "";
                    foreach(string var in vars)
                    {
                        string[] pair = var.Split(':');
                        if(pair.Length != 2) continue;
                        stats.Add(pair[0], pair[1]);
                        if (pair[0] == "name") item_name = pair[1];
                    }
                    if(item_name != "") itemStats.Add(item_name,stats);
                }
            }
            void loadGraphics()
            {
                if (name == "") return;
                Vector2 size = GridBlocks.TV.SurfaceSize;
                map = new Tilemap(size.X,size.Y);
                map.LoadTiles(SceneCollection.GetScene(name + ".Tiles.0.CustomData"));
                AnimatedCharacter.LoadCharacters(SceneCollection.GetScene(name + ".Characters.0.CustomData"));
            }
            void loadMapsList()
            {
                GridInfo.Echo("loadMapsList:");
                string[] parts = SceneCollection.GetScene(name + ".Maps.0.CustomData").Split('║');
                if (!parts[0].Contains("type:atlas")) return;
                foreach(string part in parts)
                {
                    if (!part.Contains("type:atlas"))
                    {
                        string[] vars = part.Split(',');
                        foreach(string var in vars)
                        {
                            //map_name:map_address
                            string[] pair = var.Split(':');
                            if (pair.Length != 2) continue;
                            maps.Add(pair[0], pair[1]);
                            

                        }
                    }
                }
            }
            //-------------------------------------------------------------------
            // load a game save
            // note: i haven't figured out what the save string looks like yet
            //-------------------------------------------------------------------
            public void LoadGameSave(string save)
            {
                string[] parts = save.Split('║');
                foreach(string part in parts)
                {
                    if (part.Contains("type:player")) parsePlayer(part);
                }
            }
            bool firstLoadMap = true;
            public void LoadMap(string map_name, int x, int y)
            {
                GridInfo.Echo("LoadMap: " + map_name);
                if (!maps.ContainsKey(map_name)) return;
                currentMap = map_name;
                GridInfo.Echo("map address: " + maps[map_name]);
                if (!firstLoadMap)
                {
                    map.RemoveFromScreen(tv);
                    map.ClearMap();
                }
                map.LoadMap(SceneCollection.GetScene(name+"."+maps[map_name]));
                if (!firstLoadMap)
                {
                    tv.RemoveSprite(player);
                    map.AddToScreen(tv);
                    tv.AddSprite(player);
                }
                playerX = x;
                playerY = y;
                map.SetViewCenter(playerX, playerY);
                player.Position = map.GridPosToScreenPos(playerX, playerY);
                GridInfo.Echo("player position: ("+ playerX+", "+ playerY+ ") " + player.Position);
                firstLoadMap = false;
            }
            //-------------------------------------------------------------------
            // add to screen
            //-------------------------------------------------------------------
            public void AddToScreen(Screen screen)
            {
                tv = screen;
                map.AddToScreen(screen);
                screen.AddSprite(player);
            }
            // remove from screen
            public void RemoveFromScreen(Screen screen)
            {
                map.RemoveFromScreen(screen);
                screen.RemoveSprite(player);
            }
            //-------------------------------------------------------------------
            // update
            //-------------------------------------------------------------------
            public void Update()
            {
                map.SetViewCenter(playerX, playerY);
                map.MoveNPCs();
                player.Position = map.GridPosToScreenPos(playerX, playerY);
            }
            void ShowDialog(string dialog)
            {
                dialogWindow = new DialogWindow(dialog, new Vector2(500, 100),actionBar);
                dialogWindow.AddToScreen(tv);
            }
            void ShowDialogPrompt(string dialog, npc npc, string tag)
            {
                GridInfo.Echo("ShowDialogPrompt: " + dialog);
                ShowDialog(dialog);
                promptNPC = npc;
                actionBar.SetActions("Yes No     Back");
                promptTag = tag;
                GridInfo.Echo("ShowDialogPrompt: " + promptTag);
            }
            //-------------------------------------------------------------------
            // handle input
            //-------------------------------------------------------------------
            public string HandleInput(string input)
            {
                if(playerStatsWindow != null) playerStatsWindow.Update(playerStats);
                string action = "";
                if (dialogWindow != null) action = dialogWindow.HandleInput(input);
                else if(gameActionMenu == null) action = actionBar.HandleInput(input);
                else action = gameActionMenu.HandleInput(input);
                GridInfo.Echo("game: input: " +input+ " -> action: " + action);
                if(action == "<")
                {
                    player.SetDirection("left");
                    TryMovePlayer(playerX - 1, playerY);
                    return "";
                }
                else if(action == ">")
                {
                    player.SetDirection("right");
                    TryMovePlayer(playerX + 1, playerY);
                }
                else if(action == "^")
                {
                    player.SetDirection("up");
                    TryMovePlayer(playerX, playerY - 1);
                }
                else if(action == "v")
                {
                    player.SetDirection("down");
                    TryMovePlayer(playerX, playerY + 1);
                }
                else if(action == "menu")
                {
                    gameActionMenu = new GameActionMenu("Menu", 255, actionBar, GetNPCActions());
                    gameActionMenu.AddToScreen(tv);
                    playerStatsWindow = new PlayerStatsWindow(playerStats, 155);
                    playerStatsWindow.AddToScreen(tv);
                    actionBar.SetActions(gameActionMenu.menuScrollActions);
                    return "";
                }
                else if(action == "back")
                {
                    /*gameActionMenu.RemoveFromScreen(tv);
                    gameActionMenu = null;
                    playerStatsWindow.RemoveFromScreen(tv);
                    playerStatsWindow = null;
                    actionBar.SetActions(controls);*/
                    CloseMenu();
                    return "";
                }
                else if(action == "close")
                {
                    CloseDialog();
                    /*
                    dialogWindow.RemoveFromScreen(tv);
                    dialogWindow = null;
                    if(gameActionMenu != null)
                    {
                        gameActionMenu.RemoveFromScreen(tv);
                        gameActionMenu = null;
                        playerStatsWindow.RemoveFromScreen(tv);
                        playerStatsWindow = null;
                        actionBar.SetActions(controls);
                    }
                    */
                }
                else if(promptNPC != null && action == "yes")
                {
                    GridInfo.Echo("promptNPC: " + promptNPC.yes.Count+ " :: "+promptTag);
                    dialogWindow.RemoveFromScreen(tv);
                    dialogWindow = null;
                    if (promptNPC.yes.ContainsKey(promptTag)) promptNPC.yes[promptTag].Run();
                    promptNPC = null;
                    promptTag = "";
                    return "";
                }
                else if(promptNPC != null && action == "no")
                {
                    GridInfo.Echo("promptNPC: " + promptNPC.no.Count+" :: "+promptTag);
                    dialogWindow.RemoveFromScreen(tv);
                    dialogWindow = null;
                    if (promptNPC.no.ContainsKey(promptTag)) promptNPC.no[promptTag].Run();
                    promptNPC = null;
                    promptTag = "";
                    return "";
                }
                return action;
            }
            void CloseDialog()
            {
                CloseMenu();
                if (dialogWindow == null) return;
                dialogWindow.RemoveFromScreen(tv);
                dialogWindow = null;
            }
            void CloseMenu()
            {
                if (gameActionMenu == null) return;
                gameActionMenu.RemoveFromScreen(tv);
                gameActionMenu = null;
                playerStatsWindow.RemoveFromScreen(tv);
                playerStatsWindow = null;
                actionBar.SetActions(controls);
            }
            //Try move player
            public void TryMovePlayer(int x, int y)
            {
                GridInfo.Echo("TryMovePlayer: (" + x + ", " + y + ")");
                TilemapExit exit = map.ExitOn(x, y);
                if (map.IsOccupied(x, y)) return;
                GridInfo.Echo("not occupied");
                if (exit != null)
                {
                    GridInfo.Echo("exit: " + exit.Map);
                    LoadMap(exit.Map, exit.MapX, exit.MapY);
                    //playerX = exit.MapX;
                    //playerY = exit.MapY;
                    //map.SetViewCenter(playerX, playerY);
                    //player.Position = map.GridPosToScreenPos(playerX, playerY);
                    return;
                }
                if (!map.IsGround(x, y)) return;
                GridInfo.Echo("is ground");
                playerX = x;
                playerY = y;
                map.SetViewCenter(playerX, playerY);
                player.Position = map.GridPosToScreenPos(playerX, playerY);
                // take damage from toxic tiles
                if(playerStats.ContainsKey(playerHP)) playerStats[playerHP] -= map.ToxicLevel(x, y);
            }
            // get game actions from npc in front of player (1 or 2 tiles away)
            public List<GameAction> GetNPCActions() {                 
                List<GameAction> actions = new List<GameAction>();
                int x_offset = 0;
                int y_offset = 0;
                if (player.Direction == "up") y_offset--;
                else if (player.Direction == "down") y_offset++;
                else if (player.Direction == "left") x_offset--;
                else if (player.Direction == "right") x_offset++;
                // directly in front of player
                npc npc = map.GetNPCOn(playerX + x_offset, playerY + y_offset);
                if (npc != null && npc.NPCVisible)
                {
                    actions = npc.actions;
                    npc.FacePlayer(player);
                }
                else if (map.IsShopCounter(playerX + x_offset, playerY + y_offset))
                {
                    // 2 tiles away (like a shop keeper on the other side of a wall)
                    npc = map.GetNPCOn(playerX + (x_offset * 2), playerY + (y_offset * 2));
                    if (npc != null && npc.NPCVisible)
                    {
                        actions = npc.actions;
                        npc.FacePlayer(player);
                    }
                }
                return actions;
            }
            //-------------------------------------------------------------------
        }
        //-----------------------------------------------------------------------
    }
}
