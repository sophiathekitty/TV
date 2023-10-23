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
        // ScreenMenu
        //----------------------------------------------------------------------
        public class ScreenMenu
        {
            ScreenSprite back;
            ScreenSprite title;
            public List<ScreenMenuItem> menuItems = new List<ScreenMenuItem>();
            int selectedIndex = 0;
            public float ItemHeight = 30f;
            public float ItemIndent = 20f;
            float _width = 100f;
            float headerHeight = 1.2f;
            ScreenActionBar actionBar;
            public bool handleEditing = true;
            public string menuScrollActions
            {
                get
                {
                    if (actionBar != null)
                    {
                        if (actionBar.Count == 5) return "Up Down Select  Back";
                        if (actionBar.Count == 4) return "Up Down Select Back";
                    }
                    return "Up Down Select";
                }
            }
            public string menuEditActions
            {
                get
                {
                    if (actionBar != null)
                    {
                        if (actionBar.Count == 5) return "+ ++ - -- Done";
                        if (actionBar.Count == 4) return "+ -  Done";
                    }
                    return "+ - Done";
                }
            }
            public float Width
            {
                get { return _width; }
                set
                {
                    _width = value;
                    foreach (ScreenMenuItem item in menuItems)
                    {
                        item.Width = value;
                    }
                }
            }
            public float Height
            {
                get
                {
                    if (menuItems.Count == 0) return 0;
                    return ItemHeight * menuItems.Count + (ItemHeight * headerHeight);
                }
            }
            public int SelectedIndex
            {
                get { return selectedIndex; }
                set
                {
                    if (value >= 0 && value < menuItems.Count)
                    {
                        if (menuItems.Count > 0) menuItems[selectedIndex].Selected = false;
                        selectedIndex = value;
                        menuItems[selectedIndex].Selected = true;
                    }
                }
            }
            public bool Visible
            {
                get
                {
                    return title.Visible;
                }
                set
                {
                    back.Visible = value;
                    title.Visible = value;
                    foreach (ScreenMenuItem item in menuItems)
                    {
                        item.Visible = value;
                    }
                }
            }
            public ScreenMenuItem SelectedItem
            {
                get
                {
                    if (menuItems.Count == 0) return null;
                    return menuItems[selectedIndex];
                }
            }
            public ScreenMenu(string title, float width, ScreenActionBar actionBar)
            {
                _width = width;
                this.title = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, Vector2.Zero, headerHeight, Vector2.Zero, Color.White, "White", title, TextAlignment.LEFT, SpriteType.TEXT);
                this.actionBar = actionBar;
            }
            public void SetBackgroundColor(Color color)
            {
                if(back == null)
                {
                    back = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterLeft, Vector2.Zero, 0, Vector2.Zero, color, "", "SquareSimple", TextAlignment.LEFT, SpriteType.TEXTURE);
                }
                else
                {
                    back.Color = color;
                }
            }
            //
            // add a label to the menu
            //
            public void AddLabel(string label)
            {
                menuItems.Add(new ScreenMenuItem(label, _width));
            }
            public void AddLabel(string label, string defaultValue)
            {
                GridInfo.Echo("AddLabel: " + label + ": " + defaultValue);
                menuItems.Add(new ScreenMenuItem(label, _width, defaultValue));
            }
            //
            // add variable to the menu
            //
            public void AddVariable(string label, string varName, string defaultValue = "")
            {
                menuItems.Add(new ScreenMenuItem(label, varName, _width, defaultValue));
            }
            // remove an item from the menu
            public void RemoveItem(string label)
            {
                foreach (ScreenMenuItem item in menuItems)
                {
                    if (item.Label == label)
                    {
                        menuItems.Remove(item);
                        break;
                    }
                }
            }
            // remove all items from the menu
            public void Clear()
            {
                menuItems.Clear();
            }
            //
            // add to screen
            //
            public void AddToScreen(Screen screen)
            {
                Vector2 position = new Vector2(ItemIndent, Height / -2);
                if(back != null)
                {
                    back.Position = new Vector2(-5,10);
                    back.Size = new Vector2(Width+10, Height+30);
                    screen.AddSprite(back);
                }
                title.Position = position;
                position.Y += ItemHeight * headerHeight;
                int i = 0;
                screen.AddSprite(title);
                foreach (ScreenMenuItem item in menuItems)
                {
                    item.Selected = (i++ == selectedIndex);
                    item.Position = position;
                    position.Y += ItemHeight;
                    item.AddToScreen(screen);
                }
            }
            //
            // remove from screen
            //
            public void RemoveFromScreen(Screen screen)
            {
                screen.RemoveSprite(back);
                screen.RemoveSprite(title);
                foreach (ScreenMenuItem item in menuItems)
                {
                    item.RemoveFromScreen(screen);
                }
            }
            //
            // handle input
            //
            public virtual string HandleInput(string argument)
            {
                if (argument.ToLower().StartsWith("btn"))
                {
                    // handle button press
                    string[] args = argument.Split(' ');
                    int btn = -1;
                    if (args.Length > 1)
                    {
                        int.TryParse(args[1], out btn);
                    }
                    if (btn > 0)
                    {
                        string action = actionBar.HandleInput(btn - 1);
                        if (action == "up")
                        {
                            SelectedItem.Editing = false;
                            SelectedIndex--;
                        }
                        else if (action == "down")
                        {
                            SelectedItem.Editing = false;
                            SelectedIndex++;
                        }
                        else if (action == "select")
                        {
                            if (SelectedItem.IsAction || !handleEditing)
                            {
                                return (SelectedItem.Label.ToLower());
                            }
                            else if (SelectedItem.IsToggle)
                            {
                                SelectedItem.Editing = true;
                                SelectedItem.DataAsBool = !SelectedItem.DataAsBool;
                            }
                            else
                            {
                                SelectedItem.Editing = true;
                                actionBar.SetActions(menuEditActions);
                            }
                        }
                        else if (action == "+")
                        {
                            SelectedItem.DataAsInt++;
                        }
                        else if (action == "++")
                        {
                            SelectedItem.DataAsInt += 10;
                        }
                        else if (action == "-")
                        {
                            SelectedItem.DataAsInt--;
                        }
                        else if (action == "--")
                        {
                            SelectedItem.DataAsInt -= 10;
                        }
                        else if (action == "done")
                        {
                            SelectedItem.Editing = false;
                            actionBar.SetActions(menuScrollActions);
                        }
                        else
                        {
                            return action;
                        }
                    }
                }
                return "";
            }

        }
        //----------------------------------------------------------------------
    }
}
