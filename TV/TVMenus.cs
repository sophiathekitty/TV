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
        //----------------------------------------------------------------------
        // Handle the menus for the TV
        //----------------------------------------------------------------------
        public class TVMenus
        {
            Dictionary<string, ScreenMenu> menus = new Dictionary<string, ScreenMenu>();
            string currentMenu = "main";
            Screen screen;
            ScreenActionBar actionBar;
            // constructor
            public TVMenus(Screen screen, ScreenActionBar actionBar, float width = 300)
            {
                this.screen = screen;
                this.actionBar = actionBar;
                menus.Add("main", new MainMenu(actionBar));
                menus.Add("options", new OptionMenu(width, actionBar));
            }
            // handle input
            public string HandleInput(string input)
            {
                string action = "";
                if(menus.ContainsKey(currentMenu))
                {
                    action = menus[currentMenu].HandleInput(input);
                    if (menus.ContainsKey(action))
                    {
                        SetMenu(action);
                        return "";
                    }
                    else if (action == "back")
                    {
                        if (currentMenu != "main") SetMenu("main");
                        else return action;
                        return "";
                    }
                }
                else
                {
                    return actionBar.HandleInput(input);
                }
                return action;
            }
            // set the current menu
            public void SetMenu(string menu)
            {
                if (menus.ContainsKey(menu))
                {
                    menus[currentMenu].RemoveFromScreen(screen);
                    currentMenu = menu;
                    menus[currentMenu].AddToScreen(screen);
                }
            }
            public void Hide()
            {
                menus[currentMenu].RemoveFromScreen(screen);
            }
            public void Show()
            {
                if(!menus.ContainsKey(currentMenu)) currentMenu = "main";
                menus[currentMenu].AddToScreen(screen);
            }
        }
    }
}
