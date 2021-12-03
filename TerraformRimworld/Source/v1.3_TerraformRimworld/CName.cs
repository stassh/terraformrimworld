using System.Collections.Generic;

namespace TerraformRimworld
{
	public static class _Plant
	{
		public const string Unknown = "Plant_Unknown";

		public static bool IsNaturalPlant(string name)
		{
			if (name == "Plant_Agave" ||
				name == "Plant_Alocasia" ||
				name == "Plant_Ambrosia" ||
				name == "Plant_Astragalus" ||
				name == "Plant_Berry" ||
				name == "Plant_Brambles" ||
				name == "Plant_Bush" ||
				name == "Plant_Chokevine" ||
				name == "Plant_Clivia" ||
				name == "Plant_Corn" ||
				name == "Plant_Cotton" ||
				name == "Plant_Dandelion" ||
				name == "Plant_Daylily" ||
				name == "Plant_Devilstrand" ||
				name == "Plant_Grass" ||
				name == "Plant_Haygrass" ||
				name == "Plant_Healroot" ||
				name == "Plant_HealrootWild" ||
				name == "Plant_Hops" ||
				name == "Plant_Moss" ||
				name == "Plant_PincushionCactus" ||
				name == "Plant_Potato" ||
				name == "Plant_Psychoid" ||
				name == "Plant_Rafflesia" ||
				name == "Plant_Rice" ||
				name == "Plant_Rose" ||
				name == "Plant_SaguaroCactus" ||
				name == "Plant_ShrubLow" ||
				name == "Plant_Smokeleaf" ||
				name == "Plant_Strawberry" ||
				name == "Plant_TallGrass" ||
				name == "Plant_TreeBamboo" ||
				name == "Plant_TreeBirch" ||
				name == "Plant_TreeCecropia" ||
				name == "Plant_TreeCocoa" ||
				name == "Plant_TreeCypress" ||
				name == "Plant_TreeDrago" ||
				name == "Plant_TreeMaple" ||
				name == "Plant_TreeOak" ||
				name == "Plant_TreePalm" ||
				name == "Plant_TreePine" ||
				name == "Plant_TreePoplar" ||
				name == "Plant_TreeTeak" ||
				name == "Plant_TreeWillow" ||
				name == "Agarilux" ||
				name == "Bryolux" ||
				name == "BurnedTree" ||
				name == "Glowstool")
				return true;
			else
				return false;
		}
	}

	public static class _Color
	{
		public const string White = "TColor_White";
		public const string LightGray = "TColor_LightGray";
		public const string Gray = "TColor_Gray";
		public const string DarkGray= "TColor_DarkGray";
		public const string Graphite = "TColor_Graphite";
		public const string Black = "TColor_Black";
		public const string NavyBlue= "TColor_NavyBlue";
		public const string DarkBlue= "TColor_DarkBlue";
		public const string RoyalBlue= "TColor_RoyalBlue";
		public const string Blue = "TColor_Blue";
		public const string PureBlue= "TColor_PureBlue";
		public const string LightBlue= "TColor_LightBlue";
		public const string SkyBlue= "TColor_SkyBlue";
		public const string Maroon = "TColor_Maroon";
		public const string Burgundy = "TColor_Burgundy";
		public const string DarkRed= "TColor_DarkRed";
		public const string Red = "TColor_Red";
		public const string PureRed= "TColor_PureRed";
		public const string LightRed= "TColor_LightRed";
		public const string HotPink= "TColor_HotPink";
		public const string Pink = "TColor_Pink";
		public const string DarkPurple= "TColor_DarkPurple";
		public const string Purple = "TColor_Purple";
		public const string LightPurple= "TColor_LightPurple";
		public const string Teal = "TColor_Teal";
		public const string Turquoise = "TColor_Turquoise";
		public const string DarkBrown= "TColor_DarkBrown";
		public const string Brown = "TColor_Brown";
		public const string LightBrown= "TColor_LightBrown";
		public const string Tawny = "TColor_Tawny";
		public const string BlazeOrange= "TColor_BlazeOrange";
		public const string Orange = "TColor_Orange";
		public const string LightOrange= "TColor_LightOrange";
		public const string Gold = "TColor_Gold";
		public const string YellowGold= "TColor_YellowGold";
		public const string Yellow = "TColor_Yellow";
		public const string DarkYellow= "TColor_DarkYellow";
		public const string Chartreuse = "TColor_Chartreuse";
		public const string LightYellow= "TColor_LightYellow";
		public const string DarkGreen= "TColor_DarkGreen";
		public const string Green = "TColor_Green";
		public const string PureGreen= "TColor_PureGreen";
		public const string LimeGreen= "TColor_LimeGreen";
		public const string LightGreen= "TColor_LightGreen";
		public const string DarkOlive= "TColor_DarkOlive";
		public const string Olive = "TColor_Olive";
		public const string OliveDrab= "TColor_OliveDrab";
		public const string FoilageGreen= "TColor_FoilageGreen";
		public const string Tan = "TColor_Tan";
		public const string Beige = "TColor_Beige";
		public const string Khaki = "TColor_Khaki";
		public const string Peach = "TColor_Peach";
		public const string Cream = "TColor_Cream";
	}

	public static class _Recipe
	{
		#region replicator
		public const string ReplPlaceLocalFilth = "RCP_ReplPlaceLocalFilth";
		public const string ReplPlaceGlobalFilth = "RCP_ReplPlaceGlobalFilth";
		public const string ReplCleanLocal = "RCP_ReplCleanLocal";
		public const string ReplCleanGlobal = "RCP_ReplCleanGlobal";
		public const string ReplPlaceItem = "RCP_ReplPlaceItem";
		public const string ReplAbsorbItem = "RCP_ReplAbsorbItem";
		#endregion

		#region terra       
		public const string TerraPlaceLocal = "RCP_TerraPlaceLocal";
        public const string TerraPlaceGlobal = "RCP_TerraPlaceGlobal";
		public const string TerraRemoveLocal = "RCP_TerraRemoveLocal";
		public const string TerraRemoveGlobal = "RCP_TerraRemoveGlobal";
		public const string TerraReplaceLocal = "RCP_TerraReplaceLocal";
        public const string TerraReplaceGlobal = "RCP_TerraReplaceGlobal";
        public const string TerraReplaceBiome = "RCP_TerraReplaceBiome";
		public const string TerraColorize = "RCP_TerraColorize";
		#endregion

		#region flora
		public const string FloraPlaceLocal = "RCP_FloraPlaceLocal";
		public const string FloraPlaceGlobal = "RCP_FloraPlaceGlobal";
		public const string FloraRemoveLocal = "RCP_FloraRemoveLocal";
		public const string FloraRemoveGlobal = "RCP_FloraRemoveGlobal";
		public const string FloraReplaceLocal = "RCP_FloraReplaceLocal";
		public const string FloraReplaceGlobal = "RCP_FloraReplaceGlobal";
		public const string FloraPlaceOfBiome = "RCP_FloraPlaceOfBiome";
		public const string FloraRemoveOfBiome = "RCP_FloraRemoveOfBiome";
		public const string FloraAddFragrance = "RCP_FloraAddFragrance";
		public const string FloraSubFragrance = "RCP_FloraSubFragrance";
		public const string FloraColorize = "RCP_FloraColorize";
		#endregion

		#region tectonic
		public const string TectonicCustomPulse = "RCP_TectonicCustomPulse";
		public const string TectonicAddRotation = "RCP_TectonicAddRotation";
		public const string TectonicSubRotation = "RCP_TectonicSubRotation";
		#endregion

		#region roofs
		public const string RoofPlaceLocal = "RCP_RoofPlaceLocal";
		public const string RoofRemoveLocal = "RCP_RoofRemoveLocal";
		public const string RoofPlaceGlobal = "RCP_RoofPlaceGlobal";
		public const string RoofRemoveGlobal = "RCP_RoofRemoveGlobal";
		#endregion

		#region weather
		public const string WeatherChange = "RCP_WeatherChange";
		public const string WeatherAddTemp = "RCP_WeatherAddTemp";
		public const string WeatherSubTemp = "RCP_WeatherSubTemp";
		public const string WeatherRefog = "RCP_WeatherRefog";
		//public const string ToxicFallout = "RCP_ToxicFallout";
		//public const string VolcanicWinter = "RCP_VolcanicWinter";
		//public const string Heatwave = "RCP_HeatWave";
		//public const string Coldsnap = "RCP_ColdSnap";
		//public const string Flashstorm = "RCP_FlashStorm";
		//public const string Eclipse = "RCP_Eclipse";
		#endregion

		#region rocks
		public const string RockPlaceLocal = "RCP_RockPlaceLocal";
		public const string RockPlaceGlobal = "RCP_RockPlaceGlobal";
        public const string RockRemoveLocal = "RCP_RockRemoveLocal";
        public const string RockRemoveGlobal = "RCP_RockRemoveGlobal";
		public const string RockReplaceLocal = "RCP_RockReplaceLocal";
		public const string RockReplaceGlobal = "RCP_RockReplaceGlobal";
		public const string RockColorize = "RCP_RockColorize";
		#endregion

		#region minerals
		public const string MineralPlaceLocal = "RCP_MineralPlaceLocal";
		public const string MineralPlaceGlobal = "RCP_MineralPlaceGlobal";
		public const string MineralRemoveLocal = "RCP_MineralRemoveLocal";
		public const string MineralRemoveGlobal = "RCP_MineralRemoveGlobal";
		public const string MineralReplaceLocal = "RCP_MineralReplaceLocal";
		public const string MineralReplaceGlobal = "RCP_MineralReplaceGlobal";
		public const string MineralColorize = "RCP_MineralColorize";
		#endregion
	}

	public static class _ThingCategory
    {
        public const string Terraform = "TR_ThingCategoryTerraform";
        public const string Terrain = "TR_ThingCategoryTerrain";
        public const string Flora = "TR_ThingCategoryFlora";
		public const string Biome = "TR_ThingCategoryBiome";
        public const string Rocks = "TR_ThingCategoryRocks";
        public const string Minerals = "TR_ThingCategoryMinerals";
		public const string Filth = "TR_ThingCategoryFilth";
		public const string FromTerrain = "TR_ThingCategoryFromTerrain";
		public const string FromFlora = "TR_ThingCategoryFromFlora";
		public const string FromRock = "TR_ThingCategoryFromRock";
		public const string FromMineral = "TR_ThingCategoryFromMineral";
		public const string Farbe = "TR_ThingCategoryFarbe";
		public const string ToTerrain = "TR_ThingCategoryToTerrain";
		public const string LBL_Terraform = "Terraform";
        public const string LBL_Terrain = "Terrain";
		public const string LBL_FromTerrain = "FromTerrain";
		public const string LBL_FromFlora = "FromFlora";
		public const string LBL_FromRock = "FromRocks";
		public const string LBL_FromMineral = "FromMinerals";
		public const string LBL_Farbe = "Color";
		public const string LBL_ToTerrain = "ToTerrain";
        public const string LBL_Flora = "Flora";
		public const string LBL_Biome = "Biome";
        public const string LBL_Rocks = "Rocks";
        public const string LBL_Minerals = "Minerals";
		public const string LBL_Filth = "Filth";

        public const string BuildingMisc = "BuildingsMisc";
    }

    public static class _Terrain
    {
        public const string Blueprint = "TerraformBlueprint";

        public const string Soil = "Soil";
        public const string MossyTerrain = "MossyTerrain";
        public const string Gravel = "Gravel";
        public const string Sand = "Sand";
        public const string BrokenAsphalt = "BrokenAsphalt";
        public const string PackedDirt = "PackedDirt";
        public const string Ice = "Ice";
        public const string SoftSand = "SoftSand";
        public const string Mud = "Mud";
        public const string Marsh = "Marsh";
        public const string MarshyTerrain = "MarshyTerrain";
        public const string SoilRich = "SoilRich";
        public const string WaterShallow = "WaterShallow";
        public const string WaterOceanShallow = "WaterOceanShallow";
        public const string WaterMovingChestDeep = "WaterMovingChestDeep";
        public const string WaterMovingShallow = "WaterMovingShallow";
        public const string WaterDeep = "WaterDeep";
        public const string WaterOceanDeep = "WaterOceanDeep";

        public const string Lava = "Lava";
        public const string LavaBlue = "LavaBlue";
        public const string LavaRim = "LavaRim";
        public const string Moon = "Moon";
        public const string MetalRim = "MetalRim";
        public const string IceDeep = "IceDeep";
        public const string DrySandstoneSmooth = "DrySandstone_Smooth";
        public const string DrySandstoneRough = "DrySandstone_Rough";
        public const string DrySandstoneRoughHewn = "DrySandstone_RoughHewn";
        public const string WhiteLimestoneSmooth = "WhiteLimestone_Smooth";
        public const string WhiteLimestoneRough = "WhiteLimestone_Rough";
        public const string WhiteLimestoneRoughHewn = "WhiteLimestone_RoughHewn";

        public const string Any = "Any";
        public const string Rocky = "Rocky";
        public const string Stones = "Stones";
        public const string None = "None";
        public const string Water = "Water";

        public static string GetUIIcon(string terrainName, string fallbackIcon)
        {
            if (terrainName == IceDeep)
                return "IconIceDeep";
            else if (terrainName == Lava)
                return "IconLava";
            else if (terrainName == LavaBlue)
                return "IconLavaBlue";
            else if (terrainName == LavaRim)
                return "IconLavaHot";
            else if (terrainName == MetalRim)
                return "IconMetal";
            else if (terrainName == Moon)
                return "IconMoon";
            else
                return fallbackIcon;
        }
    }

    public static class _Emiter
    {
		public const string ReplicatorPrototyp = "ReplicatorPrototyp";
		public const string TectonicEmiter = "TectonicEmiter";      
        public const string TerraEmiter = "TerraEmiter";
        public const string FloraEmiter = "FloraEmiter";
        public const string RockEmiter = "RockEmiter";
        public const string MineralEmiter = "MineralEmiter";
        public const string MountainsEmiter = "MountainsEmiter";
        public const string WeatherEmiter = "WeatherEmiter";
    }

    public static class _Sound
    {
        public const string MeleeHit_Metal_Sharp = "MeleeHit_Metal_Sharp";
        public const string MeleeHit_Wood = "MeleeHit_Wood";
        public const string BulletImpact_Metal = "BulletImpact_Metal";        
    }

    public static class _Text
    {
        public const string _Smooth = "_Smooth";
        public const string _RoughHewn = "_RoughHewn";
        public const string _Rough = "_Rough";
        public const string T_ = "T_";
		public const string Color = "Color ";
        public const string RESROCK_ = "RESROCK_";
		public const string TRPLNT_ = "TRPLNT_";
        public const string ThickRoof_ = "ThickRoof_";
        public const string RoofRockThick = "RoofRockThick";
		public const string TerraformingArea = "Terraforming";
		public const string TBI_ = "TBI_";
		public const string TColor_ = "TColor_";
		public const string FromTerrain_ = "FromTerrain_";
	}

    public static class _Stones
    {
        public const string Claystone = "Claystone";
        public const string Andesite = "Andesite";
        public const string Syenite = "Syenite";
        public const string Gneiss = "Gneiss";
        public const string Quartzite = "Quartzite";
        public const string Schist = "Schist";
        public const string Gabbro = "Gabbro";
        public const string Diorite = "Diorite";
        public const string Dunite = "Dunite";
        public const string Pegmatite = "Pegmatite";
        public const string DrySandstone = "DrySandstone";
        public const string WhiteLimestone = "WhiteLimestone";
        public const string Obsidian = "Obsidian";
        public const string Resource = "Resource";
        public const string ResourceBlueprint = "ResourceBlueprint";
        public const string RockBlueprint = "RockBlueprint";
        public const string Marmor = "Marble";
        public const string Schiefer = "Slate";
        public const string Kalkstein = "Limestone";
        public const string Granit = "Granite";
        public const string Sandstein = "Sandstone";
        public const string ComponentsIndustrial = "MineableComponentsIndustrial";
        public const string CollapsedRocks = "CollapsedRocks";

        public struct IconSpecial
        {
            public const string IconBanded = "IconR_Banded";
            public const string IconFlecked = "IconR_Flecked";
            public const string IconGranitic = "IconR_Granitic";
            public const string IconMarbled = "IconR_Marbled";
            public const string IconNormal = "IconR_Normal";
            public const string IconSmooth = "IconR_Smooth";
            public const string IconSmoothed = "IconR_Smoothed";
            public const string IconLava = "IconR_Lava";
            public const string IconVanilla = "IconR_Vanilla";
            public const string IconResource = "IconR_Resource";
            public const string IconResourceBlueprint = "IconR_ResourceBlueprint";
            public const string IconRockBlueprint = "IconR_VanillaBlueprint";
            public const string IconComponent = "IconR_Component";
            public const string IconCollapsed = "IconR_Collapsed";
        }

        public struct StoneIcon
        {
            public const string IconSmooth = "IconRSmooth";
            public const string IconGranitic = "IconRGranitic";
            public const string IconBanded = "IconRBanded";
            public const string IconNormal = "IconRNormal";
            public const string IconVanilla = "IconRVanilla";
            public const string IconMarbled = "IconRMarbled";
            public const string IconSmoothRock = "IconSmoothRock";
            public const string IconRLava = "IconRLava";
            public const string IconResRock = "IconResRock";
        }

        public static string GetUIIconByName(string name)
        {
            if (name == _Stones.Claystone)
                return StoneIcon.IconSmooth;
            else if (name == _Stones.Andesite)
                return StoneIcon.IconGranitic;
            else if (name == _Stones.Syenite)
                return StoneIcon.IconGranitic;
            else if (name == _Stones.Gneiss)
                return StoneIcon.IconBanded;
            else if (name == _Stones.Quartzite)
                return StoneIcon.IconNormal;
            else if (name == _Stones.Schist)
                return StoneIcon.IconNormal;
            else if (name == _Stones.Gabbro)
                return StoneIcon.IconGranitic;
            else if (name == _Stones.Diorite)
                return StoneIcon.IconGranitic;
            else if (name == _Stones.Dunite)
                return StoneIcon.IconNormal;
            else if (name == _Stones.Pegmatite)
                return StoneIcon.IconNormal;
            else if (name == _Stones.Obsidian)
                return StoneIcon.IconRLava;
            else if (name.StartsWith("Smoothed"))
                return StoneIcon.IconSmoothRock;
            else if (name.StartsWith(_Text.RESROCK_))
                return StoneIcon.IconResRock;
            else
            {
                if (TRMod.isCuproModActive)
                {
                    if (name == _Stones.Marmor)
                        return StoneIcon.IconMarbled;
                    else if (name == _Stones.Schiefer)
                        return StoneIcon.IconSmooth;
                    else if (name == _Stones.Kalkstein)
                        return StoneIcon.IconSmooth;
                    else if (name == _Stones.Granit)
                        return StoneIcon.IconGranitic;
                    else if (name == _Stones.Sandstein)
                        return StoneIcon.IconSmooth;
                    else if (name.StartsWith("Smoothed"))
                        return StoneIcon.IconSmoothRock;
                    else
                        return StoneIcon.IconVanilla;
                }
                else if (name.StartsWith("Smoothed"))
                    return StoneIcon.IconSmoothRock;
                else
                    return StoneIcon.IconVanilla;
            }
        }

        public static string GetUIIconByNameCubed(string name, bool isMineral)
        {
            if (name == _Stones.Claystone)
                return IconSpecial.IconSmooth;
            else if (name == _Stones.Andesite)
                return IconSpecial.IconGranitic;
            else if (name == _Stones.Syenite)
                return IconSpecial.IconGranitic;
            else if (name == _Stones.Gneiss)
                return IconSpecial.IconBanded;
            else if (name == _Stones.Quartzite)
                return IconSpecial.IconNormal;
            else if (name == _Stones.Schist)
                return IconSpecial.IconNormal;
            else if (name == _Stones.Gabbro)
                return IconSpecial.IconGranitic;
            else if (name == _Stones.Diorite)
                return IconSpecial.IconGranitic;
            else if (name == _Stones.Dunite)
                return IconSpecial.IconNormal;
            else if (name == _Stones.Pegmatite)
                return IconSpecial.IconNormal;
            else if (name == _Stones.Obsidian)
                return IconSpecial.IconLava;
            else if (name == _Stones.Resource)
                return IconSpecial.IconResource;
            else if (name == _Stones.ResourceBlueprint)
                return IconSpecial.IconResourceBlueprint;
            else if (name == _Stones.RockBlueprint)
                return IconSpecial.IconRockBlueprint;
            else if (name == _Stones.ComponentsIndustrial)
                return IconSpecial.IconComponent;
            else if (name == _Stones.CollapsedRocks)
                return IconSpecial.IconCollapsed;
            else
            {
                if (TRMod.isCuproModActive)
                {
                    if (name == _Stones.Marmor)
                        return IconSpecial.IconMarbled;
                    else if (name == _Stones.Schiefer)
                        return IconSpecial.IconSmooth;
                    else if (name == _Stones.Kalkstein)
                        return IconSpecial.IconSmooth;
                    else if (name == _Stones.Granit)
                        return IconSpecial.IconGranitic;
                    else if (name == _Stones.Sandstein)
                        return IconSpecial.IconSmooth;
                    else if (name.StartsWith("Smoothed"))
                        return IconSpecial.IconSmoothed;
                    else if (isMineral)
                        return IconSpecial.IconResource;
                    else
                        return IconSpecial.IconVanilla;
                }
                else if (name.StartsWith("Smoothed"))
                    return IconSpecial.IconSmoothed;
                else if (isMineral)
                    return IconSpecial.IconResource;
                else
                    return IconSpecial.IconVanilla;
            }
        }
    }

    public static class _Roof
    {
        public const string BlueprintThick = "ThickRockBlueprint";
        public const string IconThick = "addMountainRoof";
    }
}
