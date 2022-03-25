using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace LimitlessSelection;

[HarmonyPatch(typeof(Selector), "Select")]
internal static class Patch_Selector_Select
{
    private static readonly MethodInfo _PlaySelectionSoundFor =
        typeof(Selector).GetMethod("PlaySelectionSoundFor", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo _selected_fi =
        typeof(Selector).GetField("selected", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo _shelved_fi =
        typeof(Selector).GetField("shelved", BindingFlags.NonPublic | BindingFlags.Instance);

    public static bool Prefix(this Selector __instance, object obj, bool playSound, bool forceDesignatorDeselect)
    {
        Log.Message("Hello from Patch_Selector_Select Prefix");
        var _selected = _selected_fi.GetValue(__instance) as List<object>;
        var _shelved = _shelved_fi.GetValue(__instance) as List<object>;
        if (obj == null)
        {
            Log.Error("Cannot select null.");
            return false;
        }

        var thing = obj as Thing;
        if (thing == null && !(obj is Zone))
        {
            Log.Error("Tried to select " + obj + " which is neither a Thing nor a Zone.");
            return false;
        }

        if (thing is { Destroyed: true })
        {
            Log.Error("Cannot select destroyed thing.");
            return false;
        }

        if (obj is Pawn pawn && pawn.IsWorldPawn())
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

        var map = thing != null ? thing.Map : ((Zone)obj).Map;
        if (_selected == null)
        {
            return false;
        }

        for (var num = _selected.Count - 1; num >= 0; num--)
        {
            if ((_selected[num] is Thing thing2 ? thing2.Map : ((Zone)_selected[num]).Map) != map)
            {
                __instance.Deselect(_selected[num]);
            }
        }

        //if (_selected.Count < 200 && !__instance.IsSelected(obj))
        if (__instance.IsSelected(obj))
        {
            return false;
        }

        if (map != Find.CurrentMap)
        {
            Current.Game.CurrentMap = map;
            SoundDefOf.MapSelected.PlayOneShotOnCamera();
            var cell = thing?.Position ?? ((Zone)obj).Cells[0];
            Find.CameraDriver.JumpToCurrentMapLoc(cell);
        }

        if (playSound)
        {
            //PlaySelectionSoundFor(obj);
            _PlaySelectionSoundFor.Invoke(__instance, new[] { obj });
        }

        _selected.Add(obj);
        _shelved?.Clear();

        SelectionDrawer.Notify_Selected(obj);

        return false;
    }
}