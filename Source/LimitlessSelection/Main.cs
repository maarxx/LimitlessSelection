using System.Reflection;
using HarmonyLib;
using Verse;

namespace LimitlessSelection;

[StaticConstructorOnStartup]
internal class Main
{
    static Main()
    {
        var harmony = new Harmony("com.github.harmony.rimworld.maarx.limitlessselection");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}

//public void Select(object obj, bool playSound = true, bool forceDesignatorDeselect = true)
//private void SelectInternal(object obj, bool playSound = true, bool forceDesignatorDeselect = true, bool clearShelfOnAdd = true)