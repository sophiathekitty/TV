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
        // show handler - handles generating shows
        //----------------------------------------------------------------------
        public class ShowHandler
        {
            List<string> playlist = new List<string>();
            PlaylistBuilder builder = new PlaylistBuilder();
            int index = 0;
            string name = "";
            string title = "";
            public string Name { get { return name; } }
            public string Current { get { return name+"."+playlist[index]; } }
            public string Next()
            {
                if (++index >= playlist.Count)
                {
                    index = playlist.Count - 1;
                    //if (OnShowDone != null) OnShowDone(name);
                    //return "";
                }
                return name + "." + playlist[index];
            }
            public bool IsDone { get { return index+1 >= playlist.Count; } }
            //public Action<string> OnShowDone;
            public ShowHandler(string show, Action<string> onDone = null)
            {
                GridInfo.Echo("ShowHandler: constructor: " + show);
                //OnShowDone = onDone;
                name = show;
                string[] showData = SceneCollection.GetScene(show, "Main", 0, true).Split('║');
                foreach (string s in showData)
                {
                    if (s.Contains("type:show")) ParseShowData(s);
                    else if (s.Contains("type:playlist")) builder.ParsePlaylist(s);
                    else if (s.Contains("type:segment")) builder.AddSegment(s);
                }
                playlist = builder.BuildPlaylist();
            }
            // parse show data from string (type:show,title:The Show Name,name:showname)
            void ParseShowData(string showData)
            {
                string[] data = showData.Split(',');
                foreach (string d in data)
                {
                    string[] kv = d.Split(':');
                    if (kv[0] == "title") title = kv[1];
                }
            }
            // dispose
            public void Dispose()
            {
                playlist.Clear();
                builder.playlist.Clear();
                builder.segments.Clear();
                //OnShowDone = null;
            }
            //----------------------------------------------------------------------
            // playlist builder
            //----------------------------------------------------------------------
            class PlaylistBuilder
            {
                public List<string> playlist = new List<string>();
                public Dictionary<string, List<string>> segments = new Dictionary<string, List<string>>();
                public void ParsePlaylist(string data)
                {
                    playlist.Clear();
                    string[] dataParts = data.Split('═');
                    if (dataParts.Length == 2 && dataParts[0] == "type:playlist")
                    {
                        playlist = dataParts[1].Split(':').ToList();
                    }
                }
                public void AddSegment(string data)
                {
                    string[] dataParts = data.Split('═');
                    if (dataParts.Length == 2 && dataParts[0].Contains("type:segment"))
                    {
                        string[] segmentInfo = dataParts[0].Split(',');
                        string segmentName = "";
                        foreach (string s in segmentInfo)
                        {
                            string[] kv = s.Split(':');
                            if (kv[0] == "name") segmentName = kv[1];
                        }
                        if (segmentName != "")
                        {
                            if (!segments.ContainsKey(segmentName)) segments.Add(segmentName, new List<string>());
                            segments[segmentName].Add(dataParts[1]);
                        }
                    }
                    // build a playlist from the segments

                }
                // build a playlist from the segments
                public List<string> BuildPlaylist()
                {
                    List<string> list = new List<string>();
                    foreach (string s in playlist)
                    {
                        if (segments.ContainsKey(s))
                        {
                            // add a random segment from the list
                            list.AddRange(segments[s][new Random().Next(segments[s].Count)].Split(':'));
                        } else
                        {
                            list.Add(s);
                        }
                    }
                    return list;
                }
            }
            //----------------------------------------------------------------------
        }
        //----------------------------------------------------------------------
    }
}
