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
        public class GameItem
        {
            public string Name { get; set; }
            public string type { get; set; }
            GameAction useAction;
            public int count = 1;
            public GameItem(string name, int ammount, GameAction useAction = null)
            {
                GridInfo.Echo("GameItem:1: "+name);
                Name = name;
                count = ammount;
                this.useAction = useAction;
            }
            public GameItem(string name, string type, int ammount)
            {
                GridInfo.Echo("GameItem:2: "+name);
                Name = name;
                this.type = type;
                count = ammount;
            }
            public bool Run()
            {
                if(count <= 0) return false;
                if (useAction != null)
                {
                    return useAction.Run();
                } 
                else if(GameAction.GameInventory.CanEquip(Name))//GameRPG.playerGear.ContainsKey(type))
                {
                    if(GameAction.GameInventory.HasEquipped(Name)) GameAction.GameInventory.Unequip(Name);
                    else GameAction.GameInventory.Equip(Name);
                    //if (GameRPG.playerGear[type] == Name) GameRPG.playerGear[type] = "";
                    //else GameRPG.playerGear[type] = Name;
                }
                return false;
            }
        }
    }
}
