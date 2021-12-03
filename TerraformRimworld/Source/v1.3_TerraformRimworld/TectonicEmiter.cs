using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace TerraformRimworld
{
	public static class TectonicEmiterManager
	{
		public static void Init()
		{
			CreateTectonicEmiter();
		}

		private static void CreateTectonicEmiter()
		{
			Helper.CreateGeysirReplacer(ResearchProjectDefOf.TectonicTerraforming);

			ThingDef emiter = DefDatabase<ThingDef>.GetNamed(_Emiter.TectonicEmiter, false);
			if (emiter == null)
			{
				List<RecipeDef> recipes = new List<RecipeDef>();
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TectonicCustomPulse, null, ResearchProjectDefOf.TectonicEmiter, false, "Custom Tectonic Pulse", "Custom Tectonic Pulsing", "Pulsing with configured mod options."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TectonicAddRotation, null, ResearchProjectDefOf.TectonicEmiter, false, "Planet Rotation +", "Planet Rotation +", ""));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TectonicSubRotation, null, ResearchProjectDefOf.TectonicEmiter, false, "Planet Rotation -", "Planet Rotation -", ""));

				Helper.CreateDefaultEmiter(_Emiter.TectonicEmiter, typeof(TectonicEmiter), _Emiter.TectonicEmiter, ResearchProjectDefOf.TectonicEmiter, recipes, 100, 0, "Tectonic-Emiter", "");
			}
			else
				Helper.UpdateEmiter(_Emiter.TectonicEmiter);
		}
	}

	public class PlaceWorker_Geyser : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			var sourceTerrain = map.terrainGrid.TerrainAt(loc);
			if (sourceTerrain == null)
				return false;

			if (map.thingGrid.CellContains(loc, ThingCategory.Building))
				return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
			else if (TRMod.OPTION_PlaceWithoutRestrictions)
				return true;
			else if (!sourceTerrain.affordances.Contains(TerrainAffordanceDefOf.Heavy))
				return new AcceptanceReport("TerrainCannotSupport".Translate());
			else
				return true;
		}

		public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
		{
			//base.PostPlace(map, def, loc, rot);
			if (TRMod.OPTION_InstantConstruction)
			{
				ThingDef tdef = DefDatabase<ThingDef>.GetNamed(def.defName);
				TectonicGeyser.CreateGeyser(tdef, map, loc);

				Designator_Cancel cancel = new Designator_Cancel();
				cancel.DesignateSingleCell(loc);
			}
		}
	}

	public class TectonicEmiter : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
	{
		#region vars

		private Bill b;
		private CompPowerTrader cpt;
		private int efficiency;
		private bool hasDoneBill;
		private int maxRange;
		#endregion vars

		#region constructor and overrides

		public TectonicEmiter()
		{
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			HugsLibController.Instance.DistributedTicker.RegisterTickability(StartTectonicEmiter, TRMod.OPTION_EmiterTick, this);
			cpt = this.GetComp<CompPowerTrader>();
		}

		public override void Tick()
		{
			//base.Tick();
		}

		public override void TickLong()
		{
			//base.TickLong();
		}
		#endregion constructor and overrides

		#region effect

		private void DoEffect()
		{
			FleckMaker.ThrowHeatGlow(base.Position, base.Map, 1.0f);
			FleckMaker.ThrowSmoke(base.DrawPos, base.Map, 1.5f);

			b.Notify_IterationCompleted(null, null);
			b.suspended = !b.ShouldDoNow();
			hasDoneBill = true;
		}

		#endregion effect

		#region recipe func

		private void DoCustomTectonicPulse()
		{
			Helper.DoCustomTectonicPulse(base.Map);
			DoEffect();
		}

		#endregion recipe func

		public void StartTectonicEmiter()
		{
			if (cpt.PowerOn)
				cpt.PowerOutput = -cpt.Props.basePowerConsumption;
			else
				return;

			try
			{
				efficiency = Helper.GetEfficiency();
				maxRange = Helper.GetEmiterRange();
				hasDoneBill = false;
				foreach (Bill bill in BillStack.Bills)
				{
					if (bill.suspended)
						continue;
					else
						b = bill;

					if (b.recipe.defName == _Recipe.TectonicCustomPulse)
					{
						DoCustomTectonicPulse();
					}
					else if (b.recipe.defName.StartsWith(_Recipe.TectonicAddRotation))
					{
						Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + TRMod.OPTION_EmiterTick + 20);
						DoEffect();
					}
					else if (b.recipe.defName.StartsWith(_Recipe.TectonicSubRotation))
					{
						Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame - TRMod.OPTION_EmiterTick - 20);
						DoEffect();
					}

					if (hasDoneBill)
						break;
				}
			}
			catch (Exception e)
			{
				BillStack.Clear();
				if (TRMod.isDebug)
					Helper.ShowDialog(e.ToString());
			}
		}
	}

	public class TectonicGeyser : Building
	{
		public static void CreateGeyser(ThingDef def, Map map, IntVec3 loc)
		{
			ThingDef geyser = DefDatabase<ThingDef>.GetNamed("SteamGeyser", false);
			if (geyser == null)
				return;

			GenPlace.TryPlaceThing(ThingMaker.MakeThing(geyser), loc, map, ThingPlaceMode.Direct);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);

			CreateGeyser(def, map, base.Position);

			Destroy(DestroyMode.Vanish);
		}
	}
}
