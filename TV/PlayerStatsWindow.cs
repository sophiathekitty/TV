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
        // PlayerStatsWindow
        //----------------------------------------------------------------------
        public class PlayerStatsWindow
        {
            ScreenSprite back;
            ScreenSprite title;
            List<PlayerStatsWindowItem> items;
            float width;
            public float ItemHeight = 30f;
            public float Indent = 10f;
            float headerHeight = 1.2f;

            public float Height
            {
                get
                {
                    if (items.Count == 0) return 0;
                    return ItemHeight * items.Count + (ItemHeight * headerHeight);
                }
            }
            public PlayerStatsWindow(Dictionary<string,double> stats, float width)
            {
                this.width = width;
                back = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterRight,Vector2.Zero,0,Vector2.Zero,Color.Black,"","SquareSimple",TextAlignment.LEFT,SpriteType.TEXTURE);
                title = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterRight,new Vector2(-width,0),headerHeight,Vector2.Zero,Color.White,"White","Stats",TextAlignment.LEFT,SpriteType.TEXT);
                items = new List<PlayerStatsWindowItem>();
                foreach(var stat in stats)
                {
                    items.Add(new PlayerStatsWindowItem(stat.Key,Math.Round(stat.Value).ToString(),width));
                }
            }
            public void AddToScreen(Screen screen)
            {
                Vector2 position = new Vector2(-Indent, Height / -2);
                back.Size = new Vector2(width+Indent+30, Height+30);
                back.Position = new Vector2(0-width-Indent-Indent,0);
                //GridInfo.Echo("back.Position: " + back.Position.ToString());
                //GridInfo.Echo("back.Size: " + back.Size.ToString());
                screen.AddSprite(back);
                title.Position = new Vector2(0-width,position.Y);
                screen.AddSprite(title);
                position.Y += ItemHeight * headerHeight;
                foreach (var item in items)
                {
                    item.SetPosition(position);
                    item.AddToScreen(screen);
                    position.Y += ItemHeight;
                }
            }
            public void RemoveFromScreen(Screen screen)
            {
                screen.RemoveSprite(back);
                screen.RemoveSprite(title);
                foreach (var item in items)
                {
                    item.RemoveFromScreen(screen);
                }
            }
            public void SetColor(Color color)
            {
                back.Color = color;
                title.Color = color;
                foreach (var item in items)
                {
                    item.SetColor(color);
                }
            }
            public void Update(Dictionary<string,double> stats)
            {
                foreach(var stat in stats)
                {
                    var item = items.Find(x => x.Title == stat.Key);
                    if (item != null)
                    {
                        item.Value = Math.Round(stat.Value).ToString();
                    }
                }
            }
        }
        //----------------------------------------------------------------------
        // StatsWindowItem
        //----------------------------------------------------------------------
        public class PlayerStatsWindowItem
        {
            ScreenSprite title;
            ScreenSprite value;
            float width;
            public string Value
            {
                get { return value.Data; }
                set { this.value.Data = value; }
            }
            public string Title
            {
                get { return title.Data; }
            }
            public PlayerStatsWindowItem(string itm_title, string itm_value, float width)
            {
                this.width = width;
                title = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterRight,new Vector2(-width,0),1f,Vector2.Zero,Color.White,"White",itm_title,TextAlignment.LEFT,SpriteType.TEXT);
                value = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.CenterRight,Vector2.Zero,1f,Vector2.Zero,Color.White,"White",itm_value,TextAlignment.RIGHT,SpriteType.TEXT);
            }
            public void AddToScreen(Screen screen)
            {
                screen.AddSprite(title);
                screen.AddSprite(value);
            }
            public void SetPosition(Vector2 position)
            {
                title.Position = new Vector2(position.X-width,position.Y);
                value.Position = position;
            }
            public void RemoveFromScreen(Screen screen)
            {
                screen.RemoveSprite(title);
                screen.RemoveSprite(value);
            }
            public void SetColor(Color color)
            {
                title.Color = color;
                value.Color = color;
            }
        }
    }
}
