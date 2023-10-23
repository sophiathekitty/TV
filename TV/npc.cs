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
        //----------------------------------------------------------------------------------------------------
        // npc
        // animated and/or interactable props that can be characters or chests and locked doors
        // can block movement or not (for example, a chest would not block movement. but a door would)
        // can be animated or not.
        // uses a simple action scripting system to control behavior (simple math and conditional flow.
        // with some aditional special functions. like give to give an item. or shop to show the shop.)
        //----------------------------------------------------------------------------------------------------
        public class npc : AnimatedCharacter
        {
            public static npc MakeNPC(string element)
            {
                string[] parts = element.Split('═');
                foreach (string part in parts)
                {
                    if (part.Contains("type:npc"))
                    {
                        string[] info = part.Split(',');
                        foreach (string var in info)
                        {
                            string[] pair = var.Split(':');
                            if (pair[0] == "character") return new npc(pair[1], parts);
                        }
                    }
                }
                return null;
            }
            public List<GameAction> actions = new List<GameAction>();
            public Dictionary<string,GameAction> yes = new Dictionary<string, GameAction>();
            public Dictionary<string,GameAction> no = new Dictionary<string, GameAction>(); 
            bool randomWalk = false;
            public bool BlocksMovement = true;
            int walkTimer = 0;
            static int walkTime = 90;
            bool visible = true;
            public bool NPCVisible
            {
                get { return visible; }
                set
                {
                    visible = value;
                    Visible = value;
                }
            }
            public bool DoWalk
            {
                get
                {
                    if(!randomWalk) return false;
                    walkTimer++;
                    if (walkTimer >= walkTime)
                    {
                        walkTimer = 0;
                        return true;
                    }
                    return false;
                }
            }
            // constructor
            public npc(string character, string[] parts) : base(AnimatedCharacter.CharacterLibrary[character])
            {
                //GridInfo.Echo("npc: constructor: " + character);
                foreach (string part in parts)
                {
                    if (part.Contains("type:npc"))
                    {
                        string[] info = part.Split(',');
                        foreach (string var in info)
                        {
                            string[] pair = var.Split(':');
                            if (pair[0] == "x") X = int.Parse(pair[1]);
                            else if (pair[0] == "y") Y = int.Parse(pair[1]);
                            else if (pair[0] == "walk") randomWalk = bool.Parse(pair[1]);
                            else if (pair[0] == "direction") SetDirection(pair[1]);
                            else if (pair[0] == "blocks") BlocksMovement = bool.Parse(pair[1]);
                            else if (pair[0] == "visible") NPCVisible = bool.Parse(pair[1]);
                        }
                    }
                    else if (part.Contains("action:"))
                    {
                        //GridInfo.Echo("npc: constructor: action:");
                        actions.Add(new GameAction(part,this));
                    }
                    else if(part.Contains("yes:"))
                    {
                        GameAction y = new GameAction(part, this);
                        yes.Add(y.Name, y);
                        //GridInfo.Echo("npc: constructor: yes:"+y.Name);
                    }
                    else if (part.Contains("no:"))
                    {
                        GameAction n = new GameAction(part, this);
                        no.Add(n.Name, n);
                        //GridInfo.Echo("npc: constructor: no:"+n.Name);
                    }
                    // else it's setting a properety
                    else if (part.StartsWith("visible"))
                    {
                        //GridInfo.Echo("npc: constructor: visible: "+part);
                        string[] pair = part.Split(':');
                        if(pair.Length > 2) Visible = GameAction.GameVars.GetVarAs<bool>(pair[1], this, bool.Parse(pair[2]));
                        else Visible = GameAction.GameVars.GetVarAs<bool>(pair[1],this);
                        NPCVisible = Visible;
                        //GridInfo.Echo("npc: constructor: visible: "+Visible);
                    }
                }
                SetDirection("down");
            }
            public void FacePlayer(AnimatedCharacter player)
            {
                if (player == null) return;
                if (player.Direction == "up") SetDirection("down",false);
                else if (player.Direction == "down") SetDirection("up",false);
                else if (player.Direction == "left") SetDirection("right",false);
                else if (player.Direction == "right") SetDirection("left", false);
            }
        }
        //----------------------------------------------------------------------------------------------------
    }
}
