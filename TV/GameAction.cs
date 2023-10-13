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
            public string Name { get; set; }
            List<string> Commands = new List<string>();
            npc me;
            public GameAction(string element, npc me)
            {
                this.me = me;
                string[] parts = element.Split(';');
                foreach (string part in parts)
                {
                    if (part.StartsWith("action:"))
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
                bool skipping = false;
                foreach (string command in Commands)
                {
                    GridInfo.Echo("GameAction: " + command +" (skipping:"+skipping+")");
                    if(command == "") continue;
                    string[] parts = command.Split(':');
                    string cmd = "";
                    string param = "";
                    cmd = parts[0].ToLower();
                    if(parts.Length > 1) param = parts[1];
                    GridInfo.Echo("GameAction: " + cmd + " " + param);
                    // conditional flow commands (that can end skipping)
                    if (cmd == "endif")
                    {
                        skipping = false;
                    }
                    else if (cmd == "else")
                    {
                        skipping = !skipping;
                    }
                    else if(skipping && cmd == "elseif")
                    {
                        skipping = !Compare(param);
                    }
                    else if(skipping && cmd == "elseifnot")
                    {
                        skipping = Compare(param);
                    }
                    // skip commands until we hit an else or endif (or elseif)
                    if(skipping) continue;
                    // conditional flow commands (that can start skipping)
                    if(cmd == "if")
                    {
                        GridInfo.Echo("GameAction: if(" + param + ")");
                        bool res = Compare(param);
                        skipping = !res;
                        GridInfo.Echo("GameAction: if("+res+") skipping: " + skipping);
                    }
                    else if(cmd == "ifnot")
                    {
                        skipping = Compare(param);
                    }
                    // math commands
                    else if (cmd == "set")
                    {
                        string[] pair = param.Split('=');
                        SetValue(pair[0], pair[1]);
                    }
                    else if(cmd == "add")
                    {
                        string[] pair = param.Split('=');
                        float value = GetValueAs<float>(pair[0],me);
                        value += float.Parse(pair[1]);
                        SetValue(pair[0], value.ToString());
                    }
                    else if(cmd == "sub")
                    {
                        string[] pair = param.Split('=');
                        float value = GetValueAs<float>(pair[0], me);
                        value -= float.Parse(pair[1]);
                        SetValue(pair[0], value.ToString());
                    }
                    else if(cmd == "mul")
                    {
                        string[] pair = param.Split('=');
                        float value = GetValueAs<float>(pair[0], me);
                        value *= float.Parse(pair[1]);
                        SetValue(pair[0], value.ToString());
                    }
                    else if(cmd == "div")
                    {
                        string[] pair = param.Split('=');
                        float value = GetValueAs<float>(pair[0], me);
                        value /= float.Parse(pair[1]);
                        SetValue(pair[0], value.ToString());
                    }
                    // give item
                    else if(cmd == "give")
                    {
                        int count = 1;
                        if (param.Contains("-"))
                        {
                            string[] pair = param.Split('-');
                            param = pair[0];
                            count = int.Parse(pair[1]);
                        }
                        if(GameRPG.playerInventory.ContainsKey(param)) GameRPG.playerInventory[param] += count;
                        else GameRPG.playerInventory.Add(param, count);
                    }
                    else if(cmd == "say") 
                    {
                        // say something
                        GridInfo.Echo("GameAction: say: " + param);
                        GameRPG.Say(param);
                        return false;
                    }
                    else if(cmd == "savegame")
                    {
                        // save the game
                        GridInfo.Echo("GameAction: savegame");
                    }
                    else
                    {
                        GridInfo.Echo("GameAction: unknown command: " + cmd);
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
                string value = GetValue(pair[0], me);
                float fvalue;
                bool isFloat = float.TryParse(value, out fvalue);
                if (isFloat)
                {
                    float fparam = float.Parse(pair[1]);
                    if (opperator == '=' && fvalue == fparam) result = true;
                    else if (opperator == '<' && fvalue < fparam) result = true;
                    else if (opperator == '>' && fvalue > fparam) result = true;
                }
                else
                {
                    if (opperator == '=' && value == pair[1]) result = true;
                }
                return result;
            }
            // set a value
            void SetValue(string name, string value)
            {
                GridInfo.Echo("SetValue: " + name + " = " + value);
                string objectName = "";
                if (name.Contains("."))
                {
                    string[] parts = name.Split('.');
                    GridInfo.Echo("SetValue: parts: " + parts.Length);
                    objectName = parts[0];
                    name = parts[1];
                }
                // find the object to set the value on
                if (objectName == "player")
                {
                    GameRPG.playerStats[name] = float.Parse(value);
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
                }
                else
                {
                    GridInfo.Echo("SetValue: unknown object: " + objectName);
                }   
            }
            void SetNPCValue(string name, string value)
            {
                GridInfo.Echo("SetNPCValue: " + name + " = " + value);  
                name = name.ToLower();
                if(name.StartsWith("vis")) {
                    GridInfo.Echo("SetNPCValue: visible");
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
                if (objectName == "player" && GameRPG.playerStats.ContainsKey(name))
                {
                    return GameRPG.playerStats[name].ToString();
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
                else if (objectName == "ints" && GameRPG.gameBools.ContainsKey(name))
                {
                    return GameRPG.gameInts[name].ToString();
                }
                else
                {
                    GridInfo.Echo("GetValue: unknown object: " + objectName + "... or key wasn't present in dictionary.");
                }
                return "";
            }
            static string GetNPCValue(string name, npc me)
            {
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
                GridInfo.Echo("GetNPCValue: unknown property: " + name);
                return "";
            }
            public static T GetValueAs<T>(string name, npc me, T defaultValue = default(T))
            {
                GridInfo.Echo("GetValueAs: (key) " + name+" (default) "+defaultValue.ToString());
                string value = GetValue(name, me,true);
                GridInfo.Echo("GetValueAs: (value) " + value);
                if (value == "") return defaultValue;
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }
        //----------------------------------------------------------------------
    }
}
