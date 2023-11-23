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
        public class ProgramScreen : Screen
        {
            ScreenSprite showInfo;
            public ProgramScreen(IMyTextSurface drawingSurface) : base(drawingSurface)
            {
                float fontSize = 0.5f;
                if(drawingSurface.SurfaceSize.Y > drawingSurface.SurfaceSize.X) fontSize = 1f;
                showInfo = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft, Vector2.Zero,fontSize,Vector2.Zero,Color.White,"White","Show Info",TextAlignment.LEFT,SpriteType.TEXT);
                AddSprite(showInfo);
            }
            public override void Draw()
            {
                showInfo.Data = "Database:\nShows: " + SceneCollection.shows.Count + "\nGames:" + SceneCollection.games.Count + "\nMedia:";
                int used = 0;
                foreach (var show in SceneCollection.scenes)
                {
                    int blocks = 0;
                    foreach (var scene in show.Value)
                    {
                        blocks += scene.Value.Count;
                    }
                    showInfo.Data += "\n" + show.Key + ": " + blocks + " blks";
                    used += blocks;
                }
                int total = SceneCollection.unused.Count + used;
                showInfo.Data += "\nBlocks:\nUsed: " + used + "/" + total + "\nAvailable:" + SceneCollection.unused.Count+"\n\nDownload:";
                foreach(var show in SceneCollection.remoteShows)
                {
                    showInfo.Data += "\n" + show.Key + ": " + show.Value.blocks + " blks";
                }
                base.Draw();
            }
        }
    }
}
