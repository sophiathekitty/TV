﻿using Sandbox.Game.EntityComponents;
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
using VRage.Scripting;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        //----------------------------------------------------------------------
        // keeps track of where to find the scene data
        //----------------------------------------------------------------------
        public class SceneCollection
        {
            public static Dictionary<string, Dictionary<string,List<IMyTextPanel>>> scenes = new Dictionary<string, Dictionary<string, List<IMyTextPanel>>>();
            public static List<IMyTextPanel> unused = new List<IMyTextPanel>();
            public static List<string> StockDialog = new List<string>();
            public static Dictionary<string, RemoteShow> remoteShows = new Dictionary<string, RemoteShow>();
            static bool sending = false;
            static long sendingTo = 0;
            static int resend = 0;
            static Dictionary<IMyTextPanel,bool> sendingBlocks = new Dictionary<IMyTextPanel, bool>();
            public static List<string> shows
            {
                get
                {
                    List<string> list = new List<string>();
                    foreach (string show in scenes.Keys)
                    {
                        if (GetScene(show, "Main", 0, true).Contains("type:show")) list.Add(show);
                    }
                    return list;
                }
            }
            public static List<string> games
            {
                get
                {
                    List<string> list = new List<string>();
                    foreach (string show in scenes.Keys)
                    {
                        if (GetScene(show, "Main", 0, true).Contains("type:game")) list.Add(show);
                    }
                    return list;
                }
            }
            // constructor
            public static void Init()
            {
                //GridInfo.Echo("SceneCollection: constructor");
                // sort the database by the custom name
                GridBlocks.Database.Sort((a, b) => a.CustomName.CompareTo(b.CustomName));
                foreach (IMyTextPanel panel in GridBlocks.Database)
                {
                    panel.ContentType = ContentType.TEXT_AND_IMAGE;
                    if(panel.CustomName.ToLower().Contains("unused") || panel.CustomName.ToLower().Contains("unassigned"))
                    {
                        panel.CustomData = "-";
                        panel.WriteText("-");
                        unused.Add(panel);
                        continue;
                    }
                    SceneAddress address = new SceneAddress(panel.CustomName);
                    if (!scenes.ContainsKey(address.show)) scenes.Add(address.show, new Dictionary<string, List<IMyTextPanel>>());
                    if (!scenes[address.show].ContainsKey(address.scene)) scenes[address.show].Add(address.scene, new List<IMyTextPanel>());
                    scenes[address.show][address.scene].Add(panel);
                    panel.CustomName = address.ToBlockName();
                }
                GridInfo.IGC.SendBroadcastMessage("ReportShows", "");
                GridInfo.AddBroadcastListener("ReportShows");
            }
            public static void Network()
            {
                if (sending)
                {
                    if(sendingBlocks.Count > 0)
                    {
                        if(resend > 0)
                        {
                            resend--;
                            return;
                        }
                        var block = sendingBlocks.First();
                        GridInfo.Echo("SceneCollection: Network: Sending: " + block.Key.CustomName);
                        GridInfo.IGC.SendUnicastMessage(sendingTo, block.Key.CustomName+".CustomData", block.Key.CustomData);
                        GridInfo.IGC.SendUnicastMessage(sendingTo, block.Key.CustomName+".Text", block.Key.GetText());
                        resend = 100;
                    } else sending = false;
                }
                List<MyIGCMessage> messages = GridInfo.CheckMessages();
                foreach(MyIGCMessage message in messages)
                {
                    if(message.Tag == "ReportShows")
                    {
                        GridInfo.Echo("SceneCollection: Network: ReportShows");
                        string shows = "";
                        bool first = true;
                        foreach(var show in SceneCollection.scenes)
                        {
                            if (first) first = false;
                            else shows += ","; // separate shows with a comma (so we can split the string later)
                            int blks = 0;
                            foreach(var scene in show.Value)
                            {
                                blks += scene.Value.Count;
                            }
                            shows += show.Key + ":" + blks;
                        }
                        GridInfo.IGC.SendUnicastMessage(message.Source, "Shows", shows);
                    }
                    else if(message.Tag == "Shows")
                    {
                        GridInfo.Echo("SceneCollection: Network: Shows");
                        string[] shows = message.As<string>().Split(',');
                        foreach(string show in shows)
                        {
                            string[] parts = show.Split(':');
                            GridInfo.Echo("SceneCollection: Network: Shows: " + show);
                            if (parts.Length == 2)
                            {
                                int blocks = 0;
                                string name = parts[0].ToLower();
                                if (int.TryParse(parts[1], out blocks))
                                {
                                    GridInfo.Echo("SceneCollection: Network: Shows: " + show + " - " + blocks);
                                    if (!remoteShows.ContainsKey(name)) remoteShows.Add(name, new RemoteShow(parts[0],message.Source,blocks));
                                    else remoteShows[name] = new RemoteShow(parts[0],message.Source, blocks);
                                }
                            }
                        }
                    }
                    else if(message.Tag == "Download")
                    {
                        GridInfo.Echo("SceneCollection: Network: Download");
                        // send the requested scene to the requester
                        string show = message.As<string>();
                        if (scenes.ContainsKey(show)) { sending = true; sendingTo = message.Source; sendingBlocks.Clear(); }
                        else continue;
                        foreach(var sceneGroup in scenes[show])
                        {
                            foreach(IMyTextPanel block in sceneGroup.Value)
                            {
                                GridInfo.Echo("SceneCollection: Network: Download: " + block.CustomName);
                                //GridInfo.IGC.SendUnicastMessage(message.Source, block.CustomName+".CustomData", block.CustomData);
                                //GridInfo.IGC.SendUnicastMessage(message.Source, block.CustomName+".Text", block.GetText());
                                sendingBlocks.Add(block,false);
                            }
                        }
                    }
                    else if(message.Tag == "Recieved")
                    {
                        sendingBlocks.Remove(GridInfo.GridTerminalSystem.GetBlockWithName(message.As<string>()) as IMyTextPanel);
                        resend = 0;
                    }
                    else if (message.Tag.StartsWith("DB:"))
                    {
                        GridInfo.Echo("SceneCollection: Network: Downloading: "+message.Tag);
                        SceneAddress address = new SceneAddress(message.Tag);
                        IMyTextPanel block = GridInfo.GridTerminalSystem.GetBlockWithName(address.ToBlockName()) as IMyTextPanel;
                        if (block == null)
                        {
                            // get an unused block...
                            if (unused.Count > 0)
                            {
                                block = unused[0];
                                unused.RemoveAt(0);
                            }
                        }
                        block.CustomName = address.ToBlockName();
                        if (!scenes.ContainsKey(address.show)) scenes.Add(address.show, new Dictionary<string, List<IMyTextPanel>>());
                        if (!scenes[address.show].ContainsKey(address.scene)) scenes[address.show].Add(address.scene, new List<IMyTextPanel>());
                        if (!scenes[address.show][address.scene].Contains(block)) scenes[address.show][address.scene].Add(block);
                        if (address.custom_data) block.CustomData = message.As<string>();
                        else block.WriteText(message.As<string>());
                        if (block.CustomData != "-" && block.GetText() != "-") GridInfo.IGC.SendUnicastMessage(message.Source, "Recieved", block.CustomName);
                    }
                }
            }
            public static void DeleteShow(string show)
            {
                if(scenes.ContainsKey(show))
                {
                    foreach(var sceneGroup in scenes[show])
                    {
                        foreach(IMyTextPanel block in sceneGroup.Value)
                        {
                            block.CustomName = "DB:Unused";
                            block.CustomData = "-";
                            block.WriteText("-");
                            unused.Add(block);
                        }
                    }
                    scenes.Remove(show);
                }
            }
            public static string GetScene(string show, string scene, int index, bool custom_data)
            {
                if(scenes.ContainsKey(show))
                {
                    if (scenes[show].ContainsKey(scene))
                    {
                        if (index >= 0 && index < scenes[show][scene].Count)
                        {
                            string dialog = GetStockDialog(scene);
                            if(custom_data) return scenes[show][scene][index].CustomData+dialog;
                            else return scenes[show][scene][index].GetText()+dialog;
                        }
                    }
                }
                return "";
            }
            static string GetStockDialog(string scene)
            {
                if (scene == "Stock" && StockDialog.Count >= 4)
                {
                    string dialog = "║type:subtitles,delay:200,animation:loop═";
                    // grab the first 4 stock dialog lines
                    dialog += StockDialog[0] + ":" + StockDialog[1] + ":" + StockDialog[2] + ":" + StockDialog[3];
                    // now remove them from the list
                    StockDialog.RemoveRange(0, 4);
                    return dialog;

                }
                return "";
            }
            // save a scene at an address
            public static bool SaveScene(string address, string data)
            {
                //GridInfo.Echo("SceneCollection: SaveScene(" + address + ")");
                SceneAddress sceneAddress = new SceneAddress(address);
                if (scenes.ContainsKey(sceneAddress.show) && scenes[sceneAddress.show].ContainsKey(sceneAddress.scene))
                {
                    if (sceneAddress.index >= 0 && sceneAddress.index < scenes[sceneAddress.show][sceneAddress.scene].Count)
                    {
                        if (sceneAddress.custom_data) scenes[sceneAddress.show][sceneAddress.scene][sceneAddress.index].CustomData = data;
                        else scenes[sceneAddress.show][sceneAddress.scene][sceneAddress.index].WriteText(data);
                        return true;
                    }
                }
                return false;
            }
            // get the scene from an address (show.scene.index.CustomeData or show.scene.index.Text)
            public static string GetScene(string address)
            {
                //GridInfo.Echo("SceneCollection: GetScene(" + address + ")");
                return GetScene(new SceneAddress(address));
            }
            // get the scene using a scene address
            public static string GetScene(SceneAddress sceneAddress)
            {
                if (sceneAddress.length == 4)
                {
                    return GetScene(sceneAddress.show, sceneAddress.scene, sceneAddress.index, sceneAddress.custom_data);
                }
                else if (sceneAddress.length == 3)
                {
                    return GetScene(sceneAddress.show, sceneAddress.scene, sceneAddress.index);
                }
                else if (sceneAddress.length == 2)
                {
                    return GetRandomScene(sceneAddress.show, sceneAddress.scene);
                }
                return "";
            }
            // get a random scene from the given show and scene
            public static string GetRandomScene(string show, string scene)
            {
                if (scenes.ContainsKey(show))
                {
                    if (scenes[show].ContainsKey(scene))
                    {
                        int index = new Random().Next(0, scenes[show][scene].Count);
                        // random pick Text or CustomData
                        bool custom_data = new Random().Next(0, 2) == 0;
                        return GetScene(show, scene, index, custom_data);
                    }
                }
                return "";
            }
            // get a scene at an index from the given show and scene (remap index to account for customdata and text of each block)
            // so for example 0 is the first block's text, 1 is the first block's customdata, 2 is the second block's text, 3 is the second block's customdata, etc.
            public static string GetScene(string show, string scene, int index)
            {
                if (scenes.ContainsKey(show))
                {
                    if (scenes[show].ContainsKey(scene))
                    {
                        if (index >= 0 && index < scenes[show][scene].Count * 2)
                        {
                            int block_index = index / 2;
                            bool custom_data = index % 2 == 1;
                            return GetScene(show, scene, block_index, custom_data);
                        }
                    }
                }
                return "";
            }
            public static string[] GetDefaultSave(string game)
            {
                return GetSave(game, 0);
            }
            public static List<string> GetSaveNames(string game)
            {
                List<string> names = new List<string>();
                if (scenes.ContainsKey(game))
                {
                    if (scenes[game].ContainsKey("Main"))
                    {
                        if (scenes[game]["Main"].Count > 0)
                        {
                            string[] saves = scenes[game]["Main"][0].GetText().Split('║');
                            for(int i = 1; i < saves.Length; i++)
                            {
                                string[] save = saves[i].Split('═');
                                string[] var = save[0].Split(':');
                                names.Add(var[1]);
                            }
                        }
                    }
                }
                return names;
            }
            public static string[] GetSave(string game, int id)
            {
                if (scenes.ContainsKey(game))
                {
                    if (scenes[game].ContainsKey("Main"))
                    {
                        if (scenes[game]["Main"].Count > 0)
                        {
                            string[] saves = scenes[game]["Main"][0].GetText().Split('║');
                            if(id < saves.Length)return saves[id].Split('═');
                        }
                    }
                }
                return null;
            }
            public static string GetDefaultSaveName(string game)
            {
                string[] save = GetDefaultSave(game);
                if(save == null) return "Log";
                string[] var = save[0].Split(':');
                return var[1];
            }
            public static void ResetSaves(string game)
            {
                // need to get the default save and then set all the saves to that. but we also need to update the save names to be like DefaultSaveName 1, DefaultSaveName 2, etc.
                string[] defaultSave = GetDefaultSave(game);
                if (defaultSave == null) return;
                string saveName = defaultSave[0].Split(':')[1];
                string[] saves = scenes[game]["Main"][0].GetText().Split('║');
                for (int i = 1; i < saves.Length; i++)
                {
                    defaultSave[0] = "save:" + saveName + " " + i;
                    saves[i] = string.Join("═",defaultSave);
                }
                scenes[game]["Main"][0].WriteText(string.Join("║", saves));
            }
            public class SceneAddress
            {
                public string show;
                public string scene;
                public int index;
                public bool custom_data;
                public int x;
                public int y;
                public int length;
                // constructor
                public SceneAddress(string address)
                {
                    if (address.StartsWith("DB:")) address = address.Substring(3);
                    string[] parts = address.Split('.');
                    length = parts.Length;
                    if (parts.Length == 6)
                    {
                        int index = 0;
                        int x = 0;
                        int y = 0;
                        if (int.TryParse(parts[2], out index) && int.TryParse(parts[4], out x) && int.TryParse(parts[5], out y))
                        {
                            this.show = parts[0];
                            this.scene = parts[1];
                            this.index = index;
                            this.custom_data = parts[3].ToLower() == "customdata";
                            this.x = x;
                            this.y = y;
                        }
                    }
                    else if (parts.Length == 4)
                    {
                        int index = 0;
                        if (int.TryParse(parts[2], out index))
                        {
                            this.show = parts[0];
                            this.scene = parts[1];
                            this.index = index;
                            this.custom_data = parts[3].ToLower() == "customdata";
                        }
                    }
                    else if (parts.Length == 3)
                    {
                        int index = 0;
                        if (int.TryParse(parts[2], out index))
                        {
                            this.show = parts[0];
                            this.scene = parts[1];
                            this.index = index;
                            this.custom_data = false;
                        }
                    }
                    else if (parts.Length == 2)
                    {
                        this.show = parts[0];
                        this.scene = parts[1];
                        this.index = -1;
                        this.custom_data = false;
                    }
                }
                // to string
                public override string ToString()
                {
                    if(index >= 0) return show + "." + scene + "." + index + "." + (custom_data ? "customdata" : "text");
                    return show + "." + scene;
                }
                public string ToBlockName()
                {
                    if(index < 10) return "DB:" + show + "." + scene + ".0" + index;
                    if (index >= 0) return "DB:" + show + "." + scene + "." + index;
                    return "TV." + show + "." + scene +".0";
                }
            }
        }
        //----------------------------------------------------------------------
    }
}
