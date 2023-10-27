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
        // GameSpellMenu
        //-----------------------------------------------------------------------
        public class GameSpellMenu : ScreenMenu
        {
            public GameSpellMenu(string title, float width, ScreenActionBar actionBar) : base(title, width, actionBar)
            {
                handleEditing = false;
                lowerCaseActions = false;
                SetBackgroundColor(Color.Black);
                Dictionary<string, int> spells = GameAction.GameSpells.GetSpells();
                bool inCombat = GameAction.GameVars.GetVarAs<bool>("encounter.inCombat",null,false);
                foreach (string spell in spells.Keys)
                {
                    if (GameAction.GameSpells.HasUnlockedSpell(spell))
                    {
                        if (inCombat)
                        {
                            if (GameAction.GameSpells.IsCombatSpell(spell)) AddLabel(spell);
                        }
                        else
                        {
                            if (GameAction.GameSpells.IsFieldSpell(spell)) AddLabel(spell);
                        }
                    }
                }
            }
            public override string HandleInput(string action)
            {
                action = base.HandleInput(action);
                GridInfo.Echo("GameSpellMenu:1:Action " + action);
                if (GameAction.GameSpells.CanCastSpell(action))
                {
                    GridInfo.Echo("GameSpellMenu:2:Cast " + action);
                    GameAction.GameSpells.CastSpell(action);
                    return "";
                }
                return action;
            }
        }
        //-----------------------------------------------------------------------
    }
}
