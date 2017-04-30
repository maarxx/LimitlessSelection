using HugsLib.Source.Detour;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace LimitlessSelection
{
    static class Detours
    {

        static MethodInfo _PlaySelectionSoundFor = typeof(WorldSelector).GetMethod("PlaySelectionSoundFor", BindingFlags.NonPublic | BindingFlags.Instance);

        static FieldInfo _selected = typeof(WorldSelector).GetField("selected", BindingFlags.NonPublic | BindingFlags.Instance);
        static MethodInfo _Add = typeof(List<WorldObject>).GetMethod("Add", BindingFlags.Instance);

        [DetourMethod(typeof(WorldSelector), "Select")]
        private static void Select(this WorldSelector self, WorldObject obj, bool playSound = true)
        {
            if (obj == null)
            {
                Log.Error("Cannot select null.");
                return;
            }
            self.selectedTile = -1;
            if (!self.IsSelected(obj))
            {
                if (playSound)
                {
                    //this.PlaySelectionSoundFor(obj);
                    _PlaySelectionSoundFor.Invoke(self, new object[] { obj });
                }
                //self.selected.Add(obj);
                _Add.Invoke(_selected, new object[] { obj });
                WorldSelectionDrawer.Notify_Selected(obj);
            }
        }
    }
}
