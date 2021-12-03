using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TerraformRimworld
{
	public class DME_Flora : DefModExtension
	{
		public string result;
	}

	public class TR_Plant : Building
	{
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);

			CreatePlant(def, map, base.Position);

			Destroy(DestroyMode.Vanish);
		}

		public static void CreatePlant(ThingDef def, Map map, IntVec3 loc)
		{
			var plant = def.GetModExtension<DME_Flora>();
			if (plant == null)
				return;

			ThingDef targetPlant = DefDatabase<ThingDef>.GetNamed(plant.result, false);
			if (targetPlant == null)
				return;
			
			if (!TRMod.OPTION_InstantConstruction)
				Helper.ClearCell(loc, map, true);
			GenPlace.TryPlaceThing(ThingMaker.MakeThing(targetPlant), loc, map, ThingPlaceMode.Near);
		}
	}

	public class PlaceWorker_Plant : PlaceWorker
	{
		public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
		{
			if (TRMod.OPTION_InstantConstruction)
			{
				ThingDef tdef = DefDatabase<ThingDef>.GetNamed(def.defName);
				TR_Plant.CreatePlant(tdef, map, loc);

				Designator_Cancel cancel = new Designator_Cancel();
				cancel.DesignateSingleCell(loc);
			}		
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			var plantToBuild = checkingDef.GetModExtension<DME_Flora>();
			var sourceTerrain = map.terrainGrid.TerrainAt(loc);
			if (sourceTerrain == null)
				return false;

			if (map.thingGrid.CellContains(loc, ThingCategory.Building))
				return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
			else if (TRMod.OPTION_PlaceWithoutRestrictions)
				return true;
			else if (!sourceTerrain.affordances.Contains(TerrainAffordanceDefOf.Light))
				return new AcceptanceReport("TerrainCannotSupport".Translate());
			else
				return true;
		}
	}

	public class FloraEmiter : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
	{
		#region vars
		CompHeatPusher chp;
        CompRefuelable cpr;
        int efficiency;
		int maxRange;
		Thing thingToDestroy;
		Bill b;
		bool hasDoneBill;
		#endregion

		#region constructor and overrides
		public FloraEmiter()
		{
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			HugsLibController.Instance.DistributedTicker.RegisterTickability(StartFloraEmiter, TRMod.OPTION_EmiterTick, this);
			Area aTerraforming = Helper.GetTerraformingArea(base.Map);
            chp = base.GetComp<CompHeatPusher>();
            cpr = base.GetComp<CompRefuelable>();
		}

        public override void TickLong()
		{
			//base.TickLong();
		}

		public override void Tick()
		{
			//base.Tick();
		}
		#endregion

		#region effect
		private void DoEffects(IntVec3 posSpawn, bool absorbingEffects = false, int specialEffect = 0)
        {
			Vector3 vecSpawn = new Vector3(posSpawn.x, posSpawn.y, posSpawn.z);			
            if (specialEffect == 1)
			{
				FleckMaker.ThrowAirPuffUp(base.DrawPos, base.Map);
				FleckMaker.ThrowMicroSparks(base.DrawPos, base.Map);
				FleckMaker.ThrowHeatGlow(base.Position, base.Map, 1.0f);
				FleckMaker.ThrowSmoke(base.DrawPos, base.Map, 5.5f);
			}
			else if (specialEffect == 2)
			{
				FleckMaker.ThrowLightningGlow(base.DrawPos, base.Map, 5.0f);
				FleckMaker.ThrowSmoke(base.DrawPos, base.Map, 5.5f);
			}
			else if (absorbingEffects)
			{
				FleckMaker.ThrowMicroSparks(vecSpawn, base.Map);
				FleckMaker.ThrowDustPuffThick(vecSpawn, base.Map, 1.0f, Color.black);
			}
			else
            {
                FleckMaker.ThrowLightningGlow(vecSpawn, base.Map, 1.0f);
                FleckMaker.ThrowHeatGlow(posSpawn, base.Map, 2.0f);
                FleckMaker.ThrowSmoke(vecSpawn, base.Map, 1.5f);
            }

			if (specialEffect == 0)
			{
				FleckMaker.ThrowHeatGlow(base.Position, base.Map, 1.0f);
				FleckMaker.ThrowSmoke(base.DrawPos, base.Map, 1.5f);
			}

			b.Notify_IterationCompleted(null, null);
			b.suspended = !b.ShouldDoNow();
			cpr.ConsumeFuel((cpr.Props.fuelConsumptionRate - 10.0f + TRMod.OPTION_EmiterBaseConsumption) / (float)efficiency);
            chp.CompTick();
			hasDoneBill = true;
		}
		#endregion

		#region support func
		private void TryToHarvest(IntVec3 posSpawn, Thing thing)
        {
            if (((Plant)thing).HarvestableNow)
            {
                if (thing.HitPoints <= 100)
                {
                    int max = efficiency;
                    if (max > (int)thing.def.plant.harvestYield)
                        max = (int)thing.def.plant.harvestYield;
                    int val = TRMod.zufallswert.Next(1, max);
                    for (int k = 0; k < val; k++)
                    {
                        GenPlace.TryPlaceThing(ThingMaker.MakeThing(thing.def.plant.harvestedThingDef, null), posSpawn, base.Map, ThingPlaceMode.Near, null);
                    }
                }
            }
        }
		#endregion

		#region recipe func
		#region place flora
		private bool CanPlaceFlora(IntVec3 posSpawn, ThingDef thing, Area aTerraforming)
		{
			if (aTerraforming == null || aTerraforming[posSpawn])
				return (Helper.CheckTerrainIsGoodForPlacing(posSpawn, base.Map) && Helper.CheckPlaceIsFree(thing, posSpawn, base.Map));
			else
				return false;
		}
		private IntVec3 GetNearestPlaceFlora(ThingDef thing, int range, Area aTerraforming)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			
			} while (!CanPlaceFlora(posSpawn, thing, aTerraforming));
			return posSpawn;
		}
		private void AddFlora(List<ThingDef> listOfPlants, bool global)
        {
			ThingDef targetPlant = Helper.GetRandomListElement(listOfPlants);
			if (targetPlant == null || targetPlant.defName.StartsWith("Unnamed"))
				return;
                  
            Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestPlaceFlora(targetPlant, range, aTerraforming);
				if (CanPlaceFlora(posSpawn, targetPlant, aTerraforming))
				{
					GenPlace.TryPlaceThing(ThingMaker.MakeThing(targetPlant, null), posSpawn, base.Map, ThingPlaceMode.Near, null);
					DoEffects(posSpawn);
					break;
				}
			}
        }
		#endregion

		#region remove flora
		private Thing FindThingToDestroyPlant(IntVec3 posSpawn, List<ThingDef> l)
		{
			List<Thing> lt = posSpawn.GetThingList(base.Map);
			Thing foundThingToDestroy = null;
			foreach (Thing thing in lt)
			{
				if (thing != null && thing.IsPlant() && l.Contains(thing.def))
				{
					foundThingToDestroy = thing;
					break;
				}
			}
			return foundThingToDestroy;
		}
		private bool CanRemovePlant(IntVec3 posSpawn, List<ThingDef> l, Area aTerraforming, out Thing thingToDestroy)
		{
			thingToDestroy = null;
			if (aTerraforming == null || aTerraforming[posSpawn])
				thingToDestroy = FindThingToDestroyPlant(posSpawn, l);
			return thingToDestroy != null;
		}
		private IntVec3 GetNearestRemovePlant(List<ThingDef> l, int range, Area aTerraforming, out Thing thingToDestroy)
		{
			thingToDestroy = null;
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;

			} while (!CanRemovePlant(posSpawn, l, aTerraforming, out thingToDestroy));
			return posSpawn;
		}
		private void RemoveFlora(List<ThingDef> listOfPlants, bool global)
		{
			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestRemovePlant(listOfPlants, range, aTerraforming, out thingToDestroy);
				if (CanRemovePlant(posSpawn, listOfPlants, aTerraforming, out thingToDestroy))
				{
					TryToHarvest(posSpawn, thingToDestroy);
					if (thingToDestroy.def.plant.IsTree)
						GenExplosion.DoExplosion(posSpawn, base.Map, 0.2f, DamageDefOf.Mining, null, 100, 0, TRMod.SND_FLORA_NANITES);
					else
						GenExplosion.DoExplosion(posSpawn, base.Map, 0.2f, DamageDefOf.Mining, null, 100, 0, SoundDefOf.Designate_CutPlants);
					DoEffects(posSpawn, true);
					break;
				}
			}
		}
		#endregion

		#region animal fragrance
		private void IncreaseAnimalDensity()
        {
            if (base.Map.Biome.animalDensity < 1000000.0f)
            {
                base.Map.Biome.animalDensity += 10;
                Messages.Message("Fragrance Particles=" + base.Map.Biome.animalDensity.ToString() + " Moving Lifeforms=" + base.Map.mapPawns.AllPawns.Count().ToString(), MessageTypeDefOf.SilentInput, false);
				DoEffects(base.Position, false, 1);
            }           
        }

        private void DecreaseAnimalDensity()
        {
            if (base.Map.Biome.animalDensity > 10.0f)
            {
                base.Map.Biome.animalDensity -= 10;
                Messages.Message("Fragrance Particles=" + base.Map.Biome.animalDensity.ToString() + " Moving Lifeforms=" + base.Map.mapPawns.AllPawns.Count().ToString(), MessageTypeDefOf.SilentInput, false);
				DoEffects(base.Position, false, 2);
            }
        }
		#endregion

		#region replace flora
		private bool CanReplaceFlora(IntVec3 pos, List<ThingDef> lFrom, List<ThingDef> lTo, Area area)
		{
			if (area == null || area[pos])
			{
				List<Thing> l = pos.GetThingList(base.Map);
				ThingDef from = null;
				foreach (Thing thing in l)
				{
					if (thing != null && thing.IsPlant() && lFrom.Contains(thing.def))
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
		private IntVec3 GetNearestReplaceFlora(List<ThingDef> lFrom, List<ThingDef> lTo, int range, Area area)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;

			} while (!CanReplaceFlora(posSpawn, lFrom, lTo, area));
			return posSpawn;
		}
		private void ReplaceFlora(List<ThingDef> lFrom, List<ThingDef> lTo, bool global)
		{
			ThingDef targetPlant = Helper.GetRandomListElement(lTo);
			if (targetPlant == null || targetPlant.defName.StartsWith("Unnamed"))
				return;

			Area area = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestReplaceFlora(lFrom, lTo, range, area);
				if (CanReplaceFlora(posSpawn, lFrom, lTo, area))
				{
					Helper.ClearCell(posSpawn, base.Map);
					GenPlace.TryPlaceThing(ThingMaker.MakeThing(targetPlant, null), posSpawn, base.Map, ThingPlaceMode.Direct);					
					DoEffects(posSpawn);
					break;
				}
			}
		}
		#endregion

		#region colorize flora
		private void ColorizeFlora(List<ThingDef> l, List<ThingDef> lColors)
		{
			// TODO
			//ThingDef choosenColor = Helper.GetRandomListElement(lColors);
			//bool changed = false;
			//foreach (ThingDef plant in l)
			//{
			//	//if (plant.comps == null)
			//	//	plant.comps = new List<CompProperties>();
				
			//	//CompColorable cc = new CompColorable();
			//	//cc.Color = choosenColor.uiIconColor;
			//	//plant.comps.Add(cc);
						
			//	//for (int i = 0; i < 100; i++)
			//	//{
			//	//	IntVec3 pos = Helper.GetRandomCell(base.Map);
			//	//	foreach (Thing t in pos.GetThingList(base.Map))
			//	//	{
			//	//		if (t.def != null && t.def.plant != null)
			//	//			t.DrawColor = choosenColor.uiIconColor;
			//	//		//t.Graphic.GetColoredVersion(new Shader(), choosenColor, Color.white);  //GraphicUtility.ExtractInnerGraphicFor(t.Graphic, t);
			//	//	}
			//	}
				
			//	//plant.
			//	//Graphic graphic = GraphicUtility.ExtractInnerGraphicFor()

			//	//if (plant.graphicData != null)
			//	//{
			//	//	plant.graphicData.color = choosenColor.uiIconColor;
			//	//	//if (plant.graphicData.shadowData == null)
			//	//	//plant.graphicData.shadowData = new ShadowData();
			//	//	//plant.graphicData.shadowData.volume = choosenColor.uiIconColor;					
			//	//	plant.graphicData.ResolveReferencesSpecial();
			//	//}
			//	//if (plant.graphic == null)
			//	//	plant.graphic = new Graphic();
			//	//plant.graphic.Shader. = new Shader();
			//	//plant.
			//	//plant.graphic.color = choosenColor.uiIconColor;
			//	//plant.ResolveReferences();
			//	//plant.PostLoad();
			//	changed = true;
			//}
			//if (changed)
			//{
			//	base.Map.mapDrawer.RegenerateEverythingNow();
			//	DoEffects(base.Position, true);
			//}
		}
		#endregion
		#endregion

		public void StartFloraEmiter()
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
					List<ThingDef> l = Helper.GetPlantListsFromSelected(lSelected, out lFrom);
					if (b.recipe.defName == _Recipe.FloraPlaceLocal)
					{
						AddFlora(l, false);
					}
					else if (b.recipe.defName == _Recipe.FloraPlaceGlobal)
					{
						AddFlora(l, true);
					}
					else if (b.recipe.defName == _Recipe.FloraRemoveLocal)
					{
						RemoveFlora(l, false);
					}					
					else if (b.recipe.defName == _Recipe.FloraRemoveGlobal)
					{
						RemoveFlora(l, true);
					}
					else if (b.recipe.defName == _Recipe.FloraReplaceLocal)
					{
						ReplaceFlora(lFrom, l, false);
					}
					else if (b.recipe.defName == _Recipe.FloraReplaceGlobal)
					{
						ReplaceFlora(lFrom, l, true);
					}
					else if (b.recipe.defName == _Recipe.FloraPlaceOfBiome)
					{
						List<BiomeDef> lBiomes = Helper.GetBiomeListFromSelected(lSelected);
						foreach (BiomeDef biome in lBiomes)
						{
							AddFlora(biome.AllWildPlants, true);
						}
					}
					else if (b.recipe.defName == _Recipe.FloraRemoveOfBiome)
					{
						List<BiomeDef> lBiomes = Helper.GetBiomeListFromSelected(lSelected);
						foreach (BiomeDef biome in lBiomes)
						{
							RemoveFlora(biome.AllWildPlants, true);
						}
					}
					else if (b.recipe.defName == _Recipe.FloraAddFragrance)
					{
						IncreaseAnimalDensity();
					}
					else if (b.recipe.defName == _Recipe.FloraSubFragrance)
					{
						DecreaseAnimalDensity();
					}
					else if (b.recipe.defName == _Recipe.FloraColorize)
					{
						List<ThingDef> lColors = Helper.GetColorList(lSelected);
						ColorizeFlora(l, lColors);
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

	public static class FloraEmiterManager
	{
		public static SortedDictionary<string, ThingDef> dicPlants;
		public static SortedDictionary<string, ThingDef> dicUIPlants;		

		public static void Init()
		{			
			dicPlants = new SortedDictionary<string, ThingDef>();
			dicUIPlants = new SortedDictionary<string, ThingDef>();
			foreach (ThingDef plant in DefDatabase<ThingDef>.AllDefs)
			{
				if (plant.IsPlant() && !dicPlants.ContainsKey(plant.label))
				{					
					dicPlants.Add(plant.label, plant);
					ThingCategories.AddToThingCategory(ThingCategories.FromFlora, plant);
				}
			}

			foreach (ThingDef plant in dicPlants.Values)
			{
				ThingDef uiPlant = CreateUIPlant(plant);
				if (uiPlant != null && !dicUIPlants.ContainsKey(uiPlant.label))
				{					
					dicUIPlants.Add(uiPlant.label, uiPlant);
					ThingCategories.AddToThingCategory(ThingCategories.Flora, uiPlant);
				}
			}

			// updates
			foreach (ThingDef tui in dicUIPlants.Values)
			{
				tui.UpdateStat(StatDefOf.WorkToBuild.defName, TRMod.OPTION_WorkValue);
				UpdateUI_Costs(tui);
				tui.ResolveReferences();
				tui.PostLoad();				
			}

			ThingCategories.FromFlora.ResolveReferences();
			ThingCategories.FromFlora.PostLoad();
			ThingCategories.Flora.ResolveReferences();
			ThingCategories.Flora.PostLoad();
			DesignatorDropdownGroupDefOf.Build_Flora.ResolveReferences();
			DesignatorDropdownGroupDefOf.Build_Flora.PostLoad();

			CreateFloraEmiter();
			if (TRMod.isDebug)
				Helper.ShowDialog("Flora: " + Helper.DicToString(dicPlants) + "\nTUI :" + Helper.DicToString(dicUIPlants));
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
					tui.AddCosts(TRMod.OPTION_UseSilver ? ThingDefOf.Silver : ThingDefOf.WoodLog, TRMod.OPTION_CostAmount);					
				}
			}
		}

		private static void CreateFloraEmiter()
		{
			ThingDef emiter = DefDatabase<ThingDef>.GetNamed(_Emiter.FloraEmiter, false);
			if (emiter == null)
			{
				List<RecipeDef> recipes = new List<RecipeDef>();
				List<ThingCategoryDef> lflora = new List<ThingCategoryDef>();
				lflora.Add(ThingCategories.Flora);
				List<ThingCategoryDef> lbiome = new List<ThingCategoryDef>();
				lbiome.Add(ThingCategories.Biome);
				List<ThingCategoryDef> lreplace = new List<ThingCategoryDef>();
				lreplace.Add(ThingCategories.FromFlora);
				lreplace.Add(ThingCategories.Flora);
				List<ThingCategoryDef> lcolor = new List<ThingCategoryDef>();
				lcolor.Add(ThingCategories.Flora);
				lcolor.Add(ThingCategories.Farbe);

				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraPlaceLocal, lflora, ResearchProjectDefOf.FloraEmiter, false, "Place... in Area", "Placing ... in Area", "Places Plants in 'Terraforming'-Area around. Existing Plants will not be replaced."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraPlaceGlobal, lflora, ResearchProjectDefOf.FloraEmiter, false, "Place ... in Biome", "Placing ... in Biome", "Places Plants in Biome. Existing Plants will not be replaced. Placing starts around the emiter, then continue random."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraRemoveLocal, lflora, ResearchProjectDefOf.FloraEmiter, false, "Remove ... in Area", "Removing ... in Area", "Removes Plants in 'Terraforming'-Area around. Choosen Plants will be removed."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraRemoveGlobal, lflora, ResearchProjectDefOf.FloraEmiter, false, "Remove ... in Biome", "Removing ... in Biome", "Removes Plants in Biome. Choosen Plants will be removed."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraReplaceLocal, lreplace, ResearchProjectDefOf.FloraEmiter, false, "Replace ... To ... in Area", "Replacing ... To ... in Area", "Replaces Plants in 'Terraforming'-Area around. Choosen 'From'-Plants will be replaced by choosen Plants."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraReplaceGlobal, lreplace, ResearchProjectDefOf.FloraEmiter, false, "Replace ... To ... in Biome", "Replacing ... To ... in Biome", "Replaces Plants in Biome. Choosen 'From'-Plants will be replaced by choosen Plants."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraPlaceOfBiome, lbiome, ResearchProjectDefOf.FloraEmiter, false, "Place Flora of Biome ...", "Placing Flora of Biome ...", "This will force to spread Flora of the selected Biome Type on the Map."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraRemoveOfBiome, lbiome, ResearchProjectDefOf.FloraEmiter, false, "Remove Flora of Biome ...", "Removing Flora of Biome ...", "This will absorb Flora of the selected Biome Type on the Map."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraAddFragrance, null, ResearchProjectDefOf.FloraEmiter, false, "Flora Fragrance +", "Spraying flora fragrance", "Sprays fragrance particles, that will attract wild animals. This process may take some time."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraSubFragrance, null, ResearchProjectDefOf.FloraEmiter, false, "Flora Fragrance -", "Removing flora fragrance", "Absorbs fragrance particles, until they are minimized to default biome value. This process may take some time."));
				//recipes.Add(Helper.CreateDefaultRecipe(_Recipe.FloraColorize, lcolor, ResearchProjectDefOf.FloraEmiter, false, "Temporary Colorize Plants...", "Colorizing Plants...", "Plants will be temporary colorized to new color."));

				Helper.CreateDefaultEmiter(_Emiter.FloraEmiter, typeof(FloraEmiter), _Emiter.FloraEmiter, ResearchProjectDefOf.FloraEmiter, recipes, 0, 0, "Flora-Emiter", "");
			}
			else
				Helper.UpdateEmiter(_Emiter.FloraEmiter);
		}

		private static ThingDef CreateUIPlant(ThingDef basePlant)
		{
			if (basePlant == null || basePlant.defName == null)
				return null; // keine ui defs wenn rock nicht vorher erstellt

			ThingDef tui = DefDatabase<ThingDef>.GetNamed(_Text.TRPLNT_ + basePlant.defName, false);
			if (tui != null)
				return tui; // keine duplikate erstellen

			#region naming
			tui = new ThingDef();
			tui.defName = _Text.TRPLNT_ + basePlant.defName;
			tui.label = basePlant.label;
			tui.description = basePlant.description;
			#endregion

			#region designation
			tui.designatorDropdown = DesignatorDropdownGroupDefOf.Build_Flora;
			tui.designationCategory = DesignationCategoryDefOf.Terraform;
			#endregion

			#region basis
			tui.thingClass = typeof(TR_Plant);
			tui.category = ThingCategory.Building;
			tui.altitudeLayer = AltitudeLayer.Building;
			tui.passability = Traversability.PassThroughOnly;
			tui.pathCost = 10;
			tui.thingCategories = new List<ThingCategoryDef>();
			tui.thingCategories.Add(ThingCategories.Flora);

			tui.placeWorkers = new List<Type>();
			tui.placeWorkers.Add(typeof(PlaceWorker_Plant));

			string texPath = _Plant.Unknown;			
			if (basePlant.graphicData.graphicClass == typeof(Graphic_Random))
			{				
				Graphic_Random g = basePlant.graphicData.Graphic as Graphic_Random;				
				tui.graphicData = new GraphicData();
				texPath = g.FirstSubgraphic().path;
				tui.graphicData.texPath = texPath;
				tui.graphicData.shaderType = ShaderTypeDefOf.MetaOverlay;
				tui.graphicData.graphicClass = typeof(Graphic_Single);
				tui.uiIconPath = texPath;
			}
			else if (basePlant.graphicData.graphicClass == typeof(Graphic_Single))
			{
				texPath = basePlant.graphicData.texPath;
				tui.SetGraphicDataSingle(texPath, texPath);
			}
			else
			{				
				tui.SetGraphicDataSingle(texPath, texPath);
			}
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
			#endregion

			#region statbases
			tui.AddStatBase(StatDefOf.WorkToBuild, TRMod.OPTION_WorkValue);
			#endregion

			#region research
			tui.researchPrerequisites = new List<ResearchProjectDef>();
			if (!TRMod.OPTION_VanillaLook)
				tui.researchPrerequisites.Add(ResearchProjectDefOf.FloraTerraforming);
			ResearchProjectDefOf.FloraTerraforming.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.FloraTerraforming.description, tui.label);
			#endregion

			#region costs
			UpdateUI_Costs(tui);
			#endregion

			#region mod extensions
			tui.modExtensions = new List<DefModExtension>();
			DME_Flora dme = new DME_Flora();
			dme.result = basePlant.defName;
			tui.modExtensions.Add(dme);
			#endregion

			#region mod content
			Helper.SetContentPackToThisMod(tui);			
			#endregion

			#region icon color
			if (basePlant.graphicData != null)
				tui.uiIconColor = basePlant.graphicData.color;
			#endregion


			tui.minifiedDef = TRMod.OPTION_ForceMinified ? ThingDefOf.MinifiedThing : null;
			tui.RegisterBuildingDef();
			tui.blueprintDef.graphicData.texPath = texPath;

			return tui;
		}
	}
}
