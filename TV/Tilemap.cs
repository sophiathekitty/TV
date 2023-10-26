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
        // tilemap handling
        //-----------------------------------------------------------------------
        // handles loading the tiles from a string and uses screen sprites to
        // display them. can display a visible widow of sprites in a larger map
        // and scroll the window around.
        //-----------------------------------------------------------------------
        public class Tilemap
        {
            public static float fontSize = 0.204f;
            List<ScreenSprite> visibleTiles = new List<ScreenSprite>();
            List<ScreenSprite> overlayTiles = new List<ScreenSprite>();
            static string[] tileMap;
            string[] roofMap;
            static int mapWidth;
            static int mapHeight;
            Dictionary<char,string> tiles = new Dictionary<char,string>();
            string groundTiles = "";
            static string toxicTiles = "";
            static string dangerTiles = "";
            string counterTiles = ""; // can get actions through these...
            char roofTile = ' ';
            char darkTile = ' ';
            static int _darkRadius = 1;
            public static int darkRadius 
            {
                get
                {
                    return _darkRadius;
                }
                set
                {
                    _darkRadius = value;
                    GridInfo.Echo("darkRadius: " + _darkRadius);
                    if (_darkRadius < 1) _darkRadius = 1;
                }
            }
            public static string name = "";
            bool dark = false;
            float screenWidth = 0;
            float screenHeight = 0;
            int viewPortWidth = 11;
            int viewPortHeight = 6;
            int viewPortX = 0;
            int viewPortY = 0;
            int tileWidth = 0;
            int tileHeight = 0;
            static int resolutionWidth = 178;
            static int resolutionHeight = 107;
            List<npc> npcs = new List<npc>();
            static List<TilemapExit> exits = new List<TilemapExit>();
            public static TilemapExit Exit
            {
                get
                {
                    foreach(TilemapExit exit in exits)
                    {
                        if(exit.Map == "outworld") return exit;
                    }
                    return null;
                }
            }
            public Vector2 TileSize
            {
                get
                {
                    float width = screenWidth / viewPortWidth;
                    float height = screenHeight / viewPortHeight;
                    if(width < height) return new Vector2(width, width);
                    return new Vector2(height, height);    
                }
            }
            public Tilemap(float width, float height)
            {
                screenWidth = width;
                screenHeight = height;
            }
            public Tilemap(float width, float height, string tileSet)
            {
                screenWidth = width;
                screenHeight = height;
                LoadTiles(tileSet);
            }
            public Tilemap(float width, float height, string tileSet, string map)
            {
                screenWidth = width;
                screenHeight = height;
                LoadTiles(tileSet);
                LoadMap(map);
                ApplyViewPortTiles();
            }
            // get a tile char at a position
            public static char GetTile(int x, int y)
            {
                if (x < 0 || x >= mapWidth) return ' ';
                if (y < 0 || y >= mapHeight) return ' ';
                return tileMap[y][x];
            }
            bool playerOnRoof
            {
                get { return (roofMap[GameAction.Game.GetPlayerY()][GameAction.Game.GetPlayerX()] == '1'); }
            }
            // get roof for a position
            public char GetRoof(int x, int y)
            {
                if(roofMap == null || roofMap.Length != mapHeight) return ' ';
                if (x < 0 || x >= mapWidth) return ' ';
                if (y < 0 || y >= mapHeight) return ' ';
                if (playerOnRoof)
                {
                    if (roofMap[y][x] == '0') return darkTile;
                }
                else
                {
                    if (roofMap[y][x] == '1') return roofTile;
                }
                return ' ';
            }
            // can walk on this tile?
            public bool IsGround(int x, int y)
            {
                char tile = GetTile(x, y);
                //GridInfo.Echo("IsGround: " + tile +" ??? " + groundTiles);
                return groundTiles.Contains(tile);
            }
            // is this tile toxic?
            public static bool IsToxic(int x, int y)
            {
                char tile = GetTile(x, y);
                GridInfo.Echo("IsToxic: " + tile + " ??? " + toxicTiles);
                return toxicTiles.Contains(tile);
            }
            public static int ToxicLevel(int x, int y)
            {
                char tile = GetTile(x, y);
                int index = toxicTiles.IndexOf(tile);
                if (index < 0) return 0;
                return index + 1;
            }
            public static bool IsOnToxic
            {
                get
                {
                    //GridInfo.Echo("IsOnToxic: " + GameAction.Game.GetPlayerX() + "," + GameAction.Game.GetPlayerY());
                    return IsToxic(GameAction.Game.GetPlayerX(), GameAction.Game.GetPlayerY());
                }
            }
            public static int PlayerToxicLevel
            {
                get
                {
                    return ToxicLevel(GameAction.Game.GetPlayerX(), GameAction.Game.GetPlayerY());
                }
            }
            // is this tile dangerous?
            public static bool IsDanger(int x, int y)
            {
                char tile = GetTile(x, y);
                return dangerTiles.Contains(tile);
            }
            public bool IsOccupied(int x, int y)
            {
                if(x == GameAction.Game.GetPlayerX() && y == GameAction.Game.GetPlayerY()) return true;
                foreach(npc npc in npcs)
                {
                    if (npc.BlocksMovement && npc.X == x && npc.Y == y && npc.NPCVisible) return true;
                }
                return false;
            }
            public TilemapExit ExitOn(int x, int y)
            {
                bool edge = false;
                if(x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) edge = true;
                //GridInfo.Echo("ExitOn: " + x + "," + y + " edge:"+edge.ToString());
                foreach (TilemapExit exit in exits)
                {
                    if (edge && (exit.Edge || exit.X == -1 || exit.Y == -1)) return exit;
                    else if (exit.X == x && exit.Y == y) return exit;
                }
                return null;
            }
            public bool IsShopCounter(int x, int y)
            {
                char tile = GetTile(x, y);
                return counterTiles.Contains(tile);
            }
            public npc GetNPCOn(int x, int y)
            {
                foreach(npc npc in npcs)
                {
                    if (npc.X == x && npc.Y == y) return npc;
                }
                return null;
            }
            // load the tile set from a string
            public void LoadTiles(string data)
            {
                //GridInfo.Echo("LoadTiles:");
                if(tiles == null) tiles = new Dictionary<char,string>();
                tiles.Clear();
                string[] parts = data.Split('║');
                foreach(string part in parts)
                {
                    // get tile set info
                    if (part.Contains("type:tiles"))
                    {
                        // get tileset info
                        string[] vars = part.Split(',');
                        foreach(string var in vars)
                        {
                            string[] pair = var.Split(':');
                            //if (pair.Length > 0) GridInfo.Echo("LoadTiles: " + pair[0]);
                            if (pair.Length == 2)
                            {
                                if (pair[0] == "ground") groundTiles = pair[1];
                                else if (pair[0] == "toxic") toxicTiles = pair[1];
                                else if (pair[0] == "danger") dangerTiles = pair[1];
                                else if (pair[0] == "width") tileWidth = int.Parse(pair[1]);
                                else if (pair[0] == "height") tileHeight = int.Parse(pair[1]);
                                else if (pair[0] == "counter") counterTiles = pair[1];
                                else if (pair[0] == "roof") roofTile = pair[1][0];
                                else if (pair[0] == "dark") darkTile = pair[1][0];
                            }
                        }
                        if(tileHeight != 0)  viewPortHeight = (int)(resolutionHeight / tileHeight);
                        if(tileWidth != 0) viewPortWidth = (int)(resolutionWidth / tileWidth);
                    }
                    else
                    {
                        // get the tiles
                        string[] tileSetData = part.Split('═');
                        foreach(string tileData in tileSetData)
                        {
                            string[] tile = tileData.Split(':');
                            if (tile.Length == 2)
                            {
                                tiles.Add(tile[0][0], tile[1]);
                            }
                        }
                    }
                }
            }
            // load a tile map from a string
            public void LoadMap(string data)
            {
                roofMap = null;
                //darkRadius = 1;
                //GridInfo.Echo("LoadMap: data: "+data.Length);
                string[] parts = data.Split('║');
                //GridInfo.Echo("LoadMap: parts: "+parts.Length);    
                foreach(string part in parts)
                {
                    if (part.StartsWith("type:map"))
                    {
                        //GridInfo.Echo("LoadMap: map: "+part.Length);
                        // get map tiles
                        string[] map_parts = part.Split('═');
                        if(map_parts.Length != 2) return;
                        dark = map_parts[0].Contains("dark");
                        string[] lines = map_parts[1].Split('\n');
                        mapHeight = lines.Length;
                        if (mapHeight == 0) return;
                        mapWidth = lines[0].Length;
                        tileMap = new string[mapHeight];
                        for (int y = 0; y < mapHeight; y++)
                        {
                            tileMap[y] = lines[y];
                        }
                    }
                    else if (part.StartsWith("type:roof"))
                    {
                        string[] map_parts = part.Split('═');
                        if (map_parts.Length != 2) continue;
                        string[] lines = map_parts[1].Split('\n');
                        if(lines.Length != mapHeight) continue;
                        roofMap = new string[mapHeight];
                        for (int y = 0; y < mapHeight; y++)
                        {
                            roofMap[y] = lines[y];
                        }
                    }
                    else if (part.StartsWith("type:npc"))
                    {
                        // load an npc
                        npcs.Add(npc.MakeNPC(part));
                    }
                    else if (part.StartsWith("type:exit"))
                    {
                        // load an exit
                        exits.Add(new TilemapExit(part));
                    }
                }
            }
            // clear current map so we can load a new one
            public void ClearMap()
            {
                tileMap = null;
                npcs.Clear();
                exits.Clear();
            }
            public void SetViewCenter(int x, int y)
            {
                //GridInfo.Echo("SetViewCenter: Center: " + x + "," + y);
                viewPortX = x - viewPortWidth / 2;
                viewPortY = y - viewPortHeight / 2;
                if (viewPortX < 0) viewPortX = 0;
                if (viewPortY < 0) viewPortY = 0;
                if (viewPortX + viewPortWidth > mapWidth) viewPortX = mapWidth - viewPortWidth;
                if (viewPortY + viewPortHeight > mapHeight) viewPortY = mapHeight - viewPortHeight;
                //GridInfo.Echo("SetViewCenter: Position: " + viewPortX + "," + viewPortY);
                ApplyViewPortTiles();
            }
            public Vector2 GridPosToScreenPos(int x, int y)
            {
                Vector2 tileSize = TileSize;//new Vector2(screenWidth / viewPortWidth, screenHeight / viewPortHeight);
                x -= viewPortX;
                y -= viewPortY;
                return new Vector2(x * tileSize.X, y * tileSize.Y);
            }
            public void ApplyViewPortTiles()
            {
               // go through the viewports tiles and apply them to the visible tiles
                //Vector2 tileSize = new Vector2(screenWidth / viewPortWidth, screenHeight / viewPortHeight);
                Vector2 tilePos = new Vector2(0, 0);
                int i = 0;
                if(visibleTiles.Count == 0) return;
                //GridInfo.Echo("ApplyViewPortTiles: " + viewPortX + "," + viewPortY + " | "+i+" / "+visibleTiles.Count);
                for(int y = 0; y < viewPortHeight; y++)
                {
                    for(int x = 0; x < viewPortWidth; x++)
                    {
                        //GridInfo.Echo("ApplyViewPortTiles: " + (x + viewPortX) + "," + (y + viewPortY));
                        char tile = GetTile(x + viewPortX, y + viewPortY);
                        //GridInfo.Echo("ApplyViewPortTiles: ("+i+")" + tile);
                        if (tiles.ContainsKey(tile) && visibleTiles.Count > i) visibleTiles[i].Data = tiles[tile];
                        else if(visibleTiles.Count > i) visibleTiles[i].Data = "";
                        // if the tile is dark, make overlay tiles dark if they're not near player
                        if (dark)
                        {
                            // if the x and y are within the radius of the player, set overlay to "" else set it to the dark tile
                            if(GameAction.Game.GetPlayerX() + darkRadius > x + viewPortX && GameAction.Game.GetPlayerX() - darkRadius < x + viewPortX && GameAction.Game.GetPlayerY() + darkRadius > y + viewPortY && GameAction.Game.GetPlayerY() - darkRadius < y + viewPortY)
                            {
                                if(overlayTiles.Count > i) overlayTiles[i].Data = "";
                            }
                            else if (overlayTiles.Count > i) overlayTiles[i].Data = tiles[darkTile];
                        }
                        else
                        {
                            char roof = GetRoof(x + viewPortX, y + viewPortY);
                            if (roof != ' ' && overlayTiles.Count > i) overlayTiles[i].Data = tiles[roof];
                            else if (overlayTiles.Count > i) overlayTiles[i].Data = "";
                        }
                        i++;
                    }
                }
                //GridInfo.Echo("ApplyViewPortTiles: " + i);
                MoveNPCs();
                //GridInfo.Echo("ApplyViewPortTiles: npcs moved...");
            }
            public void AddToScreen(Screen screen)
            {
                if (screen == null) return;
                // clear out the old sprites
                foreach(ScreenSprite sprite in visibleTiles)
                {
                    screen.RemoveSprite(sprite);
                }
                visibleTiles.Clear();
                overlayTiles.Clear();
                // setup the screen sprites
                // we need to space them out evenly on the screen
                Vector2 tileSize = TileSize;//new Vector2(screenWidth / viewPortWidth, screenHeight / viewPortHeight);
                Vector2 tilePos = new Vector2(0, 0);
                for(int y = 0; y < viewPortHeight; y++)
                {
                    for(int x = 0; x < viewPortWidth; x++)
                    {
                        char tile = GetTile(x, y);
                        if (tiles.ContainsKey(tile))
                        {
                            ScreenSprite sprite = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, tilePos, fontSize, tileSize, Color.White, "Monospace", tiles[tile],TextAlignment.LEFT,SpriteType.TEXT);
                            ScreenSprite spriteO = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, tilePos, fontSize, tileSize, Color.White, "Monospace", "",TextAlignment.LEFT,SpriteType.TEXT);
                            visibleTiles.Add(sprite);
                            screen.AddSprite(sprite);
                            overlayTiles.Add(spriteO);
                        }
                        tilePos.X += tileSize.X;
                    }
                    tilePos.X = 0;
                    tilePos.Y += tileSize.Y;
                }
                //GridInfo.Echo("AddToScreen: " + visibleTiles.Count);
                // add npcs to screen
                foreach(npc npc in npcs)
                {
                    // calculate the position of the npc on the screen based on it's grid position and the viewport offset
                    int x = npc.X - viewPortX;
                    int y = npc.Y - viewPortY;
                    //GridInfo.Echo("npc: " + npc.X + "," + npc.Y + " screen: " + x + "," + y);
                    npc.Position = new Vector2(x * tileSize.X, y * tileSize.Y);
                    //GridInfo.Echo("npc: " + npc.Position.ToString());
                    npc.SetDirection("down");
                    screen.AddSprite(npc);
                    //npc.RotationOrScale = 0.5f;
                    //GridInfo.Echo("npc: added "+npc.RotationOrScale);
                }
            }
            public void AddOverlayToScreen(Screen screen)
            {
                //GridInfo.Echo("AddOverlayToScreen: " + overlayTiles.Count);
                foreach(ScreenSprite sprite in overlayTiles)
                {
                    screen.AddSprite(sprite);
                }
            }
            public void MoveNPCs()
            {
                //GridInfo.Echo("MoveNPCs:");
                // have every random walk npc attempt to move
                Vector2 tileSize = TileSize; //new Vector2(screenWidth / viewPortWidth, screenHeight / viewPortHeight);
                string[] dirs = { "up", "down", "left", "right" };
                Random random = new Random();
                foreach (npc npc in npcs)
                {
                    if (npc.DoWalk)
                    {
                        // pick a random direction
                        int dir = random.Next(0, dirs.Length * 3);
                        if (dir < dirs.Length)
                        {
                            // attempt to move in that direction
                            int newX = npc.X;
                            int newY = npc.Y;
                            if (dirs[dir] == "up") newY--;
                            else if (dirs[dir] == "down") newY++;
                            else if (dirs[dir] == "left") newX--;
                            else if (dirs[dir] == "right") newX++;
                            // check if the new position is valid
                            if (IsGround(newX, newY) && !IsOccupied(newX, newY) && !IsToxic(newX,newY))
                            {
                                // move the npc
                                npc.X = newX;
                                npc.Y = newY;
                            }
                            npc.SetDirection(dirs[dir]);
                        }
                    }
                    // calculate the position of the npc on the screen based on it's grid position and the viewport offset
                    int x = npc.X - viewPortX;
                    int y = npc.Y - viewPortY;
                    npc.Position = new Vector2(x * tileSize.X, y * tileSize.Y);
                    npc.Visible = (npc.Y < viewPortY + viewPortHeight && npc.NPCVisible);
                }
            }
            public void RemoveFromScreen(Screen screen)
            {
                if (screen == null) return;
                foreach(ScreenSprite sprite in visibleTiles)
                {
                    screen.RemoveSprite(sprite);
                }
                // remove npcs from screen
                foreach(npc npc in npcs)
                {
                    screen.RemoveSprite(npc);
                }
                foreach(ScreenSprite sprite in overlayTiles)
                {
                    screen.RemoveSprite(sprite);
                }
            }
        }
    }
}
