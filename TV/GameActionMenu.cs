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
        public class GameActionMenu : ScreenMenu
        {
            List<GameAction> actions;
            public GameActionMenu(string title, float width, ScreenActionBar actionBar, List<GameAction> actions, List<string> baseActions = null) : base(title, width, actionBar)
            {
                SetBackgroundColor(Color.Black);
                this.actions = actions;
                foreach(GameAction action in actions)
                {
                    AddLabel(action.Name);
                }
                if(baseActions != null)
                {
                    foreach (string action in baseActions)
                    {
                        AddLabel(action);
                    }
                }
                //AddLabel("Items");
                //AddLabel("Spells");
                AddLabel("Quit Game");
            }
            // override input handling
            public override string HandleInput(string input)
            {
                string action = base.HandleInput(input);
                foreach(GameAction gameAction in actions)
                {
                    if (action == gameAction.Name.ToLower())
                    {
                        if(gameAction.Run()) return "back";
                        return "";
                    }
                }
                return action;
            }
        }
    }
}
