using HugsLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TerraformRimworld
{
	public static class WeatherEmiterManager
	{
		public static void Init()
		{
			CreateWeatherEmiter();
		}

		private static void CreateWeatherEmiter()
		{
			ThingDef emiter = DefDatabase<ThingDef>.GetNamed(_Emiter.WeatherEmiter, false);
			if (emiter == null)
			{
				List<RecipeDef> recipes = new List<RecipeDef>();
				List<ThingCategoryDef> lcat = new List<ThingCategoryDef>();
				//lcat.Add(ThingCategories.Rocks);
				//lcat.Add(ThingCategories.Minerals);
				lcat.Add(ThingCategories.Farbe);

				foreach (WeatherDef weather in DefDatabase<WeatherDef>.AllDefs)
					recipes.Add(Helper.CreateDefaultRecipe(_Recipe.WeatherChange + weather.defName, lcat, ResearchProjectDefOf.WeatherEmiter, true, weather.label, weather.label, weather.description));

				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.WeatherAddTemp, null, ResearchProjectDefOf.WeatherEmiter, false, "Temperature +", "Increasing Biome Temperature", "Biome Temperature will be permanently increased."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.WeatherSubTemp, null, ResearchProjectDefOf.WeatherEmiter, false, "Temperature -", "Decreasing Biome Temperature", "Biome Temperature will be permanently decreased."));
				recipes.Add(Helper.CreateDefaultRecipe(_Recipe.WeatherRefog, null, ResearchProjectDefOf.WeatherEmiter, false, "Refog the map", "Refog the map", "Fogs all detected areas, as they were undetected."));

				Helper.CreateDefaultEmiter(_Emiter.WeatherEmiter, typeof(WeatherEmiter), _Emiter.WeatherEmiter, ResearchProjectDefOf.WeatherEmiter, recipes, 1000, 0, "Weather-Emiter", "");
			}
			else
				Helper.UpdateEmiter(_Emiter.WeatherEmiter);
		}
	}

	public class GameCondition_TempChange : GameCondition
	{
		public float MaxTempOffset = 0.1f;
		public float TempIncrement = 0.1f;
		private const int LerpTicks = 12000;
		public GameCondition_TempChange()
		{
			base.Permanent = true;
		}

		public override float TemperatureOffset()
		{
			return MaxTempOffset;
		}

		public float UpdateTemperature(bool isIncrease)
		{
			float changeValue = TempIncrement;
			changeValue = isIncrease ? changeValue : (-1) * changeValue;
			float currentValue = MaxTempOffset;
			currentValue = (float)Math.Round((currentValue + changeValue), 1);
			if (currentValue < -275.0f)
				currentValue = -275.0f;
			MaxTempOffset = currentValue;
			return currentValue;
		}
	}

	public class IncidentWorker_TempChange : IncidentWorker_MakeGameCondition
	{
		public IncidentWorker_TempChange()
		{
		}

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			return map.mapTemperature.SeasonalTemp >= 20f;
		}
	}
	public class WeatherEmiter : Building_WorkTable, IBillGiver, IBillGiverWithTickAction
	{
		#region vars

		private Bill b;
		private CompPowerTrader cpt;
		private int efficiency;
		private bool hasDoneBill;
		private List<TerrainDef> lOfIce;
		private List<TerrainDef> lOfLava;
		private int maxRange;
		#endregion vars

		#region constructor and overrides

		public WeatherEmiter()
		{
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			HugsLibController.Instance.DistributedTicker.RegisterTickability(StartWeatherEmiter, TRMod.OPTION_EmiterTick, this);
			cpt = this.GetComp<CompPowerTrader>();

			lOfLava = new List<TerrainDef>();
			foreach (TerrainDef t in DefDatabase<TerrainDef>.AllDefs)
			{
				if (t != null && t.defName != null && (t.defName.StartsWith("Lava") || t.defName == "Obsidian_RoughHewn") && t.defName != "LavaBlue")
					lOfLava.Add(t);
			}
			lOfIce = new List<TerrainDef>();
			foreach (TerrainDef t in DefDatabase<TerrainDef>.AllDefs)
			{
				if (t != null && t.defName != null && t.defName.StartsWith("Ice"))
					lOfIce.Add(t);
			}
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

		#region support func

		private void ChangeWeatherColor(WeatherDef weather, Color col)
		{
			weather.skyColorsDay = new SkyColorSet();
			weather.skyColorsDay.sky = col;
			weather.skyColorsDay.shadow = new Color(0.92f, 0.92f, 0.92f);
			weather.skyColorsDay.overlay = col;
			weather.skyColorsDay.saturation = 0.9f;

			weather.skyColorsDusk = new SkyColorSet();
			weather.skyColorsDusk.sky = Helper.GetNewReducedColor(weather.skyColorsDay.sky);
			weather.skyColorsDusk.shadow = new Color(0.92f, 0.92f, 0.92f);
			weather.skyColorsDusk.overlay = Helper.GetNewReducedColor(weather.skyColorsDay.sky);
			weather.skyColorsDusk.saturation = 0.9f;

			weather.skyColorsNightEdge = new SkyColorSet();
			weather.skyColorsNightEdge.sky = Helper.GetNewReducedColor(weather.skyColorsDusk.sky);
			weather.skyColorsNightEdge.shadow = new Color(0.92f, 0.92f, 0.92f);
			weather.skyColorsNightEdge.overlay = Helper.GetNewReducedColor(weather.skyColorsDusk.sky);
			weather.skyColorsNightEdge.saturation = 0.9f;

			weather.skyColorsNightMid = new SkyColorSet();
			weather.skyColorsNightMid.sky = Helper.GetNewReducedColor(weather.skyColorsNightEdge.sky);
			weather.skyColorsNightMid.shadow = new Color(0.92f, 0.92f, 0.92f);
			weather.skyColorsNightMid.overlay = Helper.GetNewReducedColor(weather.skyColorsNightEdge.sky);
			weather.skyColorsNightMid.saturation = 0.9f;

			weather.ResolveReferences();
			weather.PostLoad();
		}

		private GameCondition_TempChange GetTemperatureCondition()
		{
			foreach (GameCondition gc in base.Map.GameConditionManager.ActiveConditions)
			{
				if (gc.def.defName == "TempChange")
					return (GameCondition_TempChange)gc;
			}
			return null;
		}

		#endregion support func

		#region recipe func

		private void ChangeWeather(List<ThingDef> lColors)
		{
			string defName = b.recipe.defName.Replace(_Recipe.WeatherChange, "");
			WeatherDef weather = DefDatabase<WeatherDef>.GetNamed(defName, false);
			ThingDef choosen = Helper.GetRandomListElement(lColors);
			if (weather != null && choosen != null)
			{
				Color col = choosen.uiIconColor;
				if (TRMod.isDebug)
					Messages.Message("weather=" + weather.label + " color=" + col.r.ToString() + "," + col.g.ToString() + "," + col.b.ToString(), MessageTypeDefOf.SilentInput, false);

				ChangeWeatherColor(weather, col);
				base.Map.weatherManager.TransitionTo(weather);
				base.Map.weatherManager.TransitionTo(weather);
				DoEffect();
			}
		}

		private void DoTemperatureChange(bool isIncrease)
		{
			GameCondition_TempChange g = GetTemperatureCondition();
			if (g == null)
			{
				Helper.StartIncidentByName("TempChange", "Temperature started to change.", "Temperature Change");
			}
			else
			{
				float currentValue = g.UpdateTemperature(isIncrease);
				int countLimit = TRMod.OPTION_TemperatureLimit - 20;

				//if (isIncrease && currentValue > 70.0f)
				//{
				//	WeatherDef weather = DefDatabase<WeatherDef>.GetNamed("STORMYASHRAIN");
				//	ChangeWeatherColor(weather, new Color(0.6f, 0.1f, 0.1f));
				//	base.Map.weatherManager.TransitionTo(weather);
				//}
				if (currentValue < -20.0f)
				{
					base.Map.weatherManager.TransitionTo(WeatherDef.Named("SnowHard"));
				}
				else if (currentValue > 20.0f)
				{
					if (base.Map.weatherManager.curWeather != null && base.Map.weatherManager.curWeather.defName.Contains("Snow"))
						base.Map.weatherManager.TransitionTo(WeatherDefOf.Clear);
				}

				if (isIncrease && currentValue > TRMod.OPTION_TemperatureLimit)
				{
					int randchane = TRMod.zufallswert.Next(0, 10);
					if (randchane < 2)
						GenExplosion.DoExplosion(base.Position, base.Map, 1.0f, DamageDefOf.Bomb, null, 0);
					GameCondition_Flashstorm gameCondition_Flashstorm = (GameCondition_Flashstorm)GameConditionMaker.MakeCondition(GameConditionDefOf.Flashstorm, 50000);
					gameCondition_Flashstorm.centerLocation = base.Position.ToIntVec2;
					base.Map.gameConditionManager.RegisterCondition(gameCondition_Flashstorm);

					int count = (int)currentValue - countLimit;
					for (int i = 0; i < count; i++)
					{
						TerrainDef t = Helper.GetRandomListElement(lOfLava);
						IntVec3 posSpawn = Helper.GetRandomCell(base.Map);
						if (t != null)
							base.Map.terrainGrid.SetTerrain(posSpawn, t);
					}

					Helper.DoCustomTectonicPulse(base.Map);
				}
				if (currentValue < ((-1) * TRMod.OPTION_TemperatureLimit))
				{
					int count = ((int)currentValue + countLimit) * (-1);
					for (int i = 0; i < count; i++)
					{
						TerrainDef t = Helper.GetRandomListElement(lOfIce);
						IntVec3 posSpawn = Helper.GetRandomCell(base.Map);
						if (t != null)
							base.Map.terrainGrid.SetTerrain(posSpawn, t);
					}
				}

				string val = currentValue.ToString();
				if (!val.Contains("."))
					val += ".0";

				g.def.ClearCachedData();
				if (currentValue > 0)
					g.def.label = "Temperature +" + val + "°C";
				else
					g.def.label = "Temperature " + val + "°C";
				g.def.ResolveReferences();
				FleckMaker.ThrowHeatGlow(base.Position, base.Map, currentValue / 40.0f);
			}
			DoEffect();
		}

		private void RefogMap()
		{
			FloodFillerFog.DebugRefogMap(base.Map);
			DoEffect();
		}
		#endregion recipe func

		public void StartWeatherEmiter()
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

					List<ThingDef> lSelected = b.ingredientFilter.AllowedThingDefs.ToList();
					if (b.recipe.defName.StartsWith(_Recipe.WeatherChange))
					{
						List<ThingDef> lColors = Helper.GetColorList(lSelected);
						ChangeWeather(lColors);
					}
					else if (b.recipe.defName.StartsWith(_Recipe.WeatherRefog))
					{
						RefogMap();
					}
					else if (b.recipe.defName == _Recipe.WeatherAddTemp)
					{
						DoTemperatureChange(true);
					}
					else if (b.recipe.defName == _Recipe.WeatherSubTemp)
					{
						DoTemperatureChange(false);
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
