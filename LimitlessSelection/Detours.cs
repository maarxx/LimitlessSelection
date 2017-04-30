using HugsLib.Source.Detour;
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
    static class Detours
    {

        static MethodInfo _PlaySelectionSoundFor = typeof(Selector).GetMethod("PlaySelectionSoundFor", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo _selected = typeof(Selector).GetField("selected", BindingFlags.NonPublic | BindingFlags.Instance);

        [DetourMethod(typeof(Selector), "Select")]
        private static void Select(this Selector self, object obj, bool playSound = true, bool forceDesignatorDeselect = true)
        {

            IntVec3 intVec3;
            if (obj == null)
            {
                Log.Error("Cannot select null.");
                return;
            }
            Thing thing = obj as Thing;
            if (thing == null && !(obj is Zone))
            {
                Log.Error(string.Concat("Tried to select ", obj, " which is neither a Thing nor a Zone."));
                return;
            }
            if (thing != null && thing.Destroyed)
            {
                Log.Error("Cannot select destroyed thing.");
                return;
            }
            Pawn pawn = obj as Pawn;
            if (pawn != null && pawn.IsWorldPawn())
            {
                Log.Error("Cannot select world pawns.");
                return;
            }
            if (forceDesignatorDeselect)
            {
                Find.DesignatorManager.Deselect();
            }
            if (self.SelectedZone != null && !(obj is Zone))
            {
                self.ClearSelection();
            }
            if (obj is Zone && self.SelectedZone == null)
            {
                self.ClearSelection();
            }
            Map map = (thing == null ? ((Zone)obj).Map : thing.Map);

            List<object> mySelected = (List<object>)_selected.GetValue(self);

            //for (int i = this.selected.Count - 1; i >= 0; i--)
            for (int i = mySelected.Count - 1; i >= 0; i--)
            {
                //Thing item = this.selected[i] as Thing;
                Thing item = mySelected[i] as Thing;

                //if ((item == null ? ((Zone)this.selected[i]).Map : item.Map) != map)
                if ((item == null ? ((Zone)mySelected[i]).Map : item.Map) != map)
                {
                    //self.Deselect(this.selected[i]);
                    self.Deselect((Thing)item);
                }
            }

            /*
            if (this.selected.Count >= 80)
            {
                return;
            }
            */

            if (!self.IsSelected(obj))
            {
                if (map != Current.Game.VisibleMap)
                {
                    Current.Game.VisibleMap = map;
                    SoundDefOf.MapSelected.PlayOneShotOnCamera();
                    intVec3 = (thing == null ? ((Zone)obj).Cells[0] : thing.Position);
                    Find.CameraDriver.JumpTo(intVec3);
                }
                if (playSound)
                {
                    //this.PlaySelectionSoundFor(obj);
                    _PlaySelectionSoundFor.Invoke(self, new object[] { obj });
                }
                //this.selected.Add(obj);
                mySelected.Add(obj);
                SelectionDrawer.Notify_Selected(obj);
            }

        }
    }
}
