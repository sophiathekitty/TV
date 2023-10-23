using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
        // GameAction - for some simple game logic scripting support
        //----------------------------------------------------------------------
        // action name is what gets added to the game menu (Talk, Open, Search, etc)
        // the actions use a set of simple commands.
        // commands:
        //      set:[object.]property=value
        //      if:[object.]property=value
        //      ifnot:[object.]property=value
        //      elseif:[object.]property=value
        //      elseifnot:[object.]property=value
        //      else
        //      endif
        //      add:[object.]property=value
        //      sub:[object.]property=value
        //      mul:[object.]property=value
        //      div:[object.]property=value
        //      say:some string of text to say
        //      savegame
        //----------------------------------------------------------------------
        public class GameAction
        {
            public static IGameVars GameVars;
            static Random random = new Random();
            public string Name { get; set; }
            List<string> Commands = new List<string>();
            npc me;
            public GameAction(string element, npc me = null)
            {
                this.me = me;
                string[] parts = element.Split(';');
                foreach (string part in parts)
                {
                    if (part.StartsWith("action:") || part.StartsWith("yes:") || part.StartsWith("no:"))
                    {
                        string[] info = part.Split(':');
                        Name = info[1];
                    }
                    else
                    {
                        if (part != "") Commands.Add(part);
                    }
                }
            }
            // run the action
            public bool Run()
            {
                GridInfo.Echo("GameAction: running " + Name);
                bool skipping = false;
                bool ifwastrue = false;
                foreach (string command in Commands)
                {
                    if(command == "") continue;
                    string[] parts = command.Split(':');
                    string cmd = "";
                    string param = "";
                    cmd = parts[0].ToLower().Trim();
                    if(parts.Length > 1) param = parts[1];
                    // conditional flow commands (that can end skipping)
                    if (cmd == "endif")
                    {
                        skipping = false;
                        ifwastrue = false;
                        continue;
                    }
                    else if (cmd == "else")
                    {
                        GridInfo.Echo("GameAction: else");
                        skipping = !skipping;
                        if(ifwastrue) skipping = true;
                        GridInfo.Echo("GameAction: else: " + ifwastrue);
                        continue;
                    }
                    else if(skipping && cmd == "elseif")
                    {
                        if (ifwastrue)
                        {
                            skipping = true;
                            continue;
                        }
                        GridInfo.Echo("GameAction: elseif: "+param);
                        skipping = !Compare(param);
                        if (!skipping) ifwastrue = true;
                        GridInfo.Echo("GameAction: elseif: " + param + " = " + ifwastrue);
                        continue;
                    }
                    else if(skipping && cmd == "elseifnot")
                    {
                        if (ifwastrue)
                        {
                            skipping = true;
                            continue;
                        }
                        GridInfo.Echo("GameAction: elseifnot: " + param);
                        skipping = Compare(param);
                        if (!skipping) ifwastrue = true;
                        GridInfo.Echo("GameAction: elseifnot: " + param + " = " + ifwastrue);
                        continue;
                    }
                    //GridInfo.Echo("\n\nGameAction:\n" + cmd + " " + param + "\n(skipping:" + skipping + ")\n");
                    // skip commands until we hit an else or endif (or elseif)
                    if (skipping) continue;
                    // conditional flow commands (that can start skipping)
                    if(cmd == "if")
                    {
                        //GridInfo.Echo("GameAction: if(" + param + ")");
                        bool res = Compare(param);
                        skipping = !res;
                        //GridInfo.Echo("GameAction: if("+res+") skipping: " + skipping);
                        if(res) ifwastrue = true;
                        GridInfo.Echo("GameAction: if: " + param + " = " + ifwastrue);
                    }
                    else if(cmd == "ifnot")
                    {
                        skipping = Compare(param);
                        if(!skipping) ifwastrue = true;
                        GridInfo.Echo("GameAction: ifnot: " + param + " = " + ifwastrue);
                    }
                    // math commands
                    else if (cmd == "set")
                    {
                        string[] pair = param.Split('=');
                        //SetValue(pair[0], pair[1]);
                        GameVars.SetVar(pair[0], me, pair[1]);
                    }
                    else if(cmd == "setto")
                    {
                        //GridInfo.Echo("GameAction: setto: " + param);
                        string[] pair = param.Split('=');
                        //string value = GetValue(pair[1], me);
                        string value = GameVars.GetVarAs<string>(pair[1], me);
                        //GridInfo.Echo("GameAction: setto: " + pair[0] + " = " + value);
                        GameVars.SetVar(pair[0], me, value);
                    }
                    else if(cmd == "add")
                    {
                        string[] pair = param.Split('=');
                        float value = GameVars.GetVarAs<float>(pair[0],me);
                        float value2 = 0;
                        if (float.TryParse(pair[1], out value2)) value += value2;
                        else value += GameVars.GetVarAs<float>(pair[1], me);
                        GameVars.SetVar(pair[0], me, value.ToString());
                    }
                    else if(cmd == "sub")
                    {
                        //GridInfo.Echo("GameAction: sub: " + param);
                        string[] pair = param.Split('=');
                        float value = GameVars.GetVarAs<float>(pair[0], me);
                        float value2 = 0;
                        if (!float.TryParse(pair[1], out value2))
                        {
                            value2 = GameVars.GetVarAs<float>(pair[1], me);
                            //GridInfo.Echo("GameAction: sub: getvalueas? " + pair[1] + " = " + value2);
                        }
                        //GridInfo.Echo("GameAction: sub: " + pair[0] + " = " + value + " - " + value2);
                        value -= value2;
                        GameVars.SetVar(pair[0], me, value.ToString());
                        //GridInfo.Echo("GameAction: sub: " + pair[0] + " = " + value);
                    }
                    else if(cmd == "mul")
                    {
                        string[] pair = param.Split('=');
                        float value = GameVars.GetVarAs<float>(pair[0], me);
                        float value2 = 0;
                        if (float.TryParse(pair[1], out value2)) value *= value2;
                        else value *= GameVars.GetVarAs<float>(pair[1], me);
                        GameVars.SetVar(pair[0], me, value.ToString());
                    }
                    else if(cmd == "div")
                    {
                        string[] pair = param.Split('=');
                        float value = GameVars.GetVarAs<float>(pair[0], me);
                        float value2 = 0;
                        if (float.TryParse(pair[1], out value2)) value /= value2;
                        else value /= GameVars.GetVarAs<float>(pair[1], me);
                        GameVars.SetVar(pair[0], me, value.ToString());
                    }
                    else if(cmd == "rand")
                    {
                        string[] pair = param.Split('=');
                        if (pair.Length != 2) continue;
                        string[] range = pair[1].Split(',');
                        if(range.Length != 2) continue;
                        GameVars.SetVar(pair[0], me, random.Next(int.Parse(range[0]), int.Parse(range[1])).ToString());
                    }
                    // give item
                    else if(cmd == "give")
                    {
                        int count = 1;
                        if (param.Contains("-"))
                        {
                            string[] pair = param.Split('-');
                            param = pair[0];
                            int.TryParse(pair[1], out count);
                        }
                        if(GameRPG.playerInventory.ContainsKey(param)) GameRPG.playerInventory[param] += count;
                        else GameRPG.playerInventory.Add(param, count);
                        GridInfo.Echo("GameAction: give: " + param + " = " + GameRPG.playerInventory[param]);
                    }
                    else if(cmd == "take")
                    {
                           int count = 1;
                        if (param.Contains("-"))
                        {
                            string[] pair = param.Split('-');
                            param = pair[0];
                            int.TryParse(pair[1], out count);
                        }
                        if (GameRPG.playerInventory.ContainsKey(param))
                        {
                            GameRPG.playerInventory[param] -= count;
                            if (GameRPG.playerInventory[param] <= 0) GameRPG.playerInventory.Remove(param);
                        }
                        GridInfo.Echo("GameAction: take: " + param + " = " + GameRPG.playerInventory[param]);
                    }
                    else if(cmd == "say") 
                    {
                        // say something
                        //GridInfo.Echo("GameAction: say: " + param);
                        GameRPG.Say(param);
                        return false;
                    }
                    else if(cmd == "ask")
                    {
                        //GridInfo.Echo("GameAction: ask: " + param);
                        GameRPG.Ask(param,me,Name);
                        return false;
                    }
                    else if(cmd == "go")
                    {
                        string map;
                        int x;
                        int y;
                        if (param.Contains(','))
                        {
                            string[] props = param.Split(',');
                            map = props[0];
                            x = int.Parse(props[1]);
                            y = int.Parse(props[2]);
                            GameRPG.Go(map, x, y);
                        }
                        else if(param == "map.Exit")
                        {
                            TilemapExit exit = Tilemap.Exit;
                            if (exit == null) continue;
                            map = exit.Map;
                            x = exit.X;
                            y = exit.Y;
                            GameRPG.Go(map, x, y);
                        }
                    }
                    else if(cmd == "savegame")
                    {
                        // save the game
                        //GridInfo.Echo("GameAction: savegame");
                    }
                    else if(cmd == "run")
                    {
                        GridInfo.Echo("GameAction: run: " + param);
                        if(GameRPG.gameLogic.ContainsKey(param))
                        {
                            GameRPG.gameLogic[param].Run();
                        }
                        else
                        {
                            //GridInfo.Echo("GameAction: run: unknown action: " + param);
                        }
                    }
                    else if(cmd == "exit")
                    {
                        return true;
                    }
                    else
                    {
                        //GridInfo.Echo("GameAction: unknown command: " + cmd);
                    }
                }
                return true;
            }
            // do a comparison
            bool Compare(string param)
            {
                bool result = false;
                // do check
                char opperator = '=';
                if (param.Contains("<")) opperator = '<';
                else if (param.Contains(">")) opperator = '>';
                string[] pair = param.Split(opperator);
                string value = GameVars.GetVarAs<string>(pair[0], me);
                float fvalue;
                bool bvalue;
                bool isFloat = float.TryParse(value, out fvalue);
                bool isBool = bool.TryParse(value, out bvalue);
                if (isBool)
                {
                    bool bparam;
                    if (!bool.TryParse(pair[1], out bparam))
                    {
                        bparam = GameVars.GetVarAs<bool>(pair[1], me);
                    }
                    GridInfo.Echo("GameAction:bool: Compare: " + bvalue + " " + opperator + " " + bparam);
                    return bvalue == bparam;
                }
                else if(isFloat)
                {
                    float fparam;
                    if (!float.TryParse(pair[1], out fparam))
                    {
                        fparam = GameVars.GetVarAs<float>(pair[1], me);
                    }
                    GridInfo.Echo("GameAction:float: Compare: " + fvalue + " " + opperator + " " + fparam);
                    if (opperator == '=' && fvalue == fparam) result = true;
                    else if (opperator == '<' && fvalue < fparam) result = true;
                    else if (opperator == '>' && fvalue > fparam) result = true;
                }
                else
                {
                    GridInfo.Echo("GameAction:string: Compare: " + value + " " + opperator + " " + pair[1]);
                    // string comparison
                    if (opperator == '=' && value == pair[1]) result = true;
                }
                return result;
            }
            /*
            // set a value
            void SetValue(string name, string value)
            {
                //GridInfo.Echo("SetValue: " + name + " = " + value);
                string objectName = "";
                if (name.Contains("."))
                {
                    string[] parts = name.Split('.');
                    //GridInfo.Echo("SetValue: parts: " + parts.Length);
                    objectName = parts[0];
                    name = parts[1];
                }
                // find the object to set the value on
                if (objectName == "player")
                {
                    GameRPG.playerStats[name] = double.Parse(value);
                }
                else if(objectName == "playerMax")
                {
                    GameRPG.playerMaxStats[name] = int.Parse(value);
                }
                else if(objectName == "")
                {
                    SetNPCValue(name, value);
                }
                else if(objectName == "inventory")
                {
                    GameRPG.playerInventory[name] = int.Parse(value);
                }
                else if(objectName == "bools")
                {
                    GameRPG.gameBools[name] = bool.Parse(value);
                }
                else if(objectName == "ints")
                {
                    GameRPG.gameInts[name] = int.Parse(value);
                    //GridInfo.Echo("SetValue: ints: " + name + " = " + value + " = " + GameRPG.gameInts[name]);
                }
                else if(objectName == "map")
                {
                    if (name == "darkRadius") Tilemap.darkRadius = int.Parse(value);
                }
                else
                {
                    //GridInfo.Echo("SetValue: unknown object: " + objectName);
                }   
            }
            void SetNPCValue(string name, string value)
            {
                if(me == null) return;
                //GridInfo.Echo("SetNPCValue: " + name + " = " + value);  
                name = name.ToLower();
                if(name.StartsWith("vis")) {
                    //GridInfo.Echo("SetNPCValue: visible");
                    me.NPCVisible = bool.Parse(value);
                }
                else if(name.Contains("block"))
                {
                    me.BlocksMovement = bool.Parse(value);
                }
                else if(name == "x")
                {
                    me.X = int.Parse(value);
                }
                else if(name == "y")
                {
                    me.Y = int.Parse(value);
                }
                else if(name.StartsWith("dir"))
                {
                    me.SetDirection(value);
                }
            }
            // get a value
            public static string GetValue(string name, npc me, bool hasDefault = false)
            {
                string objectName = "";
                if (name.Contains("."))
                {
                    string[] parts = name.Split('.');
                    objectName = parts[0];
                    name = parts[1];
                }
                // find the object to set the value on
                if (objectName == "player")
                {
                    if(name == "x") return GameRPG.playerX.ToString();
                    else if(name == "y") return GameRPG.playerY.ToString();
                    if(GameRPG.playerStats.ContainsKey(name)) return GameRPG.playerStats[name].ToString();
                }
                else if(objectName == "playerMax" && GameRPG.playerMaxStats.ContainsKey(name))
                {
                    return GameRPG.playerMaxStats[name].ToString();
                }
                else if (objectName == "")
                {
                    return GetNPCValue(name, me);
                }
                else if (objectName == "inventory" && GameRPG.playerInventory.ContainsKey(name))
                {
                    return GameRPG.playerInventory[name].ToString();
                }
                else if (objectName == "bools")
                {
                    if (GameRPG.gameBools.ContainsKey(name)) return GameRPG.gameBools[name].ToString();
                    else if(!hasDefault) return "false";
                }
                else if (objectName == "ints")
                {
                    if (GameRPG.gameInts.ContainsKey(name))
                    {
                        //GridInfo.Echo("GetValue: ints: " + name + " = " + GameRPG.gameInts[name]);
                        return GameRPG.gameInts[name].ToString();
                    }
                    else if (!hasDefault) return "0";
                }
                else if(objectName == "map")
                {
                    //GridInfo.Echo("GetValue: map: " + name);
                    if (name == "darkRadius") return Tilemap.darkRadius.ToString();
                    else if (name == "OnToxicTile") return Tilemap.IsOnToxic.ToString();
                    else if (name == "ToxicLevel") return Tilemap.PlayerToxicLevel.ToString();
                    else if (name == "name") return Tilemap.name;
                }
                else if(objectName == "enemy")
                {
                    if (GameRPG.enemyStats.ContainsKey(name)) return GameRPG.enemyStats[name].ToString();
                    else return "";
                }
                else
                {
                    //GridInfo.Echo("GetValue: unknown object: " + objectName + "... or key ("+name+") wasn't present in dictionary.");
                }
                return "";
            }
            static string GetNPCValue(string name, npc me)
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
            public static T GetValueAs<T>(string name, npc me, T defaultValue = default(T))
            {
                //GridInfo.Echo("GetValueAs: (key) " + name+" (default) "+defaultValue.ToString());
                string value = GetValue(name, me,true);
                //GridInfo.Echo("GetValueAs: (value) " + value);
                if (value == "") return defaultValue;
                return (T)Convert.ChangeType(value, typeof(T));
            }
            */
        }
        //----------------------------------------------------------------------
    }
}
