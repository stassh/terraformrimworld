using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace TerraformRimworld
{
	public static class Extensions
    {
        #region extend terrain
        public static List<string> GetNaturalTerrains()
        {
            List<string> l = new List<string>();
            l.Add("Soil");
            l.Add("SoilRich");
            l.Add("MossyTerrain");
            l.Add("MarshyTerrain");
            l.Add("Gravel");
            l.Add("Mud");
            l.Add("Sand");
            l.Add("SoftSand");
            l.Add("BrokenAsphalt");
            l.Add("PackedDirt");
            l.Add("Ice");                                    
            l.Add("Marsh");                     
            l.Add("WaterShallow");
            l.Add("WaterOceanShallow");
            l.Add("WaterMovingChestDeep");
            l.Add("WaterMovingShallow");
            l.Add("WaterDeep");
            l.Add("WaterOceanDeep");
            return l;
        }

        public static List<string> GetLiquidTerrains()
        {
            List<string> l = new List<string>();
            l.Add("Marsh");            
            l.Add("WaterShallow");
            l.Add("WaterOceanShallow");
            l.Add("WaterMovingChestDeep");
            l.Add("WaterMovingShallow");
            l.Add("WaterDeep");
            l.Add("WaterOceanDeep");
            return l;
        }

        public static List<string> GetBasicTerrains()
        {
            List<string> l = new List<string>();
            l.Add("Soil");
            l.Add("SoilRich");
            l.Add("MossyTerrain");
            l.Add("MarshyTerrain");
            l.Add("Gravel");
            l.Add("Mud");
            l.Add("Sand");
            l.Add("SoftSand");
            l.Add("BrokenAsphalt");
            l.Add("PackedDirt");
            l.Add("Ice");
            return l;
        }


        public static bool IsNatural(this TerrainDef terrain)
        {
            return GetNaturalTerrains().Contains(terrain.defName);
        }

        public static bool IsLiquid(this TerrainDef terrain)
        {
            return GetLiquidTerrains().Contains(terrain.defName);
        }

        public static bool IsBasic(this TerrainDef terrain)
        {
            return GetBasicTerrains().Contains(terrain.defName);
        }

        public static bool IsLava(this TerrainDef terrain)
        {
            return (terrain.defName != null && terrain.defName.StartsWith("Lava"));
        }

        public static bool IsRocky(this TerrainDef terrain)
        {
            return (terrain.defName != null && (terrain.defName.EndsWith("_Smooth") || terrain.defName.EndsWith("_RoughHewn") || terrain.defName.EndsWith("_Rough")));
        }

        public static bool IsExotic(this TerrainDef terrain)
        {
            return (terrain.defName != null && (terrain.defName.StartsWith("Exotic") || terrain.defName == "Moon" || terrain.defName == "MetalRim" || terrain.defName == "IceDeep"));
        }

        public static bool IsOther(this TerrainDef terrain)
        {
            return TRMod.OPTION_UseAllTerrain || (!terrain.IsNatural() && !terrain.IsRocky() && !terrain.IsExotic() && !terrain.IsLava() && !terrain.IsCarpet && terrain.designationCategory == null && terrain.designatorDropdown == null);
        }
		#endregion

		#region extend intvec
		public static ThingDef GetFilthFromPos(this IntVec3 pos, Map map)
		{
			List<Thing> thingList = pos.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Filth filth = thingList[i] as Filth;
				if (filth != null)
					return filth.def;
			}
			return null;
		}
		#endregion

		#region extend thingdef
		public static bool IsPlant(this Thing thing)
		{
			return (thing != null && thing.def != null && thing.def.defName != null && thing.def.plant != null);
		}

		public static bool IsPlant(this ThingDef td)
		{
			return (td != null && td.defName != null && td.plant != null);
		}

		public static bool IsMineableRock(this ThingDef td)
		{
			return (td != null && td.defName != null && td.building != null && td.building.mineableThing != null && !td.building.isResourceRock);
		}

		public static bool IsMineableMineral(this ThingDef td)
		{
			return (td != null && td.defName != null && td.building != null && td.building.mineableThing != null && td.building.isResourceRock);
		}

		public static void UpdateCosts(this ThingDef td, Dictionary<ThingDef, int> dicCosts)
		{
			if (td.costList == null)
				td.costList = new List<ThingDefCountClass>();
			else
				td.costList.Clear();

			foreach (ThingDef t in dicCosts.Keys)
				td.AddCosts(t, dicCosts[t]);
		}

		public static void UpdateStat(this ThingDef td, string statDefName, float value)
		{
			foreach (StatModifier sm in td.statBases)
			{
				if (sm.stat != null && sm.stat.defName == statDefName)
					sm.value = value;
			}
		}

		public static void AddStatBase(this ThingDef td, StatDef statDef, float value)
		{
			if (td.statBases == null)
				td.statBases = new List<StatModifier>();
			StatModifier sm = new StatModifier();
			sm.stat = statDef;
			sm.value = value;
			td.statBases.Add(sm);
		}

		public static void AddCosts(this ThingDef td, ThingDef costDef, int value)
		{
			if (td.costList == null)
				td.costList = new List<ThingDefCountClass>();
			td.costList.Add(new ThingDefCountClass(costDef, value));
		}

		public static void SetGraphicDataSingle(this ThingDef td, string texPath, string uiTexPath)
		{
			td.graphicData = new GraphicData();
			td.graphicData.texPath = texPath;
			td.graphicData.shaderType = ShaderTypeDefOf.MetaOverlay;
			td.graphicData.graphicClass = typeof(Graphic_Single);
			td.uiIconPath = uiTexPath;			
		}

		public static void RegisterBuildingDef(this ThingDef td)
		{
			if (DefDatabase<ThingDef>.GetNamed(td.defName, false) != null)
				return; // don't register already created things

			Type type = typeof(ThingDefGenerator_Buildings);
			MethodInfo frameInfo = type.GetMethod(name: "NewFrameDef_Thing", bindingAttr: BindingFlags.NonPublic | BindingFlags.Static) ?? throw new ArgumentNullException();
			td.frameDef = (ThingDef)frameInfo.Invoke(obj: null, parameters: new object[] { td });
			td.frameDef.ResolveReferences();
			td.frameDef.PostLoad();

			MethodInfo blueprintInfo = type.GetMethod(name: "NewBlueprintDef_Thing", bindingAttr: BindingFlags.NonPublic | BindingFlags.Static) ?? throw new ArgumentNullException();
			td.blueprintDef = (ThingDef)blueprintInfo.Invoke(obj: null, parameters: new object[] { td, false, null });
			td.blueprintDef.entityDefToBuild = td;
			td.blueprintDef.ResolveReferences();
			td.blueprintDef.PostLoad();

			ThingDef minifiedDef = null;
			if (td.minifiedDef != null)
			{
				minifiedDef = (ThingDef)blueprintInfo.Invoke(obj: null, parameters: new object[] { td, true, td.blueprintDef });
				minifiedDef.ResolveReferences();
				minifiedDef.PostLoad();
			}

    		td.ResolveReferences();
			td.PostLoad();


			MethodInfo shortHashGiver = typeof(ShortHashGiver).GetMethod(name: "GiveShortHash", bindingAttr: BindingFlags.NonPublic | BindingFlags.Static) ?? throw new ArgumentNullException();
			Type t = typeof(ThingDef);
			shortHashGiver.Invoke(obj: null, parameters: new object[] { td, t });
			if (td.minifiedDef != null)
				shortHashGiver.Invoke(obj: null, parameters: new object[] { minifiedDef, t });
			shortHashGiver.Invoke(obj: null, parameters: new object[] { td.blueprintDef, t });
			shortHashGiver.Invoke(obj: null, parameters: new object[] { td.frameDef, t });

			DefDatabase<ThingDef>.Add(td);
			if (td.minifiedDef != null)
				DefDatabase<ThingDef>.Add(minifiedDef);
			DefDatabase<ThingDef>.Add(td.blueprintDef);
			DefDatabase<ThingDef>.Add(td.frameDef);
		}
		#endregion

		#region extend string
		public static string SubstringFrom(this String text, string from, int occuranceCount)
        {
            string temp = text;
            for (int i = 0; i < occuranceCount; i++)
                temp = temp.SubstringFrom(from);
            return temp;
        }
        public static string SubstringTo(this String text, string to, int occuranceCount)
        {
            string temp = text;
            string sub = "";
            for (int i = 0; i < occuranceCount; i++)
            {
                sub += temp.SubstringTo(to, false);
                temp = temp.SubstringFrom(to);
            }
            return sub;
        }
        /// <summary>
        /// Teilstring ab dem angegebenen 'startFrom' String
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startFrom"></param>
        /// <returns></returns>
        public static string SubstringFrom(this String text, string startFrom, bool withoutIt = true)
        {
            int pos = text.IndexOf(startFrom);
            if (pos >= 0)
            {
                if (withoutIt)
                    return text.Substring(pos + startFrom.Length);
                else
                    return text.Substring(pos);
            }
            else
                return text;
        }
        /// <summary>
        /// Teilstring bis zum angegebenen 'endOn' String
        /// </summary>
        /// <param name="text"></param>
        /// <param name="endOn"></param>
        /// <returns></returns>
        public static string SubstringTo(this String text, string endOn, bool withoutIt = true)
        {
            int pos = text.IndexOf(endOn);
            if (pos >= 0)
            {
                if (withoutIt)
                    return text.Substring(0, pos);
                else
                    return text.Substring(0, pos + endOn.Length);
            }
            else
                return text;
        }
		#endregion
    }
}
