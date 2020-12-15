using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using Verse.Sound;

namespace LimitlessSelection
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.github.harmony.rimworld.maarx.limitlessselection");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    //public void Select(object obj, bool playSound = true, bool forceDesignatorDeselect = true)
    //private void SelectInternal(object obj, bool playSound = true, bool forceDesignatorDeselect = true, bool clearShelfOnAdd = true)
    [HarmonyPatch(typeof(Selector), "Select")]
    static class Patch_Selector_Select
    {
        static MethodInfo _PlaySelectionSoundFor = typeof(Selector).GetMethod("PlaySelectionSoundFor", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo _selected_fi = typeof(Selector).GetField("selected", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo _shelved_fi = typeof(Selector).GetField("shelved", BindingFlags.NonPublic | BindingFlags.Instance);
        public  static bool Prefix(this Selector __instance, object obj, bool playSound, bool forceDesignatorDeselect)
        {
            Log.Message("Hello from Patch_Selector_Select Prefix");
            bool clearShelfOnAdd = true;
            List<Object> _selected = _selected_fi.GetValue(__instance) as List<Object>;
            List<Object> _shelved = _shelved_fi.GetValue(__instance) as List<Object>;
            if (obj == null)
            {
                Log.Error("Cannot select null.");
                return false;
            }
            Thing thing = obj as Thing;
            if (thing == null && !(obj is Zone))
            {
                Log.Error("Tried to select " + obj + " which is neither a Thing nor a Zone.");
                return false;
            }
            if (thing != null && thing.Destroyed)
            {
                Log.Error("Cannot select destroyed thing.");
                return false;
            }
            Pawn pawn = obj as Pawn;
            if (pawn != null && pawn.IsWorldPawn())
            {
                Log.Error("Cannot select world pawns.");
                return false;
            }
            if (forceDesignatorDeselect)
            {
                Find.DesignatorManager.Deselect();
            }
            if (__instance.SelectedZone != null && !(obj is Zone))
            {
                __instance.ClearSelection();
            }
            if (obj is Zone && __instance.SelectedZone == null)
            {
                __instance.ClearSelection();
            }
            Map map = (thing != null) ? thing.Map : ((Zone)obj).Map;
            for (int num = _selected.Count - 1; num >= 0; num--)
            {
                Thing thing2 = _selected[num] as Thing;
                if (((thing2 != null) ? thing2.Map : ((Zone)_selected[num]).Map) != map)
                {
                    __instance.Deselect(_selected[num]);
                }
            }
            //if (_selected.Count < 200 && !__instance.IsSelected(obj))
            if (!__instance.IsSelected(obj))
            {
                if (map != Find.CurrentMap)
                {
                    Current.Game.CurrentMap = map;
                    SoundDefOf.MapSelected.PlayOneShotOnCamera();
                    IntVec3 cell = thing?.Position ?? ((Zone)obj).Cells[0];
                    Find.CameraDriver.JumpToCurrentMapLoc(cell);
                }
                if (playSound)
                {
                    //PlaySelectionSoundFor(obj);
                    _PlaySelectionSoundFor.Invoke(__instance, new object[] { obj });
                }
                _selected.Add(obj);
                if (clearShelfOnAdd)
                {
                    _shelved.Clear();
                }
                SelectionDrawer.Notify_Selected(obj);
            }
            return false;
        }
    }
}
