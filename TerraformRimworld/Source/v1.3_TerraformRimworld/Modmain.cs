using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace TerraformRimworld
{
	[DefOf]
    public static class ResearchProjectDefOf
    {
        public static ResearchProjectDef BasicTerrain;
        public static ResearchProjectDef WaterTerrain;
        public static ResearchProjectDef LavaTerrain;
        public static ResearchProjectDef RockyTerrain;
        public static ResearchProjectDef ExoticTerrain;
        public static ResearchProjectDef OtherTerrain;
        public static ResearchProjectDef RockTerraforming;
		public static ResearchProjectDef FloraTerraforming;
		public static ResearchProjectDef MineralTerraforming;
        public static ResearchProjectDef MountainsTerraforming;
        public static ResearchProjectDef ReplicatorNanites;
        public static ResearchProjectDef TerraEmiter;
		public static ResearchProjectDef TectonicTerraforming;
        public static ResearchProjectDef TectonicEmiter;
		public static ResearchProjectDef FloraEmiter;
        public static ResearchProjectDef RockEmiter;
        public static ResearchProjectDef MineralEmiter;
        public static ResearchProjectDef MountainsEmiter;
        public static ResearchProjectDef WeatherEmiter;
		public static ResearchProjectDef EmiterRange7;
		public static ResearchProjectDef EmiterRange15;
		public static ResearchProjectDef EmiterEfficiency2;                       
        public static ResearchProjectDef EmiterEfficiency10;
		public static ResearchProjectDef ReplicatorComplete;
	}

    [DefOf]
	public static class DesignationCategoryDefOf
	{
		public static DesignationCategoryDef Terraform;
	}

	[DefOf]
	public static class DesignatorDropdownGroupDefOf
	{
		public static DesignatorDropdownGroupDef Build_Roofs;
		public static DesignatorDropdownGroupDef Build_Rocks;
		public static DesignatorDropdownGroupDef Build_Flora;
		public static DesignatorDropdownGroupDef Build_ResRock;
		public static DesignatorDropdownGroupDef Emiter;
		public static DesignatorDropdownGroupDef ExoticTerrain;
		public static DesignatorDropdownGroupDef LavaTerrain;
        public static DesignatorDropdownGroupDef OtherTerrain;
        public static DesignatorDropdownGroupDef TerrainRockyRough;
		public static DesignatorDropdownGroupDef TerrainRockyRoughHewn;
		public static DesignatorDropdownGroupDef TerrainRockySmooth;
		public static DesignatorDropdownGroupDef Anomalies;
	}

	[DefOf]
	public static class JobDefOf
	{
		public static JobDef RemoveThickRoof;
	}


	[StaticConstructorOnStartup]
	public class TRMod : ModBase
	{
		#region vars
        #region mod options
        public static bool OPTION_InstantConstruction;
		public static bool OPTION_CostsEnabled;
        public static bool OPTION_UseSilver;
        public static int OPTION_CostAmount;
        public static string OPTION_CostCustom;
        public static bool OPTION_PlaceWithoutRestrictions;
		public static bool OPTION_RockyScatterEnabled;
		public static bool OPTION_UseAllTerrain;
		public static int OPTION_WorkValue;
		public static bool OPTION_AlternativeIcon;
		public static int OPTION_EmiterTick;
		public static float OPTION_EmiterBaseConsumption;
		public static int OPTION_EmiterRandomDistance;
		public static bool OPTION_VanillaLook;
		public static int OPTION_TemperatureLimit;
		public static bool OPTION_KeyedLanguange;
		public static bool OPTION_ForceMinified;
		public static bool OPTION_EmiterEnabled;

        public static int OPTION_LavaSlowdown;

		public static int OPTION_ChanceOfFire;
		public static int OPTION_ChanceOfHeatGlow;
		public static int OPTION_ChanceOfFireGlow;
		public static int OPTION_ChanceOfLightningGlow;
		public static int OPTION_ChanceOfDustPuff;
		public static int OPTION_ChanceOfAirPuff;
		public static int OPTION_ChanceOfDustPuffthick;
		public static int OPTION_ChanceOfMetaPuff;
		public static int OPTION_ChanceOfSmoke;
		public static int OPTION_ChanceOfTornadoPuff;

		public static float OPTION_MaxSizeFire;
		public static float OPTION_MaxSizeHeatGlow;
		public static float OPTION_MaxSizeFireGlow;
		public static float OPTION_MaxSizeLightningGlow;
		public static float OPTION_MaxSizeDustPuff;
		public static float OPTION_MaxSizeDustPuffThick;
		public static float OPTION_MaxSizeSmoke;
		public static float OPTION_MaxSizeTornadoPuff;
		#endregion

		#region holder
		private Func<bool> PARAM_VanillaLook;
		private Func<bool> PARAM_InstantConstruction;
		private Func<bool> PARAM_CostEnabled;
		private Func<int> PARAM_CostAmount;
        private Func<string> PARAM_CostCustom;
		private Func<int> PARAM_WorkValue;
		private Func<bool> PARAM_PlaceWithoutRestrictions;
		private Func<bool> PARAM_RockyScatterEnabled;
		private Func<bool> PARAM_UseAllTerrain;
		private Func<bool> PARAM_UseSilver;
		private Func<bool> PARAM_AlternativeIcon;
		private Func<int> PARAM_EmiterTick;
		private Func<float> PARAM_EmiterBaseConsumption;
		private Func<int> PARAM_EmiterRandomDistance;
		private Func<int> PARAM_TemperatureLimit;
		private Func<bool> PARAM_KeyedLanguange;
		private Func<bool> PARAM_ForceMinified;
		private Func<bool> PARAM_EmiterEnabled;

		private Func<int> PARAM_LavaSlowdown;

		private Func<int> PARAM_ChanceOfFire;
		private Func<int> PARAM_ChanceOfHeatGlow;
		private Func<int> PARAM_ChanceOfFireGlow;
		private Func<int> PARAM_ChanceOfLightningGlow;
		private Func<int> PARAM_ChanceOfDustPuff;
		private Func<int> PARAM_ChanceOfAirPuff;
		private Func<int> PARAM_ChanceOfDustPuffthick;
		private Func<int> PARAM_ChanceOfMetaPuff;
		private Func<int> PARAM_ChanceOfSmoke;
		private Func<int> PARAM_ChanceOfTornadoPuff;

		private Func<float> PARAM_MaxSizeFire;
		private Func<float> PARAM_MaxSizeHeatGlow;
		private Func<float> PARAM_MaxSizeFireGlow;
		private Func<float> PARAM_MaxSizeLightningGlow;
		private Func<float> PARAM_MaxSizeDustPuff;
		private Func<float> PARAM_MaxSizeDustPuffThick;
		private Func<float> PARAM_MaxSizeSmoke;
		private Func<float> PARAM_MaxSizeTornadoPuff;
        #endregion

        #region own vars
        public const string MODNAME = "TerraformRimworld";
		public const string IDENTI = "1874652867";
		public const string TABNAME = "Terraform";
		public const string PACKID = "void.terraformrimworld";
        public static bool isDebug;
        public static bool isGerman { get { return (currentLanguage != null && currentLanguage.ToLower() == "german"); } }
		public static string currentLanguage = null;
		public static bool isCuproModActive = false;
		public static bool isSZTerrainsModActive = false;
		public static bool isSZStonesAndTerrainsModActive = false;
		public static bool isAdvancedBiomesModActive = false;
		public static System.Random zufallswert;
		public static SoundDef SND_ROCK_NANITES;
		public static SoundDef SND_FLORA_NANITES;
        public static Dictionary<string, ThingDefCountClass> dicCustomCosts;		
        #endregion
        #endregion

        public override string ModIdentifier
		{
			get { return MODNAME; }
		}

        #region settings
        private bool HasSomethingChanged()
		{
			bool somethingChanged = false;
			bool cuproActive = (DefDatabase<ThingDef>.GetNamed("Claystone", false) != null);

            if ((OPTION_CostsEnabled != PARAM_CostEnabled()) ||
				(OPTION_CostAmount != PARAM_CostAmount()) ||
                (OPTION_CostCustom != PARAM_CostCustom()) ||
                (OPTION_WorkValue != PARAM_WorkValue()) ||
				(OPTION_AlternativeIcon != PARAM_AlternativeIcon()) ||
				(OPTION_EmiterTick != PARAM_EmiterTick()) ||
				(OPTION_EmiterBaseConsumption != PARAM_EmiterBaseConsumption()) ||
				(OPTION_EmiterRandomDistance != PARAM_EmiterRandomDistance()) ||
				(OPTION_VanillaLook != PARAM_VanillaLook()) ||
				(OPTION_InstantConstruction != PARAM_InstantConstruction()) ||
				(OPTION_PlaceWithoutRestrictions != PARAM_PlaceWithoutRestrictions()) ||
				(OPTION_RockyScatterEnabled != PARAM_RockyScatterEnabled()) ||
				(OPTION_UseAllTerrain != PARAM_UseAllTerrain()) ||
				(OPTION_UseSilver != PARAM_UseSilver()) ||
				(OPTION_LavaSlowdown != PARAM_LavaSlowdown()) ||
				(OPTION_TemperatureLimit != PARAM_TemperatureLimit()) ||
				(OPTION_KeyedLanguange != PARAM_KeyedLanguange()) ||
				(OPTION_ForceMinified != PARAM_ForceMinified()) ||
				(OPTION_EmiterEnabled != PARAM_EmiterEnabled()) ||
				(isCuproModActive != cuproActive) ||
				(isSZTerrainsModActive != ModLister.HasActiveModWithName("SZ_Terrains")) ||
				(isSZStonesAndTerrainsModActive != ModLister.HasActiveModWithName("SZ_StonesAndTerrains")) ||
				(isAdvancedBiomesModActive != ModLister.HasActiveModWithName("Advanced Biomes")) ||
				(currentLanguage != LanguageDatabase.activeLanguage.FriendlyNameEnglish.ToLower()))
				somethingChanged = true;
			return somethingChanged;
		}

		public override void WorldLoaded()
		{
			base.WorldLoaded();
			ResearchAll();						
		}

		private void ResearchAll()
		{
			if (TRMod.OPTION_VanillaLook)
			{
				foreach (ResearchProjectDef proj in DefDatabase<ResearchProjectDef>.AllDefs)
				{
					if (proj != null && proj.tab != null && proj.tab.defName == TABNAME)
					{			
						Find.ResearchManager.FinishProject(proj, false, null);
					}
				}
			}			
		}

		public override void SettingsChanged()
		{
			if (ModIsActive)
			{
				if (HasSomethingChanged())
				{
                    bool customCostsEmpty = (String.IsNullOrEmpty(OPTION_CostCustom) && String.IsNullOrEmpty(PARAM_CostCustom()));

                    if ((OPTION_CostsEnabled != PARAM_CostEnabled()) ||
						(OPTION_CostAmount != PARAM_CostAmount()) ||
                        (!customCostsEmpty && (OPTION_CostCustom != PARAM_CostCustom())) ||                        
						(OPTION_AlternativeIcon != PARAM_AlternativeIcon()) ||
						(OPTION_EmiterTick != PARAM_EmiterTick()) ||
                        (OPTION_RockyScatterEnabled != PARAM_RockyScatterEnabled()) ||
						(OPTION_UseAllTerrain != PARAM_UseAllTerrain()) ||
						(OPTION_UseSilver != PARAM_UseSilver()) ||
						(OPTION_ForceMinified != PARAM_ForceMinified()) ||
						(OPTION_EmiterEnabled != PARAM_EmiterEnabled()) ||
						(OPTION_KeyedLanguange != PARAM_KeyedLanguange()) ||
						(OPTION_VanillaLook != PARAM_VanillaLook()))
					{
						Helper.ShowDialog("Some changes needs a def reload. To apply all changes the defs have to be reloaded. Do reset now?", true);
					}
					else
					{						
						LoadAndResolveModData();
					}
				}
			}
		}

		public override void DefsLoaded()
		{
			if (ModIsActive)
			{
                LoadAndResolveModData();
				//MessageTool.Show("TerraformRimworld 1.3 loaded");
			}
		}

        #region validators
        public static bool ValidWorkToBuild(string _value)
		{
			int iNumber = 100;
			int.TryParse(_value, out iNumber);
			return !(iNumber < 100 || iNumber > 9999);
		}

		public static bool ValidCosts(string _value)
		{
			int iNumber = 1;
			int.TryParse(_value, out iNumber);
			return !(iNumber < 1 || iNumber > 9999);
		}

		public static bool ValidTemperatureLimit(string _value)
		{
			int iNumber = 20;
			int.TryParse(_value, out iNumber);
			return !(iNumber < 20 || iNumber > 9999);
		}

		public static bool ValidTick(string _value)
		{
			int iNumber = 30;
			int.TryParse(_value, out iNumber);
			return !(iNumber < 30 || iNumber > 60000);
		}

		public static bool ValidLava(string _value)
		{
			int iNumber = 0;
			int.TryParse(_value, out iNumber);
			return !(iNumber < 0 || iNumber > 10);
		}

		public static bool ValidMaxLava(string _value)
		{
			float fNumber = 0.0f;
			float.TryParse(_value, out fNumber);
			return !(fNumber < 0.0f || fNumber > 50.0f);
		}

		public static bool ValidLavaSlowdown(string _value)
		{
			int iNumber = 0;
			int.TryParse(_value, out iNumber);
			return !(iNumber < 0 || iNumber > 300);
		}

		public static bool ValidFuelConsumption(string _value)
		{
			float fNumber = 0.0f;
			float.TryParse(_value, out fNumber);
			return !(fNumber < 0.0f || fNumber > 100.0f);
		}

		public static bool ValidEmiterRandomDistance(string _value)
		{
			int iNumber = 0;
			int.TryParse(_value, out iNumber);
			return !(iNumber < 0 || iNumber > 100);
		}
        #endregion

        private void LoadSavedSettings()
        {
            OPTION_CostsEnabled = PARAM_CostEnabled();
            OPTION_CostAmount = PARAM_CostAmount();
			OPTION_CostCustom = PARAM_CostCustom();
			OPTION_VanillaLook = PARAM_VanillaLook();
            OPTION_InstantConstruction = PARAM_InstantConstruction();
            OPTION_WorkValue = PARAM_WorkValue();
			OPTION_AlternativeIcon = PARAM_AlternativeIcon();
			OPTION_EmiterTick = PARAM_EmiterTick();
			OPTION_EmiterBaseConsumption = PARAM_EmiterBaseConsumption();
			OPTION_EmiterRandomDistance = PARAM_EmiterRandomDistance();
            OPTION_PlaceWithoutRestrictions = PARAM_PlaceWithoutRestrictions();
            OPTION_RockyScatterEnabled = PARAM_RockyScatterEnabled();
            OPTION_UseAllTerrain = PARAM_UseAllTerrain();
            OPTION_UseSilver = PARAM_UseSilver();
			OPTION_TemperatureLimit = PARAM_TemperatureLimit();
			OPTION_KeyedLanguange = PARAM_KeyedLanguange();
			OPTION_ForceMinified = PARAM_ForceMinified();
			OPTION_EmiterEnabled = PARAM_EmiterEnabled();

            OPTION_LavaSlowdown = PARAM_LavaSlowdown();

            OPTION_ChanceOfFire = PARAM_ChanceOfFire();
            OPTION_ChanceOfHeatGlow = PARAM_ChanceOfHeatGlow();
            OPTION_ChanceOfFireGlow = PARAM_ChanceOfFireGlow();
            OPTION_ChanceOfLightningGlow = PARAM_ChanceOfLightningGlow();
            OPTION_ChanceOfDustPuff = PARAM_ChanceOfDustPuff();
            OPTION_ChanceOfAirPuff = PARAM_ChanceOfAirPuff();
            OPTION_ChanceOfDustPuffthick = PARAM_ChanceOfDustPuffthick();
            OPTION_ChanceOfMetaPuff = PARAM_ChanceOfMetaPuff();
            OPTION_ChanceOfSmoke = PARAM_ChanceOfSmoke();
            OPTION_ChanceOfTornadoPuff = PARAM_ChanceOfTornadoPuff();

            OPTION_MaxSizeFire = PARAM_MaxSizeFire();
            OPTION_MaxSizeHeatGlow = PARAM_MaxSizeHeatGlow();
            OPTION_MaxSizeFireGlow = PARAM_MaxSizeFireGlow();
            OPTION_MaxSizeLightningGlow = PARAM_MaxSizeLightningGlow();
            OPTION_MaxSizeDustPuff = PARAM_MaxSizeDustPuff();
            OPTION_MaxSizeDustPuffThick = PARAM_MaxSizeDustPuffThick();
            OPTION_MaxSizeSmoke = PARAM_MaxSizeSmoke();
            OPTION_MaxSizeTornadoPuff = PARAM_MaxSizeTornadoPuff();
        }

        private void InitializeSettings()
		{
            #region defaults
            const bool DEF_InstantConstruction = false;
            const bool DEF_CostsEnabled = true;
            const bool DEF_UseSilver = false;
            const int DEF_CostAmount = 1;
            const string DEF_CostCustom = "";
            const bool DEF_AllowPlacing = false;
            const bool DEF_RemoveRockyScatter = false;
            const bool DEF_GrabAllTerrain = false;
            const int DEF_WorkValue = 1000;
			const bool DEF_AlternativeIcon = false;
			const int DEF_EmiterTick = 60;
			const float DEF_FuelConsumption = 1.0f;
			const int DEF_EmiterRandomDistance = 30;
			const bool DEF_VanillaLook = false;
			const int DEF_TemperatureLimit = 200;
			const bool DEF_ForceMinified = false;
			const bool DEF_KeyedLanguage = false;
			const bool DEF_EmiterEnabled = true;

            const int DEF_LavaSlowdown = 200;            
			const int DEF_ChanceOfFire = 1;
			const int DEF_ChanceOfHeatGlow = 7;
			const int DEF_ChanceOfFireGlow = 7;
			const int DEF_ChanceOfLightningGlow = 2;
			const int DEF_ChanceOfDustPuff = 2;
			const int DEF_ChanceOfAirPuff = 2;
			const int DEF_ChanceOfDustPuffthick = 2;
			const int DEF_ChanceOfMetaPuff = 2;
			const int DEF_ChanceOfSmoke = 2;
			const int DEF_ChanceOfTornadoPuff = 2;

			const float DEF_MaxSizeFire = 1.25f;
			const float DEF_MaxSizeHeatGlow = 5.0f;
			const float DEF_MaxSizeFireGlow = 5.0f;
			const float DEF_MaxSizeLightningGlow = 3.5f;
			const float DEF_MaxSizeDustPuff = 7.0f;
			const float DEF_MaxSizeDustPuffThick = 7.0f;
			const float DEF_MaxSizeSmoke = 7.0f;
			const float DEF_MaxSizeTornadoPuff = 7.0f;
            #endregion

            SettingHandle.ValueIsValid isValidWorkValue = new SettingHandle.ValueIsValid(ValidWorkToBuild);
			SettingHandle.ValueIsValid isValidCostValue = new SettingHandle.ValueIsValid(ValidCosts);
			SettingHandle.ValueIsValid isValidLavaValue = new SettingHandle.ValueIsValid(ValidLava);
			SettingHandle.ValueIsValid isValidMaxLavaValue = new SettingHandle.ValueIsValid(ValidMaxLava);
			SettingHandle.ValueIsValid isValidLavaSlowdownValue = new SettingHandle.ValueIsValid(ValidLavaSlowdown);
			SettingHandle.ValueIsValid isValidTickValue = new SettingHandle.ValueIsValid(ValidTick);
			SettingHandle.ValueIsValid isValidFuelConsumption = new SettingHandle.ValueIsValid(ValidFuelConsumption);
			SettingHandle.ValueIsValid isValidEmiterRandomDistance = new SettingHandle.ValueIsValid(ValidEmiterRandomDistance);
			SettingHandle.ValueIsValid isValidTemperatureLimit = new SettingHandle.ValueIsValid(ValidTemperatureLimit);
			try
			{
				((Action)(() =>
				{
					var settings = HugsLibController.Instance.Settings.GetModSettings(MODNAME, MODNAME);
					settings.EntryName = MODNAME;

					#region basic options
					object handleZ = settings.GetHandle("P_VanillaLook", "[Mod] Vanilla Look", "Enabled=all terraforming research will be completed. Default=Disabled", DEF_VanillaLook);
					PARAM_VanillaLook = () => (SettingHandle<bool>)handleZ;

					object handleY = settings.GetHandle("P_EmiterEnabled", "[Emiter] enabled", "Enabled=you can build emiter, Disabled=emiter will be hidden. Default=Enabled", DEF_EmiterEnabled);
					PARAM_EmiterEnabled = () => (SettingHandle<bool>)handleY;

                    object handleX = settings.GetHandle("P_InstantConstruction", "[Instant] construction", "Enabled=play like in god mode - construction will be done instantly. Disabled=same as vanilla. Default=Disabled", DEF_InstantConstruction);
					PARAM_InstantConstruction = () => (SettingHandle<bool>)handleX;

					object handle3 = settings.GetHandle("P_AllowParameter", "[Placing] without restrictions", "Enabled=no terrain check. Disabled=terrain will be checked for suitable placing. Default=Enabled", DEF_AllowPlacing);
					PARAM_PlaceWithoutRestrictions = () => (SettingHandle<bool>)handle3;

					object handle4 = settings.GetHandle("P_ScatterParameter", "[Scatter] on rocky terrain", "Enabled=same as vanilla. Disabled=no more scatter on rocky terrains. Default=Disabled", DEF_RemoveRockyScatter);
					PARAM_RockyScatterEnabled = () => (SettingHandle<bool>)handle4;

					object handle6 = settings.GetHandle("P_GrabAllTerrainParameter", "[Terrain] use all terrains", "Enabled=all terrain defs will be used. Disabled=carpets, floors and other terrains will not be used. Default=Disabled", DEF_GrabAllTerrain);
					PARAM_UseAllTerrain = () => (SettingHandle<bool>)handle6;

					object handle = settings.GetHandle("P_CostParameter", "[Costs] for terraforming", "Enabled=terraforming will cost some resources. Disabled=terraform for free. Default=Enabled", DEF_CostsEnabled);
					PARAM_CostEnabled = () => (SettingHandle<bool>)handle;
					
					object handle25 = settings.GetHandle("P_UseSilverParameter", "[Costs] silver instead of wood", "Enabled=terraforming of standard tiles cost some silver. Disabled=terraforming of standard tiles cost some wood. Default=Disabled", DEF_UseSilver);
					PARAM_UseSilver = () => (SettingHandle<bool>)handle25;

					object handle24 = settings.GetHandle("P_CostValue", "[Costs] value", "Set the amount of wood or silver [1-9999] needed to terraform a terrain tile. Default=1", DEF_CostAmount, ValidCosts);
					PARAM_CostAmount = () => (SettingHandle<int>)handle24;

                    object handle23 = settings.GetHandle("P_CostCustom", "[Costs] custom", "Set custom terrain, rock costs. Syntax: TerrainNameA|CostName(Value);TerrainNameB|CostName(Value);... Example: Soil|Steel(4); Water|None(0); Granite|Chemfuel(1);", DEF_CostCustom);
                    PARAM_CostCustom = () => (SettingHandle<string>)handle23;

					object handle2 = settings.GetHandle<int>("P_WorkParameter", "[Work] to build", "Set the amount of work [100-9999] for terraforming. The value is based on FPS. So a value of 60 means for unskilled 1 second, 120 means 2 seconds, etc. Default=1000", DEF_WorkValue, ValidWorkToBuild);
					PARAM_WorkValue = () => (SettingHandle<int>)handle2;

					object handle31 = settings.GetHandle<int>("P_TemeratureLimit", "[Weather-Emiter] effect temperature", "Set the temperature threshold from which weather emiter will cause map changing effects [20-9999]. Default=200", DEF_TemperatureLimit, ValidCosts);
					PARAM_TemperatureLimit = () => (SettingHandle<int>)handle31;

					object handle27 = settings.GetHandle<int>("P_EmiterTick", "[Emiter] tick frequency [fps]", "Set tick interval in fps [30-60000]. The value is based on FPS. So a value of 60 mean 1 second. Default=60", DEF_EmiterTick, ValidTick);
					PARAM_EmiterTick = () => (SettingHandle<int>)handle27;

					object handle22 = settings.GetHandle<int>("P_EmiterRandomDistance", "[Emiter] search distance", "Search distance for nearest free tile around the emiter [0-100]. When no valid tile found, tiles will be selected randomly. Reducing this distance, will force to find more random, instead of near tiles. Default=30", DEF_EmiterRandomDistance, ValidEmiterRandomDistance);
					PARAM_EmiterRandomDistance = () => (SettingHandle<int>)handle22;				

					object handle29 = settings.GetHandle<float>("P_EmiterBaseConsumption", "[Emiter] base fuel consumption", "Set base fuel consumption per tick [0.0-100.0]. Default=1.0", DEF_FuelConsumption, ValidFuelConsumption);
					PARAM_EmiterBaseConsumption = () => (SettingHandle<float>)handle29;																								

					object handle28 = settings.GetHandle<bool>("P_AlternativeIcon", "[Emiter] use alternative icon", "Enabled=ui icons will use alternative icons, when available. Default=Disabled", DEF_AlternativeIcon);
					PARAM_AlternativeIcon = () => (SettingHandle<bool>)handle28;

					object handle33 = settings.GetHandle<bool>("P_ForceMinified", "[Mod] force minified creation", "Enabled=minified defs will be created for ui objects. Default=Disabled", DEF_ForceMinified);
					PARAM_ForceMinified = () => (SettingHandle<bool>)handle33;

					object handle34 = settings.GetHandle<bool>("P_KeyedLanguage", "[Mod] use keyed language", "Enabled=text will be used from language files. Default=Disabled", DEF_KeyedLanguage);
					PARAM_KeyedLanguange = () => (SettingHandle<bool>)handle34;
                    #endregion

                    #region lava options
                    object handle40 = settings.GetHandle("P_LavaSlowdown", "[Lava] slowdown", "[0-300] 300=impassable, 0=no slowdown. Default=200", DEF_LavaSlowdown, ValidLavaSlowdown);
					PARAM_LavaSlowdown = () => (SettingHandle<int>)handle40;

					object handle50 = settings.GetHandle("P_chanceOfFire", "[Lava] chance of fire", "[0-10] Default=1", DEF_ChanceOfFire, ValidLava);
					PARAM_ChanceOfFire = () => (SettingHandle<int>)handle50;

					object handle51 = settings.GetHandle("P_chanceOfHeatGlow", "[Lava] chance of heat glow", "[0-10] Default=7", DEF_ChanceOfHeatGlow, ValidLava);
					PARAM_ChanceOfHeatGlow = () => (SettingHandle<int>)handle51;

					object handle52 = settings.GetHandle("P_chanceOfFireGlow", "[Lava] chance of fire glow", "[0-10] Default=7", DEF_ChanceOfFireGlow, ValidLava);
					PARAM_ChanceOfFireGlow = () => (SettingHandle<int>)handle52;

					object handle53 = settings.GetHandle("P_chanceOfLightningGlow", "[Lava] chance of lightning glow", "[0-10] Default=2", DEF_ChanceOfLightningGlow, ValidLava);
					PARAM_ChanceOfLightningGlow = () => (SettingHandle<int>)handle53;

					object handle54 = settings.GetHandle("P_chanceOfDustPuff", "[Lava] chance of dust puff", "[0-10] Default=2", DEF_ChanceOfDustPuff, ValidLava);
					PARAM_ChanceOfDustPuff = () => (SettingHandle<int>)handle54;

					object handle55 = settings.GetHandle("P_chanceOfAirPuff", "[Lava] chance of air puff", "[0-10] Default=2", DEF_ChanceOfAirPuff, ValidLava);
					PARAM_ChanceOfAirPuff = () => (SettingHandle<int>)handle55;

					object handle56 = settings.GetHandle("P_chanceOfDustPuffThick", "[Lava] chance of dust puff thick", "[0-10] Default=2", DEF_ChanceOfDustPuffthick, ValidLava);
					PARAM_ChanceOfDustPuffthick = () => (SettingHandle<int>)handle56;

					object handle57 = settings.GetHandle("P_chanceOfMetaPuff", "[Lava] chance of meta puff", "[0-10] Default=2", DEF_ChanceOfMetaPuff, ValidLava);
					PARAM_ChanceOfMetaPuff = () => (SettingHandle<int>)handle57;

					object handle58 = settings.GetHandle("P_chanceOfSmoke", "[Lava] chance of smoke", "[0-10] Default=2", DEF_ChanceOfSmoke, ValidLava);
					PARAM_ChanceOfSmoke = () => (SettingHandle<int>)handle58;

					object handle59 = settings.GetHandle("P_chanceOfTornadoPuff", "[Lava] chance of tornado puff", "[0-10] Default=2", DEF_ChanceOfTornadoPuff, ValidLava);
					PARAM_ChanceOfTornadoPuff = () => (SettingHandle<int>)handle59;


					object handle60 = settings.GetHandle("P_maxSizeFire", "[Lava] max size of fire", "[0.0-50.0] Default=1.25", DEF_MaxSizeFire, ValidMaxLava);
					PARAM_MaxSizeFire = () => (SettingHandle<float>)handle60;

					object handle61 = settings.GetHandle("P_maxSizeHeatGlow", "[Lava] max size of heat glow", "[0.0-50.0] Default=5.0", DEF_MaxSizeHeatGlow, ValidMaxLava);
					PARAM_MaxSizeHeatGlow = () => (SettingHandle<float>)handle61;

					object handle62 = settings.GetHandle("P_maxSizeFireGlow", "[Lava] max size of fire glow", "[0.0-50.0] Default=5.0", DEF_MaxSizeFireGlow, ValidMaxLava);
					PARAM_MaxSizeFireGlow = () => (SettingHandle<float>)handle62;

					object handle63 = settings.GetHandle("P_maxSizeLightningGlow", "[Lava] max size of lightning glow", "[0.0-50.0] Default=3.5", DEF_MaxSizeLightningGlow, ValidMaxLava);
					PARAM_MaxSizeLightningGlow = () => (SettingHandle<float>)handle63;

					object handle64 = settings.GetHandle("P_maxSizeDustPuff", "[Lava] max size of dust puff", "[0.0-50.0] Default=25.0", DEF_MaxSizeDustPuff, ValidMaxLava);
					PARAM_MaxSizeDustPuff = () => (SettingHandle<float>)handle64;

					object handle65 = settings.GetHandle("P_maxSizeDustPuffThick", "[Lava] max size of dust puff thick", "[0.0-50.0] Default=25.0", DEF_MaxSizeDustPuffThick, ValidMaxLava);
					PARAM_MaxSizeDustPuffThick = () => (SettingHandle<float>)handle65;

					object handle66 = settings.GetHandle("P_maxSizeSmoke", "[Lava] max size of smoke", "[0.0-50.0] Default=25.0", DEF_MaxSizeSmoke, ValidMaxLava);
					PARAM_MaxSizeSmoke = () => (SettingHandle<float>)handle66;

					object handle67 = settings.GetHandle("P_maxSizeTornadoPuff", "[Lava] max size of tornado puff", "[0.0-50.0] Default=25.0", DEF_MaxSizeTornadoPuff, ValidMaxLava);
					PARAM_MaxSizeTornadoPuff = () => (SettingHandle<float>)handle67;
                    #endregion

                }))();
				return;
			}
			catch (TypeLoadException)
			{
			}
			#region reset to default
			PARAM_VanillaLook = () => DEF_VanillaLook;
            PARAM_InstantConstruction = () => DEF_InstantConstruction;
			PARAM_CostEnabled = () => DEF_CostsEnabled;
			PARAM_CostAmount = () => DEF_CostAmount;
            PARAM_CostCustom = () => DEF_CostCustom;
			PARAM_WorkValue = () => DEF_WorkValue;
			PARAM_AlternativeIcon = () => DEF_AlternativeIcon;
			PARAM_EmiterTick = () => DEF_EmiterTick;
			PARAM_EmiterBaseConsumption = () => DEF_FuelConsumption;
			PARAM_EmiterRandomDistance = () => DEF_EmiterRandomDistance;
			PARAM_PlaceWithoutRestrictions = () => DEF_AllowPlacing;
			PARAM_RockyScatterEnabled = () => DEF_RemoveRockyScatter;
			PARAM_UseAllTerrain = () => DEF_GrabAllTerrain;
			PARAM_UseSilver = () => DEF_UseSilver;
			PARAM_TemperatureLimit = () => DEF_TemperatureLimit;
			PARAM_ForceMinified = () => DEF_ForceMinified;
			PARAM_KeyedLanguange = () => DEF_KeyedLanguage;
			PARAM_EmiterEnabled = () => DEF_EmiterEnabled;

			PARAM_LavaSlowdown = () => DEF_LavaSlowdown;

			PARAM_ChanceOfFire = () => DEF_ChanceOfFire;
			PARAM_ChanceOfHeatGlow = () => DEF_ChanceOfHeatGlow;
			PARAM_ChanceOfFireGlow = () => DEF_ChanceOfFireGlow;
			PARAM_ChanceOfLightningGlow = () => DEF_ChanceOfLightningGlow;
			PARAM_ChanceOfDustPuff = () => DEF_ChanceOfDustPuff;
			PARAM_ChanceOfAirPuff = () => DEF_ChanceOfAirPuff;
			PARAM_ChanceOfDustPuffthick = () => DEF_ChanceOfDustPuffthick;
			PARAM_ChanceOfMetaPuff = () => DEF_ChanceOfMetaPuff;
			PARAM_ChanceOfSmoke = () => DEF_ChanceOfSmoke;
			PARAM_ChanceOfTornadoPuff = () => DEF_ChanceOfTornadoPuff;

            PARAM_MaxSizeFire = () => DEF_MaxSizeFire;
			PARAM_MaxSizeHeatGlow = () => DEF_MaxSizeHeatGlow;
			PARAM_MaxSizeFireGlow = () => DEF_MaxSizeFireGlow;
			PARAM_MaxSizeLightningGlow = () => DEF_MaxSizeLightningGlow;
			PARAM_MaxSizeDustPuff = () => DEF_MaxSizeDustPuff;
			PARAM_MaxSizeDustPuffThick = () => DEF_MaxSizeDustPuffThick;
			PARAM_MaxSizeSmoke = () => DEF_MaxSizeSmoke;
			PARAM_MaxSizeTornadoPuff = () => DEF_MaxSizeTornadoPuff;
            #endregion
        }
        #endregion

        TRMod()
		{
			Harmony harmony = new Harmony("rimworld.mod.terraformrimworld");
			harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
			InitializeSettings();
		}



		private void LoadAndResolveModData()
		{   // TODO see river defs => this can be manipulated!
            isDebug = false;
            currentLanguage = LanguageDatabase.activeLanguage.FriendlyNameEnglish.ToLower();
            isCuproModActive = (DefDatabase<ThingDef>.GetNamed("Claystone", false) != null);
            isSZTerrainsModActive = ModLister.HasActiveModWithName("SZ_Terrains");
            isSZStonesAndTerrainsModActive = ModLister.HasActiveModWithName("SZ_StonesAndTerrains");
            isAdvancedBiomesModActive = ModLister.HasActiveModWithName("Advanced Biomes");
            zufallswert = new System.Random(50);
            SND_ROCK_NANITES = DefDatabase<SoundDef>.GetNamed(_Sound.MeleeHit_Metal_Sharp, false);
			SND_FLORA_NANITES = DefDatabase<SoundDef>.GetNamed(_Sound.MeleeHit_Wood, false);


            LoadSavedSettings();

            Helper.ResolveCustomCosts();
			Helper.CreateStormyAshRain();


			ThingCategories.Init();

			RoofEmiterManager.Init();
			ReplicatorPrototypManager.Init();
			RoofEmiterManager.CreateRoofEmiter();
			TectonicEmiterManager.Init();
			FloraEmiterManager.Init();
			MineralEmiterManager.Init();
			RockEmiterManager.Init();			
			TerraEmiterManager.Init();
			WeatherEmiterManager.Init();
			
			DesignatorDropdownGroupDefOf.Emiter.ResolveReferences();
	     	DesignatorDropdownGroupDefOf.Emiter.PostLoad();

			ThingCategories.UpdateRootCategories();

			Helper.ResortSpecialTerraformDesignators();

			DesignationCategoryDefOf.Terraform.ResolveReferences();
			DesignationCategoryDefOf.Terraform.PostLoad();

			if (!TRMod.OPTION_EmiterEnabled)
			{
				foreach (ThingDef thing in DefDatabase<ThingDef>.AllDefs)
				{
					if (thing != null && thing.designatorDropdown == DesignatorDropdownGroupDefOf.Emiter)
					{
						thing.designatorDropdown = null;
						thing.designationCategory = null;
						thing.ResolveReferences();
						thing.PostLoad();
					}
				}
				DesignatorDropdownGroupDefOf.Emiter.ResolveReferences();
				DesignatorDropdownGroupDefOf.Emiter.PostLoad();
				DesignationCategoryDefOf.Terraform.ResolveReferences();
				DesignationCategoryDefOf.Terraform.PostLoad();
			}

			foreach (ThingDef t in DefDatabase<ThingDef>.AllDefs)
			{
				if (t.defName != null && !t.IsBlueprint && !t.IsFrame && !t.IsEdifice() && !t.label.NullOrEmpty() && t.category != ThingCategory.Ethereal && t.category != ThingCategory.Mote)
					t.forceDebugSpawnable = true;
			}
		}
	}
}
