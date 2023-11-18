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
        // DeleteShowsMenu
        //----------------------------------------------------------------------
        public class DeleteShowsMenu : ScreenMenu
        {
            public DeleteShowsMenu(float width, ScreenActionBar actionBar) : base("Delete Shows", width, actionBar)
            {
            }
            public override void AddToScreen(Screen screen)
            {
                menuItems.Clear();
                foreach (var show in SceneCollection.scenes)
                {
                    AddLabel(show.Key);
                }
                base.AddToScreen(screen);
            }
            public override string HandleInput(string argument)
            {
                string action = base.HandleInput(argument);
                if (SelectedItem != null && SelectedItem.Label.ToLower() == action)
                {
                    SceneCollection.DeleteShow(SelectedItem.Label);
                    return "back";
                }
                return action;
            }
        }
    }
}
