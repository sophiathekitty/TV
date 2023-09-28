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
        // menu for the animated sprite editor
        //----------------------------------------------------------------------
        public class AnimatedSpriteEditMenu : ScreenMenu
        {
            public AnimatedSpriteEditMenu(float width, ScreenActionBar actionBar) : base("Sprite Editor", width, actionBar)
            {
                AddVariable("Sprite X", "SpriteX", "0");
                AddVariable("Sprite Y", "SpriteY", "0");
                AddVariable("Delay", "SpriteDelay", "10");
                AddVariable("Start X", "SpriteStartX", "0");
                AddVariable("Start Y", "SpriteStartY", "0");
                AddVariable("Has Start", "SpriteHasStart", "false");
            }
        }
        //----------------------------------------------------------------------
    }
}
