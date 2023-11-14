﻿using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
            public static Action<string> PlayMusic;
            public static Action StopMusic;
            public static Action<string> LaunchGame;
            IMySoundBlock tvSound;
            GameRPG game;
            ScreenScene currentScene;
            AnimatedSceneEditor editor;
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
                PlayMusic = PlaySound;
                StopMusic = StopSound;
                LaunchGame = PlayGame;
                tvSound = GridBlocks.TVSound;
                // setup show
                List<string> availableShows = SceneCollection.shows.ToList();

                foreach (string show in availableShows)
                {
                    //GridInfo.Echo("TV:Show: " + show);
                    shows.Add(show);
                }
                // clone the list of shows so we can remove them as we go
                //shows = SceneCollection.shows.ToList();
                if(SceneCollection.shows.Count > 0)
                {
                    /*
                    // start the first show
                    int index = new Random().Next(availableShows.Count);
                    //GridInfo.Echo("TV:Starting:Show: " + availableShows[index]);
                    currentShow = new ShowHandler(availableShows[index], ShowDone);
                    //GridInfo.Echo("TV:Starting:Scene: " + currentShow.Current);
                    SetScene(SceneCollection.GetScene(currentShow.Current));
                    */
                    PlayRandomShow();
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
            void PlaySound(string sound)
            {
                GridInfo.Echo("TV:PlaySound: " + sound);
                if(tvSound == null) return;
                List<string> sounds = new List<string>();
                tvSound.GetSounds(sounds);
                GridInfo.Echo("TV:PlaySound: " + sounds.Count);
                if (sounds.Contains(sound))
                {
                    GridInfo.Echo("TV:PlaySound:found: " + sound);
                    tvSound.LoopPeriod = 300;
                    tvSound.SelectedSound = sound;
                    tvSound.Play();
                    tvSound.ApplyAction("PlaySound");
                }
                else StopSound();
            }
            void StopSound() { 
                GridInfo.Echo("TV:StopSound");
                if (tvSound == null) return;
                tvSound.Stop();
                tvSound.ApplyAction("StopSound");
                GridInfo.Echo("TV:StopSound:Done");
            }
            void PlayGame(string gameName)
            {
                //GridInfo.Echo("TV:Games");
                if (currentScene != null) currentScene.RemoveFromScreen(this);
                //GridInfo.Echo("TV:Games:SceneRemoved");
                if (currentShow != null) currentShow.Dispose();
                //GridInfo.Echo("TV:Games:ShowDisposed");
                currentShow = null;
                menus.Hide();
                // for now we're going to load the first game in the games list to see if it works
                game = new GameRPG(gameName, actionBar);
                game.AddToScreen(this);
                //GridInfo.Echo("TV:Games:GameAddedToScreen");
                game.Update();
            }
            // hand show done event
            void ShowDone(string show)
            {
                //GridInfo.Echo("TV:ShowDone(" + show+")");
                if (currentShow != null)
                {
                    currentShow.Dispose();
                }
                currentShow = new ShowHandler(show, ShowDone);
                SetScene(SceneCollection.GetScene(currentShow.Current));
                //GridInfo.Echo("TV:ShowDone:Scene: " + currentShow.Current);

            }
            void PlayRandomShow()
            {
                //GridInfo.Echo("TV:PlayRandomShow shows: "+shows.Count);
                // select a random show from the shows list
                if (shows.Count > 0)
                {
                    int index = new Random().Next(shows.Count);
                    currentShow = new ShowHandler(shows[index], ShowDone);
                    SetScene(SceneCollection.GetScene(currentShow.Current));
                    shows.RemoveAt(index);
                    //GridInfo.Echo("TV:PlayRandomShow: " + index + " / " + shows.Count);
                }
                else
                {
                    // no more shows to play
                    // restart the show list
                    List<string> availableShows = SceneCollection.shows.ToList();
                    shows.Clear();
                    foreach (string show in availableShows)
                    {
                        //GridInfo.Echo("TV:Show: " + show);
                        if(show != currentShow.Name) shows.Add(show);
                    }
                    if(shows.Count > 0) PlayRandomShow();
                }
            }
            //
            // set the scene to the given data
            //
            public void SetScene(string data)
            {
                // remove the current scene
                if (currentScene != null)
                {
                    currentScene.RemoveFromScreen(this);
                }
                // load the scene from the data
                if (data.Contains("type:animation"))
                {
                    currentScene = new AnimatedScene(data,OnAnimatedSceneDone);
                    currentScene.AddToScreen(this);
                }
                else GridInfo.Echo("TV:Scene:Error: data.Length=" + data.Length);
                // if menu is visible we need to move it to the top of the render queue
                if (menuVisible) {                     
                    actionBar.RemoveFromScreen(this);
                    menus.Hide();
                    actionBar.AddToScreen(this);
                    menus.Show();
                }
            }
            //
            // load the sccene editor
            //
            public void LoadEditor()
            {
                GridInfo.SetVar("EditingSprite", "-1");
                if (currentScene != null)
                {
                    currentScene.RemoveFromScreen(this);
                }
                string data = GridBlocks.GetSurfaceCustomData("TV");
                if (data.Contains("type:animation"))
                {
                    currentScene = editor = new AnimatedSceneEditor(data);
                    currentScene.AddToScreen(this);
                }
                actionBar.RemoveFromScreen(this);
                menus.Hide();
                actionBar.AddToScreen(this);
                menus.Show();
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
                if(game != null)
                {
                    game.Update();
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
                        //GridInfo.Echo("TV:Show:Done: " + currentShow.Name);
                        PlayRandomShow();
                    }
                    else
                    {
                        SetScene(SceneCollection.GetScene(currentShow.Next()));
                        //GridInfo.Echo("TV:Next:Scene: " + currentShow.Current);

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
                    menus.SetMenu("main");
                    menus.Hide();
                    actionBar.RemoveFromScreen(this);
                    StopSound();
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
                    if (game != null) action = game.HandleInput(input);
                    else if (menuVisible) action = menus.HandleInput(input);
                    else action = actionBar.HandleInput(input);
                    if (action == "back")
                    {
                        actionBar.RemoveFromScreen(this);
                        menus.Hide();
                        barVisible = false;
                        menuVisible = false;
                        barTimeout = 0;
                        return "";
                    }
                    else if (action == "editor")
                    {
                        LoadEditor();
                        AnimatedScene scene = (AnimatedScene)currentScene;
                        if (scene != null) menus.SetMenu("editor", scene.animatedSprites.Count);
                    }
                    else if(action == "close sprite")
                    {
                        AnimatedScene scene = (AnimatedScene)currentScene;
                        if (scene != null) menus.SetMenu("editor", scene.animatedSprites.Count);
                    }
                    else if(action == "close")
                    {
                        OnAnimatedSceneDone();
                    }
                    else if(action == "save scene")
                    {
                        if(editor != null)
                        {
                            editor.Save();
                        }
                        menus.SetMenu("main");
                        currentShow.Dispose();
                        currentShow = new ShowHandler(SceneCollection.shows[0], ShowDone);
                        SetScene(SceneCollection.GetScene(currentShow.Current));
                        return "";
                    }
                    else if(action == "games")
                    {
                        //GridInfo.Echo("TV:Games");
                        if(currentScene != null) currentScene.RemoveFromScreen(this);
                        //GridInfo.Echo("TV:Games:SceneRemoved");
                        if (currentShow != null) currentShow.Dispose();
                        //GridInfo.Echo("TV:Games:ShowDisposed");
                        currentShow = null;
                        menus.Hide();
                        // for now we're going to load the first game in the games list to see if it works
                        if (SceneCollection.games.Count > 0) GridInfo.Echo("TV:Games:Scene: " + SceneCollection.games[0]);
                        if (SceneCollection.games.Count > 0) game = new GameRPG(SceneCollection.games[0], actionBar);
                        //GridInfo.Echo("TV:Games:GameCreated");
                        //game.LoadMap("throne",2,6);
                        //GridInfo.Echo("TV:Games:MapLoaded");
                        game.AddToScreen(this);
                        //GridInfo.Echo("TV:Games:GameAddedToScreen");
                        game.Update();
                        return "";
                    }
                    else if(action == "quit game")
                    {
                        GridInfo.Echo("TV:0:QuitGame");
                        StopSound();
                        GridInfo.Echo("TV:1:StopSound");
                        game.RemoveFromScreen(this);
                        GridInfo.Echo("TV:2:GameRemovedFromScreen");
                        game = null;
                        GridInfo.Echo("TV:3:GameDisposed");
                        actionBar.RemoveFromScreen(this);
                        GridInfo.Echo("TV:4:ActionBarRemovedFromScreen");
                        menus.Hide();
                        GridInfo.Echo("TV:5:MenusHidden");
                        barVisible = false;
                        GridInfo.Echo("TV:6:BarHidden");
                        menuVisible = false;
                        GridInfo.Echo("TV:7:MenuHidden");
                        barTimeout = 0;
                        GridInfo.Echo("TV:8:BarTimeoutReset");
                        PlayRandomShow();
                        GridInfo.Echo("TV:9:PlayRandomShow");
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
