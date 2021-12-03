using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TerraformRimworld
{
	public static class ReplicatorPrototypManager
	{
		public static List<ThingDef> listOfFilth;

		public static void CreateReplicatorPrototyp()
		{
			ThingDef emiter = DefDatabase<ThingDef>.GetNamed(_Emiter.TerraEmiter, false);
			if (emiter == null)
			{
				List<ThingCategoryDef> lfilth = new List<ThingCategoryDef>();
				lfilth.Add(ThingCategories.Filth);

				List<ThingCategoryDef> lcat = new List<ThingCategoryDef>();
				foreach (ThingCategoryDef cat in DefDatabase<ThingCategoryDef>.AllDefs)
				{
					if (cat != null && cat.defName != null && cat.childThingDefs != null && cat.childThingDefs.Count > 0 && !cat.defName.Contains("Corpse"))
						lcat.Add(cat);
				}

				List<RecipeDef> recipes = new List<RecipeDef>();
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.ReplPlaceItem, lcat, ResearchProjectDefOf.ReplicatorComplete, false, "Replicate Item and place in Stockpile", "Replicating Item", "Selected Item will be replicated and placed in nearest Stockpile."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.ReplAbsorbItem, lcat, ResearchProjectDefOf.ReplicatorComplete, false, "Absorb Item from Stockpile", "Absorbing Item", "Selected Item will be absorbed from Stockpile."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.ReplPlaceLocalFilth, lfilth, ResearchProjectDefOf.ReplicatorNanites, false, "Place Filth in Area", "Placing Filth in Area", "Places Filth in 'Terraforming'-Area around"));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.ReplPlaceGlobalFilth, lfilth, ResearchProjectDefOf.ReplicatorNanites, false, "Place Filth in Biome", "Placing Filth in Biome", "Places Filth in Biome. Placing starts around the emiter, then continue random."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.ReplCleanLocal, lfilth, ResearchProjectDefOf.ReplicatorNanites, false, "Remove Filth in Area", "Removing Filth in Area", "Removes Filth in 'Terraforming'-Area around."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.ReplCleanGlobal, lfilth, ResearchProjectDefOf.ReplicatorNanites, false, "Remove Filth in Biome", "Removing Filth in Biome", "Removes Filth in Biome. Removal starts around the emiter, then continue random."));
				for (int i = 0; i < lcat.Count - 3; i++)
				{
					if (lcat[i].defName.ToLower() == "animals")
						continue;
					recipes.Add(Helper.CreateDefaultRecipe(_Recipe.ReplPlaceItem + i.ToString(), new List<ThingCategoryDef>() { lcat[i] }, ResearchProjectDefOf.ReplicatorComplete, false, "Replicate " + lcat[i].LabelCap, "Replicate " + lcat[i].LabelCap, "Selected Item will be replicated and placed in nearest Stockpile."));
				}
				Helper.CreateDefaultEmiter(_Emiter.ReplicatorPrototyp, typeof(ReplicatorPrototyp), _Emiter.ReplicatorPrototyp, ResearchProjectDefOf.ReplicatorNanites, recipes, 0.0f, 1.0f, "Replicator-Prototyp", "");
			}
			else
				Helper.UpdateEmiter(_Emiter.ReplicatorPrototyp);
		}

		public static void Init()
		{
			listOfFilth = new List<ThingDef>();
			foreach (ThingDef td in DefDatabase<ThingDef>.AllDefs)
			{
				if (td != null && td.defName.StartsWith("Filth"))
				{
					listOfFilth.Add(td);
					ThingCategories.AddToThingCategory(ThingCategories.Filth, td);
				}
			}
			ThingCategories.Filth.ResolveReferences();
			ThingCategories.Filth.PostLoad();

			CreateReplicatorPrototyp();
			if (TRMod.isDebug)
				Helper.ShowDialog("Filth : " + Helper.ListToString(listOfFilth));
		}
	}

	public class ReplicatorPrototyp : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
	{
		#region vars

		private Bill b;

		private CompHeatPusher chp;

		//CompPowerTrader cpt;
		private CompRefuelable cpr;
		private int efficiency;
		private bool hasDoneBill;
		private int maxRange;
		#endregion vars

		#region constructor and overrides

		public ReplicatorPrototyp()
		{
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			HugsLibController.Instance.DistributedTicker.RegisterTickability(StartReplicator, TRMod.OPTION_EmiterTick, this);
			//cpt = this.GetComp<CompPowerTrader>();
			chp = base.GetComp<CompHeatPusher>();
			cpr = base.GetComp<CompRefuelable>();
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

		private void DoEffects(IntVec3 posSpawn, bool absorbingEffects = false)
		{
			Vector3 vecSpawn = new Vector3(posSpawn.x, posSpawn.y, posSpawn.z);
			if (absorbingEffects)
			{
				FleckMaker.ThrowMicroSparks(vecSpawn, base.Map);
				FleckMaker.ThrowDustPuffThick(vecSpawn, base.Map, 1.0f, Color.black);
			}
			else
			{
				FleckMaker.ThrowFireGlow(vecSpawn, base.Map, 1.0f);
				FleckMaker.ThrowSmoke(vecSpawn, base.Map, 1.5f);
				FleckMaker.ThrowDustPuffThick(vecSpawn, base.Map, 2.0f, Color.black);
			}

			FleckMaker.ThrowHeatGlow(base.Position, base.Map, 1.0f);
			FleckMaker.ThrowSmoke(base.DrawPos, base.Map, 1.5f);

			b.Notify_IterationCompleted(null, null);
			bool doSuspend = true;
			if (b.recipe.defName == _Recipe.ReplPlaceItem)
			{
				Bill_Production bp = b as Bill_Production;
				doSuspend = !(bp.repeatMode == BillRepeatModeDefOf.TargetCount);
			}
			b.suspended = (!b.ShouldDoNow() && doSuspend);
			cpr.ConsumeFuel((cpr.Props.fuelConsumptionRate - 10.0f + TRMod.OPTION_EmiterBaseConsumption) / (float)efficiency);
			chp.CompTick();
			hasDoneBill = true;
		}

		#endregion effect

		#region recipe func

		#region place filth

		private void AddFilth(List<ThingDef> listOfFilth, bool global)
		{
			ThingDef ThingToSpawn = Helper.GetRandomListElement(listOfFilth);
			if (ThingToSpawn == null)
				return;

			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestPlaceFilth(listOfFilth, range, aTerraforming);
				if (CanPlaceFilth(posSpawn, listOfFilth, aTerraforming))
				{
					GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingToSpawn), posSpawn, base.Map, ThingPlaceMode.Near);
					DoEffects(posSpawn);
					break;
				}
			}
		}

		private bool CanPlaceFilth(IntVec3 posSpawn, List<ThingDef> l, Area aTerraforming)
		{
			if (aTerraforming == null || aTerraforming[posSpawn])
				return !l.Contains(posSpawn.GetFilthFromPos(base.Map));
			else
				return false;
		}

		private IntVec3 GetNearestPlaceFilth(List<ThingDef> l, int range, Area aTerraforming)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanPlaceFilth(posSpawn, l, aTerraforming));
			return posSpawn;
		}
		#endregion place filth

		#region remove filth

		private bool CanRemoveFilth(IntVec3 posSpawn, List<ThingDef> l, Area aTerraforming)
		{
			if (aTerraforming == null || aTerraforming[posSpawn])
				return l.Contains(posSpawn.GetFilthFromPos(base.Map));
			else
				return false;
		}

		private IntVec3 GetNearestRemoveFilth(List<ThingDef> l, int range, Area aTerraforming)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanRemoveFilth(posSpawn, l, aTerraforming));
			return posSpawn;
		}

		private void RemoveFilth(List<ThingDef> listOfFilth, bool global)
		{
			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestRemoveFilth(listOfFilth, range, aTerraforming);
				if (CanRemoveFilth(posSpawn, listOfFilth, aTerraforming))
				{
					FilthMaker.RemoveAllFilth(posSpawn, base.Map);
					DoEffects(posSpawn, true);
					break;
				}
			}
		}

		#endregion remove filth

		#region replicate

		private bool CanPlaceInStockpile(IntVec3 posSpawn, ThingDef t)
		{
			Zone zone = base.Map.zoneManager.ZoneAt(posSpawn);
			if (zone != null && zone.GetType() == typeof(Zone_Stockpile) && Helper.CheckPlaceIsFree(t, posSpawn, base.Map, true))
				return true;
			else
				return false;
		}

		private IntVec3 GetNearestPlaceInStockpile(ThingDef t, int range)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanPlaceInStockpile(posSpawn, t));
			return posSpawn;
		}

		private void ReplicateItem(List<ThingDef> listOfItems, Bill b)
		{
			IntVec3 posSpawn = base.Position;
			ThingDef t = Helper.GetRandomListElement(listOfItems);
			if (t == null || t.defName.StartsWith("Unnamed"))
				return;

			Bill_Production bp = b as Bill_Production;
			if (bp.repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				if (b.recipe.products != null)
					b.recipe.products.Clear();
				else
					b.recipe.products = new List<ThingDefCountClass>();
				foreach (ThingDef tx in listOfItems)
					b.recipe.products.Add(new ThingDefCountClass(tx, 1));

				if (!b.ShouldDoNow())
					return;
			}

			ThingDef stuff = null;
			if (t.stuffCategories != null && t.stuffCategories.Count > 0)
				stuff = Rand.Element<ThingDef>(ThingDefOf.WoodLog, ThingDefOf.Steel);

			int range = maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				posSpawn = GetNearestPlaceInStockpile(t, range);
				if (CanPlaceInStockpile(posSpawn, t))
				{
					GenPlace.TryPlaceThing(ThingMaker.MakeThing(t, stuff), posSpawn, base.Map, ThingPlaceMode.Near);
					DoEffects(posSpawn);
					break;
				}
			}
		}

		#endregion replicate

		#region absorb

		private void AbsorbItem(List<ThingDef> listOfItems)
		{
			Thing thingToDestroy = null;
			int range = maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestRemoveInStockpile(listOfItems, range, out thingToDestroy);
				if (CanRemoveInStockpile(posSpawn, listOfItems, out thingToDestroy))
				{
					if (thingToDestroy != null)
						thingToDestroy.Destroy(DestroyMode.Vanish);
					DoEffects(posSpawn, true);
					break;
				}
			}
		}

		private bool CanRemoveInStockpile(IntVec3 posSpawn, List<ThingDef> l, out Thing thingToDestroy)
		{
			thingToDestroy = null;
			Zone zone = base.Map.zoneManager.ZoneAt(posSpawn);
			if (zone != null && zone.GetType() == typeof(Zone_Stockpile))
			{
				thingToDestroy = FindThingToDestroyItem(posSpawn, l);
				return thingToDestroy != null;
			}
			else
				return false;
		}

		private Thing FindThingToDestroyItem(IntVec3 posSpawn, List<ThingDef> l)
		{
			List<Thing> lt = posSpawn.GetThingList(base.Map);
			Thing foundThingToDestroy = null;
			foreach (Thing thing in lt)
			{
				if (thing != null && thing.def != null && l.Contains(thing.def))
				{
					foundThingToDestroy = thing;
					break;
				}
			}
			return foundThingToDestroy;
		}
		private IntVec3 GetNearestRemoveInStockpile(List<ThingDef> l, int range, out Thing thingToDestroy)
		{
			thingToDestroy = null;
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanRemoveInStockpile(posSpawn, l, out thingToDestroy));
			return posSpawn;
		}
		#endregion absorb

		#endregion recipe func

		public void StartReplicator()
		{
			if (cpr.FuelPercentOfMax <= 0.0f)
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

					List<ThingDef> lSelected = b.ingredientFilter.AllowedThingDefs.ToList();
					if (b.recipe.defName == _Recipe.ReplPlaceLocalFilth)
					{
						AddFilth(lSelected, false);
					}
					else if (b.recipe.defName == _Recipe.ReplPlaceGlobalFilth)
					{
						AddFilth(lSelected, true);
					}
					else if (b.recipe.defName == _Recipe.ReplCleanLocal)
					{
						RemoveFilth(lSelected, false);
					}
					else if (b.recipe.defName == _Recipe.ReplCleanGlobal)
					{
						RemoveFilth(lSelected, true);
					}
					else if (b.recipe.defName.StartsWith(_Recipe.ReplPlaceItem))
					{
						ReplicateItem(lSelected, b);
					}
					else if (b.recipe.defName == _Recipe.ReplAbsorbItem)
					{
						AbsorbItem(lSelected);
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
}
