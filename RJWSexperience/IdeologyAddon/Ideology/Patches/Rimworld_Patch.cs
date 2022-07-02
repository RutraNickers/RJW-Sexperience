﻿using HarmonyLib;
using RimWorld;
using rjw;
using RJWSexperience.Ideology.Precepts;
using System.Collections.Generic;
using Verse;

namespace RJWSexperience.Ideology.Patches
{
	[HarmonyPatch(typeof(MarriageCeremonyUtility), "Married")]
	public static class Rimworld_Patch_Marriage
	{
		public static void Postfix(Pawn firstPawn, Pawn secondPawn)
		{
			if (IdeoUtility.IsIncest(firstPawn, secondPawn, false))
			{
				if (IdeoUtility.IsIncest(firstPawn, secondPawn, true))
				{
					Find.HistoryEventsManager.RecordEvent(VariousDefOf.RJWSI_CloseRelativeMarriage.CreateEvent(firstPawn));
					Find.HistoryEventsManager.RecordEvent(VariousDefOf.RJWSI_CloseRelativeMarriage.CreateEvent(secondPawn));
				}
				Find.HistoryEventsManager.RecordEvent(VariousDefOf.Incestuos_Marriage.CreateEvent(firstPawn));
				Find.HistoryEventsManager.RecordEvent(VariousDefOf.Incestuos_Marriage.CreateEvent(secondPawn));
			}
			else
			{
				Find.HistoryEventsManager.RecordEvent(VariousDefOf.RJWSI_NonIncestuosMarriage.CreateEvent(firstPawn));
				Find.HistoryEventsManager.RecordEvent(VariousDefOf.RJWSI_NonIncestuosMarriage.CreateEvent(secondPawn));
			}
		}
	}

	[HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryRomanceChanceFactor")]
	public static class Rimworld_Patch_SecondaryRomanceChanceFactor
	{
		public static void Postfix(Pawn otherPawn, Pawn ___pawn, ref float __result)
		{
			Ideo ideo = ___pawn.Ideo;
			if (ideo?.HasPrecept(VariousDefOf.Incestuos_IncestOnly) == true && IdeoUtility.IsIncest(___pawn, otherPawn, false))
			{
				__result *= 8f;
			}
		}
	}

	[HarmonyPatch(typeof(RitualOutcomeEffectWorker_FromQuality), "GiveMemoryToPawn")]
	public static class Rimworld_Patch_GiveMemoryToPawn
	{
		public static bool Prefix(Pawn pawn, ThoughtDef memory, LordJob_Ritual jobRitual)
		{
			if (pawn.IsAnimal()) return false;

			return true;
		}
	}

	[HarmonyPatch(typeof(IdeoFoundation), "CanAdd")]
	public static class Rimworld_Patch_IdeoFoundation
	{
		public static void Postfix(PreceptDef precept, bool checkDuplicates, ref IdeoFoundation __instance, ref AcceptanceReport __result)
		{
			DefExtension_MultipleMemesRequired extension = precept.GetModExtension<DefExtension_MultipleMemesRequired>();

			if (extension == null)
				return;

			if (extension.requiredAllMemes.NullOrEmpty())
				return;

			for (int i = 0; i < extension.requiredAllMemes.Count; i++)
			{
				if (!__instance.ideo.memes.Contains(extension.requiredAllMemes[i]))
				{
					List<string> report = new List<string>();
					foreach (MemeDef meme in extension.requiredAllMemes) report.Add(meme.LabelCap);

					__result = new AcceptanceReport("RequiresMeme".Translate() + ": " + report.ToCommaList());
					return;
				}
			}
		}
	}
}
