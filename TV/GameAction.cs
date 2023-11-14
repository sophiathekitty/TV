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
            public static IGameShop GameShop;
            public static IGameDialog Game;
            public static IGameInventory GameInventory;
            public static IGameSpells GameSpells;
            public static IGameEncounters GameEncounters;
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
                        //GridInfo.Echo("GameAction: else");
                        skipping = !skipping;
                        if(ifwastrue) skipping = true;
                        //GridInfo.Echo("GameAction: else: " + ifwastrue);
                        continue;
                    }
                    else if(skipping && cmd == "elseif")
                    {
                        if (ifwastrue)
                        {
                            skipping = true;
                            continue;
                        }
                        //GridInfo.Echo("GameAction: elseif: "+param);
                        skipping = !Compare(param);
                        if (!skipping) ifwastrue = true;
                        //GridInfo.Echo("GameAction: elseif: " + param + " = " + ifwastrue);
                        continue;
                    }
                    else if(skipping && cmd == "elseifnot")
                    {
                        if (ifwastrue)
                        {
                            skipping = true;
                            continue;
                        }
                        //GridInfo.Echo("GameAction: elseifnot: " + param);
                        skipping = Compare(param);
                        if (!skipping) ifwastrue = true;
                        //GridInfo.Echo("GameAction: elseifnot: " + param + " = " + ifwastrue);
                        continue;
                    }
                    // skip commands until we hit an else or endif (or elseif)
                    if (skipping) continue;
                    GridInfo.Echo("\n\nGameAction:\n" + cmd + " " + param + "\n");
                    // conditional flow commands (that can start skipping)
                    if (cmd == "if")
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
                        GridInfo.Echo("GameAction: setto: " + param);
                        string[] pair = param.Split('=');
                        //string value = GetValue(pair[1], me);
                        string value = GameVars.GetVarAs<string>(pair[1], me);
                        GridInfo.Echo("GameAction: setto: " + pair[0] + " = " + value);
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
                        //GridInfo.Echo("GameAction: div: " + param);
                        string[] pair = param.Split('=');
                        double value = GameVars.GetVarAs<double>(pair[0], me);
                        //GridInfo.Echo("GameAction: div: " + pair[0] + " = " + value);
                        double value2 = 1;
                        if (!double.TryParse(pair[1], out value2)) value2 = GameVars.GetVarAs<double>(pair[1], me);
                        //GridInfo.Echo("GameAction: div: " + pair[1] + " = " + value2);
                        value = Math.Round(value / value2);
                        //GridInfo.Echo("GameAction: div: " + pair[0] + " = " + value);
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
                        /*
                        if(GameRPG.playerInventory.ContainsKey(param)) GameRPG.playerInventory[param] += count;
                        else GameRPG.playerInventory.Add(param, count);
                        GridInfo.Echo("GameAction: give: " + param + " = " + GameRPG.playerInventory[param]);
                        */
                        GameInventory.AddItem(param);
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
                        GameInventory.RemoveItem(param);
                    }
                    else if(cmd == "csay")
                    {
                        GridInfo.Echo("GameAction: csay: " + param);
                        Game.Say(param);
                        continue;
                    }
                    else if(cmd == "say") 
                    {
                        // say something
                        //GridInfo.Echo("GameAction: say: " + param);
                        Game.Say(param);
                        return false;
                    }
                    else if(cmd == "ask")
                    {
                        //GridInfo.Echo("GameAction: ask: " + param);
                        Game.Ask(param,me,Name);
                        return false;
                    }
                    else if(cmd == "shop")
                    {
                        //GridInfo.Echo("GameAction: shop: " + param);
                        GameShop.Shop(param);
                        return false;
                    }
                    else if(cmd == "sell")
                    {
                        //GridInfo.Echo("GameAction: sell: ");
                        GameShop.Sell();
                        return false;
                    }
                    else if(cmd == "go")
                    {
                        if (param.Contains(','))
                        {
                            string[] props = param.Split(',');
                            Game.Go(props[0], int.Parse(props[1]), int.Parse(props[2]));
                        }
                        else if(param == "map.Exit")
                        {
                            TilemapExit exit = Tilemap.Exit;
                            if (exit == null) continue;
                            Game.Go(exit.Map, exit.MapX, exit.MapY);
                        }
                    }
                    else if(cmd == "savegame")
                    {
                        // save the game
                        //GridInfo.Echo("GameAction: savegame");
                        Game.SaveGame();
                    }
                    else if(cmd == "run")
                    {
                        //GridInfo.Echo("GameAction: run: " + param);
                        Game.Run(param);
                    }
                    else if (cmd == "encounter")
                    {
                        GridInfo.Echo("GameAction: encounter: " + param);
                        if(param == "map.encounter") param = Tilemap.GetEncounter(Game.GetPlayerX(),Game.GetPlayerY());
                        GameEncounters.StartEncounter(param,me);
                    }
                    else if (cmd == "encounterOver")
                    {
                        GameEncounters.EndEncounter();
                    }
                    else if(cmd == "exit")
                    {
                        return true;
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
                GridInfo.Echo("GameAction:0: Compare: " + param);
                bool result = false;
                // do check
                char opperator = '=';
                if (param.Contains("<")) opperator = '<';
                else if (param.Contains(">")) opperator = '>';
                string[] pair = param.Split(opperator);
                string value = GameVars.GetVarAs<string>(pair[0], me);
                GridInfo.Echo("GameAction:1: Compare: " + pair[0] + " = " + value);
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
        }
        //----------------------------------------------------------------------
    }
}
