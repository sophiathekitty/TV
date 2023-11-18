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
        public class TextGridKeyboard
        {
            ScreenSprite background;
            ScreenSprite curser;
            ScreenSprite TextField;
            ScreenSprite Label;
            List<ScreenSprite> keys = new List<ScreenSprite>();
            ScreenActionBar actionBar;
            string variableName = "";
            int curserIndex = 0;
            public TextGridKeyboard(ScreenActionBar actionBar, string header, string varName = "")
            {
                this.actionBar = actionBar;
                actionBar.SetActions("< v ^ > Select");
                int keySize = 30;
                variableName = varName;
                background = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, new Vector2(0, 0), 0f, new Vector2(keySize*10+90, keySize*9+30), Color.Black, "", "SquareSimple", TextAlignment.CENTER, SpriteType.TEXTURE);
                Vector2 pos = new Vector2(keySize*-5, keySize*-4);
                Label = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, pos + new Vector2(-20, -5), 1f, Vector2.Zero, Color.White, "Monospace", header, TextAlignment.LEFT, SpriteType.TEXT);
                TextField = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, pos + new Vector2((header.Length*20)-20, -8), 1.25f, Vector2.Zero, Color.White, "White", "", TextAlignment.LEFT, SpriteType.TEXT);
                pos.Y += keySize;
                curser = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, pos+new Vector2(0,15),0f,new Vector2(keySize,30),Color.White,"", "SquareHollow",TextAlignment.CENTER,SpriteType.TEXTURE);
                string[] keyNames = new string[] {
                    "A", "B", "C", "D", "E", "F", "G", "H", "I", "J",
                    "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T",
                    "U", "V", "W", "X", "Y", "Z", "a", "-", "_", "@",
                    "a", "b", "c", "d", "e", "f", "g", "h", "i", "j",
                    "k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
                    "u", "v", "w", "x", "y", "z", "!", "1", "2", "3",
                    "4", "5", "6", "7", "8", "9", "0", ".", ",", "?",
                    "(", ")", " ",  " ", "Del", "Done" };
                int col = 0;
                for (int i = 0; i < keyNames.Length; i++)
                {
                    keys.Add(new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center, pos, 1f, Vector2.Zero, Color.White, "Monospace", keyNames[i], TextAlignment.CENTER, SpriteType.TEXT));
                    pos.X += keySize * keyNames[i].Length;
                    if (++col == 10)
                    {
                        pos.X = keySize * -5;
                        pos.Y += keySize;
                        col = 0;
                    }
                }
            }
            public void AddToScreen(Screen screen)
            {
                screen.AddSprite(background);
                screen.AddSprite(Label);
                screen.AddSprite(TextField);
                screen.AddSprite(curser);
                foreach (ScreenSprite key in keys)
                {
                    screen.AddSprite(key);
                }
            }
            public void RemoveFromScreen(Screen screen)
            {
                screen.RemoveSprite(background);
                screen.RemoveSprite(Label);
                screen.RemoveSprite(TextField);
                screen.RemoveSprite(curser);
                foreach (ScreenSprite key in keys)
                {
                    screen.RemoveSprite(key);
                }
            }
            public string HandleInput(string input)
            {
                string action = actionBar.HandleInput(input);
                //GridInfo.Echo("TextGridKeyboard:HandleInput:action: " + action);
                if (action == "<") curserIndex--;
                else if (action == ">") curserIndex++;
                else if (action == "^") curserIndex -= 10;
                else if (action == "v") curserIndex += 10;
                else if (action == "select")
                {
                    action = keys[curserIndex].Data;
                    if (action == "Del")
                    {
                        if (TextField.Data.Length > 0) TextField.Data = TextField.Data.Remove(TextField.Data.Length - 1, 1);
                        return "";
                    }
                    else if(action == "Done")
                    {
                        if (variableName != "") GridInfo.SetVar(variableName, TextField.Data);
                        return "text_results║" + TextField.Data;
                    }
                    TextField.Data += keys[curserIndex].Data;
                    action = "";
                }
                if(curserIndex < 0) curserIndex = 0;
                if(curserIndex > keys.Count-1) curserIndex = keys.Count-1;
                curser.Position = keys[curserIndex].Position + new Vector2(0,15);
                curser.Size = new Vector2(keys[curserIndex].Data.Length*30, 30);
                if ("<^v>".Contains(action)) return "";
                return action;
            }
        }
    }
}
