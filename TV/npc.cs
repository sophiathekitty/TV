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

            bool randomWalk = false;
            public bool BlocksMovement = true;
            int walkTimer = 0;
            static int walkTime = 10;
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
            public npc(string character, string[] parts) : base(AnimatedCharacter.CharacterLibrary[character])
            {
                GridInfo.Echo("npc: constructor: " + character);
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
                        }
                    }
                }
                SetDirection("down");
            }
        }
    }
}
