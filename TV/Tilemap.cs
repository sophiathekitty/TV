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
            List<ScreenSprite> visibleTiles = new List<ScreenSprite>();
            string[] tileMap;
            int mapWidth;
            int mapHeight;
            Dictionary<char,string> tiles = new Dictionary<char,string>();
            string groundTiles = "";
            string toxicTiles = "";
            string dangerTiles = "";
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
            public char GetTile(int x, int y)
            {
                if (x < 0 || x >= mapWidth) return ' ';
                if (y < 0 || y >= mapHeight) return ' ';
                return tileMap[y][x];
            }
            // can walk on this tile?
            public bool IsGround(int x, int y)
            {
                char tile = GetTile(x, y);
                return groundTiles.Contains(tile);
            }
            // is this tile toxic?
            public bool IsToxic(int x, int y)
            {
                char tile = GetTile(x, y);
                return toxicTiles.Contains(tile);
            }
            // is this tile dangerous?
            public bool IsDanger(int x, int y)
            {
                char tile = GetTile(x, y);
                return dangerTiles.Contains(tile);
            }
            // load the tile set from a string
            public void LoadTiles(string data)
            {
                if(tiles == null) tiles = new Dictionary<char,string>();
                tiles.Clear();
                string[] parts = data.Split('║');
                foreach(string part in parts)
                {
                    // get tile set info
                    if (part.Contains("type:sprites"))
                    {
                        // get tileset info
                        string[] vars = part.Split(',');
                        foreach(string var in vars)
                        {
                            string[] pair = var.Split(':');
                            if (pair.Length == 2)
                            {
                                if (pair[0] == "ground") groundTiles = pair[1];
                                else if (pair[0] == "toxic") toxicTiles = pair[1];
                                else if (pair[0] == "danger") dangerTiles = pair[1];
                                else if (pair[0] == "width") tileWidth = int.Parse(pair[1]);
                                else if (pair[0] == "height") tileHeight = int.Parse(pair[1]);
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
                // example map:
                // aaaa
                // abba
                // aaaa
                // need to find the width and height of the map
                string[] lines = data.Split('\n');
                mapHeight = lines.Length;
                if(mapHeight == 0) return;
                mapWidth = lines[0].Length;
                tileMap = new string[mapHeight];
                for(int y = 0; y < mapHeight; y++)
                {
                    tileMap[y] = lines[y];
                }
            }
            public void SetViewCenter(int x, int y)
            {
                viewPortX = x - viewPortWidth / 2;
                viewPortY = y - viewPortHeight / 2;
                if (viewPortX < 0) viewPortX = 0;
                if (viewPortY < 0) viewPortY = 0;
                if (viewPortX + viewPortWidth > mapWidth) viewPortX = mapWidth - viewPortWidth;
                if (viewPortY + viewPortHeight > mapHeight) viewPortY = mapHeight - viewPortHeight;
                ApplyViewPortTiles();
            }
            public void ApplyViewPortTiles()
            {
               // go through the viewports tiles and apply them to the visible tiles
                Vector2 tileSize = new Vector2(screenWidth / viewPortWidth, screenHeight / viewPortHeight);
                Vector2 tilePos = new Vector2(0, 0);
                int i = 0;
                for(int y = 0; y < viewPortHeight; y++)
                {
                    for(int x = 0; x < viewPortWidth; x++)
                    {
                        char tile = GetTile(x + viewPortX, y + viewPortY);
                        if (tiles.ContainsKey(tile)) visibleTiles[i].Data = tiles[tile];
                        else visibleTiles[i].Data = "";
                        i++;
                    }
                }
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
                // setup the screen sprites
                // we need to space them out evenly on the screen
                Vector2 tileSize = new Vector2(screenWidth / viewPortWidth, screenHeight / viewPortHeight);
                Vector2 tilePos = new Vector2(0, 0);
                for(int y = 0; y < viewPortHeight; y++)
                {
                    for(int x = 0; x < viewPortWidth; x++)
                    {
                        char tile = GetTile(x, y);
                        if (tiles.ContainsKey(tile))
                        {
                            ScreenSprite sprite = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, tilePos, 0.02f, tileSize, Color.White, "Monospace", tiles[tile],TextAlignment.LEFT,SpriteType.TEXT);
                            visibleTiles.Add(sprite);
                            screen.AddSprite(sprite);
                        }
                        tilePos.X += tileSize.X;
                    }
                    tilePos.X = 0;
                    tilePos.Y += tileSize.Y;
                }
            }
            public void RemoveFromScreen(Screen screen)
            {
                if (screen == null) return;
                foreach(ScreenSprite sprite in visibleTiles)
                {
                    screen.RemoveSprite(sprite);
                }
                //visibleTiles.Clear();
            }
        }
    }
}
