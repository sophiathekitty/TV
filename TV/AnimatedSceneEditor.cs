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
        // animated scene editor
        //----------------------------------------------------------------------
        public class AnimatedSceneEditor : AnimatedScene
        {
            int editingSprite = -1;
            // constructor
            public AnimatedSceneEditor(string data) : base(data, null)
            {
                GridInfo.AddChangeListener("SceneLength", OnSceneLengthChange);
                GridInfo.AddChangeListener("EditingSprite", OnEditingSpriteChange);
                GridInfo.AddChangeListener("SpriteX", OnSpriteXChange);
                GridInfo.AddChangeListener("SpriteY", OnSpriteYChange);
                GridInfo.AddChangeListener("SpriteDelay", OnSpriteDelayChange);
                GridInfo.AddChangeListener("SpriteStartX", OnSpriteStartXChange);
                GridInfo.AddChangeListener("SpriteStartY", OnSpriteStartYChange);
                GridInfo.AddChangeListener("SpriteHasStart", OnSpriteHasStartChange);
            }
            public void Save()
            {
                string data = "type:animation,length:" + options.length.ToString()+",animation:loop";
                data += "║type:background,width:178,height:107═" + backgroundImage.Data;
                foreach (AnimatedSprite sprite in animatedSprites)
                {
                    data += "║type:sprite,width:" + sprite.Size.X + ",height:"+sprite.Size.Y+",animation:random,delay:"+sprite.delay;
                    if(sprite.HasStart)
                    {
                        data += ",x:" + sprite.endPosition.X + ",y:" + sprite.endPosition.Y;
                        data += ",start_x:" + sprite.startPosition.X + ",start_y:" + sprite.startPosition.Y;
                    }
                    else
                    {
                        data += ",x:" + sprite.Position.X + ",y:" + sprite.Position.Y;
                    }
                    data += "═";
                    bool first = true;
                    foreach(string cell in sprite.cells)
                    {
                        if(first) first = false;
                        else data += ":";
                        data += cell;
                    }
                }
                if(subtitles != null)
                {
                    data += "║type:subtitles,delay:200,animation:loop═";
                    bool first = true;
                    foreach(string subtitle in subtitles.subtitles)
                    {
                        if (first) first = false;
                        else data += ":";
                        data += subtitle;
                    }
                }
                GridBlocks.SetSurfaceCustomData("TV", data);
            }
            // on scene length change
            void OnSceneLengthChange(string name, string value)
            {
                if (name == "SceneLength")
                {
                    int length = 0;
                    if (int.TryParse(value, out length))
                    {
                        options.length = length;
                    }
                }
            }
            // on editing sprite change
            void OnEditingSpriteChange(string name, string value)
            {
                if (name == "EditingSprite")
                {
                    int sprite = 0;
                    if (int.TryParse(value, out sprite))
                    {
                        editingSprite = sprite;
                        if(editingSprite < -1)
                        {
                            GridInfo.SetVar("EditingSprite", "-1");
                        }
                        else if(editingSprite >= animatedSprites.Count)
                        {
                            GridInfo.SetVar("EditingSprite", (animatedSprites.Count - 1).ToString());
                        }
                        else if(editingSprite >= 0 && editingSprite < animatedSprites.Count)
                        {
                            // load the new sprite data into the editor
                            if (animatedSprites[editingSprite].HasStart)
                            {
                                GridInfo.SetVar("SpriteX", animatedSprites[editingSprite].endPosition.X.ToString());
                                GridInfo.SetVar("SpriteY", animatedSprites[editingSprite].endPosition.Y.ToString());
                            }
                            else
                            {
                                GridInfo.SetVar("SpriteX", animatedSprites[editingSprite].Position.X.ToString());
                                GridInfo.SetVar("SpriteY", animatedSprites[editingSprite].Position.Y.ToString());
                            }
                            GridInfo.SetVar("SpriteDelay", animatedSprites[editingSprite].delay.ToString());
                            GridInfo.SetVar("SpriteStartX", animatedSprites[editingSprite].startPosition.X.ToString());
                            GridInfo.SetVar("SpriteStartY", animatedSprites[editingSprite].startPosition.Y.ToString());
                            GridInfo.SetVar("SpriteHasStart", animatedSprites[editingSprite].HasStart.ToString());
                        }
                    }
                }
            }
            // on sprite x change
            void OnSpriteXChange(string name, string value)
            {
                if (name == "SpriteX")
                {
                    float x = 0;
                    if (float.TryParse(value, out x))
                    {
                        if (editingSprite >= 0 && editingSprite < animatedSprites.Count)
                        {
                            if (animatedSprites[editingSprite].HasStart)
                            {
                                animatedSprites[editingSprite].endPosition = new Vector2(x, animatedSprites[editingSprite].endPosition.Y);
                            }
                            else
                            {
                                animatedSprites[editingSprite].Position = new Vector2(x, animatedSprites[editingSprite].Position.Y);
                            }
                        }
                    }
                }
            }
            // on sprite y change
            void OnSpriteYChange(string name, string value)
            {
                if (name == "SpriteY")
                {
                    float y = 0;
                    if (float.TryParse(value, out y))
                    {
                        if (editingSprite >= 0 && editingSprite < animatedSprites.Count)
                        {
                            if (animatedSprites[editingSprite].HasStart)
                            {
                                animatedSprites[editingSprite].endPosition = new Vector2(animatedSprites[editingSprite].endPosition.X, y);
                            }
                            else
                            {
                                animatedSprites[editingSprite].Position = new Vector2(animatedSprites[editingSprite].Position.X, y);
                            }
                        }
                    }
                }
            }
            // on sprite delay change
            void OnSpriteDelayChange(string name, string value)
            {
                if (name == "SpriteDelay")
                {
                    int delay = 0;
                    if (int.TryParse(value, out delay))
                    {
                        if (editingSprite >= 0 && editingSprite < animatedSprites.Count)
                        {
                            animatedSprites[editingSprite].delay = delay;
                        }
                    }
                }
            }
            // on sprite start x change
            void OnSpriteStartXChange(string name, string value)
            {
                if (name == "SpriteStartX")
                {
                    float x = 0;
                    if (float.TryParse(value, out x))
                    {
                        if (editingSprite >= 0 && editingSprite < animatedSprites.Count)
                        {
                            animatedSprites[editingSprite].startPosition = new Vector2(x, animatedSprites[editingSprite].startPosition.Y);
                        }
                    }
                }
            }
            // on sprite start y change
            void OnSpriteStartYChange(string name, string value)
            {
                if (name == "SpriteStartY")
                {
                    float y = 0;
                    if (float.TryParse(value, out y))
                    {
                        if (editingSprite >= 0 && editingSprite < animatedSprites.Count)
                        {
                            animatedSprites[editingSprite].startPosition = new Vector2(animatedSprites[editingSprite].startPosition.X, y);
                        }
                    }
                }
            }
            // on sprite has start change
            void OnSpriteHasStartChange(string name, string value)
            {
                if (name == "SpriteHasStart")
                {
                    bool hasStart = false;
                    if (bool.TryParse(value, out hasStart))
                    {
                        if (editingSprite >= 0 && editingSprite < animatedSprites.Count)
                        {
                            animatedSprites[editingSprite].HasStart = hasStart;
                        }
                    }
                }
            }
        }
        //----------------------------------------------------------------------
    }
}
