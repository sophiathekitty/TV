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
                //GridInfo.Echo("ShowHandler: constructor: " + show);
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
                public List<List<string>> stockDialog = new List<List<string>>();
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
                        bool isStock = false;
                        int stockCount = 0;
                        foreach (string s in segmentInfo)
                        {
                            string[] kv = s.Split(':');
                            if (kv[0] == "name") segmentName = kv[1];
                            else if (kv[0] == "stock")
                            {
                                int.TryParse(kv[1], out stockCount);
                                isStock = true;
                            }
                        }
                        if (segmentName != "")
                        {
                            if (!segments.ContainsKey(segmentName)) segments.Add(segmentName, new List<string>());
                            // if not stock, add the segment
                            if (!isStock) segments[segmentName].Add(dataParts[1]);
                            else
                            {
                                // if stock, add the segment multiple times
                                List<string> dialog = dataParts[1].Split(':').ToList<string>();
                                stockDialog.Add(dialog);
                                string segment = "";
                                bool first = true;
                                for (int i = 0; i < stockCount; i++)
                                {
                                    if(first) first = false;
                                    else segment += ":";
                                    segment += "Stock";
                                }
                                segments[segmentName].Add(segment);
                            }
                        }
                        // if there's stock dialog set that to the SceneCollection
                        if (stockDialog.Count > 0)
                        {
                            // set to a random list of stock dialog
                            SceneCollection.StockDialog = stockDialog[new Random().Next(stockDialog.Count)];
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
