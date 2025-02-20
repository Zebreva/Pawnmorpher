﻿// HediffDefPatches.cs created by Iron Wolf for Pawnmorph on 08/24/2021 7:44 PM
// last updated 08/24/2021  7:44 PM

using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    
    static class HediffDefPatches
    {
        [NotNull] private static readonly Dictionary<HediffDef, bool> _immunityCache = new Dictionary<HediffDef, bool>();

        [HarmonyPatch(typeof(HediffDef), nameof(HediffDef.PossibleToDevelopImmunityNaturally))]
        private static class HediffImmunityPatch
        {
            private static bool Prefix(HediffDef __instance, ref bool __result)
            {
                if (_immunityCache.TryGetValue(__instance, out bool chk))
                {
                    __result = chk;
                    return false;
                }


                var hediffCompProperties_Immunizable = __instance.CompProps<HediffCompProperties_Immunizable>();
                if (hediffCompProperties_Immunizable != null
                 && (hediffCompProperties_Immunizable.immunityPerDayNotSick > 0f
                  || hediffCompProperties_Immunizable.immunityPerDaySick > 0f))
                    __result = true;
                else
                    __result = false;

                _immunityCache[__instance] = __result;
                return false;
            }
        }
    }
}