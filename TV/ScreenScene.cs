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
        // screen scene - encapsulates a scene for a screen
        //----------------------------------------------------------------------

        public class ScreenScene
        {
            public SceneOptions options;
            public List<ScreenSprite> sprites = new List<ScreenSprite>();
            public void AddToScreen(Screen screen)
            {
                foreach (ScreenSprite sprite in sprites)
                {
                    screen.AddSprite(sprite);
                }
            }
            public void RemoveFromScreen(Screen screen) 
            { 
                foreach (ScreenSprite sprite in sprites)
                {
                    screen.RemoveSprite(sprite);
                }
            }
            public virtual void Update() { }
        }
    }
}
