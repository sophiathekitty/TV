using EmptyKeys.UserInterface.Generated.EditFactionIconView_Bindings;
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
        public class GridBlocks
        {
            static List<IMyTextPanel> db = new List<IMyTextPanel>();
            static List<IMyShipController> seats = new List<IMyShipController>();
            static Dictionary<string,IMyTerminalBlock> surfaceProviders = new Dictionary<string,IMyTerminalBlock>();
            public static Dictionary<string,List<IMyTextSurface>> surfaces = new Dictionary<string,List<IMyTextSurface>>();
            static List<IMySoundBlock> soundBlocks = new List<IMySoundBlock>();
            static void GetBlocks()
            {
                GridInfo.GridTerminalSystem.GetBlocksOfType<IMyShipController>(seats, b => b.IsSameConstructAs(GridInfo.Me));
                List<IMyFunctionalBlock> blocks = new List<IMyFunctionalBlock>();
                GridInfo.GridTerminalSystem.GetBlocksOfType<IMyFunctionalBlock>(blocks, b => b.IsSameConstructAs(GridInfo.Me));
                foreach (var block in blocks)
                {
                    if (block is IMyTextSurfaceProvider)
                    {
                        var textProvider = block as IMyTextSurfaceProvider;
                        if (textProvider != null)
                        {
                            if (block.CustomName.StartsWith("DB:"))
                            {
                                IMyTextPanel panel = textProvider as IMyTextPanel;
                                if (panel != null)
                                {
                                    db.Add(panel);
                                }
                            }
                            else
                            {
                                if (!surfaces.ContainsKey(block.CustomName)) surfaces.Add(block.CustomName, new List<IMyTextSurface>());
                                if (!surfaceProviders.ContainsKey(block.CustomName)) surfaceProviders.Add(block.CustomName, block);
                                for (int i = 0; i < textProvider.SurfaceCount; i++)
                                {
                                    IMyTextSurface surface = textProvider.GetSurface(i);
                                    surfaces[block.CustomName].Add(surface);
                                }
                            }
                        }
                    }
                }
                GridInfo.GridTerminalSystem.GetBlocksOfType<IMySoundBlock>(soundBlocks, b => b.IsSameConstructAs(GridInfo.Me));
            }
            public static List<IMyTextPanel> Database
            {
                get
                {
                    if(db.Count == 0)
                    {
                        GetBlocks();
                    }
                    return db;
                }
            }
            public static IMyTextSurface TV
            {
                get
                {
                    if(surfaces.Count == 0)
                    {
                        GetBlocks();
                    }
                    if(surfaces.ContainsKey("TV")) return surfaces["TV"][0];
                    foreach(var surface in surfaces)
                    {
                        if(surface.Key.Contains("TV")) return surface.Value[0];
                    }
                    return null;
                }
            }
            public static IMySoundBlock TVSound
            {
                get
                {
                    if (soundBlocks.Count == 0)
                    {
                        GetBlocks();
                    }
                    foreach (var sound in soundBlocks)
                    {
                        if (sound.CustomName.ToLower().Contains("tv")) return sound;
                    }
                    return null;
                }
            }
            public static IMyTextSurface FindSurface(string key, int index = 0)
            {
                if(surfaces.Count == 0)
                {
                    GetBlocks();
                }
                if(surfaces.ContainsKey(key)) return surfaces[key][index];
                foreach(var surface in surfaces)
                {
                    if(surface.Key.Contains(key)) return surface.Value[index];
                }
                return null;
            }
            public static string GetSurfaceCustomData(string key)
            {
                if (surfaceProviders.Count == 0)
                {
                    GetBlocks();
                }
                //GridInfo.Echo("GetSurfaceCustomData: " + key);
                if (surfaceProviders.ContainsKey(key)) return surfaceProviders[key].CustomData;
                //GridInfo.Echo("GetSurfaceCustomData: " + key + " searching....");
                foreach(var block in surfaceProviders)
                {
                    if(block.Key.Contains(key)) return block.Value.CustomData;
                }
                //GridInfo.Echo("GetSurfaceCustomData: " + key + " not found");
                return "";
            }
            // set surface custom data
            public static void SetSurfaceCustomData(string key, string data)
            {
                if (surfaceProviders.Count == 0)
                {
                    GetBlocks();
                }
                if (surfaceProviders.ContainsKey(key))
                {
                    surfaceProviders[key].CustomData = data;
                }
                else
                {
                    foreach (var block in surfaceProviders)
                    {
                        if (block.Key.Contains(key))
                        {
                            block.Value.CustomData = data;
                        }
                    }
                }
            }
            public static IMyShipController Couch
            {
                get
                {
                    if (seats.Count == 0)
                    {
                        GetBlocks();
                    }
                    foreach (var seat in seats)
                    {
                        if (seat.CustomName.ToLower().Contains("couch")) return seat;
                    }
                    return null;
                }
            }
            public static IMyShipController Keyboard
            {
                get
                {
                    if (seats.Count == 0)
                    {
                        GetBlocks();
                    }
                    foreach (var seat in seats)
                    {
                        if (seat.CustomName.ToLower().Contains("tv")) return seat;
                    }
                    return null;
                }
            }
        }
    }
}
