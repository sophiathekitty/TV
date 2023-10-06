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
            AnimatedCharacter player;
            Dictionary<string,int> playerStats = new Dictionary<string,int>();
            Dictionary<string,int> playerMaxStats = new Dictionary<string,int>();
            Dictionary<string,string> playerGear = new Dictionary<string,string>();
            Dictionary<string,int> playerInventory = new Dictionary<string,int>();
            Dictionary<string,int> enemyStats = new Dictionary<string,int>();
            Dictionary<string,Dictionary<string,string>> itemStats = new Dictionary<string,Dictionary<string,string>>();
            Dictionary<string,bool> gameBools = new Dictionary<string,bool>();
            Dictionary<string,int> gameInts = new Dictionary<string,int>();
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
            public GameRPG(string element,float width, float height)
            {
                string[] parts = element.Split('║');
                foreach(string part in parts)
                {
                    if (part.Contains("type:player")) parsePlayer(part);
                    else if(part.Contains("type:game")) parseInfo(part);
                    else if(part.Contains("type:item")) parseItems(part);
                }
            }
            // parse game info
            void parseInfo(string data)
            {
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
                string[] parts = data.Split('═');
                if(parts.Length > 1) parsePlayerStats(parts[1]);
                if(parts.Length > 2) parsePlayerGear(parts[2]);
            }
            // parse player stats
            void parsePlayerStats(string data)
            {
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
                string[] parts = data.Split(',');
                foreach(string part in parts)
                {
                    string[] pair = part.Split(':');
                    playerGear.Add(pair[0], pair[1]);
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
            //-------------------------------------------------------------------
            // add to screen
            //-------------------------------------------------------------------
            public void AddToScreen(Screen screen)
            {
                map.AddToScreen(screen);
            }
            // remove from screen
            public void RemoveFromScreen(Screen screen)
            {
                map.RemoveFromScreen(screen);
            }
            //-------------------------------------------------------------------

        }
        //-----------------------------------------------------------------------
    }
}
