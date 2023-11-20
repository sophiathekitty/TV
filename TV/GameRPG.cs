using Sandbox.Game.EntityComponents;
using Sandbox.Game.GUI.DebugInputComponents;
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
        public class GameRPG : IGameVars, IGameShop, IGameDialog, IGameInventory, IGameSpells, IGameEncounters
        {
            //public static Action<string> Say;
            //public static Action<string> Shop;
            //public static Action Sell;
            //public static Action<string,int,int> Go;
            //public static Action<string,npc,string> Ask;
            public static Action<string> Encounter;
            npc promptNPC = null;
            string promptTag = "";
            string name;
            string title;
            string save_tag = "Game";
            int saveId = 0;
            Tilemap map;
            int playerX = 0;
            int playerY = 0;
            string currentMap = "";
            AnimatedCharacter player;
            Dictionary<string,double> playerStats = new Dictionary<string,double>();
            Dictionary<string,int> playerMaxStats = new Dictionary<string,int>();
            Dictionary<string, string> playerGear = new Dictionary<string, string>();
            Dictionary<string, int> playerInventory = new Dictionary<string, int>();
            string playerStatus = "ok";
            Dictionary<string, int> enemyStats = new Dictionary<string, int>();
            string enemyStatus = "ok";
            string enemyName = "";
            string playerName = "";
            bool encounterOver = false;
            Dictionary<string, Dictionary<string, string>> itemStats = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, bool> gameBools = new Dictionary<string, bool>();
            Dictionary<string, int> gameInts = new Dictionary<string, int>();
            Dictionary<string, string> maps = new Dictionary<string, string>();
            Dictionary<string,GameAction> gameLogic = new Dictionary<string,GameAction>();
            Dictionary<string, Dictionary<string, string>> playerSpells = new Dictionary<string, Dictionary<string, string>>();
            string spellsLevel = "level";
            string spellsCost = "mp";
            string spellsBattle = "combat";
            string titleMusic = "";
            string battleMusic = "";
            BattleMenu battleMenu;
            BattleWindow battleWindow;
            bool isPlayerTurn = true;
            string[] enemyPattern;
            //string playerHP = "hp";
            Screen tv;
            ScreenActionBar actionBar;
            // game ui stuff
            GameTitleScreen gameTitleScreen;
            GameActionMenu gameActionMenu;
            GameItemMenu gameItemMenu;
            ShopMenu shopMenu;
            GameSpellMenu gameSpellMenu;
            PlayerStatsWindow playerStatsWindow;
            DialogWindow dialogWindow;
            EndScreen endScreen;
            string controls = "< v ^ > Menu";
            string NothingToSell = "You have nothing to sell.";
            List<string> systemActions = new List<string>() { "Items", "Spells" };
            // get a subset of the item library
            /*
            Dictionary<string, Dictionary<string, string>> ItemsOfType(string type)
            {
                Dictionary<string, Dictionary<string, string>> items = new Dictionary<string, Dictionary<string, string>>();
                foreach(KeyValuePair<string,Dictionary<string,string>> item in itemStats)
                {
                    if (item.Value["item_type"] == type) items.Add(item.Key,item.Value);
                }
                return items;
            }
            */
            //-------------------------------------------------------------------
            // load a game from a string
            //-------------------------------------------------------------------
            public GameRPG(string game, ScreenActionBar actionBar)
            {
                Tilemap.Reset();
                AnimatedCharacter.CharacterLibrary.Clear();
                //GridInfo.Echo("Loading game: " + game);
                this.actionBar = actionBar;
                //actionBar.SetActions(controls);
                name = game;
                string element = SceneCollection.GetScene(game + ".Main.0.CustomData");
                string[] parts = element.Split('║');
                foreach(string part in parts)
                {
                    if (part.Contains("type:player")) parsePlayer(part);
                    else if(part.Contains("type:game")) parseInfo(part);
                    else if(part.Contains("type:item")) parseItems(part);
                    else if(part.Contains("type:logic")) parseGameLogic(part);
                    else if(part.Contains("type:spells")) parseSpells(part);
                }
                loadGraphics();                
                loadMapsList();
                player = new AnimatedCharacter(AnimatedCharacter.CharacterLibrary["player"]);
                player.Position = new Vector2(-100, -100); // hide player for now.
                gameTitleScreen = new GameTitleScreen(title,name,save_tag, 300, actionBar, SceneCollection.GetScene(game + ".Title.0.CustomData"));
                actionBar.SetActions(gameTitleScreen.MenuAction);
                GameAction.GameVars = this;
                GameAction.GameShop = this;
                GameAction.Game = this;
                GameAction.GameInventory = this;
                GameAction.GameSpells = this;
                GameAction.GameEncounters = this;
                if(titleMusic != "") TV.PlayMusic(titleMusic);
                //GridInfo.Echo("GameRPG: " + name + " loaded");
            }
            // parse game info
            void parseInfo(string data)
            {
                //GridInfo.Echo("parseInfo:");
                string[] vars = data.Split(',');
                foreach(string var in vars)
                {
                    string[] pair = var.Split(':');
                    if (pair[0].Trim() == "name") name = pair[1].Trim();
                    else if (pair[0].Trim() == "title") title = pair[1].Trim();
                    else if (pair[0].Trim() == "battleMusic") battleMusic = pair[1].Trim();
                    else if (pair[0].Trim() == "titleMusic") titleMusic = pair[1].Trim();
                    else if (pair[0].Trim() == "spellsLevel") spellsLevel = pair[1].Trim();
                    else if (pair[0].Trim() == "spellsCost") spellsCost = pair[1].Trim();
                    else if (pair[0].Trim() == "spellsBattle") spellsBattle = pair[1].Trim();
                    else if (pair[0].Trim() == "save_tag") save_tag = pair[1].Trim();
                }
            }
            // parse player info (create the default player)
            void parsePlayer(string data)
            {
                //GridInfo.Echo("parsePlayer:");
                string[] parts = data.Split('═');
                if(parts.Length > 1) parsePlayerStats(parts[1].Trim());
                if(parts.Length > 2) parsePlayerGear(parts[2].Trim());
                if(parts.Length > 3) parsePlayerLocation(parts[3].Trim());
            }
            // parse player stats
            void parsePlayerStats(string data, string maxData = "")
            {
                //GridInfo.Echo("parsePlayerStats:");
                string[] parts = data.Split(',');
                foreach(string part in parts)
                {
                    string[] pair = part.Split(':');
                    string stat_name = pair[0].Trim();
                    string stat_value = pair[1].Trim();
                    //GridInfo.Echo("parsePlayerStats: " + stat_name + " = " + stat_value);
                    // stat...
                    if (playerStats.ContainsKey(stat_name)) playerStats[stat_name] = int.Parse(stat_value);
                    else playerStats.Add(stat_name, int.Parse(stat_value));
                    // max stat...
                    if(maxData == "")
                    {
                        if (playerMaxStats.ContainsKey(stat_name)) playerMaxStats[stat_name] = int.Parse(stat_value);
                        else playerMaxStats.Add(stat_name, int.Parse(stat_value));
                    }
                }
                if(maxData != "")
                {
                    parts = maxData.Split(',');
                    foreach (string part in parts)
                    {
                        string[] pair = part.Split(':');
                        playerMaxStats[pair[0].Trim()] = int.Parse(pair[1].Trim());
                        //GridInfo.Echo("parsePlayerStats: " + pair[0].Trim() + " = " + pair[1].Trim());
                    }
                }
            }
            // parse player gear
            void parsePlayerGear(string data)
            {
                //GridInfo.Echo("parsePlayerGear:");
                string[] parts = data.Split(',');
                foreach(string part in parts)
                {
                    string[] pair = part.Split(':');
                    if (playerGear.ContainsKey(pair[0].Trim())) playerGear[pair[0].Trim()] = pair[1].Trim();
                    else playerGear.Add(pair[0].Trim(), pair[1].Trim());
                    //playerGear.Add(pair[0].Trim(), pair[1].Trim());
                    if (!playerInventory.ContainsKey(pair[1].Trim())) playerInventory.Add(pair[1].Trim(), 1);
                }
            }
            // parse player inventory
            void parsePlayerInventory(string data)
            {
                string[] parts = data.Split(',');
                foreach(string part in parts)
                {
                    string[] pair = part.Split(':');
                    if (pair.Length != 2) continue;
                    if (!playerInventory.ContainsKey(pair[0].Trim())) playerInventory.Add(pair[0].Trim(), int.Parse(pair[1].Trim()));
                    else playerInventory[pair[0].Trim()] = int.Parse(pair[1].Trim());
                }
            }
            // parse player location
            void parsePlayerLocation(string data)
            {
                //GridInfo.Echo("parsePlayerLocation:");
                string[] parts = data.Split(',');
                foreach(string part in parts)
                {
                    string[] vars = part.Split(':');
                    string var_name = vars[0].Trim();
                    if(vars.Length != 2) continue;
                    if (var_name == "map") currentMap = vars[1].Trim();
                    else if (var_name == "x") playerX = int.Parse(vars[1].Trim());
                    else if (var_name == "y") playerY = int.Parse(vars[1].Trim());
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
                        if (pair[0].Trim() == "item_type") item_type = pair[1].Trim();
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
                        stats.Add(pair[0].Trim(), pair[1].Trim());
                        if (pair[0].Trim() == "name") item_name = pair[1].Trim();
                    }
                    if(item_name != "") itemStats.Add(item_name,stats);
                }
            }
            // load graphics
            void loadGraphics()
            {
                if (name == "") return;
                Vector2 size = GridBlocks.TV.SurfaceSize;
                map = new Tilemap(size.X,size.Y);
                map.LoadTiles(SceneCollection.GetScene(name + ".Tiles.0.CustomData"));
                AnimatedCharacter.LoadCharacters(SceneCollection.GetScene(name + ".Characters.0.CustomData"));
            }
            // load maps list
            void loadMapsList()
            {
                //GridInfo.Echo("loadMapsList:");
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
                            maps.Add(pair[0].Trim(), pair[1].Trim());
                            

                        }
                    }
                }
            }
            // parse game logic
            void parseGameLogic(string data)
            {
                string[] actions = data.Split('═');
                foreach(string action in actions)
                {
                    if (action.StartsWith("action:"))
                    {
                        GameAction gameAction = new GameAction(action);
                        gameLogic.Add(gameAction.Name, gameAction);
                        //GridInfo.Echo("gameLogic: " + gameAction.Name);
                    }
                }
            }
            // parse spells
            void parseSpells(string data)
            {
                string[] parts = data.Split('═');
                foreach(string part in parts)
                {
                    if (part.StartsWith("type:spells"))
                    {
                        // get spells settings stuff... (not right now...)
                    }
                    else
                    {
                        Dictionary<string, string> spell = new Dictionary<string, string>();
                        string[] vars = part.Split(',');
                        foreach(string var in vars)
                        {
                            string[] pair = var.Split(':');
                            if (pair.Length != 2) continue;
                            spell.Add(pair[0].Trim(), pair[1].Trim());
                        }
                        if (spell.ContainsKey("name")) playerSpells.Add(spell["name"], spell);
                    }
                }
            }
            // parse bools
            void parseBools(string data)
            {
                //GridInfo.Echo("parseBools:1: " + data);
                string[] vars = data.Split(',');
                foreach(string var in vars)
                {
                    //GridInfo.Echo("parseBools:2: " + var);
                    string[] pair = var.Split(':');
                    if (pair.Length != 2) continue;
                    //GridInfo.Echo("parseBools:3: " + pair[0].Trim() + " = " + pair[1].Trim());
                    gameBools.Add(pair[0].Trim(), bool.Parse(pair[1].Trim()));
                }
            }
            // parse ints
            void parseInts(string data)
            {
                //GridInfo.Echo("parseInts:1: " + data);
                string[] vars = data.Split(',');
                foreach (string var in vars)
                {
                    //GridInfo.Echo("parseInts:2: " + var);
                    string[] pair = var.Split(':');
                    if (pair.Length != 2) continue;
                    //GridInfo.Echo("parseInts:3: " + pair[0].Trim() + " = " + pair[1].Trim());
                    gameInts.Add(pair[0].Trim(), int.Parse(pair[1].Trim()));
                }
            }
            //-------------------------------------------------------------------
            // load a game save
            // note: i haven't figured out what the save string looks like yet
            //-------------------------------------------------------------------
            public void LoadGameSave(string save)
            {
                playerGear.Clear();
                playerInventory.Clear();
                playerStats.Clear();
                playerMaxStats.Clear();

                string[] parts = save.Split('═');
                if (parts.Length < 8) return;
                string[] var = parts[0].Split(':');
                if(var.Length < 2) return;
                playerName = var[1];
                if(playerName.ToLower().Contains(save_tag.ToLower())) playerName = GridInfo.GetVarAs<string>("NewGameName");
                if(playerName == "") playerName = "Hero";
                parsePlayerStats(parts[1], parts[2]);
                parsePlayerGear(parts[3]);
                parsePlayerInventory(parts[4]);
                parsePlayerLocation(parts[5]);
                parseBools(parts[6]);
                parseInts(parts[7]);
                LoadMap(currentMap, playerX, playerY);
                //LoadMap(currentMap, playerX, playerY);
                actionBar.SetActions(controls);
            }
            public void SaveGame()
            {
                string game_saves = SceneCollection.GetScene(name + ".Main.0.Text");
                string[] save_data = game_saves.Split('║');
                if (saveId <= 0 || saveId >= save_data.Length) return;
                save_data[saveId] = GetSaveData();
                game_saves = "";
                bool first = true;
                foreach(string save in save_data)
                {
                    if (first) first = false;
                    else game_saves += "║";
                    game_saves += save;
                }
                SceneCollection.SaveScene(name + ".Main.0.Text", game_saves);
            }
            string GetSaveData()
            {
                string save = "save:" + playerName + "═";
                bool first = true;
                foreach(var stat in playerStats)
                {
                    if (first) first = false;
                    else save += ",";
                    save += stat.Key + ":" + stat.Value;
                }
                save += "═";
                first = true;
                foreach (var stat in playerMaxStats)
                {
                    if (first) first = false;
                    else save += ",";
                    save += stat.Key + ":" + stat.Value;
                }
                save += "═";
                first = true;
                foreach (var gear in playerGear)
                {
                    if (first) first = false;
                    else save += ",";
                    save += gear.Key + ":" + gear.Value;
                }
                save += "═";
                first = true;
                foreach (var item in playerInventory)
                {
                    if (first) first = false;
                    else save += ",";
                    save += item.Key + ":" + item.Value;
                }
                save += "═";
                save += "map:" + currentMap + ",x:" + playerX + ",y:" + playerY;
                save += "═";
                first = true;
                foreach (var gameBool in gameBools)
                {
                    if (first) first = false;
                    else save += ",";
                    save += gameBool.Key + ":" + gameBool.Value;
                }
                save += "═";
                first = true;
                foreach (var gameInt in gameInts)
                {
                    if (first) first = false;
                    else save += ",";
                    save += gameInt.Key + ":" + gameInt.Value;
                }
                return save;
            }
            //-------------------------------------------------------------------
            //
            // load a map
            //
            //-------------------------------------------------------------------
            bool firstLoadMap = false;
            public void LoadMap(string map_name, int x, int y)
            {
                CloseDialog();
                GridInfo.Echo("LoadMap: " + map_name);
                if (!maps.ContainsKey(map_name)) return;
                //currentMap = map_name;
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
                    map.AddOverlayToScreen(tv);
                }
                playerX = x;
                playerY = y;
                map.SetViewCenter(playerX, playerY);
                player.Position = map.GridPosToScreenPos(playerX, playerY);
                GridInfo.Echo("player position: ("+ playerX+", "+ playerY+ ") " + player.Position);
                firstLoadMap = false;
                Tilemap.name = map_name;
                if(Tilemap.music != "") TV.PlayMusic(Tilemap.music);
            }
            //-------------------------------------------------------------------
            // add to screen
            //-------------------------------------------------------------------
            public void AddToScreen(Screen screen)
            {
                tv = screen;
                //GridInfo.Echo("game: add to screen");
                actionBar.RemoveFromScreen(screen);
                //GridInfo.Echo("game: add to screen: actionBar removed");
                map.AddToScreen(screen);
                //GridInfo.Echo("game: add to screen: map added");
                screen.AddSprite(player);
                //GridInfo.Echo("game: add to screen: player added");
                map.AddOverlayToScreen(screen);
                //GridInfo.Echo("game: add to screen: map overlay added");
                gameTitleScreen.AddToScreen(screen);
                //GridInfo.Echo("game: add to screen: title screen added");
                actionBar.AddToScreen(screen);
                //GridInfo.Echo("game: add to screen: actionBar added");
            }
            // remove from screen
            public void RemoveFromScreen(Screen screen)
            {
                map.RemoveFromScreen(screen);
                screen.RemoveSprite(player);
                if(gameTitleScreen != null) gameTitleScreen.RemoveFromScreen(screen);
                if(gameActionMenu != null) gameActionMenu.RemoveFromScreen(screen);
                if(gameItemMenu != null) gameItemMenu.RemoveFromScreen(screen);
                if(gameSpellMenu != null) gameSpellMenu.RemoveFromScreen(screen);
                if(playerStatsWindow != null) playerStatsWindow.RemoveFromScreen(screen);
                if(dialogWindow != null) dialogWindow.RemoveFromScreen(screen);
                if(shopMenu != null) shopMenu.RemoveFromScreen(screen);
                if(battleMenu != null) battleMenu.RemoveFromScreen(screen);
                if(battleWindow != null) battleWindow.RemoveFromScreen(screen);
                if(endScreen != null) endScreen.RemoveFromScreen(screen);
            }
            //-------------------------------------------------------------------
            //
            // update
            //
            //-------------------------------------------------------------------
            public void Update()
            {
                // do enemy logic
                DoEnemyTurn();
                if(gameItemMenu != null) gameItemMenu.Update(playerInventory);
                if(dialogWindow != null || gameActionMenu != null) return;
                //GridInfo.Echo("game: update");
                map.SetViewCenter(playerX, playerY);
                //GridInfo.Echo("game: update: View Centered");
                map.MoveNPCs();
                //GridInfo.Echo("game: update: npcs moved");
                player.Position = map.GridPosToScreenPos(playerX, playerY);
                //GridInfo.Echo("game: update: player moved... update finished");
            }
            //-------------------------------------------------------------------
            string ParseDialogText(string dialog)
            {
                foreach(var gameInt in gameInts)
                {
                    dialog = dialog.Replace("ints." + gameInt.Key, gameInt.Value.ToString());
                }
                foreach (var playerStat in playerStats)
                {
                    dialog = dialog.Replace("player." + playerStat.Key, playerStat.Value.ToString());
                }
                foreach(var enemyStat in enemyStats)
                {
                    dialog = dialog.Replace("enemy." + enemyStat.Key, enemyStat.Value.ToString());
                }
                dialog = dialog.Replace("ENEMYNAME", enemyName);
                dialog = dialog.Replace("PLAYERNAME", playerName);
                return dialog;
            }
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //
            // handle input
            //
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            //-------------------------------------------------------------------
            public string HandleInput(string input)
            {
                if(playerStatsWindow != null) playerStatsWindow.Update(playerStats);
                string action = "";
                if (dialogWindow != null) action = dialogWindow.HandleInput(input);
                else if (gameSpellMenu != null) action = gameSpellMenu.HandleInput(input);
                else if (shopMenu != null) action = shopMenu.HandleInput(input);
                else if (gameItemMenu != null) action = gameItemMenu.HandleInput(input);
                else if (battleMenu != null) action = battleMenu.HandleInput(input);
                else if (gameTitleScreen != null) action = gameTitleScreen.HandleInput(input);
                else if (gameActionMenu == null) action = actionBar.HandleInput(input);
                else action = gameActionMenu.HandleInput(input);
                //GridInfo.Echo("game: input: " +input+ " -> action: " + action);
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
                    gameActionMenu = new GameActionMenu("Menu", 255, actionBar, GetNPCActions(),systemActions);
                    gameActionMenu.AddToScreen(tv);
                    playerStatsWindow = new PlayerStatsWindow(playerStats, 155);
                    playerStatsWindow.AddToScreen(tv);
                    actionBar.SetActions(gameActionMenu.menuScrollActions);
                    return "";
                }
                else if(action == "back")
                {
                    if (gameItemMenu != null) HideItemsMenu();
                    else if (gameSpellMenu != null) HideSpellMenu();
                    else if (shopMenu != null) HideShopMenu();
                    else if (battleMenu != null) return "";
                    else CloseMenu();
                    return "";
                }
                else if (action.StartsWith(save_tag.ToLower()) && gameTitleScreen != null)
                {
                    //GridInfo.Echo("load game: " + action);
                    int.TryParse(action.Replace(save_tag.ToLower(), "").Trim(), out saveId);
                    string game_saves = SceneCollection.GetScene(name + ".Main.0.Text");
                    string[] save_data = game_saves.Split('║');
                    //GridInfo.Echo("saveId: " + saveId + " :: " + save_data.Length);
                    if(saveId <= 0 || saveId >= save_data.Length) return "";                    
                    LoadGameSave(save_data[saveId]);
                    gameTitleScreen.RemoveFromScreen(tv);
                    gameTitleScreen = null;
                }
                else if(action == "close")
                {
                    //GridInfo.Echo("close.... dialog?");
                    if (shopMenu != null)
                    {
                        actionBar.SetActions(shopMenu.menuScrollActions);
                        if (dialogWindow != null)
                        {
                            dialogWindow.RemoveFromScreen(tv);
                            dialogWindow = null;
                        }
                        if(playerStatsWindow != null)
                        {
                            playerStatsWindow.Update(playerStats);
                        }
                    }
                    else if(battleMenu != null)
                    {
                        //GridInfo.Echo("close... battleMenu not null:");
                        if (dialogWindow != null)
                        {
                            dialogWindow.RemoveFromScreen(tv);
                            dialogWindow = null;
                        }
                        if (encounterOver)
                        {
                            battleWindow.RemoveFromScreen(tv);
                            battleWindow = null;
                            battleMenu.RemoveFromScreen(tv);
                            battleMenu = null;
                            actionBar.SetActions(controls);
                            playerStatsWindow.RemoveFromScreen(tv);
                            playerStatsWindow = null;
                            encounterOver = false;
                            //GridInfo.Echo("encounterOver!"+gameBools.ContainsKey("playerWon").ToString() + ", " + gameBools["playerWon"].ToString() + ", " + (promptNPC != null).ToString() + ", " + (promptNPC.encounterWin != null).ToString());
                            if(gameBools.ContainsKey("playerWon") && gameBools["playerWon"] && promptNPC != null && promptNPC.encounterWin != null)
                            {
                                //GridInfo.Echo("encounterWin: " + promptNPC.encounterWin.Name);
                                promptNPC.encounterWin.Run();
                            }
                            if(Tilemap.music != "") TV.PlayMusic(Tilemap.music);
                        } else actionBar.SetActions(battleMenu.menuScrollActions);
                        //GridInfo.Echo("close... battleMenu not null: "+battleMenu.menuScrollActions);
                        return "";
                    }
                    else
                    {
                        //GridInfo.Echo("not in battle? close dialog!");
                        CloseDialog();
                    }
                    if (gameActionMenu == null) actionBar.SetActions(controls);
                }
                else if(promptNPC != null && action == "yes")
                {
                    //GridInfo.Echo("promptNPC: " + promptNPC.yes.Count+ " :: "+promptTag);
                    dialogWindow.RemoveFromScreen(tv);
                    dialogWindow = null;
                    if (promptNPC.yes.ContainsKey(promptTag)) promptNPC.yes[promptTag].Run();
                    promptNPC = null;
                    promptTag = "";
                    return "";
                }
                else if(promptNPC != null && action == "no")
                {
                    //GridInfo.Echo("promptNPC: " + promptNPC.no.Count+" :: "+promptTag);
                    dialogWindow.RemoveFromScreen(tv);
                    dialogWindow = null;
                    if (promptNPC.no.ContainsKey(promptTag)) promptNPC.no[promptTag].Run();
                    promptNPC = null;
                    promptTag = "";
                    return "";
                }
                else if(action == "items")
                {
                    ShowItemsMenu();
                    return "";
                }
                else if(action == "spells")
                {
                    ShowSpellsMenu();
                    return "";
                }
                else if(action == "attack")
                {
                    if(gameLogic.ContainsKey("PlayerAttack")) gameLogic["PlayerAttack"].Run();
                    else GridInfo.Echo("PlayerAttack not found");
                    isPlayerTurn = false;
                    return "";
                }
                else if(action == "run")
                {
                    if(gameLogic.ContainsKey("PlayerRun")) gameLogic["PlayerRun"].Run();
                    else GridInfo.Echo("PlayerRun not found");
                }
                else if(action == "turn done")
                {
                    isPlayerTurn = false;
                    if (gameItemMenu != null) HideItemsMenu();
                    if (gameSpellMenu != null) HideSpellMenu();
                }
                return action;
            }
            void ShowSpellsMenu()
            {
                //GridInfo.Echo("ShowSpellsMenu");
                gameSpellMenu = new GameSpellMenu("Spells", 300, actionBar);
                gameSpellMenu.AddToScreen(tv);
                if (gameActionMenu != null) gameActionMenu.Visible = false;
                if (battleMenu != null) battleMenu.Visible = false;
            }
            void HideSpellMenu()
            {
                if (gameActionMenu != null && battleMenu == null) gameActionMenu.Visible = true;
                else if (battleMenu != null) battleMenu.Visible = true;
                if (gameSpellMenu == null) return;
                gameSpellMenu.RemoveFromScreen(tv);
                gameSpellMenu = null;
            }
            void ShowItemsMenu()
            {
                //GridInfo.Echo("ShowItemsMenu");
                if(gameActionMenu != null) gameActionMenu.Visible = false;
                if (battleMenu != null) battleMenu.Visible = false;
                List<GameItem> items = new List<GameItem>();
                foreach(var item in playerInventory)
                {
                    //GridInfo.Echo("item: " + item.Key + " = " + item.Value);
                    if (!itemStats.ContainsKey(item.Key)) continue;
                    //GridInfo.Echo("item: " + item.Key + " = " + item.Value + " :: " + itemStats[item.Key]["item_type"]);
                    if (itemStats[item.Key].ContainsKey("effect") && gameLogic.ContainsKey(itemStats[item.Key]["effect"]))
                    {
                        //GridInfo.Echo("item: " + item.Key + " = " + item.Value + " :: " + itemStats[item.Key]["item_type"] + " :1: " + itemStats[item.Key]["effect"]);
                        items.Add(new GameItem(item.Key, item.Value, gameLogic[itemStats[item.Key]["effect"]]));
                    }
                    else if (playerGear.ContainsKey(itemStats[item.Key]["item_type"]))
                    {
                        //GridInfo.Echo("item: " + item.Key + " = " + item.Value + " :: " + itemStats[item.Key]["item_type"] + " :2: " + playerGear[itemStats[item.Key]["item_type"]]);
                        items.Add(new GameItem(item.Key, itemStats[item.Key]["item_type"], item.Value));
                    }
                    else
                    {
                        //GridInfo.Echo("item: " + item.Key + " = " + item.Value + " :: " + itemStats[item.Key]["item_type"] + " :3: " + item.Value);
                        items.Add(new GameItem(item.Key, item.Value));
                    }
                }
                //GridInfo.Echo("ShowItemsMenu: " + items.Count);
                gameItemMenu = new GameItemMenu("Items", 300, actionBar, items);
                gameItemMenu.AddToScreen(tv);
            }
            void HideItemsMenu()
            {
                if(gameActionMenu != null && battleMenu == null) gameActionMenu.Visible = true;
                else if(battleMenu != null) battleMenu.Visible = true;
                if (gameItemMenu == null) return;
                gameItemMenu.RemoveFromScreen(tv);
                gameItemMenu = null;
            }
            void HideShopMenu()
            {
                if(gameActionMenu != null) gameActionMenu.Visible = true;
                if (shopMenu == null) return;
                shopMenu.RemoveFromScreen(tv);
                shopMenu = null;
            }
            // dialog window closed
            void CloseDialog()
            {
                /*
                if(encounterOver && battleWindow != null)
                {
                    battleWindow.RemoveFromScreen(tv);
                    battleWindow = null;
                    battleMenu.RemoveFromScreen(tv);
                    battleMenu = null;
                    encounterOver = false;
                }
                */
                CloseMenu();
                if (dialogWindow == null) return;
                dialogWindow.RemoveFromScreen(tv);
                dialogWindow = null;
            }
            void CloseMenu()
            {
                if (gameItemMenu != null) HideItemsMenu();
                if (gameSpellMenu != null) HideSpellMenu();
                if (shopMenu != null) HideShopMenu();
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
                //GridInfo.Echo("not occupied");
                if (exit != null)
                {
                    GridInfo.Echo("exit: " + exit.Map);
                    LoadMap(exit.Map, exit.MapX, exit.MapY);
                    return;
                }
                if (!map.IsGround(x, y)) return;
                //GridInfo.Echo("is ground");
                playerX = x;
                playerY = y;
                map.SetViewCenter(playerX, playerY);
                player.Position = map.GridPosToScreenPos(playerX, playerY);
                // do player step logic (for example, counting down radient spell effect)
                if(gameLogic.ContainsKey("PlayerStep")) gameLogic["PlayerStep"].Run();
            }
            // get game actions from npc in front of player (0, 1, or 2 tiles away)
            public List<GameAction> GetNPCActions() {                 
                int x_offset = 0;
                int y_offset = 0;
                npc npc = map.GetNPCOn(playerX + x_offset, playerY + y_offset);
                if (npc != null && npc.NPCVisible)
                {
                    return npc.actions;
                }
                if (player.Direction == "up") y_offset--;
                else if (player.Direction == "down") y_offset++;
                else if (player.Direction == "left") x_offset--;
                else if (player.Direction == "right") x_offset++;
                // directly in front of player
                npc = map.GetNPCOn(playerX + x_offset, playerY + y_offset);
                if (npc != null && npc.NPCVisible)
                {
                    npc.FacePlayer(player);
                    return npc.actions;
                }
                else if (map.IsShopCounter(playerX + x_offset, playerY + y_offset))
                {
                    // 2 tiles away (like a shop keeper on the other side of a wall)
                    npc = map.GetNPCOn(playerX + (x_offset * 2), playerY + (y_offset * 2));
                    if (npc != null && npc.NPCVisible)
                    {
                        npc.FacePlayer(player);
                        return npc.actions;
                    }
                }
                return new List<GameAction>();
            }
            //-------------------------------------------------------------------
            //
            // IGameVars
            //
            //-------------------------------------------------------------------
            public T GetVarAs<T>(string name, npc me, T defaultValue)
            {
                //GridInfo.Echo("GetValueAs: (key) " + name+" (default) "+defaultValue.ToString());
                string value = GetValue(name, me, true);
                //GridInfo.Echo("GetValueAs: (value) " + value);
                if (value == "") return defaultValue;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            public void SetVar<T>(string name, npc npc, T value)
            {
                //GridInfo.Echo("SetValueAs: (key) " + name + " (value) " + value.ToString());
                SetValue(name, npc, value.ToString());
            }
            void SetValue(string name, npc me, string value)
            {
                //GridInfo.Echo("SetValue: " + name + " = " + value);
                string objectName = "";
                if (name.Contains("."))
                {
                    string[] parts = name.Split('.');
                    //GridInfo.Echo("SetValue: parts: " + parts.Length);
                    objectName = parts[0].Trim();
                    name = parts[1].Trim();
                }
                // find the object to set the value on
                if (objectName == "player")
                {
                    if (name == "status") playerStatus = value;
                    else if(playerStats.ContainsKey(name)) playerStats[name] = double.Parse(value);
                }
                else if (objectName == "playerMax")
                {
                    playerMaxStats[name] = (int)double.Parse(value);
                }
                else if (objectName == "")
                {
                    SetNPCValue(name, me, value);
                }
                else if (objectName == "inventory")
                {
                    playerInventory[name] = (int)double.Parse(value);
                }
                else if (objectName == "bools")
                {
                    gameBools[name] = bool.Parse(value);
                }
                else if (objectName == "ints")
                {
                    //GridInfo.Echo("SetValue: ints: " + name + " = " + value);
                    gameInts[name] = (int)double.Parse(value);
                    //GridInfo.Echo("SetValue: ints: " + name + " = " + value + " = " + gameInts[name]);
                }
                else if (objectName == "map")
                {
                    if (name == "darkRadius") Tilemap.darkRadius = (int)double.Parse(value);
                }
                else if (objectName == "enemy")
                {
                    if(name == "name") enemyName = value;
                    else if (name == "status") enemyStatus = value;
                    else if (enemyStats.ContainsKey(name)) enemyStats[name] = (int)double.Parse(value);
                }
                else if(objectName == "encounter")
                {
                    //GridInfo.Echo("SetValue: encounter: " + name + " = " + value);
                    // set encounter vars
                    if(name == "over") encounterOver = bool.Parse(value);
                    if (encounterOver && battleWindow != null) battleWindow.HideEnemy();
                }
                else
                {
                    //GridInfo.Echo("SetValue: unknown object: " + objectName);
                }
            }
            void SetNPCValue(string name, npc me, string value)
            {
                if (me == null) return;
                //GridInfo.Echo("SetNPCValue: " + name + " = " + value);  
                name = name.ToLower();
                if (name.StartsWith("vis"))
                {
                    //GridInfo.Echo("SetNPCValue: visible");
                    me.NPCVisible = bool.Parse(value);
                }
                else if (name.Contains("block"))
                {
                    me.BlocksMovement = bool.Parse(value);
                }
                else if (name == "x")
                {
                    me.X = (int)double.Parse(value);
                }
                else if (name == "y")
                {
                    me.Y = (int)double.Parse(value);
                }
                else if (name.StartsWith("dir"))
                {
                    me.SetDirection(value);
                }
            }
            // get a value
            string GetValue(string name, npc me, bool hasDefault = false)
            {
                string objectName = "";
                if (name.Contains("."))
                {
                    string[] parts = name.Split('.');
                    objectName = parts[0].Trim();
                    name = parts[1].Trim();
                }
                // find the object to set the value on
                if (objectName == "player")
                {
                    if (name == "x") return playerX.ToString();
                    else if (name == "status") return playerStatus;
                    else if (name == "y") return playerY.ToString();
                    else
                    if (playerStats.ContainsKey(name)) return playerStats[name].ToString();
                }
                else if (objectName == "playerMax" && playerMaxStats.ContainsKey(name))
                {
                    return playerMaxStats[name].ToString();
                }
                else if (objectName == "")
                {
                    return GetNPCValue(name, me);
                }
                else if (objectName == "inventory")
                {
                    //GridInfo.Echo("GetValue: inventory: " + name);
                    if (playerInventory.ContainsKey(name))
                    {
                        //GridInfo.Echo("GetValue: inventory: " + name + " = " + playerInventory[name]);
                        return playerInventory[name].ToString();
                    }
                    //GridInfo.Echo("GetValue: inventory: " + name + " not found");
                    return "0";
                }
                else if (objectName == "bools")
                {
                    if (gameBools.ContainsKey(name)) return gameBools[name].ToString();
                    else if (!hasDefault) return "false";
                }
                else if (objectName == "ints")
                {
                    if (gameInts.ContainsKey(name))
                    {
                        //GridInfo.Echo("GetValue: ints: " + name + " = " + gameInts[name]);
                        return gameInts[name].ToString();
                    }
                    else if (!hasDefault) return "0";
                }
                else if (objectName == "map")
                {
                    //GridInfo.Echo("GetValue: map: " + name);
                    if (name == "darkRadius") return Tilemap.darkRadius.ToString();
                    else if (name == "OnDangerTile") return Tilemap.IsDanger(playerX, playerY).ToString();
                    else if (name == "OnToxicTile") return Tilemap.IsOnToxic.ToString();
                    else if (name == "ToxicLevel") return Tilemap.PlayerToxicLevel.ToString();
                    else if (name == "name") return Tilemap.name;
                    else if (name == "hasEncounters") return Tilemap.hasEncounters.ToString();
                    else if (name == "encounter") return Tilemap.GetEncounter(playerX, playerY).ToString();
                }
                else if (objectName == "enemy")
                {
                    if (name == "name") return enemyName;
                    else if (name == "status") return enemyStatus;
                    else if (enemyStats.ContainsKey(name)) return enemyStats[name].ToString();
                    else return "";
                }
                else if(objectName == "encounter")
                {
                    // get encounter vars
                    if (name == "over") return encounterOver.ToString();
                    else if (name == "enemy") return enemyName;
                    else if (name == "playerTurn") return isPlayerTurn.ToString();
                    else if (name == "inCombat") return IsInEncounter().ToString();
                }
                else if (objectName == "item")
                {
                    if (itemStats.ContainsKey(name))
                    {
                        if (itemStats[name].ContainsKey("effect")) return itemStats[name]["effect"];
                        else if (itemStats[name].ContainsKey("item_type")) return itemStats[name]["item_type"];
                        else if (itemStats[name].ContainsKey("cost")) return itemStats[name]["cost"];
                        else if (itemStats[name].ContainsKey("name")) return itemStats[name]["name"];
                        else if (itemStats[name].ContainsKey("defense")) return itemStats[name]["defense"];
                        else if (itemStats[name].ContainsKey("attack")) return itemStats[name]["attack"];
                    }
                }
                else if(objectName == "gear")
                {
                    int valueInt = 0;
                    foreach(var gear in playerGear)
                    {
                        int gearInt = 0;
                        if(itemStats.ContainsKey(gear.Value) && itemStats[gear.Value].ContainsKey(name))
                        {
                            int.TryParse(itemStats[gear.Value][name], out gearInt);
                            valueInt += gearInt;
                        }
                    }
                    return valueInt.ToString();
                }
                else
                {
                    //GridInfo.Echo("GetValue: unknown object: " + objectName + "... or key (" + name + ") wasn't present in dictionary.");
                }
                return "";
            }
            string GetNPCValue(string name, npc me)
            {
                if (me == null) return "";
                name = name.ToLower();
                if (name.StartsWith("vis"))
                {
                    return me.NPCVisible.ToString();
                }
                else if (name.Contains("block"))
                {
                    return me.BlocksMovement.ToString();
                }
                else if (name == "x")
                {
                    return me.X.ToString();
                }
                else if (name == "y")
                {
                    return me.Y.ToString();
                }
                else if (name.StartsWith("dir"))
                {
                    return me.Direction;
                }
                //GridInfo.Echo("GetNPCValue: unknown property: " + name);
                return "";
            }
            //-------------------------------------------------------------------
            // IGameShop
            //-------------------------------------------------------------------
            public void Shop(string itemlist)
            {
                //GridInfo.Echo("ShowShop: " + itemlist);
                List<ShopItem> shop_items = new List<ShopItem>();
                string[] items = itemlist.Split(',');
                foreach (string item in items)
                {
                    if (itemStats.ContainsKey(item)) shop_items.Add(new ShopItem(itemStats[item]));
                }
                //GridInfo.Echo("ShowShop: " + shop_items.Count);
                shopMenu = new ShopMenu("Buy", 420, actionBar, shop_items);
                shopMenu.playerSelling = false;
                shopMenu.AddToScreen(tv);
                gameActionMenu.Visible = false;
            }
            public void Sell()
            {
                //GridInfo.Echo("SellItems");
                List<ShopItem> shop_items = new List<ShopItem>();
                foreach (var item in playerInventory)
                {
                    //GridInfo.Echo("item: " + item.Key + " = " + item.Value);
                    if (!itemStats.ContainsKey(item.Key) || !itemStats[item.Key].ContainsKey("cost")) continue;
                    shop_items.Add(new ShopItem(itemStats[item.Key]));
                }
                //GridInfo.Echo("SellItems: " + shop_items.Count);
                if (shop_items.Count == 0)
                {
                    Say(NothingToSell);
                    return;
                }
                shopMenu = new ShopMenu("Sell", 420, actionBar, shop_items);
                shopMenu.playerSelling = true;
                shopMenu.AddToScreen(tv);
                gameActionMenu.Visible = false;
            }
            //-------------------------------------------------------------------
            // IGameDialog
            //-------------------------------------------------------------------
            public void Say(string dialog)
            {
                if (dialogWindow == null)
                {
                    dialogWindow = new DialogWindow(ParseDialogText(dialog), new Vector2(500, 100), actionBar);
                    dialogWindow.AddToScreen(tv);
                }
                else dialogWindow.Append(ParseDialogText(dialog));
            }
            public void Go(string map_name, int x, int y)
            {
                LoadMap(map_name, x, y);
            }
            public void Ask(string dialog, npc npc, string tag)
            {
                //GridInfo.Echo("ShowDialogPrompt: " + dialog);
                Say(dialog);
                promptNPC = npc;
                actionBar.SetActions("Yes No   Close");
                promptTag = tag;
                //GridInfo.Echo("ShowDialogPrompt: " + promptTag);
            }
            public void Run(string action)
            {
                if(gameLogic.ContainsKey(action)) gameLogic[action].Run();
            }
            public int GetPlayerX()
            {
                return playerX;
            }
            public int GetPlayerY()
            {
                return playerY;
            }
            public void ShowEndScreen(string message)
            {
                GridInfo.Echo("ShowEndScreen: " + message);
                endScreen = new EndScreen(message,actionBar);
                endScreen.AddToScreen(tv);
            }
            //-------------------------------------------------------------------
            // IGameInventory
            //-------------------------------------------------------------------
            public bool HasItem(string item)
            {
                return playerInventory.ContainsKey(item) && playerInventory[item] > 0;
            }
            public void AddItem(string item)
            {
                if(playerInventory.ContainsKey(item)) playerInventory[item]++;
                else playerInventory.Add(item, 1);
            }
            public void RemoveItem(string item)
            {
                if(playerInventory.ContainsKey(item))
                {
                    playerInventory[item]--;
                    if (playerInventory[item] <= 0) playerInventory.Remove(item);
                }
            }
            public bool HasEquipped(string item)
            {
                if(itemStats.ContainsKey(item) && itemStats[item].ContainsKey("item_type") && playerGear.ContainsKey(itemStats[item]["item_type"]))
                {
                    return playerGear[itemStats[item]["item_type"]] == item;
                }
                return false;
            }
            public bool CanEquip(string item)
            {
                if (itemStats.ContainsKey(item) && itemStats[item].ContainsKey("item_type"))
                {
                    return playerGear.ContainsKey(itemStats[item]["item_type"]);
                }
                return false;
            }
            public void Equip(string item)
            {
                if (itemStats.ContainsKey(item) && itemStats[item].ContainsKey("item_type") && playerGear.ContainsKey(itemStats[item]["item_type"]))
                {
                    playerGear[itemStats[item]["item_type"]] = item;
                }
                if (gameLogic.ContainsKey("CalculatePlayerStats")) gameLogic["CalculatePlayerStats"].Run();
            }
            public void Unequip(string item)
            {
                if (itemStats.ContainsKey(item) && itemStats[item].ContainsKey("item_type") && playerGear.ContainsKey(itemStats[item]["item_type"]))
                {
                    playerGear[itemStats[item]["item_type"]] = "";
                }
                if (gameLogic.ContainsKey("CalculatePlayerStats")) gameLogic["CalculatePlayerStats"].Run();
            }
            public Dictionary<string, string> GetItemStats(string item)
            {
                if(itemStats.ContainsKey(item))
                {
                    return itemStats[item];
                }
                return null;
            }
            //-------------------------------------------------------------------
            // IGameSpells
            //-------------------------------------------------------------------
            public Dictionary<string, int> GetSpells()
            {
                Dictionary<string, int> spells = new Dictionary<string, int>();
                foreach(var spell in playerSpells)
                {
                    int cost = 0;
                    if (spell.Value.ContainsKey("cost")) int.TryParse(spell.Value["cost"], out cost);
                    spells.Add(spell.Key, 0);
                }
                return spells;
            }

            public void CastSpell(string spell)
            {
                //GridInfo.Echo("CastSpell: " + spell);
                if (playerSpells.ContainsKey(spell) && playerSpells[spell].ContainsKey("effect") && gameLogic.ContainsKey(playerSpells[spell]["effect"]))
                {
                    int cost = 0;
                    int.TryParse(playerSpells[spell]["cost"], out cost);
                    //GridInfo.Echo("CastSpell: " + cost + " :: " + playerStats[spellsCost]);
                    if (playerStats[spellsCost] < cost) return;
                    playerStats[spellsCost] -= cost;
                    HideSpellMenu();
                    gameLogic[playerSpells[spell]["effect"]].Run();
                }
            }

            public bool CanCastSpell(string spell)
            {
                //GridInfo.Echo("CanCastSpell: " + spell);
                if (playerSpells.ContainsKey(spell) && playerSpells[spell].ContainsKey("cost"))
                {
                    int cost = 0;
                    int.TryParse(playerSpells[spell]["cost"], out cost);
                    //GridInfo.Echo("CanCastSpell: " + cost + " :: " + playerStats[spellsCost]);
                    return playerStats[spellsCost] >= cost;
                }
                return false;
            }
            public bool HasUnlockedSpell(string spell)
            {
                if(playerSpells.ContainsKey(spell) && playerSpells[spell].ContainsKey(spellsLevel))
                {
                    int spellLevel = 0;
                    int.TryParse(playerSpells[spell][spellsLevel], out spellLevel);
                    return playerStats[spellsLevel] >= spellLevel;
                }
                return false;
            }
            public bool IsFieldSpell(string spell)
            {
                if (!playerSpells.ContainsKey(spell)) return false;
                if (playerSpells[spell].ContainsKey(spellsBattle)) return playerSpells[spell][spellsBattle] == "false";
                return true;
            }
            public bool IsCombatSpell(string spell)
            {
                if(!playerSpells.ContainsKey(spell)) return false;
                if(playerSpells[spell].ContainsKey(spellsBattle)) return playerSpells[spell][spellsBattle] == "true";
                return true;
            }
            //-------------------------------------------------------------------
            // IGameEncounter
            //-------------------------------------------------------------------
            public void StartEncounter(string enemyGroup, npc npc = null)
            {
                promptNPC = npc;
                string[] enemies = enemyGroup.Split(',');
                Random rnd = new Random();
                int enemyIndex = rnd.Next(0, enemies.Length);
                string enemy = LoadEnemy(enemies[enemyIndex]);
                string battleBack = LoadBattleBack();
                if(enemy == "") 
                {
                    //GridInfo.Echo("StartEncounter: " + enemies[enemyIndex] + " not found");
                    return;
                }
                if(battleMusic != "") TV.PlayMusic(battleMusic);
                battleWindow = new BattleWindow(battleBack, enemy, enemyStats["x"], enemyStats["y"]);
                battleWindow.AddToScreen(tv);
                battleMenu = new BattleMenu("Combat",160, actionBar);
                battleMenu.AddToScreen(tv);
                if(gameActionMenu != null)
                {
                    gameActionMenu.RemoveFromScreen(tv);
                    gameActionMenu = null;
                }
                // bumpt to front
                if(playerStatsWindow != null) playerStatsWindow.RemoveFromScreen(tv);
                else playerStatsWindow = new PlayerStatsWindow(playerStats, 155);
                playerStatsWindow.AddToScreen(tv);
                actionBar.SetActions(battleMenu.menuScrollActions);
                actionBar.RemoveFromScreen(tv);
                actionBar.AddToScreen(tv);
                if(gameLogic.ContainsKey("StartEncounter")) gameLogic["StartEncounter"].Run();
            }
            // load an enemy from the scene collection
            string LoadEnemy(string enemy)
            {
                enemyName = enemy;
                string[] enemies = SceneCollection.GetScene(name+ ".Enemies.0.CustomData").Split('║');
                string sprite = "";
                foreach (string enemyData in enemies)
                {
                    if (enemyData.Contains("name:" + enemy))
                    {
                        enemyStats.Clear();
                        string[] parts = enemyData.Split('═');
                        if(parts.Length < 2)
                        {
                            //GridInfo.Echo("LoadEnemy: " + enemy + " not found");
                            return "";
                        }
                        string[] stats = parts[0].Split(',');
                        foreach (string stat in stats)
                        {
                            string[] statParts = stat.Split(':');
                            if (statParts.Length != 2) continue;
                            int value = 0;
                            if (int.TryParse(statParts[1].Trim(), out value)) enemyStats.Add(statParts[0].Trim(), value);
                            else if (statParts[0].Trim() == "pattern") enemyPattern = statParts[1].Split(',');
                        }
                        sprite = parts[1].Trim();
                        break;
                    }
                }
                return sprite;
            }
            string LoadBattleBack()
            {
                string[] battleBacks = SceneCollection.GetScene(name + ".Enemies.0.Text").Split('║');
                string sprite = "";
                foreach (string battleBack in battleBacks)
                {
                    if (battleBack.Contains(Tilemap.name))
                    {
                        string[] parts = battleBack.Split('═');
                        if (parts.Length < 2)
                        {
                            //GridInfo.Echo("LoadBattleBack: " + enemyName + " not found");
                            return "";
                        }
                        sprite = parts[1].Trim();
                        break;
                    }
                }
                return sprite;
            }
            public void EndEncounter()
            {
                encounterOver = true;
            }
            public bool IsInEncounter()
            {
                return battleMenu != null;
            }
            void DoEnemyTurn()
            {
                if(!IsInEncounter() || isPlayerTurn || dialogWindow != null) return;
                if(gameLogic.ContainsKey("EnemyTurn")) gameLogic["EnemyTurn"].Run();
                if (isPlayerTurn) return;
                //isPlayerTurn = true;
                if (enemyPattern == null || enemyPattern.Length == 0)
                {
                    if (gameLogic.ContainsKey("EnemyAttack")) gameLogic["EnemyAttack"].Run();
                    return;
                }
                else
                {
                    Random rnd = new Random();
                    int enemyIndex = rnd.Next(0, enemyPattern.Length);
                    if (gameLogic.ContainsKey(enemyPattern[enemyIndex])) gameLogic[enemyPattern[enemyIndex]].Run();
                    else GridInfo.Echo("EnemyTurn: " + enemyPattern[enemyIndex] + " not found");
                }
                StartPlayerTurn();
            }
            void StartPlayerTurn()
            {
                isPlayerTurn = true;
                if(gameLogic.ContainsKey("PlayerTurn")) gameLogic["PlayerTurn"].Run();
                if(!isPlayerTurn) DoEnemyTurn();
            }
            //-------------------------------------------------------------------
        }
        //-----------------------------------------------------------------------
    }
}
