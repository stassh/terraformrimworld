using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TerraformRimworld
{
	public static class RockEmiterManager
	{
		public static SortedDictionary<string, ThingDef> dicRocks;
		public static SortedDictionary<string, ThingDef> dicSmoothedRocks;
		public static SortedDictionary<string, ThingDef> dicUIRocks;

		public static void Init()
		{
			dicRocks = new SortedDictionary<string, ThingDef>();
			dicSmoothedRocks = new SortedDictionary<string, ThingDef>();
			dicUIRocks = new SortedDictionary<string, ThingDef>();
			foreach (ThingDef rock in DefDatabase<ThingDef>.AllDefs)
			{
				if (rock.IsMineableRock())
				{
					Helper.UpperLabel(rock);
					if (rock.defName.StartsWith("Smoothed"))
					{   // smoothed rock as minable (this is strange!)
						FixSmoothedLab(rock);

						if (dicSmoothedRocks.ContainsKey(rock.label))
							dicSmoothedRocks.Add(rock.label + dicSmoothedRocks.Count, rock);
						else
							dicSmoothedRocks.Add(rock.label, rock);

						ThingCategories.AddToThingCategory(ThingCategories.FromRock, rock);
						continue;
					}
					else if (rock.building != null && rock.building.smoothedThing != null)
					{   // rock has smoothed thing
						Helper.UpperLabel(rock.building.smoothedThing);
						FixSmoothedLab(rock.building.smoothedThing);

						string smLabel = rock.building.smoothedThing.label;

						if (dicSmoothedRocks.ContainsKey(smLabel))
							dicSmoothedRocks.Add(smLabel + dicSmoothedRocks.Count, rock.building.smoothedThing);
						else
							dicSmoothedRocks.Add(smLabel, rock.building.smoothedThing);

						ThingCategories.AddToThingCategory(ThingCategories.FromRock, rock.building.smoothedThing);
					}

					// rock standard
					if (dicRocks.ContainsKey(rock.label))
						dicRocks.Add(rock.label + dicRocks.Count, rock);
					else
						dicRocks.Add(rock.label, rock);

					ThingCategories.AddToThingCategory(ThingCategories.FromRock, rock);
				}
			}

			foreach (ThingDef rock in dicRocks.Values)
			{
				ThingDef uiRock = CreateUIRock(rock);
				if (uiRock != null)
				{
					if (dicUIRocks.ContainsKey(uiRock.label))
						dicUIRocks.Add(uiRock.label + dicUIRocks.Count, uiRock);
					else
						dicUIRocks.Add(uiRock.label, uiRock);
					ThingCategories.AddToThingCategory(ThingCategories.Rocks, uiRock);
				}
			}

			foreach (ThingDef smRock in dicSmoothedRocks.Values)
			{
				ThingDef uiRock = CreateUIRock(smRock);
				if (uiRock != null)
				{
					if (dicUIRocks.ContainsKey(uiRock.label))
						dicUIRocks.Add(uiRock.label + dicUIRocks.Count, uiRock);
					else
						dicUIRocks.Add(uiRock.label, uiRock);
					ThingCategories.AddToThingCategory(ThingCategories.Rocks, uiRock);
				}
			}

			// updates
			foreach (ThingDef tui in dicUIRocks.Values)
			{
				tui.UpdateStat(StatDefOf.WorkToBuild.defName, TRMod.OPTION_WorkValue);
				UpdateUI_Costs(tui);
				tui.ResolveReferences();
				tui.PostLoad();
			}

			ThingCategories.FromRock.ResolveReferences();
			ThingCategories.FromRock.PostLoad();
			ThingCategories.Rocks.ResolveReferences();
			ThingCategories.Rocks.PostLoad();
			DesignatorDropdownGroupDefOf.Build_Rocks.ResolveReferences();
			DesignatorDropdownGroupDefOf.Build_Rocks.PostLoad();

			CreateRockEmiter();
			if (TRMod.isDebug)
				Helper.ShowDialog("Rocks: " + Helper.DicToString(dicRocks) + "\nTUI: " + Helper.DicToString(dicUIRocks));
		}

		public static void UpdateUI_Costs(ThingDef tui)
		{
			if (tui.costList == null)
				tui.costList = new List<ThingDefCountClass>();
			else
				tui.costList.Clear();

			if (TRMod.OPTION_CostsEnabled)
			{
				if (!Helper.AddCustomCosts(tui))
				{   // has  no custom costs
					if (tui.defName == _Stones.CollapsedRocks)
						tui.costList.Add(new ThingDefCountClass((TRMod.OPTION_UseSilver ? ThingDefOf.Silver : ThingDefOf.Steel), TRMod.OPTION_CostAmount));

					var dme = tui.GetModExtension<DME_Rock>();
					if (dme == null)
						return;

					ThingDef tResult = DefDatabase<ThingDef>.GetNamed(dme.result, false);
					if (tResult == null)
						return;

					string baseName = tui.defName.Replace(_Text.RESROCK_, "");
					ThingDef chunk = Helper.GetChunk(baseName);
					if (chunk == null && baseName.ToLower().StartsWith("smoothed"))
						chunk = Helper.GetChunk(baseName.Replace("Smoothed", ""));

					if (chunk != null)
						tui.costList.Add(new ThingDefCountClass(chunk, 1));
					else if (tResult.building != null && tResult.building.mineableThing != null)
						tui.costList.Add(new ThingDefCountClass(tResult.building.mineableThing, tResult.building.mineableYield));
					else
						tui.costList.Add(new ThingDefCountClass((TRMod.OPTION_UseSilver ? ThingDefOf.Silver : ThingDefOf.Steel), TRMod.OPTION_CostAmount));
				}
			}
		}

		private static void CreateRockEmiter()
		{
			ThingDef emiter = DefDatabase<ThingDef>.GetNamed(_Emiter.RockEmiter, false);
			if (emiter == null)
			{
				List<RecipeDef> recipes = new List<RecipeDef>();
				List<ThingCategoryDef> lrock = new List<ThingCategoryDef>();
				lrock.Add(ThingCategories.Rocks);
				List<ThingCategoryDef> lreplace = new List<ThingCategoryDef>();
				lreplace.Add(ThingCategories.FromRock);
				lreplace.Add(ThingCategories.Rocks);
				List<ThingCategoryDef> lcolor = new List<ThingCategoryDef>();
				lcolor.Add(ThingCategories.Rocks);
				lcolor.Add(ThingCategories.Farbe);

				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RockPlaceLocal, lrock, ResearchProjectDefOf.RockEmiter, false, "Place ... in Area", "Placing ... in Area", "Places Rocks in 'Terraforming'-Area around. Existing Rocks will not be replaced."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RockPlaceGlobal, lrock, ResearchProjectDefOf.RockEmiter, false, "Place ... in Biome", "Placing ... in Biome", "Places Rocks in Biome. Existing Rocks will not be replaced. Placing starts around the emiter, then continue random."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RockRemoveLocal, lrock, ResearchProjectDefOf.RockEmiter, false, "Remove ... in Area", "Removing ... in Area", "Removes choosen Rocks in 'Terraforming'-Area."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RockRemoveGlobal, lrock, ResearchProjectDefOf.RockEmiter, false, "Remove ... in Biome", "Removing ... in Biome", "Removes choosen Rocks in Biome."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RockReplaceLocal, lreplace, ResearchProjectDefOf.RockEmiter, false, "Replace ... To ... in Area", "Replacing ... To ... in Area", "Replaces Rocks in 'Terraforming'-Area around. Choosen 'From'-Rocks will be replaced by choosen Rocks."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RockReplaceGlobal, lreplace, ResearchProjectDefOf.RockEmiter, false, "Replace ... To ... in Biome", "Replacing ... To ... in Biome", "Replaces Rocks in Biome. Choosen 'From'-Rocks will be replaced by choosen Rocks."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RockColorize, lcolor, ResearchProjectDefOf.RockEmiter, false, "Temporary Colorize Rocks...", "Colorizing Rocks...", "Rocks will be temporary colorized to new color."));

				Helper.CreateDefaultEmiter(_Emiter.RockEmiter, typeof(RockEmiter), _Emiter.RockEmiter, ResearchProjectDefOf.RockEmiter, recipes, 0, 0, "Rock-Emiter", "");
			}
			else
				Helper.UpdateEmiter(_Emiter.RockEmiter);
		}

		private static ThingDef CreateUIRock(ThingDef baseStone)
		{
			if (baseStone == null || baseStone.defName == null)
				return null; // keine ui defs wenn rock nicht vorher erstellt

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

			tui.designatorDropdown = DesignatorDropdownGroupDefOf.Build_Rocks;
			tui.designationCategory = DesignationCategoryDefOf.Terraform;

			#endregion designation

			#region basic

			tui.thingClass = typeof(TR_Rock);
			tui.category = ThingCategory.Building;
			tui.altitudeLayer = AltitudeLayer.Building;
			tui.passability = Traversability.PassThroughOnly;
			tui.pathCost = 10;
			tui.thingCategories = new List<ThingCategoryDef>();
			tui.thingCategories.Add(ThingCategories.Rocks);

			tui.placeWorkers = new List<Type>();
			tui.placeWorkers.Add(typeof(PlaceWorker_Rock));

			tui.SetGraphicDataSingle(_Stones.GetUIIconByName(baseStone.defName), _Stones.GetUIIconByName(baseStone.defName));
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
				tui.researchPrerequisites.Add(ResearchProjectDefOf.RockTerraforming);
			ResearchProjectDefOf.RockTerraforming.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.RockTerraforming.description, tui.label);

			#endregion research

			#region costs

			UpdateUI_Costs(tui);

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

			#region icon color

			if (baseStone.graphicData != null)
				tui.uiIconColor = baseStone.graphicData.color;

			#endregion icon color

			tui.minifiedDef = TRMod.OPTION_ForceMinified ? ThingDefOf.MinifiedThing : null;
			tui.RegisterBuildingDef();
			tui.blueprintDef.graphicData.texPath = _Stones.GetUIIconByNameCubed(baseStone.defName, false);

			return tui;
		}

		#region helper

		private static void FixSmoothedLab(ThingDef smRock)
		{
			ThingDef defaultSmoothedRock = DefDatabase<ThingDef>.GetNamed((TRMod.isGerman ? "SmoothedGranite" : "SmoothedMarble"), false);
			ThingDef defaultRock = DefDatabase<ThingDef>.GetNamed((TRMod.isGerman ? "Granite" : "Marble"), false);
			ThingDef normalRock = DefDatabase<ThingDef>.GetNamed(smRock.defName.Replace("Smoothed", ""), false);
			if (defaultRock == null || defaultSmoothedRock == null || normalRock == null)
				return;

			string label = defaultRock.label;
			if (!defaultSmoothedRock.label.Contains(label))
				return; // korrektur nicht möglich

			label = defaultSmoothedRock.label.Replace(label, normalRock.label);
			smRock.label = label;
		}

		#endregion helper
	}

	public class DME_Rock : DefModExtension
	{
		public string result;
	}

	public class PlaceWorker_Rock : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			var rockToBuild = checkingDef.GetModExtension<DME_Rock>();
			var sourceTerrain = map.terrainGrid.TerrainAt(loc);
			if (rockToBuild == null || sourceTerrain == null)
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
				TR_Rock.CreateRock(tdef, map, loc);

				Designator_Cancel cancel = new Designator_Cancel();
				cancel.DesignateSingleCell(loc);
			}
		}
	}

	public class RockEmiter : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
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

		public RockEmiter()
		{
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			HugsLibController.Instance.DistributedTicker.RegisterTickability(StartRockEmiter, TRMod.OPTION_EmiterTick, this);
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

		#region recipe func

		#region place rock

		private void AddRocks(List<ThingDef> listOfRocks, bool global)
		{
			ThingDef rock = Helper.GetRandomListElement(listOfRocks);
			if (rock == null)
				return;

			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestPlaceRock(rock, range, aTerraforming);
				if (CanPlaceRock(posSpawn, rock, aTerraforming))
				{
					GenPlace.TryPlaceThing(ThingMaker.MakeThing(rock), posSpawn, base.Map, ThingPlaceMode.Direct);
					DoEffects(posSpawn);
					break;
				}
			}
		}

		private bool CanPlaceRock(IntVec3 posSpawn, ThingDef thing, Area aTerraforming)
		{
			if (aTerraforming == null || aTerraforming[posSpawn])
				return (Helper.CheckTerrainIsGoodForPlacing(posSpawn, base.Map) && Helper.CheckPlaceIsFree(thing, posSpawn, base.Map));
			else
				return false;
		}

		private IntVec3 GetNearestPlaceRock(ThingDef thing, int range, Area aTerraforming)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanPlaceRock(posSpawn, thing, aTerraforming));
			return posSpawn;
		}
		#endregion place rock

		#region remove rock

		private bool CanRemoveRock(IntVec3 posSpawn, List<ThingDef> l, Area aTerraforming, out Thing thingToDestroy)
		{
			thingToDestroy = null;
			if (aTerraforming == null || aTerraforming[posSpawn])
				thingToDestroy = FindThingToDestroyRock(posSpawn, l);
			return thingToDestroy != null;
		}

		private Thing FindThingToDestroyRock(IntVec3 posSpawn, List<ThingDef> l)
		{
			List<Thing> lt = posSpawn.GetThingList(base.Map);
			Thing foundThingToDestroy = null;
			foreach (Thing thing in lt)
			{
				if (thing != null && thing.def != null && thing.def.IsMineableRock())
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
		private IntVec3 GetNearestRemoveRock(List<ThingDef> l, int range, Area aTerraforming, out Thing thingToDestroy)
		{
			thingToDestroy = null;
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanRemoveRock(posSpawn, l, aTerraforming, out thingToDestroy));
			return posSpawn;
		}

		private void RemoveRocks(List<ThingDef> listOfRocks, bool global)
		{
			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestRemoveRock(listOfRocks, range, aTerraforming, out thingToDestroy);
				if (CanRemoveRock(posSpawn, listOfRocks, aTerraforming, out thingToDestroy))
				{
					GenExplosion.DoExplosion(posSpawn, base.Map, 0.2f, DamageDefOf.Stab, null, 100, 0, TRMod.SND_ROCK_NANITES);
					DoEffects(posSpawn, true);
					break;
				}
			}
		}

		#endregion remove rock

		#region replace rock

		private bool CanReplaceRock(IntVec3 pos, List<ThingDef> lFrom, List<ThingDef> lTo, Area area)
		{
			if (area == null || area[pos])
			{
				List<Thing> l = pos.GetThingList(base.Map);
				ThingDef from = null;
				foreach (Thing thing in l)
				{
					if (thing != null && thing.def != null && thing.def.IsMineableRock())
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

		private IntVec3 GetNearestReplaceRock(List<ThingDef> lFrom, List<ThingDef> lTo, int range, Area area)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanReplaceRock(posSpawn, lFrom, lTo, area));
			return posSpawn;
		}

		private void ReplaceRocks(List<ThingDef> lFrom, List<ThingDef> lTo, bool global)
		{
			ThingDef targetRock = Helper.GetRandomListElement(lTo);
			if (targetRock == null)
				return;

			Area area = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestReplaceRock(lFrom, lTo, range, area);
				if (CanReplaceRock(posSpawn, lFrom, lTo, area))
				{
					Helper.ClearCell(posSpawn, base.Map);
					GenPlace.TryPlaceThing(ThingMaker.MakeThing(targetRock), posSpawn, base.Map, ThingPlaceMode.Direct);
					DoEffects(posSpawn);
					break;
				}
			}
		}

		#endregion replace rock

		#region colorize rock

		private void ColorizeRocks(/*Bill b, */List<ThingDef> l, List<ThingDef> lColors)
		{
			bool changed = false;
			ThingDef choosen = Helper.GetRandomListElement(lColors);
			if (choosen != null)
			{
				Color col = choosen.uiIconColor;
				foreach (ThingDef rock in l)
				{
					if (rock.graphicData != null)
					{
						rock.graphicData.color = col;
						rock.uiIconColor = col;
						ThingDef chunk = Helper.GetChunk(rock.defName);
						if (chunk != null && chunk.graphicData != null)
							chunk.graphicData.color = col;
						ThingDef blocks = Helper.GetBlocks(rock.defName);
						if (blocks != null && blocks.graphicData != null)
							blocks.graphicData.color = col;
						if (blocks != null && blocks.stuffProps != null)
							blocks.stuffProps.color = col;

						rock.ResolveReferences();
						rock.PostLoad();
						changed = true;
					}
				}
				if (changed)
				{
					base.Map.mapDrawer.RegenerateEverythingNow();
					DoEffects(base.Position, true);
					//billStack.Delete(b);
					Helper.ShowReloadDialog("To update the color of already existing rocks, a reload is necessary.\n\nDo quick autosave and autoload now?");
				}
			}
		}

		#endregion colorize rock

		#endregion recipe func

		public void StartRockEmiter()
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
					if (b.recipe.defName == _Recipe.RockPlaceLocal)
					{
						AddRocks(l, false);
					}
					else if (b.recipe.defName == _Recipe.RockPlaceGlobal)
					{
						AddRocks(l, true);
					}
					else if (b.recipe.defName == _Recipe.RockRemoveLocal)
					{
						RemoveRocks(l, false);
					}
					else if (b.recipe.defName == _Recipe.RockRemoveGlobal)
					{
						RemoveRocks(l, true);
					}
					else if (b.recipe.defName == _Recipe.RockReplaceLocal)
					{
						ReplaceRocks(lFrom, l, false);
					}
					else if (b.recipe.defName == _Recipe.RockReplaceGlobal)
					{
						ReplaceRocks(lFrom, l, true);
					}
					else if (b.recipe.defName == _Recipe.RockColorize)
					{
						List<ThingDef> lColors = Helper.GetColorList(lSelected);
						ColorizeRocks(l, lColors);
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

	public class TR_Rock : Building
	{
		public static void CreateRock(ThingDef def, Map map, IntVec3 loc)
		{
			var rock = def.GetModExtension<DME_Rock>();
			if (rock == null)
				return;

			ThingDef targetRock = DefDatabase<ThingDef>.GetNamed(rock.result, false);
			if (targetRock == null)
				return;

			GenPlace.TryPlaceThing(ThingMaker.MakeThing(targetRock), loc, map, ThingPlaceMode.Direct);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);

			CreateRock(def, map, base.Position);
		}
	}
}
