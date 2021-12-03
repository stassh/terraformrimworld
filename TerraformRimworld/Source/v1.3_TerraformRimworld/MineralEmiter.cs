using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TerraformRimworld
{
	public static class MineralEmiterManager
	{
		public static SortedDictionary<string, ThingDef> dicMinerals;
		public static SortedDictionary<string, ThingDef> dicUIMinerals;

		public static void Init()
		{
			dicMinerals = new SortedDictionary<string, ThingDef>();
			dicUIMinerals = new SortedDictionary<string, ThingDef>();
			foreach (ThingDef mineral in DefDatabase<ThingDef>.AllDefs)
			{
				if (mineral.IsMineableMineral())
				{
					if (dicMinerals.ContainsKey(mineral.label))
						dicMinerals.Add(mineral.label + dicMinerals.Count, mineral);
					else
						dicMinerals.Add(mineral.label, mineral);
					ThingCategories.AddToThingCategory(ThingCategories.FromMineral, mineral);
				}
			}

			ThingDef collapsedRock = DefDatabase<ThingDef>.GetNamed(_Stones.CollapsedRocks, false);
			if (!dicMinerals.ContainsKey(collapsedRock.label))
			{
				dicMinerals.Add(collapsedRock.label, collapsedRock);
				ThingCategories.AddToThingCategory(ThingCategories.FromMineral, collapsedRock);
			}

			foreach (ThingDef mineral in dicMinerals.Values)
			{
				ThingDef uiMineral = CreateUIMineral(mineral);
				if (uiMineral != null)
				{
					if (dicUIMinerals.ContainsKey(uiMineral.label))
						dicUIMinerals.Add(uiMineral.label + dicUIMinerals.Count, uiMineral);
					else
						dicUIMinerals.Add(uiMineral.label, uiMineral);
					ThingCategories.AddToThingCategory(ThingCategories.Minerals, uiMineral);
				}
			}

			// updates
			foreach (ThingDef tui in dicUIMinerals.Values)
			{
				tui.UpdateStat(StatDefOf.WorkToBuild.defName, TRMod.OPTION_WorkValue);
				RockEmiterManager.UpdateUI_Costs(tui);
				tui.ResolveReferences();
				tui.PostLoad();
			}

			ThingCategories.FromMineral.ResolveReferences();
			ThingCategories.FromMineral.PostLoad();
			ThingCategories.Minerals.ResolveReferences();
			ThingCategories.Minerals.PostLoad();
			DesignatorDropdownGroupDefOf.Build_ResRock.ResolveReferences();
			DesignatorDropdownGroupDefOf.Build_ResRock.PostLoad();

			CreateMineralEmiter();
			if (TRMod.isDebug)
				Helper.ShowDialog("Minerals : " + Helper.DicToString(dicMinerals) + "\nTUI: " + Helper.DicToString(dicUIMinerals));
		}

		private static void CreateMineralEmiter()
		{
			ThingDef emiter = DefDatabase<ThingDef>.GetNamed(_Emiter.MineralEmiter, false);
			if (emiter == null)
			{
				List<RecipeDef> recipes = new List<RecipeDef>();
				List<ThingCategoryDef> lmineral = new List<ThingCategoryDef>();
				lmineral.Add(ThingCategories.Minerals);
				List<ThingCategoryDef> lreplace = new List<ThingCategoryDef>();
				lreplace.Add(ThingCategories.FromMineral);
				lreplace.Add(ThingCategories.Minerals);
				List<ThingCategoryDef> lcolor = new List<ThingCategoryDef>();
				lcolor.Add(ThingCategories.Minerals);
				lcolor.Add(ThingCategories.Farbe);

				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.MineralPlaceLocal, lmineral, ResearchProjectDefOf.MineralEmiter, false, "Place ... in Area", "Placing ... in Area", "Places Minerals in 'Terraforming'-Area around. Existing Minerals will not be replaced."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.MineralPlaceGlobal, lmineral, ResearchProjectDefOf.MineralEmiter, false, "Place ... in Biome", "Placing ... in Biome", "Places Minerals in Biome. Existing Minerals will not be replaced. Placing starts around the emiter, then continue random."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.MineralRemoveLocal, lmineral, ResearchProjectDefOf.MineralEmiter, false, "Remove ... in Area", "Removing ... in Area", "Removes choosen Minerals in 'Terraforming'-Area."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.MineralRemoveGlobal, lmineral, ResearchProjectDefOf.MineralEmiter, false, "Remove ... in Biome", "Removing ... in Biome", "Removes choosen Minerals in Biome."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.MineralReplaceLocal, lreplace, ResearchProjectDefOf.MineralEmiter, false, "Replace ... To ... in Area", "Replacing ... To ... in Area", "Replaces Minerals in 'Terraforming'-Area around. Choosen 'From'-Minerals will be replaced by choosen Minerals."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.MineralReplaceGlobal, lreplace, ResearchProjectDefOf.MineralEmiter, false, "Replace ... To ... in Biome", "Replacing ... To ... in Biome", "Replaces Minerals in Biome. Choosen 'From'-Minerals will be replaced by choosen Minerals."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.MineralColorize, lcolor, ResearchProjectDefOf.MineralEmiter, false, "Temporary Colorize Minerals...", "Colorizing Minerals...", "Minerals will be temporary colorized to new color."));

				Helper.CreateDefaultEmiter(_Emiter.MineralEmiter, typeof(MineralEmiter), _Emiter.MineralEmiter, ResearchProjectDefOf.MineralEmiter, recipes, 0, 0, "Mineral-Emiter", "");
			}
			else
				Helper.UpdateEmiter(_Emiter.MineralEmiter);
		}

		private static ThingDef CreateUIMineral(ThingDef baseStone)
		{
			if (baseStone == null || baseStone.defName == null)
				return null; // keine ui defs wenn mineral nicht vorher erstellt

			ThingDef tui = DefDatabase<ThingDef>.GetNamed(_Text.RESROCK_ + baseStone.defName, false);
			if (tui != null)
				return tui; // keine duplikate erstellen

			#region naming

			tui = new ThingDef();
			tui.defName = _Text.RESROCK_ + baseStone.defName;
			tui.label = baseStone.label;
			tui.description = baseStone.description;

			#endregion naming

			#region designation

			tui.designatorDropdown = DesignatorDropdownGroupDefOf.Build_ResRock;
			tui.designationCategory = DesignationCategoryDefOf.Terraform;

			#endregion designation

			#region basic

			tui.thingClass = typeof(TR_Rock);
			tui.category = ThingCategory.Building;
			tui.altitudeLayer = AltitudeLayer.Building;
			tui.passability = Traversability.PassThroughOnly;
			tui.pathCost = 10;
			tui.thingCategories = new List<ThingCategoryDef>();
			tui.thingCategories.Add(ThingCategories.Minerals);

			tui.placeWorkers = new List<Type>();
			tui.placeWorkers.Add(typeof(PlaceWorker_Rock));

			tui.SetGraphicDataSingle(_Stones.GetUIIconByNameCubed(baseStone.defName, true), _Stones.GetUIIconByNameCubed(baseStone.defName, true));
			tui.size = new IntVec2(1, 1);
			tui.placingDraggableDimensions = 2;

			tui.rotatable = false;
			tui.selectable = true;
			tui.useHitPoints = false;
			tui.leaveResourcesWhenKilled = true;
			tui.blockWind = false;
			tui.blockLight = false;

			tui.building = new BuildingProperties();

			tui.constructEffect = EffecterDefOf.ConstructMetal;
			tui.soundImpactDefault = SoundDefOf.BulletImpact_Ground;

			#endregion basic

			#region statbases

			tui.AddStatBase(StatDefOf.WorkToBuild, TRMod.OPTION_WorkValue);

			#endregion statbases

			#region research

			tui.researchPrerequisites = new List<ResearchProjectDef>();
			if (!TRMod.OPTION_VanillaLook)
				tui.researchPrerequisites.Add(ResearchProjectDefOf.MineralTerraforming);
			ResearchProjectDefOf.MineralTerraforming.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.MineralTerraforming.description, tui.label);

			#endregion research

			#region costs

			RockEmiterManager.UpdateUI_Costs(tui);

			#endregion costs

			#region mod extensions

			tui.modExtensions = new List<DefModExtension>();
			DME_Rock dme = new DME_Rock();
			dme.result = baseStone.defName;
			tui.modExtensions.Add(dme);

			#endregion mod extensions

			#region mod content

			Helper.SetContentPackToThisMod(tui);

			#endregion mod content

			#region icon color and path

			if (baseStone.graphicData != null)
				tui.uiIconColor = baseStone.graphicData.colorTwo;

			#endregion icon color and path

			tui.minifiedDef = TRMod.OPTION_ForceMinified ? ThingDefOf.MinifiedThing : null;
			tui.RegisterBuildingDef();

			return tui;
		}
	}

	public class MineralEmiter : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
	{
		#region vars

		private Bill b;
		private CompHeatPusher chp;
		private CompRefuelable cpr;
		private int efficiency;
		private bool hasDoneBill;
		private int maxRange;
		private Thing thingToDestroy;
		#endregion vars

		#region constructor and overrides

		public MineralEmiter()
		{
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			HugsLibController.Instance.DistributedTicker.RegisterTickability(StartMineralEmiter, TRMod.OPTION_EmiterTick, this);
			Area aTerraforming = Helper.GetTerraformingArea(base.Map);
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
			b.suspended = !b.ShouldDoNow();
			cpr.ConsumeFuel((cpr.Props.fuelConsumptionRate - 10.0f + TRMod.OPTION_EmiterBaseConsumption) / (float)efficiency);
			chp.CompTick();
			hasDoneBill = true;
		}

		#endregion effect

		#region support func

		private void TryToHarvest(IntVec3 posSpawn, Thing thing)
		{
			if (thing.HitPoints <= 100 && thing.def.defName != _Stones.CollapsedRocks)
			{
				int max = efficiency;
				if (max > (int)thing.def.building.mineableYield)
					max = (int)thing.def.building.mineableYield;
				int val = TRMod.zufallswert.Next(1, max);
				for (int k = 0; k < val; k++)
				{
					GenPlace.TryPlaceThing(ThingMaker.MakeThing(thing.def.building.mineableThing, null), posSpawn, base.Map, ThingPlaceMode.Near, null);
				}
			}
		}

		#endregion support func

		#region recipe func

		#region place mineral

		private void AddMinerals(List<ThingDef> listOfMinerals, bool global)
		{
			ThingDef mineral = Helper.GetRandomListElement(listOfMinerals);
			if (mineral == null)
				return;

			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestPlaceMineral(mineral, range, aTerraforming);
				if (CanPlaceMineral(posSpawn, mineral, aTerraforming))
				{
					GenPlace.TryPlaceThing(ThingMaker.MakeThing(mineral), posSpawn, base.Map, ThingPlaceMode.Direct);
					DoEffects(posSpawn);
					break;
				}
			}
		}

		private bool CanPlaceMineral(IntVec3 posSpawn, ThingDef thing, Area aTerraforming)
		{
			if (aTerraforming == null || aTerraforming[posSpawn])
				return (Helper.CheckTerrainIsGoodForPlacing(posSpawn, base.Map) && Helper.CheckPlaceIsFree(thing, posSpawn, base.Map));
			else
				return false;
		}

		private IntVec3 GetNearestPlaceMineral(ThingDef thing, int range, Area aTerraforming)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanPlaceMineral(posSpawn, thing, aTerraforming));
			return posSpawn;
		}
		#endregion place mineral

		#region remove mineral

		private bool CanRemoveMineral(IntVec3 posSpawn, List<ThingDef> l, Area aTerraforming, out Thing thingToDestroy)
		{
			thingToDestroy = null;
			if (aTerraforming == null || aTerraforming[posSpawn])
				thingToDestroy = FindThingToDestroyMineral(posSpawn, l);
			return thingToDestroy != null;
		}

		private Thing FindThingToDestroyMineral(IntVec3 posSpawn, List<ThingDef> l)
		{
			List<Thing> lt = posSpawn.GetThingList(base.Map);
			Thing foundThingToDestroy = null;
			foreach (Thing thing in lt)
			{
				if (thing != null && thing.def != null && (thing.def.IsMineableMineral() || thing.def.defName == _Stones.CollapsedRocks))
				{
					foreach (ThingDef tui in l)
					{
						if (tui.label == thing.def.label)
						{
							foundThingToDestroy = thing;
							break;
						}
					}
				}
			}
			return foundThingToDestroy;
		}
		private IntVec3 GetNearestRemoveMineral(List<ThingDef> l, int range, Area aTerraforming, out Thing thingToDestroy)
		{
			thingToDestroy = null;
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanRemoveMineral(posSpawn, l, aTerraforming, out thingToDestroy));
			return posSpawn;
		}

		private void RemoveMinerals(List<ThingDef> listOfMinerals, bool global)
		{
			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestRemoveMineral(listOfMinerals, range, aTerraforming, out thingToDestroy);
				if (CanRemoveMineral(posSpawn, listOfMinerals, aTerraforming, out thingToDestroy))
				{
					TryToHarvest(posSpawn, thingToDestroy);
					GenExplosion.DoExplosion(posSpawn, base.Map, 0.2f, DamageDefOf.Stab, null, 100, 0, TRMod.SND_ROCK_NANITES);
					DoEffects(posSpawn, true);
					break;
				}
			}
		}

		#endregion remove mineral

		#region replace mineral

		private bool CanReplaceMineral(IntVec3 pos, List<ThingDef> lFrom, List<ThingDef> lTo, Area area)
		{
			if (area == null || area[pos])
			{
				List<Thing> l = pos.GetThingList(base.Map);
				ThingDef from = null;
				foreach (Thing thing in l)
				{
					if (thing != null && thing.def != null && (thing.def.IsMineableMineral() || thing.def.defName == _Stones.CollapsedRocks))
					{
						from = thing.def;
						break;
					}
				}
				return (from != null && lFrom.Contains(from) && !lTo.Contains(from));
			}
			else
				return false;
		}

		private IntVec3 GetNearestReplaceMineral(List<ThingDef> lFrom, List<ThingDef> lTo, int range, Area area)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanReplaceMineral(posSpawn, lFrom, lTo, area));
			return posSpawn;
		}

		private void ReplaceMinerals(List<ThingDef> lFrom, List<ThingDef> lTo, bool global)
		{
			ThingDef targetMineral = Helper.GetRandomListElement(lTo);
			if (targetMineral == null)
				return;

			Area area = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestReplaceMineral(lFrom, lTo, range, area);
				if (CanReplaceMineral(posSpawn, lFrom, lTo, area))
				{
					Helper.ClearCell(posSpawn, base.Map);
					GenPlace.TryPlaceThing(ThingMaker.MakeThing(targetMineral), posSpawn, base.Map, ThingPlaceMode.Direct);
					DoEffects(posSpawn);
					break;
				}
			}
		}

		#endregion replace mineral

		#region colorize mineral

		private void ColorizeMinerals(List<ThingDef> l, List<ThingDef> lColors)
		{
			bool changed = false;
			ThingDef choosen = Helper.GetRandomListElement(lColors);
			if (choosen != null)
			{
				Color col = choosen.uiIconColor;
				foreach (ThingDef mineral in l)
				{
					if (mineral.graphicData != null)
					{
						mineral.graphicData.colorTwo = col;
						mineral.ResolveReferences();
						mineral.PostLoad();
						changed = true;
					}
				}
				if (changed)
				{
					base.Map.mapDrawer.RegenerateEverythingNow();
					DoEffects(base.Position, true);
					//billStack.Delete(b);
					Helper.ShowReloadDialog("To update the color of already existing minerals, a reload is necessary.\n\nDo quick autosave and autoload now?");
				}
			}
		}

		#endregion colorize mineral

		#endregion recipe func

		public void StartMineralEmiter()
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
					List<ThingDef> lFrom = new List<ThingDef>();
					List<ThingDef> l = Helper.GetRockListFromSelected(lSelected, out lFrom);
					if (b.recipe.defName == _Recipe.MineralPlaceLocal)
					{
						AddMinerals(l, false);
					}
					else if (b.recipe.defName == _Recipe.MineralPlaceGlobal)
					{
						AddMinerals(l, true);
					}
					else if (b.recipe.defName == _Recipe.MineralRemoveLocal)
					{
						RemoveMinerals(l, false);
					}
					else if (b.recipe.defName == _Recipe.MineralRemoveGlobal)
					{
						RemoveMinerals(l, true);
					}
					else if (b.recipe.defName == _Recipe.MineralReplaceLocal)
					{
						ReplaceMinerals(lFrom, l, false);
					}
					else if (b.recipe.defName == _Recipe.MineralReplaceGlobal)
					{
						ReplaceMinerals(lFrom, l, true);
					}
					else if (b.recipe.defName == _Recipe.MineralColorize)
					{
						List<ThingDef> lColors = Helper.GetColorList(lSelected);
						ColorizeMinerals(l, lColors);
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
