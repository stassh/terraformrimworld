using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TerraformRimworld
{
	public static class Helper
	{
		public static void SetContentPackToThisMod(ThingDef tui)
		{
			List<ModContentPack> runningModsListForReading = LoadedModManager.RunningModsListForReading;
			foreach (ModContentPack contentPack in runningModsListForReading)
			{
				if (contentPack.PackageId == TRMod.MODNAME ||
					contentPack.PackageId == TRMod.IDENTI ||
					contentPack.PackageId == TRMod.PACKID)
				{
					tui.modContentPack = contentPack;
				}
			}
		}

		public static int GetThingAmountNear(IntVec3 pos, Map map, ThingDef t)
		{
			int amount = 0;
			for (int x = -20; x < 20; x++)
			{
				for (int y = -20; y < 20; y++)
				{
					IntVec3 loc = new IntVec3(pos.x + x, pos.y, pos.z + y);
					List<Thing> lt = loc.GetThingList(map);
					foreach (Thing thing in lt)
					{
						if (thing != null && thing.def != null && thing.def.defName == t.defName)
						{
							amount += thing.stackCount;
							break;
						}
					}
				}
			}
			return amount;
		}


		public static void UpperLabel(ThingDef t)
		{
			string l = t.label;
			try
			{
				l = l.Substring(0, 1).ToUpper() + l.Substring(1);
			}
			catch
			{
				l = t.label;
			}
			t.label = l;
		}

		public static void ClearCell(IntVec3 pos, Map map, bool removePlantsOnly = false)
		{
			try
			{
				Dictionary<int, Thing> dic = new Dictionary<int, Thing>();
				foreach (Thing t in pos.GetThingList(map))
					dic.Add(dic.Count, t);

				foreach (int key in dic.Keys)
				{
					Thing t = dic[key];
					if (t != null && t.def != null && t.def.category != ThingCategory.Pawn)
					{
						if (removePlantsOnly)
						{
							if (t.IsPlant())
								t.Destroy(DestroyMode.Vanish);
						}
						else
						{
							if (t.def.destroyable)
								t.Destroy(DestroyMode.Vanish);
							else if (t.def.category == ThingCategory.Building)
								t.DeSpawn(DestroyMode.Vanish);
						}
					}
				}
			}
			catch { }
		}

		public static string GetNewTuiDescription(string dsc, string thingLabel)
		{
			string count = dsc.SubstringFrom("(").SubstringTo(")");
			int iCount = 0;
			int.TryParse(count, out iCount);			
			dsc = dsc.Replace("(" + count + ")", "(" + (iCount + 1).ToString() + ")");
			return dsc;// + "\n" + thingLabel;
		}

		public static void DoCustomTectonicPulse(Map map)
		{
			if (TRMod.zufallswert.Next(0, 10) == 5)
			{
				foreach (IntVec3 c in map.AllCells)
				{
					TerrainDef ter = c.GetTerrain(map);
					if (ter != null)
					{
						if (ter.defName.StartsWith("Lava"))
						{
							int chance = TRMod.zufallswert.Next(110);
							if (chance > 109 - TRMod.OPTION_ChanceOfFire && chance < 110)
							{
								Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire, null);
								fire.fireSize = Rand.Range(0.1f, TRMod.OPTION_MaxSizeFire);
								int direction = Rand.Range(1, 5);
								Rot4 rotf;
								if (direction == 2)
									rotf = Rot4.South;
								else if (direction == 3)
									rotf = Rot4.East;
								else if (direction == 4)
									rotf = Rot4.West;
								else
									rotf = Rot4.North;

								GenSpawn.Spawn(fire, c, map, rotf);
								if (fire.fireSize > 0.8f)
								{
									Vector3 v = new Vector3(c.x, c.y, c.z);
									FleckMaker.ThrowMicroSparks(v, map);
								}
							}
							else if (chance > 99 - TRMod.OPTION_ChanceOfHeatGlow && chance < 100)
							{
								float size = TRMod.OPTION_MaxSizeHeatGlow / (float)TRMod.zufallswert.Next(1, 100);
								FleckMaker.ThrowHeatGlow(c, map, size);
								if (size > 0.8f)
								{
									Vector3 v = new Vector3(c.x, c.y, c.z);
									FleckMaker.ThrowMicroSparks(v, map);
								}
							}
							else if (chance > 89 - TRMod.OPTION_ChanceOfFireGlow && chance < 90)
							{
								Vector3 v = new Vector3(c.x, c.y, c.z);
								float size = TRMod.OPTION_MaxSizeFireGlow / (float)TRMod.zufallswert.Next(1, 100);
								if (size > 0.8f)
								{
									FleckMaker.ThrowFireGlow(v, map, size);
									FleckMaker.ThrowMicroSparks(v, map);
								}
							}
							else if (chance == 77)
							{
								Vector3 v = new Vector3(c.x, c.y, c.z);
								FleckMaker.ThrowMicroSparks(v, map);
							}
							else if (chance > 69 - TRMod.OPTION_ChanceOfLightningGlow && chance < 70)
							{
								Vector3 v = new Vector3(c.x, c.y, c.z);
								float size = TRMod.OPTION_MaxSizeLightningGlow / (float)TRMod.zufallswert.Next(1, 100);
								FleckMaker.ThrowLightningGlow(v, map, size);
								FleckMaker.ThrowMicroSparks(v, map);
							}
							else if (chance > 59 - TRMod.OPTION_ChanceOfDustPuff && chance < 60)
							{
								Vector3 v = new Vector3(c.x, c.y, c.z);
								float size = Rand.Range(0.05f, TRMod.OPTION_MaxSizeDustPuff);
								FleckMaker.ThrowDustPuff(v, map, size);
							}
							else if (chance > 49 - TRMod.OPTION_ChanceOfAirPuff && chance < 50)
							{
								Vector3 v = new Vector3(c.x, c.y, c.z);
								FleckMaker.ThrowAirPuffUp(v, map);
							}
							else if (chance > 39 - TRMod.OPTION_ChanceOfDustPuffthick && chance < 40)
							{
								Vector3 v = new Vector3(c.x, c.y, c.z);
								float size = Rand.Range(0.05f, TRMod.OPTION_MaxSizeDustPuffThick);
								FleckMaker.ThrowDustPuffThick(v, map, size, Color.black);
							}
							else if (chance > 29 - TRMod.OPTION_ChanceOfMetaPuff && chance < 30)
							{
								Vector3 v = new Vector3(c.x, c.y, c.z);
								FleckMaker.ThrowMetaPuff(v, map);
							}
							else if (chance > 19 - TRMod.OPTION_ChanceOfSmoke && chance < 20)
							{
								Vector3 v = new Vector3(c.x, c.y, c.z);
								float size = Rand.Range(0.05f, TRMod.OPTION_MaxSizeSmoke);
								FleckMaker.ThrowSmoke(v, map, size);
							}
							else if (chance > 9 - TRMod.OPTION_ChanceOfTornadoPuff && chance < 10)
							{
								Vector3 v = new Vector3(c.x, c.y, c.z);
								float size = Rand.Range(0.05f, TRMod.OPTION_MaxSizeTornadoPuff);
								FleckMaker.ThrowTornadoDustPuff(v, map, size, Color.black);
							}
						}
					}
				}				
			}
		}

		public static void CreateStormyAshRain()
		{
			string name = "STORMYASHRAIN";
			WeatherDef test = DefDatabase<WeatherDef>.GetNamed(name, false);
			if (test != null)
				return;

			WeatherDef rts = new WeatherDef();
			rts.defName = name;
			rts.label = "Stormy Ash Rain"; // + trueRainyThunderstorm.label;
			rts.description = "Stormy Ash Rain"; // trueRainyThunderstorm.description;
			rts.temperatureRange.min = 0;
			rts.temperatureRange.max = 999;
			rts.windSpeedFactor = 3.5f;
			rts.favorability = Favorability.Bad;
			rts.perceivePriority = 2.0f;
			rts.exposedThought = DefDatabase<ThoughtDef>.GetNamed("SoakingWet", false);
			rts.accuracyMultiplier = 0.5f;
			rts.rainRate = 1.0f;
			rts.snowRate = 1.5f;
			rts.moveSpeedMultiplier = 0.9f;
			rts.ambientSounds = new List<SoundDef>();
			SoundDef snd1 = DefDatabase<SoundDef>.GetNamed("Ambient_Wind_Storm", false);
			SoundDef snd2 = DefDatabase<SoundDef>.GetNamed("Ambient_Rain", false);
			SoundDef snd3 = DefDatabase<SoundDef>.GetNamed("Ambient_Wind_Fog", false);
			rts.ambientSounds.Add(snd1);
			rts.ambientSounds.Add(snd2);
			rts.ambientSounds.Add(snd3);
			rts.durationRange.min = 15000;
			rts.durationRange.max = 40000;
			rts.eventMakers = new List<WeatherEventMaker>();
			WeatherEventMaker event1 = new WeatherEventMaker();
			event1.averageInterval = 100;
			event1.eventClass = typeof(WeatherEvent_LightningFlash);
			WeatherEventMaker event2 = new WeatherEventMaker();
			event2.averageInterval = 100;
			event2.eventClass = typeof(WeatherEvent_LightningStrike);
			WeatherEventMaker event3 = new WeatherEventMaker();
			event3.averageInterval = 300;
			event3.eventClass = typeof(WeatherEvent_LightningFlash);
			WeatherEventMaker event4 = new WeatherEventMaker();
			event4.averageInterval = 500;
			event4.eventClass = typeof(WeatherEvent_LightningStrike);
			rts.eventMakers.Add(event1);
			rts.eventMakers.Add(event2);
			rts.eventMakers.Add(event3);
			rts.eventMakers.Add(event4);
			rts.overlayClasses = new List<Type>();
			rts.overlayClasses.Add(typeof(WeatherOverlay_Rain));
			rts.overlayClasses.Add(typeof(WeatherOverlay_Fallout));
			rts.overlayClasses.Add(typeof(WeatherOverlay_Fog));
			rts.overlayClasses.Add(typeof(WeatherOverlay_SnowHard));
			rts.commonalityRainfallFactor = new SimpleCurve();
			List<CurvePoint> lc = new List<CurvePoint>();
			lc.Add(new CurvePoint(new Vector2(0, 0)));
			lc.Add(new CurvePoint(new Vector2(300, 0.5f)));
			lc.Add(new CurvePoint(new Vector2(1300, 1.0f)));
			lc.Add(new CurvePoint(new Vector2(4000, 2.0f)));
			rts.commonalityRainfallFactor.SetPoints(lc);
			
			Color col = new Color(0.92f, 0.92f, 0.92f);
			rts.skyColorsDay = new SkyColorSet();
			rts.skyColorsDay.sky = col;
			rts.skyColorsDay.shadow = new Color(0.92f, 0.92f, 0.92f);
			rts.skyColorsDay.overlay = col;
			rts.skyColorsDay.saturation = 0.9f;

			rts.skyColorsDusk = new SkyColorSet();
			rts.skyColorsDusk.sky = GetNewReducedColor(rts.skyColorsDay.sky);
			rts.skyColorsDusk.shadow = new Color(0.92f, 0.92f, 0.92f);
			rts.skyColorsDusk.overlay = GetNewReducedColor(rts.skyColorsDay.sky);
			rts.skyColorsDusk.saturation = 0.9f;

			rts.skyColorsNightEdge = new SkyColorSet();
			rts.skyColorsNightEdge.sky = GetNewReducedColor(rts.skyColorsDusk.sky);
			rts.skyColorsNightEdge.shadow = new Color(0.92f, 0.92f, 0.92f);
			rts.skyColorsNightEdge.overlay = GetNewReducedColor(rts.skyColorsDusk.sky);
			rts.skyColorsNightEdge.saturation = 0.9f;

			rts.skyColorsNightMid = new SkyColorSet();
			rts.skyColorsNightMid.sky = GetNewReducedColor(rts.skyColorsNightEdge.sky);
			rts.skyColorsNightMid.shadow = new Color(0.92f, 0.92f, 0.92f);
			rts.skyColorsNightMid.overlay = GetNewReducedColor(rts.skyColorsNightEdge.sky);
			rts.skyColorsNightMid.saturation = 0.9f;

			rts.ResolveReferences();
			rts.PostLoad();

			DefDatabase<WeatherDef>.Add(rts);
		}

		public static IncidentParms GetRandomIncidentParams(string letter, string letterlabel)
		{
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = Find.CurrentMap;
			incidentParms.points = TRMod.zufallswert.Next(100, 10000);
			incidentParms.forced = true;
			incidentParms.customLetterText = letter;
			incidentParms.customLetterLabel = letterlabel;
			return incidentParms;
		}
		public static void StartIncidentByName(string name, string letter, string letterlabel)
		{
			IncidentDef i = DefDatabase<IncidentDef>.GetNamed(name, false);
			if (i != null)
				i.Worker.TryExecute(GetRandomIncidentParams(letter, letterlabel));
		}

		public static bool CellContainsFilth(IntVec3 posSpawn, Map map, List<ThingDef> l, Area aTerraforming)
		{
			bool isFilthy = false;
			if (aTerraforming == null || aTerraforming[posSpawn])
			{
				List<Thing> thingList = posSpawn.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Filth filth = thingList[i] as Filth;
					if (filth != null)
					{
						isFilthy = true;
						break;
					}
				}
			}
			return isFilthy;
		}

		public static bool GetNextRandomCell(IntVec3 center, Map map, int maxRange, ref int distance, out IntVec3 posSpawn)
		{
			do
			{
				posSpawn = Helper.GetRandomCell(center, (-1) * distance, distance);
			}
			while (!posSpawn.InBounds(map));
			distance++;
			if (maxRange > 0)
			{
				if (distance > maxRange)
				{
					return false;
				}
			}
			else
			{
				if (distance > TRMod.OPTION_EmiterRandomDistance)
				{
					posSpawn = Helper.GetRandomCell(map);
					return false;
				}
			}
			return true;
		}

		public static Color GetNewReducedColor(Color original)
		{
			Color col = new Color();
			col.r = (original.r - 0.1f) > 0.0f ? (original.r - 0.1f) : (original.r);
			col.g = (original.g - 0.1f) > 0.0f ? (original.g - 0.1f) : (original.g);
			col.b = (original.b - 0.1f) > 0.0f ? (original.b - 0.1f) : (original.b);
			return col;
		}

		public static Color GetColorOf(ThingDef thing, bool isStone)
		{
			Color col = new Color();
			if (isStone && thing.graphicData != null)
			{
				col = thing.graphicData.color;
			}
			else
			{ 
				if (thing.thingCategories != null && thing.thingCategories.Count > 0 && thing.thingCategories[0] == ThingCategoryDefOf.ResourcesRaw)
				{
					if (thing.stuffProps != null && thing.stuffProps.color != null)
					{
						col = thing.stuffProps.color;
					}
				}
				else if (thing.stuffProps != null && thing.stuffProps.color != null)
				{
					col = thing.stuffProps.color;
				}
			}
			return col;
		}

        public static IntVec3 GetRandomCell(Map map)
        {
            IntVec3 size = map.Size;
            int xCell = TRMod.zufallswert.Next(1, size.x - 1);
            int zCell = TRMod.zufallswert.Next(1, size.z - 1);
            IntVec3 cell = new IntVec3(xCell, 0, zCell);
            return cell;
        }

        public static ThingDef GetRandomListElement(List<ThingDef> l)
        {
            if (l == null || l.Count == 0)
                return null;
            int randomIndex = TRMod.zufallswert.Next(0, l.Count);
            return l[randomIndex];
        }

		public static TerrainDef GetRandomListElement(List<TerrainDef> l)
		{
			if (l == null || l.Count == 0)
				return null;
			int randomIndex = TRMod.zufallswert.Next(0, l.Count);
			return l[randomIndex];
		}

		public static void CreateGeysirReplacer(ResearchProjectDef research)
		{
			string defName = "SZTR_SteamGeyser";
			ThingDef geyser = DefDatabase<ThingDef>.GetNamed(defName, false);
			ThingDef baseGeyser = DefDatabase<ThingDef>.GetNamed("SteamGeyser", false);
			if (geyser != null || baseGeyser == null)
			{   // already exists
				return;
			}
			else
			{   // create one
				try
				{
					#region naming
					geyser = new ThingDef();
					geyser.defName = defName;
					geyser.label = baseGeyser.label;
					geyser.description = baseGeyser.label;
					#endregion

					#region designation
					geyser.designatorDropdown = DesignatorDropdownGroupDefOf.Anomalies;
					geyser.designationCategory = DesignationCategoryDefOf.Terraform;
					#endregion

					#region basic
					geyser.thingClass = typeof(TectonicGeyser);
					geyser.category = ThingCategory.Building;
					geyser.altitudeLayer = AltitudeLayer.Building;
					geyser.passability = Traversability.PassThroughOnly;

					geyser.placeWorkers = new List<Type>();
					geyser.placeWorkers.Add(typeof(PlaceWorker_Geyser));
					geyser.drawPlaceWorkersWhileSelected = true;

					string texPath = baseGeyser.graphicData.texPath;
					geyser.SetGraphicDataSingle(texPath, texPath);
					geyser.size = new IntVec2(4, 4);
					geyser.graphicData.drawSize = new Vector2(4, 4);
					geyser.placingDraggableDimensions = 1;

					geyser.rotatable = false;
					geyser.selectable = true;
					geyser.useHitPoints = true;
					geyser.leaveResourcesWhenKilled = false;
					geyser.destroyable = true;
					geyser.blockWind = true;
					geyser.blockLight = true;

					geyser.building = new BuildingProperties();

					geyser.terrainAffordanceNeeded = TerrainAffordanceDefOf.Heavy;
					geyser.constructionSkillPrerequisite = 5;

					geyser.constructEffect = EffecterDefOf.ConstructDirt;
					geyser.soundImpactDefault = SoundDefOf.BulletImpact_Ground;

					geyser.tickerType = TickerType.Never;
					#endregion

					#region statbases
					geyser.AddStatBase(StatDefOf.WorkToBuild, TRMod.OPTION_WorkValue * 5);
					#endregion

					#region research
					geyser.researchPrerequisites = new List<ResearchProjectDef>();
					if (!TRMod.OPTION_VanillaLook)
						geyser.researchPrerequisites.Add(research);
					ResearchProjectDefOf.TectonicTerraforming.description = Helper.GetNewTuiDescription(ResearchProjectDefOf.TectonicTerraforming.description, geyser.label);
					#endregion

					#region costlist
					geyser.costList = new List<ThingDefCountClass>();
					if (TRMod.OPTION_CostsEnabled)
					{
						geyser.costList.Add(new ThingDefCountClass(ThingDefOf.ComponentIndustrial, 1));
						geyser.costList.Add(new ThingDefCountClass(ThingDefOf.Steel, 80));
						geyser.costList.Add(new ThingDefCountClass(ThingDefOf.WoodLog, 20));						
					}
					#endregion

					geyser.RegisterBuildingDef();
				}
				catch (Exception e)
				{
					if (TRMod.isDebug)
						Helper.ShowDialog(e.ToString());
				}
			}
		}

        public static void CreateDefaultEmiter(string defName, Type thingClass, string textur, ResearchProjectDef research, List<RecipeDef> recipes, float energyCosts = 0, float uraniumCosts = 0, string dlabel = "", string ddsc = "")
        {
            ThingDef emiter = DefDatabase<ThingDef>.GetNamed(defName, false);
            if (emiter != null)
            {   // already exists
                return;
            }
            else
            {   // create one
                try
                {				
                    #region naming
                    emiter = new ThingDef();
                    emiter.defName = defName;
					emiter.label = TRMod.OPTION_KeyedLanguange ? (defName + "LBL").Translate().ToString() : dlabel;
                    emiter.description = TRMod.OPTION_KeyedLanguange ? (defName + "DSC").Translate().ToString() : ddsc;
                    #endregion

                    #region designation
                    emiter.designatorDropdown = DesignatorDropdownGroupDefOf.Emiter;
                    emiter.designationCategory = DesignationCategoryDefOf.Terraform;
                    #endregion

                    #region basic
                    emiter.thingClass = thingClass;
                    emiter.category = ThingCategory.Building;
                    emiter.altitudeLayer = AltitudeLayer.Building;
                    emiter.passability = Traversability.Impassable;
                    emiter.thingCategories = new List<ThingCategoryDef>();
                    emiter.thingCategories.Add(ThingCategoryDef.Named(_ThingCategory.BuildingMisc));
					ThingCategoryDef.Named(_ThingCategory.BuildingMisc).childThingDefs.Add(emiter);

                    emiter.placeWorkers = new List<Type>();
                    emiter.placeWorkers.Add(typeof(PlaceWorker_Heater));
                    emiter.drawPlaceWorkersWhileSelected = true;

					string texPath = TRMod.OPTION_AlternativeIcon ? "__" + textur : textur;
					emiter.SetGraphicDataSingle(texPath, texPath);
                    emiter.size = new IntVec2(1, 1);
                    emiter.placingDraggableDimensions = 1;

                    emiter.rotatable = false;
                    emiter.selectable = true;
                    emiter.useHitPoints = true;
                    emiter.leaveResourcesWhenKilled = false;
                    emiter.destroyable = true;
                    emiter.blockWind = true;
                    emiter.blockLight = true;

                    emiter.building = new BuildingProperties();
					emiter.building.claimable = true;
                    emiter.building.alwaysDeconstructible = true;
					emiter.building.soundAmbient = SoundDef.Named("GeothermalPlant_Ambience");					

                    emiter.terrainAffordanceNeeded = TerrainAffordanceDefOf.Light;
                    emiter.constructionSkillPrerequisite = 5;

                    emiter.constructEffect = EffecterDefOf.ConstructMetal;
                    emiter.soundImpactDefault = DefDatabase<SoundDef>.GetNamed(_Sound.BulletImpact_Metal, false);
                    emiter.repairEffect = EffecterDefOf.ConstructMetal;

                    //emiter.hasInteractionCell = true;
                    //emiter.interactionCellOffset = new IntVec3(0, 0, -1);

                    emiter.tickerType = TickerType.Never;
					#endregion

					#region comps
					emiter.fillPercent = 0.5f;
					if (energyCosts == 0.0f)
                    {                
                        emiter.comps = new List<CompProperties>();
                        CompProperties_Refuelable cpr = new CompProperties_Refuelable();
                        cpr.fuelConsumptionRate = 10.0f;
                        cpr.fuelCapacity = 100.0f;
                        cpr.fuelConsumptionPerTickInRain = 0.001f;
                        cpr.fuelFilter = new ThingFilter();
						if (uraniumCosts != 0.0f)
						{
							cpr.fuelFilter.AllowedThingDefs.AddItem(ThingDefOf.Uranium);
							cpr.fuelFilter.SetAllow(ThingDefOf.Uranium, true);
						}
						else
						{
							cpr.fuelFilter.AllowedThingDefs.AddItem(ThingDefOf.Chemfuel);
							cpr.fuelFilter.SetAllow(ThingDefOf.Chemfuel, true);
						}
                        cpr.initialFuelPercent = 1.0f;
                        CompProperties_HeatPusher chp = new CompProperties_HeatPusher();
                        chp.heatPerSecond = 21;
                        chp.heatPushMaxTemperature = 60;
                        emiter.comps.Add(cpr);
                        emiter.comps.Add(chp);
                        cpr.ResolveReferences(emiter);
                        chp.ResolveReferences(emiter);
                    }
                    else
                    {
                        emiter.comps = new List<CompProperties>();
                        CompProperties_Power cpr = new CompProperties_Power();
                        cpr.basePowerConsumption = energyCosts;
                        cpr.compClass = typeof(CompPowerTrader);
                        cpr.shortCircuitInRain = false;
                        cpr.transmitsPower = false;
                        emiter.comps.Add(cpr);
                        cpr.ResolveReferences(emiter);					
                    }
                    #endregion

                    #region statbases
                    emiter.AddStatBase(StatDefOf.MaxHitPoints, 120);
                    emiter.AddStatBase(StatDefOf.WorkToBuild, TRMod.OPTION_WorkValue);
                    emiter.AddStatBase(StatDefOf.Flammability, 0.2f);
                    #endregion

                    #region research
                    emiter.researchPrerequisites = new List<ResearchProjectDef>();
					if (!TRMod.OPTION_VanillaLook)
						emiter.researchPrerequisites.Add(research);

					List<ResearchProjectDef> lOfResearches = new List<ResearchProjectDef>();
					foreach (RecipeDef recipe in recipes)
					{
						if (recipe.researchPrerequisite != null && !lOfResearches.Contains(recipe.researchPrerequisite))
							lOfResearches.Add(recipe.researchPrerequisite);
					}
					foreach (ResearchProjectDef rpd in lOfResearches)
					{
						int iCount = 0;
						string rcpNames = "";
						foreach (RecipeDef rd in recipes)
						{
							if (rd.researchPrerequisite == rpd)
							{
								iCount++;
								rcpNames += "\n" + rd.label;
							}
						}
						string count = rpd.description.SubstringFrom("(").SubstringTo(")");
						rpd.description = rpd.description.Replace("(" + count + ")", "(" + iCount.ToString() + ")") + rcpNames;
					}
					#endregion

					#region costs
					Helper.AddDefaultEmiterCosts(emiter);
                    #endregion

                    #region recipes
                    emiter.recipes = new List<RecipeDef>();
                    foreach (RecipeDef recipe in recipes)
                        emiter.recipes.Add(recipe);                    
                    emiter.inspectorTabs = new List<Type>();
                    emiter.inspectorTabs.Add(typeof(ITab_Bills));
                    emiter.surfaceType = SurfaceType.Item;					
					#endregion

					#region mod content
					Helper.SetContentPackToThisMod(emiter);
					#endregion

					#region icon color
					emiter.uiIconColor = Color.white;
					#endregion
					
					emiter.minifiedDef = ThingDefOf.MinifiedThing;
                    emiter.RegisterBuildingDef();
                }
                catch (Exception e)
                {
					if (TRMod.isDebug)
                        Helper.ShowDialog(e.ToString());
                }
            }
        }

		public static void ShowMessage(string info)
		{
			Messages.Message(info, MessageTypeDefOf.SilentInput, false);
		}

        public static RecipeDef CreateDefaultRecipe(string defName, List<ThingCategoryDef> lCat, ResearchProjectDef research, bool notKeyed, string label = "", string job = "", string dsc = "")
		{
			RecipeDef newRd = DefDatabase<RecipeDef>.GetNamed(defName, false);
			if (newRd == null)
			{
				newRd = new RecipeDef();
				newRd.defName = defName;
				newRd.label = (TRMod.OPTION_KeyedLanguange && (notKeyed == false)) ? (defName + "LBL").Translate().ToString() : label;
				newRd.description = (TRMod.OPTION_KeyedLanguange && (notKeyed == false)) ? (defName + "DSC").Translate().ToString() : dsc;
				newRd.jobString = (TRMod.OPTION_KeyedLanguange && (notKeyed == false)) ? (defName + "JOB").Translate().ToString() : job;
				newRd.workAmount = 2000;
				newRd.workSpeedStat = StatDefOf.SmoothingSpeed;
				newRd.effectWorking = EffecterDefOf.Drill;
				newRd.soundWorking = SoundDefOf.EnergyShield_Reset;
				newRd.targetCountAdjustment = 20;
				newRd.ingredients = new List<IngredientCount>();
				IngredientCount ic = new IngredientCount();
				ic.filter.allowedHitPointsConfigurable = false;
				ic.filter.allowedQualitiesConfigurable = false;
				if (lCat != null)
				{
					foreach (ThingCategoryDef tcd in lCat)
					{
						ic.filter.SetAllow(tcd, true);
					}
				}				
				newRd.workSkill = SkillDefOf.Crafting;
				newRd.fixedIngredientFilter = new ThingFilter();
				if (lCat != null)
				{
					foreach (ThingCategoryDef tcd in lCat)
					{
						newRd.fixedIngredientFilter.SetAllow(tcd, true);
					}
				}				
				newRd.ingredients.Add(ic);
				newRd.ClearCachedData();			
				newRd.fixedIngredientFilter.allowedHitPointsConfigurable = false;
				newRd.fixedIngredientFilter.allowedQualitiesConfigurable = false;
				newRd.fixedIngredientFilter.ResolveReferences();
				ic.filter.ResolveReferences();				

				newRd.products = new List<ThingDefCountClass>();
				newRd.products.Add(new ThingDefCountClass(ThingDefOf.Filth_Ash, 1));
                if (research != null && !TRMod.OPTION_VanillaLook)
                    newRd.researchPrerequisite = research;

				newRd.ResolveReferences();
				newRd.PostLoad();
				
				DefDatabase<RecipeDef>.Add(newRd);
			}
			return newRd;
		}

        public static void ResolveCustomCosts()
        {
            TRMod.dicCustomCosts = new Dictionary<string, ThingDefCountClass>();
			if (!String.IsNullOrEmpty(TRMod.OPTION_CostCustom))
            {   // example ThickRoof|Steel(5) Soil|WoodLog(1);Water|0;
				string custom = TRMod.OPTION_CostCustom.Trim().Replace(" ", "");
				string[] split = custom.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in split)
                {
                    string[] keyValue = s.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    if (keyValue.Length > 1)
                    {
                        string thingName = keyValue[0];
                        string thingCostName = keyValue[1].SubstringTo("(");
                        string thingCostValue = keyValue[1].SubstringFrom("(").SubstringTo(")");

                        ThingDef t = DefDatabase<ThingDef>.GetNamed(thingCostName, false);
                        int val = 0;
                        int.TryParse(thingCostValue, out val);
                        if (t != null && val > 0)
                            TRMod.dicCustomCosts.Add(thingName, new ThingDefCountClass(t, val));
                        else
                            TRMod.dicCustomCosts.Add(thingName, new ThingDefCountClass());
                    }					
                }
			}
        }

        public static bool IsStony(string defName)
        {
            return (defName.EndsWith("_Smooth") || defName.EndsWith("_RoughHewn") || defName.EndsWith("_Rough"));
        }

        public static string GetBaseStoneName(string defName)
        {
            return IsStony(defName) ? defName.Replace("_Smooth", "").Replace("_RoughHewn", "").Replace("_Rough", "") : defName;
        }

        public static ThingDef GetBaseStone(string defName)
        {
            ThingDef baseStone = null;
            string baseStoneName = GetBaseStoneName(defName);
            if (!String.IsNullOrEmpty(baseStoneName))
                baseStone = DefDatabase<ThingDef>.GetNamed(baseStoneName, false);

            return baseStone;
        }

        public static ThingDef GetChunk(string defName)
        {
            ThingDef chunk = null;
            string baseStoneName = GetBaseStoneName(defName);
            if (!String.IsNullOrEmpty(baseStoneName))
                chunk = DefDatabase<ThingDef>.GetNamed("Chunk" + baseStoneName, false);

            return chunk;
        }

		public static ThingDef GetBlocks(string defName)
		{
			ThingDef blocks = null;
			string baseStoneName = GetBaseStoneName(defName);
			if (!String.IsNullOrEmpty(baseStoneName))
				blocks = DefDatabase<ThingDef>.GetNamed("Blocks" + baseStoneName, false);

			return blocks;
		}

        public static bool AddCustomCosts(ThingDef tui)
        {
            tui.costList = new List<ThingDefCountClass>();
            foreach (string s in TRMod.dicCustomCosts.Keys)
            {
                if (tui.defName.Contains(s))
                {
                    ThingDefCountClass tdc = TRMod.dicCustomCosts[s];
                    if (tdc != null && tdc.count > 0)
                    {
                        tui.AddCosts(tdc.thingDef, tdc.count);
                    }
					return true;
                }
            }
			return false;
        }

        public static void AddDefaultEmiterCosts(ThingDef emiter)
        {
            emiter.costList = new List<ThingDefCountClass>();
            if (TRMod.OPTION_CostsEnabled)
            {
                if (emiter.defName == _Emiter.TectonicEmiter)
                {
                    emiter.costList.Add(new ThingDefCountClass(ThingDefOf.ComponentIndustrial, 2));
                    emiter.costList.Add(new ThingDefCountClass(ThingDefOf.Steel, 120));
                }
                else
                {
					if (emiter.defName == _Emiter.ReplicatorPrototyp)
					{
						emiter.costList.Add(new ThingDefCountClass(ThingDefOf.Uranium, 100));
					}
					else
					{
						emiter.costList.Add(new ThingDefCountClass(ThingDefOf.Chemfuel, 100));
						emiter.costList.Add(new ThingDefCountClass(ThingDefOf.Uranium, 10));
					}
					emiter.costList.Add(new ThingDefCountClass(ThingDefOf.ComponentIndustrial, 20));
                    emiter.costList.Add(new ThingDefCountClass(ThingDefOf.Gold, 5));                    
                    emiter.costList.Add(new ThingDefCountClass(ThingDefOf.Plasteel, 66));
                    emiter.costList.Add(new ThingDefCountClass(ThingDefOf.Steel, 50));
                }
            }
        }

        public static void UpdateEmiter(string defName)
        {
            ThingDef emiter = DefDatabase<ThingDef>.GetNamed(defName, false);
            if (emiter != null)
            {
                emiter.UpdateStat(StatDefOf.WorkToBuild.defName, TRMod.OPTION_WorkValue);
                Helper.AddDefaultEmiterCosts(emiter);
                emiter.ResolveReferences();
                emiter.PostLoad();
            }
        }

        public static bool IsNotDesignatedUnknownTerrain(TerrainDef ter)
        {
            if (ter.designationCategory == null)
            {
                var sourceUIdef = DefDatabase<ThingDef>.GetNamed(_Text.T_ + ter.defName, false);
                if (!TerraEmiterManager.dicUITerrain.ContainsValue(sourceUIdef))
                    return true;
            }
            return false;
        }

        public static bool PlaceHasBuilding(Map map, IntVec3 loc)
        {
            List<Thing> l = map.thingGrid.ThingsListAt(loc);
            foreach (Thing t in l)
            {
                if (t != null && t.def != null && t.def.category == ThingCategory.Building)
                    return true;
            }
            return false;
        }




        public static string GetStoneName(ThingDef td)
        {
            return IsStony(td) ? td.defName.Replace(_Text._Smooth, "").Replace(_Text._RoughHewn, "").Replace(_Text._Rough, "") : td.defName;
        }

        public static string GetStoneName(TerrainDef terd)
        {
            return IsStony(terd) ? terd.defName.Replace(_Text._Smooth, "").Replace(_Text._RoughHewn, "").Replace(_Text._Rough, "") : terd.defName;
        }

        public static bool IsStony(ThingDef td)
        {
            if (td == null || td.defName == null)
                return false;
            return (td.defName.EndsWith(_Text._Smooth) || td.defName.EndsWith(_Text._RoughHewn) || td.defName.EndsWith(_Text._Rough));
        }

        public static bool IsStony(TerrainDef terd)
        {
            if (terd == null || terd.defName == null)
                return false;
            return (terd.defName.EndsWith(_Text._Smooth) || terd.defName.EndsWith(_Text._RoughHewn) || terd.defName.EndsWith(_Text._Rough));
        }

		public static string DicToString(SortedDictionary<string, TerrainDef> dic)
		{
			string ret = dic.Count() + "\n";
			foreach (string s in dic.Keys)
				ret += s + ", ";
			if (ret.EndsWith(","))
				ret = ret.Substring(0, ret.Length - 1);
			return ret;
		}

		public static string DicToString(SortedDictionary<string, ThingDef> dic)
		{
			string ret = dic.Count() + "\n";
			foreach (string s in dic.Keys)
				ret += s + ", ";
			if (ret.EndsWith(","))
				ret = ret.Substring(0, ret.Length - 1);
			return ret;
		}

        public static string ListToString(List<ThingDef> l)
		{
			string s = l.Count.ToString() + "\n";
			foreach (ThingDef t in l)
				s += t.label + ", ";
			return s;
		}

		public static string ListToString(List<TerrainDef> l)
		{
			string s = l.Count.ToString() + "\n";
			foreach (TerrainDef t in l)
				s += t.label + ", ";
			return s;
		}

		public static void ShowDialog(string s, bool doRestart = false)
		{
            if (doRestart)
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(s, delegate 
				{
					GameDataSaveLoader.SaveGame("autoterraformrimworld");
					GenCommandLine.Restart();
				}, false, null));
            }
            else
            {
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(s, null, false, null));
            }
		}

		public static void ShowReloadDialog(string s)
		{
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(s, delegate
			{
				GameDataSaveLoader.SaveGame("autoterraformrimworld");
				GameDataSaveLoader.LoadGame("autoterraformrimworld");

			}, false, null));
		}

		public static int GetEmiterRange()
		{
            int maxRange = 4;
			if (ResearchProjectDefOf.EmiterRange15.IsFinished)
				maxRange = 15;
			else if (ResearchProjectDefOf.EmiterRange7.IsFinished)
				maxRange = 7;
			else
				maxRange = 4;

            return maxRange + 1;
		}

		public static List<TerrainDef> AllTerrains(this BiomeDef biome)
		{
			if (biome == null)
				return new List<TerrainDef>();
			List<TerrainDef> l1 = Helper.GetListOfTerrainDefsFromThreshold(biome.terrainsByFertility);
			List<TerrainDef> l2 = Helper.GetListOfTerrainDefsFromPatchMaker(biome.terrainPatchMakers);
			foreach (TerrainDef terrain in l2)
				l1.Add(terrain);
			return l1;
		}

		public static List<TerrainDef> GetTerrainListsFromSelected(List<ThingDef> lSelected, out List<TerrainDef> lFrom)
		{
			List<TerrainDef> l = new List<TerrainDef>();
			lFrom = new List<TerrainDef>();
			foreach (ThingDef tselected in lSelected)
			{
				if (tselected.defName.StartsWith(_Text.T_))
				{   // tui terrain
					TerrainDef terrain = DefDatabase<TerrainDef>.GetNamed(tselected.defName.Replace(_Text.T_, ""), false);
					if (terrain != null)
						l.Add(terrain);

				}
				else if (tselected.defName.StartsWith(_Text.FromTerrain_))
				{   // from base terrain
					TerrainDef terrain = DefDatabase<TerrainDef>.GetNamed(tselected.defName.Replace(_Text.FromTerrain_, ""), false);
					if (terrain != null)
						lFrom.Add(terrain);
				}
			}
			return l;
		}

		public static List<ThingDef> GetRockListFromSelected(List<ThingDef> lSelected, out List<ThingDef> lFrom)
		{
			List<ThingDef> l = new List<ThingDef>();
			lFrom = new List<ThingDef>();
			foreach (ThingDef tselected in lSelected)
			{				
				if (tselected.defName.StartsWith(_Text.RESROCK_))
				{   // tui rock
					ThingDef rock = DefDatabase<ThingDef>.GetNamed(tselected.defName.Replace(_Text.RESROCK_, ""), false);
					if (rock != null)
						l.Add(rock);
				}
				else if (!tselected.defName.StartsWith(_Text.TColor_))
				{   // base rock
					lFrom.Add(tselected);
				}
			}
			return l;
		}

		public static List<ThingDef> GetPlantListsFromSelected(List<ThingDef> lSelected, out List<ThingDef> lFrom)
		{
			List<ThingDef> l = new List<ThingDef>();
			lFrom = new List<ThingDef>();
			foreach (ThingDef tselected in lSelected)
			{
				if (tselected.defName.StartsWith(_Text.TRPLNT_))
				{   // tui plant
					ThingDef plant = DefDatabase<ThingDef>.GetNamed(tselected.defName.Replace(_Text.TRPLNT_, ""), false);
					if (plant != null)
						l.Add(plant);
				}
				else if (!tselected.defName.StartsWith(_Text.TColor_))
				{   // base plant
					lFrom.Add(tselected);
				}
			}
			return l;
		}

		public static List<ThingDef> GetColorList(List<ThingDef> lt)
		{
			List<ThingDef> l = new List<ThingDef>();
			foreach (ThingDef td in lt)
			{
				if (td.defName.StartsWith(_Text.TColor_))
					l.Add(td);
			}
			return l;
		}

		public static List<BiomeDef> GetBiomeListFromSelected(List<ThingDef> lt)
		{
			List<BiomeDef> l = new List<BiomeDef>();
			foreach (ThingDef tui in lt)
			{
				BiomeDef biome = DefDatabase<BiomeDef>.GetNamed(tui.description.SubstringTo("\n"), false);
				if (biome != null)
					l.Add(biome);
			}
			return l;
		}

		public static List<TerrainDef> GetListOfTerrainDefsFromThreshold(List<TerrainThreshold> lt)
		{
			List<TerrainDef> l = new List<TerrainDef>();
			foreach (TerrainThreshold terrainThreshold in lt)
				l.Add(terrainThreshold.terrain);
			return l;
		}

		public static List<TerrainDef> GetListOfTerrainDefsFromPatchMaker(List<TerrainPatchMaker> lpm)
		{
			List<TerrainDef> l = new List<TerrainDef>();
			foreach (TerrainPatchMaker tpm in lpm)
			{
				List<TerrainDef> ldef = GetListOfTerrainDefsFromThreshold(tpm.thresholds);
				foreach (TerrainDef terrain in ldef)
					l.Add(terrain);
			}
			return l;
		}



		public static int GetEfficiency()
		{
            int efficieny = 0;
            if (ResearchProjectDefOf.EmiterEfficiency10.IsFinished)
                efficieny = 10;           
            else if (ResearchProjectDefOf.EmiterEfficiency2.IsFinished)
                efficieny = 2;
            else
                efficieny = 1;
            return efficieny;
		}

		public static IntVec3 GetRandomCell(IntVec3 posEmiter, int minRange, int maxRange)
		{
			int x = posEmiter.x + TRMod.zufallswert.Next(minRange, maxRange);
			int z = posEmiter.z + TRMod.zufallswert.Next(minRange, maxRange);
			return new IntVec3(x, posEmiter.y, z);			
		}

        

		public static bool CheckTerrainIsGoodForPlacing(IntVec3 posSpawn, Map map)
		{
			TerrainDef ter = posSpawn.GetTerrain(map);
			bool terrainOk = false;
			if (ter.affordances != null)
			{
				if (!ter.layerable)
				{
					foreach (TerrainAffordanceDef tad in ter.affordances)
					{
						if (tad == TerrainAffordanceDefOf.Light ||
							tad == TerrainAffordanceDefOf.Medium ||
							tad == TerrainAffordanceDefOf.Heavy ||
							tad == TerrainAffordanceDefOf.GrowSoil)
							terrainOk = true;
						else if (tad == TerrainAffordanceDefOf.MovingFluid)
						{
							terrainOk = false;
							break;
						}
					}
				}
			}
			return terrainOk;
		}

		public static bool CheckPlaceIsFree(ThingDef thingToPlace, IntVec3 posSpawn, Map map, bool ignoreItem = false)
		{
			List<Thing> lt = posSpawn.GetThingList(map).ToList<Thing>();
			foreach (Thing thing in lt)
			{
				if (thing != null && thing.def != null)
				{   // prüfe position an der gespawn wird, falls dort pawn oder building ist, dann abbruch
					if (thing.def.category == ThingCategory.Building ||
						thing.def.category == ThingCategory.Pawn ||
						(!ignoreItem && thing.def.category == ThingCategory.Item))
						return false;
					else if (thing.def.category == ThingCategory.Plant && thing.def.plant != null)
					{
						if (thingToPlace.plant == null)
							return !thing.def.plant.IsTree; // bei ignore darf alles ersetzt werden ausser tree
						else if (thingToPlace.defName.EndsWith("Grass"))
							return thing.def.defName.EndsWith("Grass");
						else
							return false; // sonst nein
					}
					else if (thing.def.thingCategories != null)
					{
						foreach (ThingCategoryDef tcd in thing.def.thingCategories)
						{
							if (tcd == ThingCategoryDefOf.Chunks ||
								tcd == ThingCategoryDefOf.StoneChunks)
								return false;
						}
					}
				}
			}
			return true;
		}

		public static Area GetTerraformingArea(Map map)
		{
			foreach (Area a in map.areaManager.AllAreas)
			{
				if (a.Label.Equals(_Text.TerraformingArea))
					return a;
			}

			Area_Allowed area = new Area_Allowed(map.areaManager, _Text.TerraformingArea);
			map.areaManager.AllAreas.Add(area);
			map.areaManager.AreaManagerUpdate();
			return area;
		}

		public static void ResortSpecialTerraformDesignators()
		{
			List<Type> l = new List<Type>();
			foreach (Type t in DesignationCategoryDefOf.Terraform.specialDesignatorClasses)
			{
				if (t == typeof(Designator_AreaNoThings) || t == typeof(Designator_AreaOvermountain))
				{
					if (TRMod.OPTION_InstantConstruction)
						l.Add(t);
				}
				else if (t != null)
					l.Add(t);
			}

			DesignationCategoryDefOf.Terraform.specialDesignatorClasses.Clear();
			if (TRMod.OPTION_InstantConstruction)
			{
				if (!l.Contains(typeof(Designator_AreaNoThings)))
					l.Add(typeof(Designator_AreaNoThings));
				if (!l.Contains(typeof(Designator_AreaOvermountain)))
					l.Add(typeof(Designator_AreaOvermountain));
			}
			foreach (Type t in l)
			{
				DesignationCategoryDefOf.Terraform.specialDesignatorClasses.Add(t);
			}
		}
	}
}
