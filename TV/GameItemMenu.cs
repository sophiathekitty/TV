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
        public class GameItemMenu : ScreenMenu
        {
            List<GameItem> items;
            public GameItemMenu(string title, float width, ScreenActionBar actionBar, List<GameItem> items) : base(title, width, actionBar)
            {
                GridInfo.Echo("GameItemMenu:1: "+title);
                handleEditing = false;
                SetBackgroundColor(Color.Black);
                this.items = items;
                foreach(GameItem item in items)
                {
                    GridInfo.Echo("GameItemMenu:2: "+item.Name);
                    AddLabel(item.Name,item.count.ToString());
                }
                GridInfo.Echo("GameItemMenu:3: "+items.Count);
            }
            public void Update(Dictionary<string,int> inventory)
            {
                int i = 0;
                foreach(GameItem item in items)
                {
                    if (inventory.ContainsKey(item.Name)) item.count = inventory[item.Name];
                    if (menuItems.Count > i && menuItems[i].Label == item.Name)
                    {
                        if (GameAction.GameInventory.HasEquipped(item.Name)) menuItems[i].Data = "E";
                        else menuItems[i].Data = item.count.ToString();
                    }
                    i++;
                }
                
            }
            // override input handling
            public override string HandleInput(string input)
            {
                string action = base.HandleInput(input);
                foreach(GameItem gameItem in items)
                {
                    if (action == gameItem.Name.ToLower())
                    {
                        if(gameItem.Run()) return "back";
                        return "turn done";
                    }
                }
                return action;
            }
        }
    }
}
