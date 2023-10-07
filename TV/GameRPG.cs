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
            string title;
            string name;
            Tilemap map;
            int playerX = 0;
            int playerY = 0;
            string currentMap = "";
            AnimatedCharacter player;
            Dictionary<string,int> playerStats = new Dictionary<string,int>();
            Dictionary<string,int> playerMaxStats = new Dictionary<string,int>();
            Dictionary<string,string> playerGear = new Dictionary<string,string>();
            Dictionary<string,int> playerInventory = new Dictionary<string,int>();
            Dictionary<string,int> enemyStats = new Dictionary<string,int>();
            Dictionary<string,Dictionary<string,string>> itemStats = new Dictionary<string,Dictionary<string,string>>();
            Dictionary<string,bool> gameBools = new Dictionary<string,bool>();
            Dictionary<string,int> gameInts = new Dictionary<string,int>();
            Dictionary<string,string> maps = new Dictionary<string,string>();
            ScreenActionBar actionBar;
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
            public void LoadMap(string map_name)
            {
                GridInfo.Echo("LoadMap: " + map_name);
                if (!maps.ContainsKey(map_name)) return;
                currentMap = map_name;
                GridInfo.Echo("map address: " + maps[map_name]);
                map.LoadMap(SceneCollection.GetScene(name+"."+maps[map_name]));
                map.SetViewCenter(playerX, playerY);
                player.Position = map.GridPosToScreenPos(playerX, playerY);
                GridInfo.Echo("player position: ("+ playerX+", "+ playerY+ ") " + player.Position);
            }
            //-------------------------------------------------------------------
            // add to screen
            //-------------------------------------------------------------------
            public void AddToScreen(Screen screen)
            {
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
            public string HandleInput(string input)
            {
                string action = actionBar.HandleInput(input);
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
                else if(action == "Menu")
                {
                    //show in game menu (todo)
                    GridInfo.Echo("show in game menu");
                }
                return action;
            }
            public void TryMovePlayer(int x, int y)
            {
                GridInfo.Echo("TryMovePlayer: (" + x + ", " + y + ")");
                if (!map.IsGround(x, y)) return;
                GridInfo.Echo("is ground");
                if(map.IsOccupied(x,y)) return;
                GridInfo.Echo("not occupied");
                playerX = x;
                playerY = y;
                map.SetViewCenter(playerX, playerY);
                player.Position = map.GridPosToScreenPos(playerX, playerY);
            }
        }
        //-----------------------------------------------------------------------
    }
}
