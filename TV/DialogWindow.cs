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
        //-----------------------------------------------------------------------
        // dialog window
        //-----------------------------------------------------------------------
        public class DialogWindow
        {
            ScreenSprite back;
            ScreenSprite text;
            ScreenActionBar actionBar;
            string dialogText;
            float width;
            float height;
            int maxlineChars = 25;
            int maxLines = 3;
            float bottomMargin = 100;
            public bool Visible 
            { 
                get { return back.Visible; } 
                set
                {
                    back.Visible = value;
                    text.Visible = value;
                }
            }
            public Color Color
            {
                get { return text.Color; }
                set { text.Color = value; }
            }
            public DialogWindow(string dialog, Vector2 size, ScreenActionBar actionBar, bool canExit = true)
            {
                this.actionBar = actionBar;
                if(canExit) actionBar.SetActions("Next    Close");
                else actionBar.SetActions("Next");
                width = size.X;
                height = size.Y;
                dialogText = dialog;
                back = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.BottomCenter, new Vector2(0,-bottomMargin-(height/2)-10),0,size+20,Color.Black,"","SquareSimple",TextAlignment.CENTER,SpriteType.TEXTURE);
                text = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.BottomCenter, new Vector2(-width/2,-bottomMargin-(height)),1,size,Color.White,"Monospace","0123456789abcdefghijklmno\npqrstuvwxyz",TextAlignment.LEFT,SpriteType.TEXT);
                ShowTextBlock();
            }
            public void Append(string text)
            {
                //GridInfo.Echo("dialog append:" +dialogText+ " | "+text);
                if (dialogText != "") dialogText += "  ";
                dialogText += text;
                //GridInfo.Echo("dialog append:" +dialogText);
                //ShowTextBlock();
            }
            void ShowTextBlock()
            {
                //GridInfo.Echo("ShowTextBlock: "+ dialogText);
                List<string> words = dialogText.Split(' ').ToList<string>();
                string textBlock = "";
                bool firstLine = true;
                //GridInfo.Echo("words: "+ words.Count);
                int lineCount = 0;
                while(words.Count > 0 && lineCount < maxLines)
                {
                    lineCount++;
                    if(firstLine) firstLine = false;
                    else textBlock += "\n";
                    string line = "";
                    bool firstWord = true;
                    //GridInfo.Echo("building lines: "+textBlock);
                    while(words.Count > 0 && line.Length +words[0].Length < maxlineChars)
                    {
                        //GridInfo.Echo("word: " + words[0]);
                        if(firstWord) firstWord = false;
                        else line += " ";
                        //GridInfo.Echo("adding word: " + line);
                        line += words[0];
                        words.RemoveAt(0);
                    }
                    textBlock += line;
                }
                text.Data = textBlock;
                if(words.Count > 0) dialogText = words.Aggregate((i, j) => i + ' ' + j);
                else dialogText = "";
            }
            public void AddToScreen(Screen screen)
            {
                screen.AddSprite(back);
                screen.AddSprite(text);
            }
            public void RemoveFromScreen(Screen screen)
            {
                screen.RemoveSprite(back);
                screen.RemoveSprite(text);
            }
            public string HandleInput(string input)
            {
                string action = actionBar.HandleInput(input);
                //GridInfo.Echo("dialog action: "+action);
                if (action == "next")
                {
                    //GridInfo.Echo("dialog text: "+dialogText);
                    // show more text
                    if(dialogText != "") ShowTextBlock();
                    // close dialog
                    else return "close";
                }
                if(text.Data == "" || text.Data == " ") return "close";
                return action;
            }
        }
        //-----------------------------------------------------------------------
    }
}
