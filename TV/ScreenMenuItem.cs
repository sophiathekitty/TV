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
        // ScreenMenuItem
        //----------------------------------------------------------------------
        public class ScreenMenuItem
        {
            ScreenSprite bullet;
            ScreenSprite label;
            ScreenSprite text;
            string variableName;
            Color Color = Color.White;
            Color selectedColor = Color.LightYellow;
            Color editingColor = Color.Orange;
            string bulletIcon = "";
            string bulletSelectedIcon = ">";
            string trueValue = "on";
            string falseValue = "off";
            bool selected = false;
            bool editing = false;
            bool isToggle = false;
            public bool IsToggle { get { return isToggle; } }
            public bool IsAction { get { return text == null; } }
            // update the label of the variable
            public string Label
            {
                get { return label.Data; }
                set { label.Data = value; }
            }
            // update the displayed value of the variable
            public string Data
            {
                get
                {
                    if (text == null) return "";
                    return text.Data;
                }
                set
                {
                    if (text != null) text.Data = value;
                }
            }
            // for when the menu is editing the value of the variable
            // updates the global variable when set
            public int DataAsInt
            {
                get
                {
                    if (text == null) return 0;
                    int result = 0;
                    int.TryParse(text.Data, out result);
                    return GridInfo.GetVarAs<int>(variableName,result);
                }
                set
                {
                    if (variableName != "") GridInfo.SetVar(variableName, value.ToString());
                }
            }
            // for when the menu is editing toggling the value of the variable
            // updates the global variable when set
            public bool DataAsBool
            {
                get
                {
                    if (text == null) return false;
                    return GridInfo.GetVarAs<bool>(variableName);
                }
                set
                {
                    if (variableName != "") GridInfo.SetVar(variableName, value.ToString());
                }
            }
            // update the icon of the variable
            public string Icon
            {
                get { return bullet.Data; }
                set { bullet.Data = value; }
            }
            public bool Selected
            {
                get { return selected; }
                set
                {
                    selected = value;
                    if (selected)
                    {
                        bullet.Data = bulletSelectedIcon;
                        label.Color = selectedColor;
                        if (text != null)
                        {
                            if (editing)
                            {
                                text.Color = editingColor;
                            }
                            else
                            {
                                text.Color = selectedColor;
                            }
                        }
                    }
                    else
                    {
                        bullet.Data = bulletIcon;
                        label.Color = Color;
                        if (text != null) text.Color = Color;
                    }
                }
            }
            public bool Editing
            {
                get { return editing; }
                set
                {
                    editing = value;
                    if (text != null)
                    {
                        if (editing)
                        {
                            text.Color = editingColor;
                        }
                        else
                        {
                            text.Color = selectedColor;
                        }

                    }
                }
            }
            public bool Visible
            {
                get { return bullet.Visible; }
                set
                {
                    bullet.Visible = value;
                    label.Visible = value;
                    if (text != null) text.Visible = value;
                }
            }
            private Vector2 _postion;
            private float _width;
            public float Width
            {
                get { return _width; }
                set
                {
                    _width = value;
                    _postion = bullet.Position;
                    _postion += new Vector2(_width, 0);
                    if (text != null) text.Position = _postion;
                }
            }
            public Vector2 Position
            {
                get { return bullet.Position; }
                set
                {
                    bullet.Position = value;
                    label.Position = value;
                    if (text != null) text.Position = value + new Vector2(_width, 0);
                }
            }
            public ScreenMenuItem(string label, string varName, float width, string defaultValue)
            {
                //GridInfo.Echo("ScreenMenuItem: constructor: " + label + " " + varName + " " + width.ToString() + " " + defaultValue);
                _postion = new Vector2(0, 0);
                _width = width;
                variableName = varName;
                GridInfo.AddChangeListener(varName, Update);
                bullet = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, _postion, ScreenSprite.DEFAULT_FONT_SIZE, new Vector2(0, 0), Color, "White", bulletIcon, TextAlignment.RIGHT, SpriteType.TEXT);
                this.label = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, _postion, ScreenSprite.DEFAULT_FONT_SIZE, new Vector2(1, 0), Color, "White", label, TextAlignment.LEFT, SpriteType.TEXT);
                _postion += new Vector2(width, 0);
                text = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, _postion, ScreenSprite.DEFAULT_FONT_SIZE, new Vector2(0, 0), Color, "White", GridInfo.GetVarAs<string>(variableName), TextAlignment.LEFT, SpriteType.TEXT);
                DisplayValue(GridInfo.GetVarAs<string>(variableName, defaultValue));
            }
            public ScreenMenuItem(string label, float width, string defaultValue)
            {
                GridInfo.Echo("ScreenMenuItem: constructor: " + label + " " + width.ToString() + " " + defaultValue);
                _postion = new Vector2(0, 0);
                _width = width;
                bullet = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, _postion, ScreenSprite.DEFAULT_FONT_SIZE, new Vector2(0, 0), Color, "White", bulletIcon, TextAlignment.RIGHT, SpriteType.TEXT);
                this.label = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, _postion, ScreenSprite.DEFAULT_FONT_SIZE, new Vector2(1, 0), Color, "White", label, TextAlignment.LEFT, SpriteType.TEXT);
                _postion += new Vector2(width, 0);
                text = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, _postion, ScreenSprite.DEFAULT_FONT_SIZE, new Vector2(0, 0), Color, "White", defaultValue, TextAlignment.LEFT, SpriteType.TEXT);
            }
            public ScreenMenuItem(string label, float width)
            {
                _postion = new Vector2(0, 0);
                _width = width;
                bullet = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, _postion, ScreenSprite.DEFAULT_FONT_SIZE, new Vector2(0, 0), Color, "White", bulletIcon, TextAlignment.RIGHT, SpriteType.TEXT);
                this.label = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, _postion, ScreenSprite.DEFAULT_FONT_SIZE, new Vector2(1, 0), Color, "White", label, TextAlignment.LEFT, SpriteType.TEXT);
            }
            // add the sprites to the render list
            public void AddSprites(ref List<ScreenSprite> sprites)
            {
                sprites.Add(bullet);
                sprites.Add(label);
                if (text != null)
                {
                    sprites.Add(text);
                }
            }
            // remove the sprites from the render list
            public void RemoveSprites(ref List<ScreenSprite> sprites)
            {
                sprites.Remove(bullet);
                sprites.Remove(label);
                if (text != null)
                {
                    sprites.Remove(text);
                }
            }
            // update the text sprite with the current value of the variable
            private void Update(string key, string value)
            {
                if (text != null && key == variableName)
                {
                    DisplayValue(value);
                }
            }
            private void DisplayValue(string value)
            {
                GridInfo.Echo("ScreenMenuItem: DisplayValue: " + value);
                // if the variable is a bool, parse it and set the text to the bool value
                bool boolValue;
                if (bool.TryParse(value, out boolValue))
                {
                    isToggle = true;
                    text.Data = boolValue ? trueValue : falseValue;
                    GridInfo.Echo("ScreenMenuItem: DisplayValue: bool: " + text.Data);
                }
                else
                {
                    isToggle = false;
                    text.Data = value;
                }
            }
            public void Style(string bullet_unselected, string bullet_selected, Color color, Color selectedColor, Color editingColor)
            {
                bulletIcon = bullet_unselected;
                bulletSelectedIcon = bullet_selected;
                Color = color;
                this.selectedColor = selectedColor;
                this.editingColor = editingColor;
                bullet.Data = bulletIcon;
                if (Selected)
                {
                    bullet.Data = bulletSelectedIcon;
                    label.Color = selectedColor;
                    if (text != null)
                    {
                        if (editing)
                        {
                            text.Color = editingColor;
                        }
                        else
                        {
                            text.Color = selectedColor;
                        }
                    }
                }
                else
                {
                    bullet.Data = bulletIcon;
                    label.Color = Color;
                    if (text != null) text.Color = Color;
                }
            }
            // add to screen
            public void AddToScreen(Screen screen)
            {
                GridInfo.Echo("ScreenMenuItem: AddToScreen: "+label.Data);
                screen.AddSprite(bullet);
                screen.AddSprite(label);
                if (text != null)
                {
                    GridInfo.Echo("ScreenMenuItem: AddToScreen: text: "+text.Data);
                    screen.AddSprite(text);
                }

            }
            // remove from screen
            public void RemoveFromScreen(Screen screen)
            {
                screen.RemoveSprite(bullet);
                screen.RemoveSprite(label);
                if (text != null)
                {
                    screen.RemoveSprite(text);
                }
            }
        }
        //----------------------------------------------------------------------
    }
}
