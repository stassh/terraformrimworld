using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TerraformRimworld
{
	public static class RoofEmiterManager
	{
		public static SortedDictionary<string, TerrainDef> dicRoofs;
		public static SortedDictionary<string, ThingDef> dicUIRoofs;

		public static void CreateRoofEmiter()
		{
			ThingDef emiter = DefDatabase<ThingDef>.GetNamed(_Emiter.MountainsEmiter, false);
			if (emiter == null)
			{
				List<RecipeDef> recipes = new List<RecipeDef>();
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RoofPlaceLocal, null, ResearchProjectDefOf.MountainsEmiter, false, "Place ... in Area", "Placing ... in Area", "Places Overhanging Mountains in 'Terraforming'-Area around. Existing Roofs will be replaced."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RoofPlaceGlobal, null, ResearchProjectDefOf.MountainsEmiter, false, "Place ... in Biome", "Placing ... in Biome", "Places Overhanging Mountains in Biome. Existing Roofs will be replaced."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RoofRemoveLocal, null, ResearchProjectDefOf.MountainsEmiter, false, "Remove ... in Area", "Removing ... in Area", "Removes Overhanging Mountains in 'Terraforming'-Area around."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.RoofRemoveGlobal, null, ResearchProjectDefOf.MountainsEmiter, false, "Remove ... in Biome", "Removing ... in Biome", "Removes Overhanging Mountains in Biome."));

				Helper.CreateDefaultEmiter(_Emiter.MountainsEmiter, typeof(RoofEmiter), _Emiter.MountainsEmiter, ResearchProjectDefOf.MountainsEmiter, recipes, 0, 0, "Mountains-Emiter", "");
			}
			else
				Helper.UpdateEmiter(_Emiter.MountainsEmiter);
		}

		public static ThingDef CreateUIThickRoof(TerrainDef baseTerrain)
		{
			if (baseTerrain == null || baseTerrain.defName == null)
				return null; // keine ui defs wenn terrain nicht vorher erstellt

			string baseStoneName = Helper.GetBaseStoneName(baseTerrain.defName);
			ThingDef tui = DefDatabase<ThingDef>.GetNamed(_Text.ThickRoof_ + baseStoneName, false);
			if (tui != null)
				return tui; // keine duplikate erstellen

			ThingDef baseStone = Helper.GetBaseStone(baseTerrain.defName);
			ThingDef chunk = Helper.GetChunk(baseTerrain.defName);
			if (baseStone == null || chunk == null)
				return null; // chunk oder stone exisitiert nicht oder kein echter stone

			#region naming

			tui = new ThingDef();
			tui.defName = _Text.ThickRoof_ + baseStoneName;
			if (TRMod.isGerman)
			{
				tui.label = "Dichtes Dach aus " + baseStone.label;
				tui.description = "Verdichte ein vorhandenes Dach mit " + baseStone.label + ". Achtung: Konstruktion erfordert eine freie Fläche. Alle Gegenstände unterhalb werden zerstört.";
			}
			else
			{
				tui.label = baseStoneName + " thick roof";
				tui.description = "Thicken an existing roof with " + baseStoneName + ". Warning: Construction needs a free spot. All builded objects underneath will be deconstructed!";
			}

			#endregion naming

			#region designation

			tui.designatorDropdown = DesignatorDropdownGroupDefOf.Build_Roofs;
			tui.designationCategory = DesignationCategoryDefOf.Terraform;

			#endregion designation

			#region basic

			tui.thingClass = typeof(TR_ThickRoof);
			tui.category = ThingCategory.Building;
			tui.altitudeLayer = AltitudeLayer.Skyfaller;
			tui.passability = Traversability.PassThroughOnly;
			tui.pathCost = 10;

			tui.placeWorkers = new List<Type>();
			tui.placeWorkers.Add(typeof(PlaceWorker_ThickRoof));
			tui.drawPlaceWorkersWhileSelected = true;

			tui.SetGraphicDataSingle(_Roof.IconThick, _Roof.IconThick);
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

			#endregion basic

			#region statbases

			tui.AddStatBase(StatDefOf.WorkToBuild, TRMod.OPTION_WorkValue);

			#endregion statbases

			#region research

			tui.researchPrerequisites = new List<ResearchProjectDef>();
			if (!TRMod.OPTION_VanillaLook)
				tui.researchPrerequisites.Add(ResearchProjectDefOf.MountainsTerraforming);
			ResearchProjectDefOf.MountainsTerraforming.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.MountainsTerraforming.description, tui.label);

			#endregion research

			#region costs

			UpdateUI_Costs(tui);

			#endregion costs

			#region mod extensions

			tui.modExtensions = new List<DefModExtension>();
			DME_ThickRoof dme = new DME_ThickRoof();
			dme.result = _Text.RoofRockThick;
			tui.modExtensions.Add(dme);

			#endregion mod extensions

			#region mod content

			Helper.SetContentPackToThisMod(tui);

			#endregion mod content

			tui.minifiedDef = TRMod.OPTION_ForceMinified ? ThingDefOf.MinifiedThing : null;
			tui.RegisterBuildingDef();

			return tui;
		}

		public static void Init()
		{
			dicRoofs = new SortedDictionary<string, TerrainDef>();
			dicUIRoofs = new SortedDictionary<string, ThingDef>();
			foreach (TerrainDef terrain in DefDatabase<TerrainDef>.AllDefs)
			{
				if (terrain != null && terrain.defName != null && Helper.IsStony(terrain.defName))
				{
					if (!dicRoofs.ContainsKey(terrain.label))
						dicRoofs.Add(terrain.label, terrain);
				}
			}

			foreach (TerrainDef terrain in dicRoofs.Values)
			{
				ThingDef uiThickRoof = CreateUIThickRoof(terrain);
				if (uiThickRoof != null && !dicUIRoofs.ContainsKey(uiThickRoof.label))
					dicUIRoofs.Add(uiThickRoof.label, uiThickRoof);
			}

			// updates
			foreach (ThingDef tui in dicUIRoofs.Values)
			{
				tui.UpdateStat(StatDefOf.WorkToBuild.defName, TRMod.OPTION_WorkValue);
				UpdateUI_Costs(tui);

				tui.ResolveReferences();
				tui.PostLoad();
			}
			DesignatorDropdownGroupDefOf.Build_Roofs.ResolveReferences();
			DesignatorDropdownGroupDefOf.Build_Roofs.PostLoad();

			//CreateRoofEmiter();
			if (TRMod.isDebug)
				Helper.ShowDialog("ThickRoofs: " + Helper.DicToString(dicUIRoofs));
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
				{    // has no custom cost
					tui.AddCosts(ThingDefOf.Steel, 5);
					ThingDef chunk = Helper.GetChunk(tui.defName.Replace(_Text.ThickRoof_, ""));
					if (chunk != null)
						tui.AddCosts(chunk, 1);
				}
			}
		}
	}

	public class DME_ThickRoof : DefModExtension
	{
		public string result;
	}

	public class PlaceWorker_ThickRoof : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			var rockyRoofToBuild = checkingDef.GetModExtension<DME_ThickRoof>();
			if (rockyRoofToBuild == null)
				return false;

			RoofDef rd = map.roofGrid.RoofAt(loc);
			if (rd == null)
			{
				string info = "Can't change the roof, because this tile has no roof";
				return new AcceptanceReport(info);
			}

			if (rd.defName == _Text.RoofRockThick)
			{
				string info = "This roof is already overhanging mountain";
				return new AcceptanceReport(info);
			}

			if (!TRMod.OPTION_PlaceWithoutRestrictions)
			{
				List<Thing> l = map.thingGrid.ThingsListAt(loc);
				foreach (Thing t in l)
				{
					if (t != null && t.def != null && t.def.category == ThingCategory.Building)
						return new AcceptanceReport("SpaceAlreadyOccupied".Translate());
				}
			}

			return true;
		}

		public override bool ForceAllowPlaceOver(BuildableDef other)
		{
			return true;
		}

		public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
		{
			base.PostPlace(map, def, loc, rot);
			if (TRMod.OPTION_InstantConstruction)
			{
				RoofDef rd = DefDatabase<RoofDef>.GetNamed(_Text.RoofRockThick, false);
				map.roofGrid.SetRoof(loc, rd);

				Designator_Cancel cancel = new Designator_Cancel();
				cancel.DesignateSingleCell(loc);
			}
		}
	}

	public class RoofEmiter : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
	{
		#region vars

		private Bill b;
		private CompHeatPusher chp;
		private CompRefuelable cpr;
		private int efficiency;
		private bool hasDoneBill;
		private int maxRange;
		#endregion vars

		#region constructor and overrides

		public RoofEmiter()
		{
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			HugsLibController.Instance.DistributedTicker.RegisterTickability(StartRoofEmiter, TRMod.OPTION_EmiterTick, this);
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

		#region place roof

		private void AddRoofs(bool global)
		{
			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestPlaceRoof(range, aTerraforming);
				if (CanPlaceRoof(posSpawn, aTerraforming))
				{
					base.Map.roofGrid.SetRoof(posSpawn, RoofDefOf.RoofRockThick);
					DoEffects(posSpawn);
					break;
				}
			}
		}

		private bool CanPlaceRoof(IntVec3 posSpawn, Area aTerraforming)
		{
			if (aTerraforming == null || aTerraforming[posSpawn])
				return (base.Map.roofGrid.RoofAt(posSpawn) != RoofDefOf.RoofRockThick);
			else
				return false;
		}

		private IntVec3 GetNearestPlaceRoof(int range, Area aTerraforming)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanPlaceRoof(posSpawn, aTerraforming));
			return posSpawn;
		}
		#endregion place roof

		#region remove roof

		private bool CanRemoveRoof(IntVec3 posSpawn, Area aTerraforming)
		{
			if (aTerraforming == null || aTerraforming[posSpawn])
				return (base.Map.roofGrid.RoofAt(posSpawn) == RoofDefOf.RoofRockThick);
			else
				return false;
		}

		private IntVec3 GetNearestRemoveRoof(int range, Area aTerraforming)
		{
			IntVec3 posSpawn;
			int distance = 1;
			do
			{
				if (!Helper.GetNextRandomCell(base.Position, base.Map, range, ref distance, out posSpawn))
					break;
			} while (!CanRemoveRoof(posSpawn, aTerraforming));
			return posSpawn;
		}

		private void RemoveRoofs(bool global)
		{
			Area aTerraforming = global ? null : Helper.GetTerraformingArea(base.Map);
			int range = global ? 0 : maxRange;
			for (int i = 0; i < efficiency; i++)
			{
				IntVec3 posSpawn = GetNearestRemoveRoof(range, aTerraforming);
				if (CanRemoveRoof(posSpawn, aTerraforming))
				{
					base.Map.roofGrid.SetRoof(posSpawn, null);
					DoEffects(posSpawn);
					break;
				}
			}
		}

		#endregion remove roof

		#endregion recipe func

		public void StartRoofEmiter()
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

					if (b.recipe.defName == _Recipe.RoofPlaceLocal)
					{
						AddRoofs(false);
					}
					else if (b.recipe.defName == _Recipe.RoofRemoveLocal)
					{
						RemoveRoofs(false);
					}
					else if (b.recipe.defName == _Recipe.RoofPlaceGlobal)
					{
						AddRoofs(true);
					}
					else if (b.recipe.defName == _Recipe.RoofRemoveGlobal)
					{
						RemoveRoofs(true);
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

	public class TR_ThickRoof : Building
	{
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);

			RoofDef rd = DefDatabase<RoofDef>.GetNamed(_Text.RoofRockThick, false);
			base.Map.roofGrid.SetRoof(base.Position, rd);

			Destroy(DestroyMode.Vanish);
		}
	}
}
