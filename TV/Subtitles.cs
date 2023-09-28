using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public class Subtitles : ScreenSprite
        {
            public List<string> subtitles;
            bool loop = false;
            bool random = false;
            int delay = 10;
            int index = 0;
            int currentDelay = 0;
            bool isDone = false;
            public Action OnAnimationDone = null;
   
            public Subtitles(List<string> data, float scale = 1f) : base(ScreenSpriteAnchor.BottomCenter, new Vector2(0,-65), scale, Vector2.Zero, Color.White, "White", data[0], TextAlignment.CENTER, SpriteType.TEXT)
            {

            }
            public Subtitles(string element) : base(ScreenSpriteAnchor.BottomCenter, new Vector2(0, -65), 2f, Vector2.Zero, Color.White, "White", "", TextAlignment.CENTER, SpriteType.TEXT)
            {
                string[] data = element.Split('═');
                SpriteOptions options = new SpriteOptions(data[0]);
                subtitles = data[1].Split(':').ToList();
                Data = subtitles[0];
                delay = options.delay;
                loop = options.loop;
                random = options.random;
            }
            // override ToMySprite
            public override MySprite ToMySprite(RectangleF _viewport)
            {
                if (delay > 0)
                {
                    currentDelay++;
                    if (currentDelay >= delay)
                    {
                        currentDelay = 0;
                        if (random)
                        {
                            index = new Random().Next(0, subtitles.Count);
                        }
                        else
                        {
                            index++;
                        }
                        if (index >= subtitles.Count)
                        {
                            if (loop)
                            {
                                index = 0;
                            }
                            else
                            {
                                index = subtitles.Count - 1;
                                if (!isDone && OnAnimationDone != null)
                                {
                                    isDone = true;
                                    OnAnimationDone?.Invoke();
                                }
                            }
                        }
                    }
                }
                return new MySprite()
                {
                    Type = Type,
                    Data = subtitles[index],
                    Position = GetPosition(_viewport),
                    RotationOrScale = RotationOrScale,
                    Color = Color,
                    Alignment = Alignment,
                    FontId = FontId
                };
            }
        }
    }
}
