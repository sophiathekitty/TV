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
        // animated scene editor menu
        //----------------------------------------------------------------------
        public class AnimatedSceneEditorMenu : ScreenMenu
        {
            public AnimatedSceneEditorMenu(int sprites, float width, ScreenActionBar actionBar) : base("Scene Editor", width, actionBar)
            {
                AddVariable("Length", "SceneLength", "800");
                for(int i = 0; i < sprites; i++)
                {
                    AddLabel("Sprite " + i);
                }
            }
            // override input handling
            public override string HandleInput(string input)
            {
                string action = base.HandleInput(input);
                if (action.StartsWith("sprite "))
                {
                    GridInfo.SetVar("EditingSprite", action.Substring(7));
                    return "edit sprite";
                }
                else if (action == "back")
                {
                    if(GridInfo.GetVarAs<int>("EditingSprite") >= 0)
                    {
                        GridInfo.SetVar("EditingSprite", "-1");
                        return "";
                    }
                    else return action;
                }
                return action;
            }
        }
        //----------------------------------------------------------------------
    }
}
