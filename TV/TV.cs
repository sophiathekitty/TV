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
        public class TV : Screen
        {
            ScreenScene currentScene;
            ScreenScene idleScene;
            bool isIdle = false;
            public TV() : base(GridBlocks.TV)
            {
                GridInfo.Echo("TV: constructor");
                // setup tv stuff?
                string data = GridBlocks.GetSurfaceCustomData("TV");
                if(data.Contains("║")) SetScene(data);
            }
            public void SetScene(string data)
            {
                GridInfo.Echo("TV: SetScene");
                // remove the current scene
                if (currentScene != null)
                {
                    currentScene.RemoveFromScreen(this);
                }
                // load the scene from the data
                if (data.Contains("type:animation"))
                {
                    GridInfo.Echo("TV: SetScene: AnimatedScene");
                    currentScene = new AnimatedScene(data);
                    currentScene.AddToScreen(this);
                }
            }
            public void Play()
            {
                //GridInfo.Echo("TV: Play");
                if(currentScene != null)
                {

                    if(isIdle && idleScene != null)
                    {
                        //GridInfo.Echo("TV: Play: switch from idleScene");
                        idleScene.RemoveFromScreen(this);
                        currentScene.AddToScreen(this);
                        isIdle = false;
                    }
                    //GridInfo.Echo("TV: Play: update currentScene");
                    currentScene.Update();
                }
                //GridInfo.Echo("TV: Play: draw");
                Draw();
            }
            public void Idle()
            {
                //GridInfo.Echo("TV: Idle");
                // show idle display
                if(!isIdle)
                {
                    //GridInfo.Echo("TV: Idle: switch to idleScene");
                    if(currentScene != null)
                    {
                        //GridInfo.Echo("TV: Idle: remove current scene");
                        currentScene.RemoveFromScreen(this);
                    }
                    if(idleScene != null)
                    {
                        //GridInfo.Echo("TV: Idle: add idle scene");
                        idleScene.AddToScreen(this);
                    }
                    isIdle = true;
                }
                else
                {
                    //GridInfo.Echo("TV: Idle: update idleScene");
                    // update idle display
                    idleScene.Update();
                }
                //GridInfo.Echo("TV: Idle: draw");
                Draw();
            }
        }
    }
}
