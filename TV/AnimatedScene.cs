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
        // AnimatedScene - encapsulates an animated scene
        //----------------------------------------------------------------------
        public class AnimatedScene : ScreenScene
        {
            BackgroundImage backgroundImage;
            List<AnimatedSprite> animatedSprites = new List<AnimatedSprite>();
            Subtitles subtitles;
            int frame = 0;
            // constructor
            public AnimatedScene(string data)
            {
                GridInfo.Echo("AnimatedScene: constructor");
                string[] elements = data.Split('║');
                options = new SceneOptions(elements[0]);
                GridInfo.Echo("AnimatedScene: options.type: " + options.type);
                for(int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Contains("type:sprite"))
                    {
                        GridInfo.Echo("AnimatedScene: constructor: sprite");
                        animatedSprites.Add(new AnimatedSprite(elements[i]));
                    }
                    else if (elements[i].Contains("type:background"))
                    {
                        GridInfo.Echo("AnimatedScene: constructor: background");
                        backgroundImage = new BackgroundImage(elements[i]);
                    }
                    else if (elements[i].Contains("type:subtitles"))
                    {
                        GridInfo.Echo("AnimatedScene: constructor: subtitles");
                        subtitles = new Subtitles(elements[i]);
                    }
                }
                sprites.Add(backgroundImage);
                foreach(AnimatedSprite sprite in animatedSprites)
                {
                    sprites.Add(sprite);
                }
                sprites.Add(subtitles);
            }
            // update
            public override void Update()
            {
                float percent = (float)frame / (float)options.length;
                GridInfo.SetVar("ScenePercent", percent.ToString());
            }
        }
        //----------------------------------------------------------------------
    }
}
