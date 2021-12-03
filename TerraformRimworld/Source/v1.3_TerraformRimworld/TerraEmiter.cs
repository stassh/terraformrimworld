using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TerraformRimworld
{
	public class DME_Terrain : DefModExtension
    {
        public List<string> allowedOn;
        public string result;
    }

    public class TR_Terrain : Building
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            CreateTerrain(def, map, base.Position);

            Destroy(DestroyMode.Vanish);
        }

        public static void CreateTerrain(ThingDef def, Map map, IntVec3 loc)
        {
            var terrain = def.GetModExtension<DME_Terrain>();
            if (terrain == null)
                return;

            TerrainDef targetTerrain = DefDatabase<TerrainDef>.GetNamed(terrain.result, false);
            if (targetTerrain == null)
                return;          

            map.terrainGrid.SetTerrain(loc, targetTerrain);

			TerrainDef oldTerrain = map.terrainGrid.TerrainAt(loc);
			if (Helper.IsStony(oldTerrain.defName))
            {
                int ChunkChance = TRMod.zufallswert.Next(5); // 20% chance
                if (ChunkChance == 0)
                {
                    ThingDef chunk = Helper.GetChunk(oldTerrain.defName);
                    if (chunk != null)
                        GenPlace.TryPlaceThing(ThingMaker.MakeThing(chunk, null), loc, map, ThingPlaceMode.Near, null);
                }
            }
        }
    }

    public class PlaceWorker_Terrain : PlaceWorker
    {
        public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
        {
            //base.PostPlace(map, def, loc, rot);
            if (TRMod.OPTION_InstantConstruction)
            {
                ThingDef tdef = DefDatabase<ThingDef>.GetNamed(def.defName);
                TR_Terrain.CreateTerrain(tdef, map, loc);

                Designator_Cancel cancel = new Designator_Cancel();
                cancel.DesignateSingleCell(loc);
            }
        }

		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{		
            var terrainToBuild = checkingDef.GetModExtension<DME_Terrain>();			
            var sourceTerrain = map.terrainGrid.TerrainAt(loc);			
            if (terrainToBuild == null || sourceTerrain == null)
                return false;

            if (terrainToBuild.result == sourceTerrain.defName)
                return new AcceptanceReport("TerrainIsAlready".Translate(new NamedArgument(checkingDef.label, checkingDef.label)));
            else if (Helper.PlaceHasBuilding(map, loc))
                return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
            else if (TRMod.OPTION_PlaceWithoutRestrictions)
                return true;
            else if (terrainToBuild.allowedOn.Contains(sourceTerrain.defName) || terrainToBuild.allowedOn.Contains("Any"))
                return true;
            else if (terrainToBuild.allowedOn.Contains(_Terrain.Stones) && Helper.IsStony(sourceTerrain.defName))
                return true;
            else
            {
                var sourceTui = DefDatabase<ThingDef>.GetNamed(_Text.T_ + sourceTerrain.defName, false);
                if (sourceTui != null && sourceTui.designatorDropdown == DesignatorDropdownGroupDefOf.OtherTerrain)
                    return true;
            }

			return new AcceptanceReport("TerrainCannotSupport".Translate());
        }
    }

    public class TerraEmiter : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
	{
		#region vars
		CompHeatPusher chp;
        CompRefuelable cpr;
        int efficiency;
		int maxRange;
		Bill b;
		bool hasDoneBill;
		#endregion

		#region constructor and overrides
		public TerraEmiter()
		{
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			HugsLibController.Instance.DistributedTicker.RegisterTickability(StartTerraEmiter, TRMod.OPTION_EmiterTick, this);
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
		private void DoEffect(IntVec3 posSpawn, bool absorbingEffects = false)
		{
			Vector3 vecSpawn = new Vector3(posSpawn.x, posSpawn.y, posSpawn.z);
			if (absorbingEffects)
            {
                FleckMaker.ThrowMicroSparks(vecSpawn, base.Map);
                FleckMaker.ThrowDustPuffThick(vecSpawn, base.Map, 1.0f, Color.black);
            }
            else
            {
                FleckMaker.ThrowFireGlow(vecSpawn, base.Map, 1.5f);
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
		#endregion

		#region recipe func
		#region place terrain
		private bool CanTerraform(ThingDef tui)
		{
			bool canTerraform = true;
			if (!TRMod.OPTION_VanillaLook)
			{
				foreach (ResearchProjectDef r in tui.researchPrerequisites)
				{
					if (!r.IsFinished)
						canTerraform = false;
				}
			}
			return canTerraform;
		}
		private bool CanPlaceTerrain(IntVec3 posSpawn, List<TerrainDef> l, Area aTerraforming)
		{
			if (aTerraforming == null || aTerraforming[posSpawn])
				return !l.Contains(posSpawn.GetTerrain(base.Map));
			else
				return false;
		}
		private IntVec3 GetNearestPlaceTerrain(List<TerrainDef> l, int range, Area aTerraforming)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;

			} while (!CanPlaceTerrain(posSpawn, l, aTerraforming));
			return posSpawn;
		}
		private void AddTerrain(List<TerrainDef> listOfTerrains, bool global)
		{
			TerrainDef terrain = Helper.GetRandomListElement(listOfTerrains);
			if (terrain == null)
				return;

			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestPlaceTerrain(listOfTerrains, range, aTerraforming);
				if (CanPlaceTerrain(posSpawn, listOfTerrains, aTerraforming))
				{
					base.Map.terrainGrid.SetTerrain(posSpawn, terrain);
					DoEffect(posSpawn);
					break;
				}
			}
		}
		#endregion

		#region remove terrain
		private bool CanRemoveTerrain(IntVec3 posSpawn, List<TerrainDef> l, Area aTerraforming)
		{
			if (aTerraforming == null || aTerraforming[posSpawn])
				return l.Contains(posSpawn.GetTerrain(base.Map));
			else
				return false;
		}
		private IntVec3 GetNearestRemoveTerrain(List<TerrainDef> l, int range, Area aTerraforming)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;

			} while (!CanRemoveTerrain(posSpawn, l, aTerraforming));
			return posSpawn;
		}
		private void RemoveTerrain(List<TerrainDef> listOfTerrains, bool global)
		{
			TerrainDef terrain = Helper.GetRandomListElement(listOfTerrains);
			if (terrain == null)
				return;

			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestRemoveTerrain(listOfTerrains, range, aTerraforming);
				if (CanRemoveTerrain(posSpawn, listOfTerrains, aTerraforming))
				{
					base.Map.terrainGrid.SetTerrain(posSpawn, TerrainDefOf.Soil);
					DoEffect(posSpawn, true);
					break;
				}
			}
		}
		#endregion

		#region replace terrain
		private bool CanReplaceTerrain(IntVec3 pos, List<TerrainDef> lFrom, List<TerrainDef> lTo, Area area)
		{
			if (area == null || area[pos])
				return lFrom.Contains(pos.GetTerrain(base.Map)) && !lTo.Contains(pos.GetTerrain(base.Map));
			else
				return false;
		}
		private IntVec3 GetNearestReplaceTerrain(List<TerrainDef> lFrom, List<TerrainDef> lTo, int range, Area area)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;

			} while (!CanReplaceTerrain(posSpawn, lFrom, lTo, area));
			return posSpawn;
		}
		private void ReplaceTerrain(List<TerrainDef> lFrom, List<TerrainDef> lTo, bool global)
		{
			TerrainDef targetTerrain = Helper.GetRandomListElement(lTo);
			if (targetTerrain == null)
				return;

			Area area = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestReplaceTerrain(lFrom, lTo, range, area);
				if (CanReplaceTerrain(posSpawn, lFrom, lTo, area))
				{
					base.Map.terrainGrid.SetTerrain(posSpawn, targetTerrain);
					DoEffect(posSpawn);
					break;
				}
			}
		}
		#endregion

		private void ColorizeTerrain(List<TerrainDef> l, List<ThingDef> lColors)
		{
			bool changed = false;
			ThingDef choosenColor = Helper.GetRandomListElement(lColors);
			if (choosenColor != null)
			{
				Color col = choosenColor.uiIconColor;
				foreach (TerrainDef terrain in l)
				{
					terrain.color = col;
					ThingDef tui = DefDatabase<ThingDef>.GetNamed(_Text.T_ + terrain.defName, false);
					if (tui != null)
					{
						tui.uiIconColor = col;
						tui.ResolveReferences();
						tui.PostLoad();
					}					
					terrain.ResolveReferences();
					terrain.PostLoad();
					changed = true;
				}
				if (changed)
				{
					base.Map.mapDrawer.RegenerateEverythingNow();
					DoEffect(base.Position, true);
				}
			}
		}
		#endregion

		public void StartTerraEmiter()
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
					List<TerrainDef> lFrom = new List<TerrainDef>();
					List<TerrainDef> l = Helper.GetTerrainListsFromSelected(lSelected, out lFrom);
					if (b.recipe.defName == _Recipe.TerraPlaceLocal)
					{
						AddTerrain(l, false);
					}
					else if (b.recipe.defName == _Recipe.TerraPlaceGlobal)
					{
						AddTerrain(l, true);
					}
					else if (b.recipe.defName == _Recipe.TerraRemoveLocal)
					{
						RemoveTerrain(l, false);
					}
					else if (b.recipe.defName == _Recipe.TerraRemoveGlobal)
					{
						RemoveTerrain(l, true);
					}
					else if (b.recipe.defName == _Recipe.TerraReplaceLocal)
					{						
						ReplaceTerrain(lFrom, l, false);
					}
					else if (b.recipe.defName == _Recipe.TerraReplaceGlobal)
					{						
						ReplaceTerrain(lFrom, l, true);
					}
					else if (b.recipe.defName == _Recipe.TerraReplaceBiome)
					{
						List<BiomeDef> lBiomes = Helper.GetBiomeListFromSelected(lSelected);
						foreach (BiomeDef biome in lBiomes)
						{							
							AddTerrain(biome.AllTerrains(), true);
						}
					}
					else if (b.recipe.defName == _Recipe.TerraColorize)
					{
						List<ThingDef> lColors = Helper.GetColorList(lSelected);
						ColorizeTerrain(l, lColors);
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

	public static class TerraEmiterManager
	{
		public static SortedDictionary<string, TerrainDef> dicTerrain;
		public static SortedDictionary<string, ThingDef> dicUITerrain;

		public static void Init()
		{
			dicTerrain = new SortedDictionary<string, TerrainDef>();
			dicUITerrain = new SortedDictionary<string, ThingDef>();
			SortedDictionary<string, TerrainDef> dicWater = new SortedDictionary<string, TerrainDef>(); // to resort terrain
			SortedDictionary<string, TerrainDef> dicLava = new SortedDictionary<string, TerrainDef>(); // to resort terrain
			foreach (TerrainDef td in DefDatabase<TerrainDef>.AllDefs)
			{
				if (td != null && td.defName != null)
				{
					if (dicTerrain.ContainsKey(td.label))
						dicTerrain.Add(td.label + dicTerrain.Count, td);
					else
						dicTerrain.Add(td.label, td);

					if (td.IsWater)
					{
						if (dicWater.ContainsKey(td.label))
							dicWater.Add(td.label + dicWater.Count, td);
						else
							dicWater.Add(td.label, td);
					}
					else if (td.IsLava())
					{
						if (dicLava.ContainsKey(td.label))
							dicLava.Add(td.label + dicLava.Count, td);
						else
							dicLava.Add(td.label, td);
					}
				}
			}
			foreach (TerrainDef terrain in dicTerrain.Values)
			{
				if (!dicWater.ContainsValue(terrain) && !dicLava.ContainsValue(terrain))
				{
					ThingDef uiTerrain = CreateUITerrain(terrain);
					if (uiTerrain != null)
					{
						if (dicUITerrain.ContainsKey(uiTerrain.label))
							dicUITerrain.Add(uiTerrain.label + dicUITerrain.Count, uiTerrain);
						else
							dicUITerrain.Add(uiTerrain.label, uiTerrain);
					}
				}
			}

			// jetzt lava hinzufügen
			foreach (TerrainDef terrain in dicLava.Values)
			{
				ThingDef uiTerrain = CreateUITerrain(terrain);
				if (uiTerrain != null)
				{
					if (dicUITerrain.ContainsKey(uiTerrain.label))
						dicUITerrain.Add(uiTerrain.label + dicUITerrain.Count, uiTerrain);
					else
						dicUITerrain.Add(uiTerrain.label, uiTerrain);
				}
			}

			// jetzt wasser hinzufügen
			foreach (TerrainDef terrain in dicWater.Values)
			{
				ThingDef uiTerrain = CreateUITerrain(terrain);
				if (uiTerrain != null)
				{
					if (dicUITerrain.ContainsKey(uiTerrain.label))
						dicUITerrain.Add(uiTerrain.label + dicUITerrain.Count, uiTerrain);
					else
						dicUITerrain.Add(uiTerrain.label, uiTerrain);
				}
			}

			// updates
			foreach (ThingDef tui in dicUITerrain.Values)
			{
				tui.UpdateStat(StatDefOf.WorkToBuild.defName, TRMod.OPTION_WorkValue);
				UpdateUI_Costs(tui);

				UpdateUI_TerrainScatter(tui);
				UpdateUI_TerrainUpdateRecipes(tui);
				UpdateUI_TerrainLanguageFixes(tui);

				ThingCategories.AddToThingCategory(ThingCategories.Terrain, tui);

				tui.ResolveReferences();
				tui.PostLoad();
			}
			//UpdateUI_IcePosition();
			UpdateObsidianTexture();
			UpdateLavaSlowdown(_Terrain.Lava);
			UpdateLavaSlowdown(_Terrain.LavaBlue);
			UpdateLavaSlowdown(_Terrain.LavaRim);	

			ThingCategories.Terrain.ResolveReferences();
			ThingCategories.Terrain.PostLoad();
			DesignatorDropdownGroupDefOf.ExoticTerrain.ResolveReferences();
			DesignatorDropdownGroupDefOf.LavaTerrain.ResolveReferences();
			DesignatorDropdownGroupDefOf.TerrainRockyRough.ResolveReferences();
			DesignatorDropdownGroupDefOf.TerrainRockyRoughHewn.ResolveReferences();
			DesignatorDropdownGroupDefOf.TerrainRockySmooth.ResolveReferences();
			DesignatorDropdownGroupDefOf.OtherTerrain.ResolveReferences();

			DesignatorDropdownGroupDefOf.ExoticTerrain.PostLoad();
			DesignatorDropdownGroupDefOf.LavaTerrain.PostLoad();
			DesignatorDropdownGroupDefOf.TerrainRockyRough.PostLoad();
			DesignatorDropdownGroupDefOf.TerrainRockyRoughHewn.PostLoad();
			DesignatorDropdownGroupDefOf.TerrainRockySmooth.PostLoad();
			DesignatorDropdownGroupDefOf.OtherTerrain.PostLoad();

			//ThingCategories.CloneCategory(ThingCategories.Terrain, ThingCategories.FromTerrain);
			ThingCategories.PopulateCategoryFTerrain(ThingCategories.Terrain);

			CreateTerraEmiter();
			if (TRMod.isDebug)
				Helper.ShowDialog("Terrain: " + Helper.DicToString(dicTerrain) + "\nTUI: " + Helper.DicToString(dicUITerrain));
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
                {   // has no custom costs
                    ThingDef chunk = Helper.GetChunk(tui.defName.Replace(_Text.T_, ""));
                    if (chunk != null)
                        tui.AddCosts(chunk, 1);
                    else
                        tui.AddCosts(TRMod.OPTION_UseSilver ? ThingDefOf.Silver : ThingDefOf.WoodLog, TRMod.OPTION_CostAmount);
                }
            }
        }

		private static void CreateTerraEmiter()
		{
			ThingDef emiter = DefDatabase<ThingDef>.GetNamed(_Emiter.TerraEmiter, false);
			if (emiter == null)
			{
				List<RecipeDef> recipes = new List<RecipeDef>();
				List<ThingCategoryDef> lterrain = new List<ThingCategoryDef>();
				lterrain.Add(ThingCategories.Terrain);
				List<ThingCategoryDef> lbiome = new List<ThingCategoryDef>();
				lbiome.Add(ThingCategories.Biome);
				List<ThingCategoryDef> lreplace = new List<ThingCategoryDef>();				
				lreplace.Add(ThingCategories.FromTerrain);
				lreplace.Add(ThingCategories.Terrain);
				List<ThingCategoryDef> lcolor = new List<ThingCategoryDef>();
				lcolor.Add(ThingCategories.Terrain);
				lcolor.Add(ThingCategories.Farbe);				

				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TerraPlaceLocal, lterrain, ResearchProjectDefOf.TerraEmiter, false, "Place ... in Area", "Placing ... in Area", "Places Terrain in 'Terraforming'-Area around. All Terrain tiles will be replaced with the choosen tiles."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TerraPlaceGlobal, lterrain, ResearchProjectDefOf.TerraEmiter, false, "Place ... in Biome", "Placing  ... in Biome", "Places Terrain in Biome. All Terrain tiles will be replaced with the choosen tiles."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TerraRemoveLocal, lterrain, ResearchProjectDefOf.TerraEmiter, false, "Remove ... in Area", "Removing ... in Area", "Removes Terrain in 'Terraforming'-Area around. Choosen Terrain tiles will be replaced by soil."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TerraRemoveGlobal, lterrain, ResearchProjectDefOf.TerraEmiter, false, "Remove ... in Biome", "Removing  ... in Biome", "Removes Terrain in Biome. Choosen Terrain tiles will be replaced by soil."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TerraReplaceLocal, lreplace, ResearchProjectDefOf.TerraEmiter, false, "Replace ... To ... in Area", "Replacing ... To ... in Area", "Replaces Terrain in 'Terraforming'-Area around. Choosen 'From'-Terrain will be replaced by choosen Terrain tiles."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TerraReplaceGlobal, lreplace, ResearchProjectDefOf.TerraEmiter, false, "Replace ... To ... in Biome", "Replacing ... To ... in Biome", "Replaces Terrain in Biome. Choosen 'From'-Terrain will be replaced by choosen Terrain tiles."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TerraReplaceBiome, lbiome, ResearchProjectDefOf.TerraEmiter, false, "Replace to Biome Terrain...", "Replacing to Biome Terrain...", "This biome will be terraformed to the selected Biome Type."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.TerraColorize, lcolor, ResearchProjectDefOf.TerraEmiter, false, "Temporary Colorize Terrain...", "Colorizing Terrain...", "Terrain will be temporary colorized to new color."));

				Helper.CreateDefaultEmiter(_Emiter.TerraEmiter, typeof(TerraEmiter), _Emiter.TerraEmiter, ResearchProjectDefOf.TerraEmiter, recipes, 0, 0, "Terrain-Emiter", "");
			}
			else
				Helper.UpdateEmiter(_Emiter.TerraEmiter);
		}

		public static ThingDef CreateUITerrain(TerrainDef baseTerrain)
		{
			if (baseTerrain == null || baseTerrain.defName == null)
				return null; // keine ui defs wenn terrain nicht vorher erstellt

			ThingDef tui = DefDatabase<ThingDef>.GetNamed(_Text.T_ + baseTerrain.defName, false);
			if (tui != null)
				return tui; // keine duplikate erstellen

            ThingDef baseStone = Helper.GetBaseStone(baseTerrain.defName);
            ThingDef chunk = Helper.GetChunk(baseTerrain.defName);

            #region naming
            tui = new ThingDef();
			tui.defName = _Text.T_ + baseTerrain.defName;
			tui.label = baseTerrain.label;
			tui.description = baseTerrain.description;
            #endregion

            #region designation
			if (baseTerrain.IsNatural())
				tui.designatorDropdown = null;
			else if (baseTerrain.defName.EndsWith(_Text._Smooth))
				tui.designatorDropdown = DesignatorDropdownGroupDefOf.TerrainRockySmooth;
			else if (baseTerrain.defName.EndsWith(_Text._RoughHewn))
				tui.designatorDropdown = DesignatorDropdownGroupDefOf.TerrainRockyRoughHewn;
			else if (baseTerrain.defName.EndsWith(_Text._Rough))
				tui.designatorDropdown = DesignatorDropdownGroupDefOf.TerrainRockyRough;
			else if (baseTerrain.IsExotic())
				tui.designatorDropdown = DesignatorDropdownGroupDefOf.ExoticTerrain;
			else if (baseTerrain.IsLava())
				tui.designatorDropdown = DesignatorDropdownGroupDefOf.LavaTerrain;			
			else if (baseTerrain.IsOther())
				tui.designatorDropdown = DesignatorDropdownGroupDefOf.OtherTerrain;
			else
				return null;
			tui.designationCategory = DesignationCategoryDefOf.Terraform;
			#endregion

			#region basic
			tui.thingClass = typeof(TR_Terrain);
            tui.category = ThingCategory.Building;
            tui.altitudeLayer = AltitudeLayer.Terrain;
            tui.passability = Traversability.PassThroughOnly;
            tui.pathCost = 10;
            tui.thingCategories = new List<ThingCategoryDef>();
            tui.thingCategories.Add(ThingCategories.Terrain);

            tui.placeWorkers = new List<Type>();
			tui.placeWorkers.Add(typeof(PlaceWorker_Terrain));

            string uiicon = _Terrain.GetUIIcon(baseTerrain.defName, baseTerrain.texturePath);
            tui.SetGraphicDataSingle(_Terrain.Blueprint, uiicon);
			tui.size = new IntVec2(1, 1);
			tui.placingDraggableDimensions = 2;

            tui.rotatable = false;
            tui.selectable = true;
            tui.useHitPoints = false;
            tui.leaveResourcesWhenKilled = true;
            tui.blockWind = false;
            tui.blockLight = false;
			
			tui.building = new BuildingProperties();


			tui.constructEffect = EffecterDefOf.ConstructDirt;
			tui.soundImpactDefault = SoundDefOf.BulletImpact_Ground;
            #endregion

            #region statbases
            tui.AddStatBase(StatDefOf.WorkToBuild, TRMod.OPTION_WorkValue);
            #endregion

            #region research
            tui.researchPrerequisites = new List<ResearchProjectDef>();
			if (baseTerrain.IsBasic())
			{
				if (!TRMod.OPTION_VanillaLook)
					tui.researchPrerequisites.Add(ResearchProjectDefOf.BasicTerrain);				
				ResearchProjectDefOf.BasicTerrain.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.BasicTerrain.description, tui.label);
			}
			else if (baseTerrain.IsWater)
			{
				if (!TRMod.OPTION_VanillaLook)
					tui.researchPrerequisites.Add(ResearchProjectDefOf.WaterTerrain);
				ResearchProjectDefOf.WaterTerrain.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.WaterTerrain.description, tui.label);
			}
			else if (baseTerrain.IsLava())
			{
				if (!TRMod.OPTION_VanillaLook)
					tui.researchPrerequisites.Add(ResearchProjectDefOf.LavaTerrain);
				ResearchProjectDefOf.LavaTerrain.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.LavaTerrain.description, tui.label);
			}
			else if (baseTerrain.IsRocky())
			{
				if (!TRMod.OPTION_VanillaLook)
					tui.researchPrerequisites.Add(ResearchProjectDefOf.RockyTerrain);
				ResearchProjectDefOf.RockyTerrain.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.RockyTerrain.description, tui.label);
			}
			else if (baseTerrain.IsExotic())
			{
				if (!TRMod.OPTION_VanillaLook)
					tui.researchPrerequisites.Add(ResearchProjectDefOf.ExoticTerrain);
				ResearchProjectDefOf.ExoticTerrain.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.ExoticTerrain.description, tui.label);
			}
			else if (baseTerrain.IsOther())
			{
				if (!TRMod.OPTION_VanillaLook)
					tui.researchPrerequisites.Add(ResearchProjectDefOf.OtherTerrain);
				ResearchProjectDefOf.OtherTerrain.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.OtherTerrain.description, tui.label);
			}
			else
				return null;
            #endregion

            #region costs
            UpdateUI_Costs(tui);
            #endregion

            #region mod extensions
            tui.modExtensions = new List<DefModExtension>();
            DME_Terrain dme = new DME_Terrain();
			dme.result = baseTerrain.defName;
			dme.allowedOn = new List<string>();
			List<string> l = GetAllowed(baseTerrain);
			foreach (string s in l)
				dme.allowedOn.Add(s);
			tui.modExtensions.Add(dme);
			#endregion

			#region mod content
			Helper.SetContentPackToThisMod(tui);
			#endregion

			#region icon color
			if (baseStone != null)
			{
				if (baseStone.graphicData != null)
					tui.uiIconColor = baseStone.graphicData.color;
			}
			else
			{
				if (baseTerrain.uiIconColor != null)
					tui.uiIconColor = baseTerrain.uiIconColor;
				else if (baseTerrain.color != null)
					tui.uiIconColor = baseTerrain.color;
			}
			#endregion


			tui.minifiedDef = TRMod.OPTION_ForceMinified ? ThingDefOf.MinifiedThing : null;
			tui.RegisterBuildingDef();
					   
			return tui;
		}

		// TODO label fixes sollten bei generierung und nicht als postprocess erfolgen!
		#region helper
		public static List<string> GetAllowed(TerrainDef ter)
		{
			List<string> l = new List<string>();
			if (Helper.IsStony(ter.defName))
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.Moon);
				l.Add(_Terrain.MetalRim);
			}
			else if (ter.defName == _Terrain.BrokenAsphalt)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.Moon);
				l.Add(_Terrain.MetalRim);
			}
			else if (ter.defName == _Terrain.PackedDirt)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.Moon);
				l.Add(_Terrain.MetalRim);
			}
			else if (ter.defName == _Terrain.Soil)
			{
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.SoilRich);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.SoftSand);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.MarshyTerrain);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.Moon);
				l.Add(_Terrain.MetalRim);
			}
			else if (ter.defName == _Terrain.MossyTerrain)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.SoilRich);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.SoftSand);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.MarshyTerrain);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.Moon);
				l.Add(_Terrain.MetalRim);
			}
			else if (ter.defName == _Terrain.SoilRich)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.MarshyTerrain);
			}
			else if (ter.defName == _Terrain.Gravel)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.SoilRich);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.Moon);
				l.Add(_Terrain.MetalRim);
			}
			else if (ter.defName == _Terrain.Sand)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.SoftSand);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.MarshyTerrain);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.Moon);
				l.Add(_Terrain.MetalRim);
			}
			else if (ter.defName == _Terrain.SoftSand)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.MarshyTerrain);
				l.Add(_Terrain.Marsh);
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.WaterMovingShallow);
			}
			else if (ter.defName == _Terrain.Mud)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.SoilRich);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.SoftSand);
				l.Add(_Terrain.MarshyTerrain);
				l.Add(_Terrain.Marsh);
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.WaterMovingShallow);
			}
			else if (ter.defName == _Terrain.MarshyTerrain)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.SoilRich);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.SoftSand);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.Marsh);
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.WaterMovingShallow);
			}
			else if (ter.defName == _Terrain.Marsh)
			{
				l.Add(_Terrain.SoftSand);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.MarshyTerrain);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.IceDeep);
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.WaterMovingShallow);
				l.Add(_Terrain.WaterDeep);
				l.Add(_Terrain.WaterOceanDeep);
			}
			else if (ter.defName == _Terrain.WaterShallow)
			{
				l.Add(_Terrain.SoftSand);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.MarshyTerrain);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.IceDeep);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.WaterMovingShallow);
				l.Add(_Terrain.Marsh);
				l.Add(_Terrain.WaterDeep);
				l.Add(_Terrain.WaterOceanDeep);
			}
			else if (ter.defName == _Terrain.WaterOceanShallow)
			{
				l.Add(_Terrain.SoftSand);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.MarshyTerrain);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.IceDeep);
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.WaterMovingShallow);
				l.Add(_Terrain.Marsh);
				l.Add(_Terrain.WaterDeep);
				l.Add(_Terrain.WaterOceanDeep);
			}
			else if (ter.defName == _Terrain.WaterMovingShallow)
			{
				l.Add(_Terrain.SoftSand);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.MarshyTerrain);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.IceDeep);
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.Marsh);
				l.Add(_Terrain.WaterDeep);
				l.Add(_Terrain.WaterOceanDeep);
			}
			else if (ter.defName == _Terrain.WaterMovingChestDeep)
			{
				l.Add(_Terrain.SoftSand);
				l.Add(_Terrain.Mud);
				l.Add(_Terrain.MarshyTerrain);
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.IceDeep);
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingShallow);
				l.Add(_Terrain.Marsh);
				l.Add(_Terrain.WaterDeep);
				l.Add(_Terrain.WaterOceanDeep);
			}
			else if (ter.defName == _Terrain.WaterDeep)
			{
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.WaterMovingShallow);
				l.Add(_Terrain.Marsh);
				l.Add(_Terrain.WaterOceanDeep);
			}
			else if (ter.defName == _Terrain.WaterOceanDeep)
			{
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.WaterMovingShallow);
				l.Add(_Terrain.Marsh);
				l.Add(_Terrain.WaterDeep);
			}
			else if (ter.defName == _Terrain.Ice)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.IceDeep);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.Marsh);
			}
			else if (ter.defName == _Terrain.IceDeep)
			{
				l.Add(_Terrain.Ice);
				l.Add(_Terrain.WaterShallow);
				l.Add(_Terrain.WaterOceanShallow);
				l.Add(_Terrain.WaterMovingShallow);
				l.Add(_Terrain.WaterMovingChestDeep);
				l.Add(_Terrain.Marsh);
			}
			else if (ter.defName == _Terrain.Moon)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.MetalRim);
			}
			else if (ter.defName == _Terrain.MetalRim)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.Moon);
			}
			else if (ter.defName == _Terrain.Lava)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.Moon);
				l.Add(_Terrain.MetalRim);
			}
			else if (ter.defName == _Terrain.LavaBlue)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaRim);
				l.Add(_Terrain.Moon);
				l.Add(_Terrain.MetalRim);
			}
			else if (ter.defName == _Terrain.LavaRim)
			{
				l.Add(_Terrain.Soil);
				l.Add(_Terrain.MossyTerrain);
				l.Add(_Terrain.Gravel);
				l.Add(_Terrain.Sand);
				l.Add(_Terrain.BrokenAsphalt);
				l.Add(_Terrain.PackedDirt);
				l.Add(_Terrain.Stones);
				l.Add(_Terrain.Lava);
				l.Add(_Terrain.LavaBlue);
				l.Add(_Terrain.Moon);
				l.Add(_Terrain.MetalRim);
			}
			else
			{
				l.Add(_Terrain.Any);
			}
			return l;
		}

		public static List<string> GetNaturalTerrains()
		{
			List<string> l = new List<string>();
			l.Add(_Terrain.Soil);
			l.Add(_Terrain.MossyTerrain);
			l.Add(_Terrain.Gravel);
			l.Add(_Terrain.Sand);
			l.Add(_Terrain.BrokenAsphalt);
			l.Add(_Terrain.PackedDirt);
			l.Add(_Terrain.Ice);
			l.Add(_Terrain.SoftSand);
			l.Add(_Terrain.Mud);
			l.Add(_Terrain.Marsh);
			l.Add(_Terrain.MarshyTerrain);
			l.Add(_Terrain.SoilRich);
			l.Add(_Terrain.WaterShallow);
			l.Add(_Terrain.WaterOceanShallow);
			l.Add(_Terrain.WaterMovingChestDeep);
			l.Add(_Terrain.WaterMovingShallow);
			l.Add(_Terrain.WaterDeep);
			l.Add(_Terrain.WaterOceanDeep);
			return l;
		}


		private static void FixDescription(string name)
		{
			ThingDef td = DefDatabase<ThingDef>.GetNamed(name, false);
			if (td == null)
				return;

            bool isGerman = TRMod.isGerman;

			var terrain = td.GetModExtension<DME_Terrain>();
            if (terrain == null)
                return;

			td.description = isGerman ? "Platzierbar auf:\n" : "placeable on:\n";
			if (terrain.allowedOn.Contains("Stones"))
				td.description += isGerman ? "Gestein\n" : "rocky terrain\n";
			else if (terrain.allowedOn.Contains("Any"))
				td.description += isGerman ? "Jedem Terrain\n" : "any terrain\n";
			foreach (string s in terrain.allowedOn)
			{
				if (s == "Stones" || s == "Any")
					continue;
				TerrainDef terd = DefDatabase<TerrainDef>.GetNamed(s, false);
				if (terd != null)
				{
					if (isGerman)
					{   // fix aktuell nur für german TODO language support machen
						string label = GetLabelForSpecialTerrains(s);
						if (!String.IsNullOrEmpty(label))
							terd.label = label;
					}
					td.description += terd.label + "\n";
				}
			}
		}

		private static void ChangeLabelText2(string uiname, string terrainName)
		{
			try
			{
				ThingDef granitDef = DefDatabase<ThingDef>.GetNamed("Granite", false);
				TerrainDef terrainDefGranitR = DefDatabase<TerrainDef>.GetNamed("Granite_Rough", false);


				ThingDef uiDef = DefDatabase<ThingDef>.GetNamed(uiname, false);
				TerrainDef terrainDef = DefDatabase<TerrainDef>.GetNamed(terrainName, false);

				if (granitDef == null || uiDef == null || terrainDef == null || terrainDefGranitR == null)
					return;

				//if (dicCuprosStones.ContainsKey(uiname) ||
				//dicSZStones.ContainsKey(uiname))
				{
					string label = granitDef.label;
					if (uiname.Contains(_Text._Smooth))
					{
						ThingDef thDef = DefDatabase<ThingDef>.GetNamed(terrainName.Replace(_Text._Smooth, ""), false);
						string labelNeu = (thDef == null ? uiname.Replace(_Text.T_, "").Replace(_Text._Smooth, "").Replace("_", "") : thDef.label);

						TerrainDef terGranit = DefDatabase<TerrainDef>.GetNamed("Granite_Smooth", false);
						if (!terGranit.label.Contains(label))
							return;
						label = terGranit.label.Replace(label, labelNeu);

						uiDef.label = label;
						terrainDef.label = label;
					}
					else if (uiname.Contains(_Text._RoughHewn))
					{
						ThingDef thDef = DefDatabase<ThingDef>.GetNamed(terrainName.Replace(_Text._RoughHewn, ""), false);
						string labelNeu = (thDef == null ? uiname.Replace(_Text.T_, "").Replace(_Text._RoughHewn, "").Replace("_", "") : thDef.label);

						TerrainDef terGranit = DefDatabase<TerrainDef>.GetNamed("Granite_RoughHewn", false);
						if (!terGranit.label.Contains(label))
							return;
						label = terGranit.label.Replace(label, labelNeu);

						uiDef.label = label;
						terrainDef.label = label;
					}
					else if (uiname.Contains(_Text._Rough))
					{
						ThingDef thDef = DefDatabase<ThingDef>.GetNamed(terrainName.Replace(_Text._Rough, ""), false);
						string labelNeu = (thDef == null ? uiname.Replace(_Text.T_, "").Replace(_Text._Rough, "").Replace("_", "") : thDef.label);

						TerrainDef terGranit = DefDatabase<TerrainDef>.GetNamed("Granite_Rough", false);
						if (!terGranit.label.Contains(label))
							return;
						label = terGranit.label.Replace(label, labelNeu);

						uiDef.label = label;
						terrainDef.label = label;
					}

				}
			}
			catch
			{
				// ignore
			}
		}

		public static string GetLabelForSpecialTerrains(string terrainName)
		{
			string ret = "";
			if (terrainName == _Terrain.Lava)
				ret = "Lava";
			else if (terrainName == _Terrain.LavaBlue)
				ret = "Lava Blau";
			else if (terrainName == _Terrain.LavaRim)
				ret = "Lava Heiss";
			else if (terrainName == _Terrain.Moon)
				ret = "Mondstein";
			else if (terrainName == _Terrain.IceDeep)
				ret = "Tiefes Eis";
			else if (terrainName == _Terrain.MetalRim)
				ret = "Mettalische Oberfläche";
			else if (terrainName == _Terrain.DrySandstoneSmooth)
				ret = "Glatter Trockener Sandstein";
			else if (terrainName == _Terrain.DrySandstoneRough)
				ret = "Rauer Trockener Sandstein";
			else if (terrainName == _Terrain.DrySandstoneRoughHewn)
				ret = "Grob behauener Trocker Sandstein";
			else if (terrainName == _Terrain.WhiteLimestoneSmooth)
				ret = "Glatter Weißer Kalkstein";
			else if (terrainName == _Terrain.WhiteLimestoneRough)
				ret = "Rauer Weißer Kalkstein";
			else if (terrainName == _Terrain.WhiteLimestoneRoughHewn)
				ret = "Grob behauener Weißer Kalkstein";
			return ret;
		}

		public static void UpdateUI_TerrainLanguageFixes(ThingDef td)
		{
			TerrainDef terrain = DefDatabase<TerrainDef>.GetNamed(td.defName.Replace(_Text.T_, ""), false);
			if (terrain != null)
			{   // es gibt ein terrain zu diesem UI
				if (TRMod.isGerman)
				{
					string label = GetLabelForSpecialTerrains(terrain.defName);
					if (!String.IsNullOrEmpty(label))
						terrain.label = label;
					else if (!terrain.IsNatural())
						ChangeLabelText2(td.defName, terrain.defName);
				}
				else if (!terrain.IsNatural())
				{
					ChangeLabelText2(td.defName, terrain.defName);
				}

				td.label = terrain.label;

				FixDescription(td.defName);
			}
		}

		public static void UpdateUI_TerrainUpdateRecipes(ThingDef tui)
		{
			if (Helper.IsStony(tui.defName))
			{
                string baseStoneName = Helper.GetBaseStoneName(tui.defName.Replace(_Text.T_, ""));
                ThingDef chunk = Helper.GetChunk(tui.defName.Replace(_Text.T_, ""));

                string recipeName = "Make_" + baseStoneName + "Chunks";
				RecipeDef recept = DefDatabase<RecipeDef>.GetNamed(recipeName, false);
				if (recept != null && chunk != null)
                {
                    if (recept.recipeUsers == null || recept.recipeUsers.Count == 0)
                    {
                        recept.recipeUsers = new List<ThingDef>();
                        recept.recipeUsers.Add(DefDatabase<ThingDef>.GetNamed("ElectricSmelter", false));
                        recept.products = new List<ThingDefCountClass>();
                        recept.products.Add(new ThingDefCountClass(chunk, 1));

                        recept.ResolveReferences();
                        recept.PostLoad();

                    }
				}
				else
				{
                    recept = DefDatabase<RecipeDef>.GetNamed("Make_RhodonitChunks", false);
					if (recept != null && chunk != null)
					{
						ThingDef xstone = DefDatabase<ThingDef>.GetNamed(baseStoneName, false);

						RecipeDef newRd = new RecipeDef();
						newRd.jobString = recept.jobString;
						newRd.workAmount = recept.workAmount;
						newRd.workSpeedStat = recept.workSpeedStat;
						newRd.effectWorking = recept.effectWorking;
						newRd.soundWorking = recept.soundWorking;
						newRd.targetCountAdjustment = recept.targetCountAdjustment;
						newRd.ingredients = recept.ingredients;
						newRd.workSkill = recept.workSkill;
						newRd.defName = recipeName;
                        if (TRMod.isGerman)
                        {
                            newRd.label = "Schmelzverfahren " + (xstone != null ? xstone.label : tui.label);
                            newRd.description = "Schmelze Steinbrocken ein um " + (xstone != null ? xstone.label : tui.label) + " herzustellen";
                        }
                        else
                        {
                            newRd.label = "Smelting method " + (xstone != null ? xstone.label : tui.label);
                            newRd.description = "Smelt chunks to get " + baseStoneName;
                        }							
						newRd.fixedIngredientFilter = recept.fixedIngredientFilter;
						newRd.products = new List<ThingDefCountClass>();
						newRd.products.Add(new ThingDefCountClass(chunk, 1));
						newRd.recipeUsers = recept.recipeUsers;

						DefDatabase<RecipeDef>.Add(newRd);
						newRd.ResolveReferences();
						newRd.PostLoad();
					}
				}
			}
		}

		public static void UpdateUI_TerrainScatter(ThingDef td)
		{
			if (Helper.IsStony(td.defName))
			{
				string scatterType = TRMod.OPTION_RockyScatterEnabled ? _Terrain.Rocky : _Terrain.None;
				string baseTerrainName = td.defName.Replace(_Text.T_, "");
				TerrainDef terraindef = DefDatabase<TerrainDef>.GetNamed(baseTerrainName, false);
				if (terraindef != null)
				{
					terraindef.scatterType = scatterType;
					terraindef.ResolveReferences();
					terraindef.PostLoad();
				}
			}
		}


		public static void UpdateObsidianTexture()
		{
			// update obisidan
			TerrainDef tspecial = DefDatabase<TerrainDef>.GetNamed("Obsidian_RoughHewn", false);
			if (tspecial != null)
			{
				tspecial.texturePath = "Lava";
				tspecial.ResolveReferences();
				tspecial.PostLoad();
			}
		}

		public static void UpdateLavaSlowdown(string name)
		{
			TerrainDef ter = DefDatabase<TerrainDef>.GetNamed(name, false);
			if (ter != null)
			{
				ter.pathCost = TRMod.OPTION_LavaSlowdown;
				if (TRMod.OPTION_LavaSlowdown >= 300)
					ter.passability = Traversability.Impassable;
				else
					ter.passability = Traversability.PassThroughOnly;

				ter.ResolveReferences();
				ter.PostLoad();
			}
		}
		#endregion
	}
}
