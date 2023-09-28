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
        public class AnimatedSprite : ScreenSprite
        {
            public List<string> cells = new List<string>();
            public Vector2 startPosition = Vector2.Zero;
            public Vector2 endPosition = Vector2.Zero;
            public bool HasStart { 
                get { return startPosition != Vector2.Zero && endPosition != Vector2.Zero; }
                set 
                { 
                    if (!value) startPosition = Vector2.Zero;
                    else if (startPosition == Vector2.Zero) startPosition = new Vector2(Position.X,Position.Y);
                }
            }
            bool loop = false;
            bool random = false;
            public bool Random 
            { 
                get { return random; } 
                set {
                    if (value) loop = true;
                    random = value;
                }
            }
            int currentCell = 0;
            public int delay = 10;
            int currentDelay = 0;
            // animation done event action
            public Action OnAnimationDone = null;
            bool isDone = false;
            // constructors
            public AnimatedSprite(List<string> cells, Vector2 position, Vector2 size, float scale = 1f, string type = "loop", int delay = 10) : base(ScreenSpriteAnchor.TopLeft, position, scale, size, Color.White, "Monospace", cells[0], TextAlignment.LEFT, SpriteType.TEXT)
            {
                this.delay = delay;
                this.cells = cells;
                if (type == "loop")
                {
                    loop = true;
                }
                else if (type == "random")
                {
                    random = true;
                    loop = true;
                }
            }
            public AnimatedSprite(List<string> cells, Vector2 startPosition, Vector2 endPosition, Vector2 size, float scale = 1f, string type = "loop", int delay = 10) : base(ScreenSpriteAnchor.TopLeft, startPosition, scale, size, Color.White, "Monospace", cells[0], TextAlignment.LEFT, SpriteType.TEXT)
            {
                this.delay = delay;
                this.cells = cells;
                this.startPosition = startPosition;
                this.endPosition = endPosition;
                if (type == "loop")
                {
                    loop = true;
                }
                else if (type == "random")
                {
                    random = true;
                    loop = true;
                }
            }
            public AnimatedSprite(string element) : base(ScreenSpriteAnchor.TopLeft, Vector2.Zero, MONOSPACE_FONT_SIZE, Vector2.Zero, Color.White, "Monospace", "", TextAlignment.LEFT, SpriteType.TEXT)
            {
                string[] data = element.Split('═');
                SpriteOptions options = new SpriteOptions(data[0]);
                cells = data[1].Split(':').ToList();
                Position = options.position;
                Size = options.size;
                loop = options.loop;
                random = options.random;
                delay = options.delay;
                Data = "";
                if (options.HasStart)
                {
                    startPosition = options.start;
                    endPosition = options.end;
                }
                else
                {
                    endPosition = options.position;
                }
                //GridInfo.Echo("AnimatedSprite: delay: " + delay.ToString());
                //GridInfo.Echo("AnimatedSprite: cells: " + cells.Count.ToString());
            }
            // override ToMySprite
            public override MySprite ToMySprite(RectangleF _viewport)
            {
                if(delay > 0)
                {
                    currentDelay++;
                    if(currentDelay >= delay)
                    {
                        currentDelay = 0;
                        if (random)
                        {
                            currentCell = new Random().Next(0, cells.Count);
                        }
                        else
                        {
                            currentCell++;
                        }
                        if (currentCell >= cells.Count)
                        {
                            if (loop)
                            {
                                currentCell = 0;
                            }
                            else
                            {
                                currentCell = cells.Count - 1;
                                if (!isDone && OnAnimationDone != null)
                                {
                                    isDone = true;
                                    OnAnimationDone?.Invoke();
                                }
                            }
                        }
                    }
                }
                float percent = GridInfo.GetVarAs<float>("ScenePercent", 0f);
                if (startPosition != Vector2.Zero && endPosition != Vector2.Zero)
                {
                    Position = Vector2.Lerp(startPosition, endPosition, percent);
                }
                return new MySprite()
                {
                    Type = Type,
                    Data = cells[currentCell],
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
