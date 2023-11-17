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
        public class StartMenu : ScreenMenu
        {
            string game;
            public StartMenu(string title, string game,List<string> saveNames, float width, ScreenActionBar actionBar, bool background = true) : base(title, width, actionBar)
            {
                this.game = game;
                if(background) SetBackgroundColor(Color.Black);
                foreach(string save in saveNames) AddLabel(save);
                AddLabel("Delete Saves");
                AddLabel("Quit Game");
            }
            public override string HandleInput(string argument)
            {
                string action = base.HandleInput(argument);
                if(action == "delete saves")
                {
                    SceneCollection.ResetSaves(game);
                    List<string> saveNames = SceneCollection.GetSaveNames(game);
                    GridInfo.Echo("StartMenu:saveNames: "+saveNames.Count);
                    for(int i = 0; i < saveNames.Count; i++)
                    {
                        GridInfo.Echo("StartMenu:saveNames: " + menuItems[i].Label + " -> " + saveNames[i]);
                        menuItems[i].Label = saveNames[i];
                    }
                }
                return action;
            }
        }
    }
}
