﻿using Sandbox.Game.EntityComponents;
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
        // TV - main class for the TV (extends Screen)
        //----------------------------------------------------------------------
        public class TV : Screen
        {
            ScreenScene currentScene;
            ScreenScene idleScene;
            ScreenActionBar actionBar;
            TVMenus menus;
            bool menuVisible = false;
            bool barVisible = false;
            int barTimeout = 0;
            int barTimeoutMax = 300;
            bool isIdle = false;
            ShowHandler currentShow;
            List<string> shows = new List<string>();
            //
            // constructor
            //
            public TV() : base(GridBlocks.TV)
            {
                // setup show
                foreach (string show in SceneCollection.shows)
                {
                    GridInfo.Echo("TV:Show: " + show);
                }
                // clone the list of shows so we can remove them as we go
                shows = SceneCollection.shows.ToList();
                if(SceneCollection.shows.Count > 0)
                {
                    // start the first show
                    GridInfo.Echo("TV:Starting:Show: " + SceneCollection.shows[0]);
                    currentShow = new ShowHandler(SceneCollection.shows[0], ShowDone);
                    GridInfo.Echo("TV:Starting:Scene: " + currentShow.Current);
                    SetScene(SceneCollection.GetScene(currentShow.Current));
                } 
                else
                {
                    // setup tv stuff?
                    string data = GridBlocks.GetSurfaceCustomData("TV");
                    if (data.Contains("║")) SetScene(data);
                }
                // setup idle scene
                idleScene = new IdleScene(Size);
                // setup menus
                actionBar = new ScreenActionBar(5, Size.X, Color.White, "Up Down Select  Back");
                menus = new TVMenus(this, actionBar);
            }
            // hand show done event
            void ShowDone(string show)
            {
                GridInfo.Echo("TV:ShowDone: " + show);
                if (currentShow != null)
                {
                    currentShow.Dispose();
                }
                currentShow = new ShowHandler(show, ShowDone);
                SetScene(SceneCollection.GetScene(currentShow.Current));
                GridInfo.Echo("TV:ShowDone:Scene: " + currentShow.Current);

            }
            //
            // set the scene to the given data
            //
            public void SetScene(string data)
            {
                // if the current show is exists and is done just return
                //if (currentShow != null && currentShow.IsDone) return;
                //GridInfo.Echo("TV:SetScene");
                // remove the current scene
                if (currentScene != null)
                {
                    currentScene.RemoveFromScreen(this);
                }
                // load the scene from the data
                if (data.Contains("type:animation"))
                {
                    //GridInfo.Echo("TV:SetScene:animation");
                    currentScene = new AnimatedScene(data,OnAnimatedSceneDone);
                    currentScene.AddToScreen(this);
                }
            }
            //
            // play the current scene
            //
            public void Play()
            {
                if(currentScene != null)
                {
                    if(isIdle && idleScene != null)
                    {
                        idleScene.RemoveFromScreen(this);
                        currentScene.AddToScreen(this);
                        isIdle = false;
                    }
                    currentScene.Update();
                    
                }
                PreDraw();
            }
            void OnAnimatedSceneDone()
            {
                // play the next scene
                if(currentShow != null)
                {
                    if(currentShow.IsDone)
                    {
                        // restart the show
                        GridInfo.Echo("TV:Show:Done: " + currentShow.Name);
                        currentShow.Dispose();
                        currentShow = new ShowHandler(SceneCollection.shows[0], ShowDone);
                        SetScene(SceneCollection.GetScene(currentShow.Current));
                    }
                    else
                    {
                        SetScene(SceneCollection.GetScene(currentShow.Next()));
                        GridInfo.Echo("TV:Next:Scene: " + currentShow.Current);

                    }
                }
                   
            }
            // show the idle display (tv off or ambient display)
            public void Idle()
            {
                // show idle display
                if(!isIdle)
                {
                    if(currentScene != null) currentScene.RemoveFromScreen(this);
                    if(idleScene != null) idleScene.AddToScreen(this);
                    isIdle = true;
                }
                else
                {
                    if(idleScene != null) idleScene.Update();
                }

                PreDraw();
            }
            void PreDraw()
            {
                if (barVisible && !menuVisible)
                {
                    barTimeout++;
                    if (barTimeout >= barTimeoutMax)
                    {
                        actionBar.RemoveFromScreen(this);
                        barVisible = false;
                        barTimeout = 0;
                    }
                }
                Draw();
            }
            // handle input from the remote control
            public string HandleInput(string input)
            {
                if(input == "") return "";
                if (barVisible)
                {
                    string action = "";
                    if(menuVisible) action = menus.HandleInput(input);
                    else action = actionBar.HandleInput(input);
                    if(action == "back")
                    {
                        actionBar.RemoveFromScreen(this);
                        menus.Hide();
                        barVisible = false;
                        menuVisible = false;
                        barTimeout = 0;
                        return "";
                    }
                    return action;
                }
                actionBar.AddToScreen(this);
                menus.Show();       
                barVisible = true;
                menuVisible = true;
                return "";
            }
        }
        //----------------------------------------------------------------------
    }
}
