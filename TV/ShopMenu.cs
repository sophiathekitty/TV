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
        // ShopMenu
        //----------------------------------------------------------------------------------------------------
        public class ShopMenu : ScreenMenu
        {
            List<ShopItem> items;
            public string playerMoney = "player.gold";
            public string insufficaintFunds = "You don't have enough money for that.";
            public string purchaseText = "You purchased ITEMNAME for ITEMCOST.";
            public string sellText = "You sold ITEMNAME for ITEMCOST.";
            public string sellFailText = "You don't have any of those to sell.";
            public bool playerSelling = false;
            public ShopMenu(string title, float width, ScreenActionBar actionBar, List<ShopItem> items) : base(title, width, actionBar)
            {
                GridInfo.Echo("ShopMenu:1: "+title);
                handleEditing = false;
                SetBackgroundColor(Color.Black);
                this.items = items;
                foreach(ShopItem item in items)
                {
                    GridInfo.Echo("ShopMenu:2: "+item.Name);
                    AddLabel(item.Name,item.Cost.ToString());
                }
                GridInfo.Echo("ShopMenu:3: "+items.Count);
            }
            public ShopMenu(string title, float width, ScreenActionBar actionBar, string items): base(title, width, actionBar)
            {
                handleEditing = false;
                SetBackgroundColor(Color.Black);
                this.items = new List<ShopItem>();
                string[] itemArray = items.Split(',');
                foreach(string item in itemArray)
                {
                    if(GameRPG.itemStats.ContainsKey(item))
                    {
                        this.items.Add(new ShopItem(GameRPG.itemStats[item]));
                        AddLabel(item, this.items[this.items.Count-1].Cost.ToString());
                    }
                }
            }
            // override input handling
            public override string HandleInput(string input)
            {
                string action = base.HandleInput(input);
                GridInfo.Echo("ShopMenu:4: "+action);
                foreach(ShopItem gameItem in items)
                {
                    if (gameItem.Name.ToLower() == action)
                    {
                        if(playerSelling)
                        {
                            SellItem(gameItem);
                        }
                        else
                        {
                            TryToBuy(gameItem);
                        }
                        return "";
                    }
                }
                return action;
            }
            // try to buy an item
            void TryToBuy(ShopItem gameItem)
            {
                int funds = GameAction.GameVars.GetVarAs<int>(playerMoney, null, 0);
                if (funds >= gameItem.Cost)
                {
                    GameAction.GameVars.SetVar<int>(playerMoney, null, funds - gameItem.Cost);
                    if (GameRPG.playerInventory.ContainsKey(gameItem.Name))
                    {
                        GameRPG.playerInventory[gameItem.Name]++;
                    }
                    else
                    {
                        GameRPG.playerInventory.Add(gameItem.Name, 1);
                    }
                    string saytxt = purchaseText.Replace("ITEMNAME", gameItem.Name);
                    saytxt = saytxt.Replace("ITEMCOST", gameItem.Cost.ToString());
                    GameRPG.Say(saytxt);
                }
                else
                {
                    GameRPG.Say(insufficaintFunds);
                }
            }
            // sell an item
            void SellItem(ShopItem gameItem)
            {
                if (GameRPG.playerInventory.ContainsKey(gameItem.Name) && GameRPG.playerInventory[gameItem.Name] > 0)
                {
                    GameRPG.playerInventory[gameItem.Name]--;
                    if (GameRPG.playerGear[GameRPG.itemStats[gameItem.Name]["item_type"]] == gameItem.Name && GameRPG.playerInventory[gameItem.Name] == 0) GameRPG.playerGear[GameRPG.itemStats[gameItem.Name]["item_type"]] = "";
                    GameAction.GameVars.SetVar<int>(playerMoney, null, GameAction.GameVars.GetVarAs<int>(playerMoney, null, 0) + gameItem.Cost);
                    string saytxt = sellText.Replace("ITEMNAME", gameItem.Name);
                    saytxt = saytxt.Replace("ITEMCOST", gameItem.Cost.ToString());
                    if (GameRPG.playerInventory[gameItem.Name] == 0) GameRPG.playerInventory.Remove(gameItem.Name);
                    GameRPG.Say(saytxt);
                }
                else GameRPG.Say(sellFailText);
            }
        }
        //----------------------------------------------------------------------------------------------------
        // ShopItem
        //----------------------------------------------------------------------------------------------------
        public class ShopItem
        {
            public string Name;
            public int Cost;
            public ShopItem(string name, int cost)
            {
                Name = name;
                Cost = cost;
            }
            public ShopItem(Dictionary<string, string> itemStats) 
            {
                if(itemStats.ContainsKey("name")) Name = itemStats["name"];
                if (itemStats.ContainsKey("cost")) int.TryParse(itemStats["cost"], out Cost);
            }
        }
        //----------------------------------------------------------------------------------------------------
    }
}
