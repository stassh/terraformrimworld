using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace TerraformRimworld
{
	public static class ThingCategories
	{
		public static ThingCategoryDef Biome;
		public static Dictionary<string, Color> dicColors;

		public static ThingCategoryDef Farbe;

		public static ThingCategoryDef Filth;

		public static ThingCategoryDef Flora;

		public static ThingCategoryDef FromFlora;

		public static ThingCategoryDef FromMineral;

		public static ThingCategoryDef FromRock;

		public static ThingCategoryDef FromTerrain;

		public static ThingCategoryDef Minerals;

		public static ThingCategoryDef Rocks;

		public static ThingCategoryDef Terraform;

		public static ThingCategoryDef Terrain;

		public enum Category
		{
			Terraform,
			FromTerrain,
			Terrain,
			FromRock,
			Rocks,
			FromMineral,
			Minerals,
			FromFlora,
			Flora,
			Biome,
			Filth,
			Farbe
		}
		public static void AddToThingCategory(ThingCategoryDef category, ThingDef thing)
		{
			if (!category.childThingDefs.Contains(thing))
				category.childThingDefs.Add(thing);
		}

		public static void CloneCategory(ThingCategoryDef sourceCategoryDef, ThingCategoryDef targetCategoryDef)
		{
			foreach (ThingDef t in sourceCategoryDef.childThingDefs)
			{
				ThingDef tfrom = new ThingDef();
				tfrom.label = t.label;
				tfrom.description = t.defName;
				AddToThingCategory(targetCategoryDef, tfrom);
			}
			targetCategoryDef.ResolveReferences();
			targetCategoryDef.PostLoad();
		}

		public static void CreateCategory(Category category)
		{
			switch (category)
			{
				case Category.Terraform:
					Terraform = CreateThingCategory(_ThingCategory.Terraform, _ThingCategory.LBL_Terraform, ThingCategoryDefOf.Root);
					break;

				case Category.Terrain:
					Terrain = CreateThingCategory(_ThingCategory.Terrain, _ThingCategory.LBL_Terrain, Terraform);
					break;

				case Category.Rocks:
					Rocks = CreateThingCategory(_ThingCategory.Rocks, _ThingCategory.LBL_Rocks, Terraform);
					break;

				case Category.Minerals:
					Minerals = CreateThingCategory(_ThingCategory.Minerals, _ThingCategory.LBL_Minerals, Terraform);
					break;

				case Category.Flora:
					Flora = CreateThingCategory(_ThingCategory.Flora, _ThingCategory.LBL_Flora, Terraform);
					break;

				case Category.Biome:
					Biome = CreateThingCategory(_ThingCategory.Biome, _ThingCategory.LBL_Biome, Terraform);
					break;

				case Category.Filth:
					Filth = CreateThingCategory(_ThingCategory.Filth, _ThingCategory.LBL_Filth, Terraform);
					break;

				case Category.FromTerrain:
					FromTerrain = CreateThingCategory(_ThingCategory.FromTerrain, _ThingCategory.LBL_FromTerrain, Terraform);
					break;

				case Category.FromFlora:
					FromFlora = CreateThingCategory(_ThingCategory.FromFlora, _ThingCategory.LBL_FromFlora, Terraform);
					break;

				case Category.FromRock:
					FromRock = CreateThingCategory(_ThingCategory.FromRock, _ThingCategory.LBL_FromRock, Terraform);
					break;

				case Category.FromMineral:
					FromMineral = CreateThingCategory(_ThingCategory.FromMineral, _ThingCategory.LBL_FromMineral, Terraform);
					break;

				case Category.Farbe:
					Farbe = CreateThingCategory(_ThingCategory.Farbe, _ThingCategory.LBL_Farbe, Terraform);
					break;

				default:
					break;
			}
		}

		public static ThingCategoryDef CreateThingCategory(string defName, string label, ThingCategoryDef parent)
		{
			ThingCategoryDef tcd = DefDatabase<ThingCategoryDef>.GetNamed(defName, false);
			if (tcd == null)
			{
				tcd = new ThingCategoryDef();
				tcd.defName = defName;
				tcd.label = label;
				tcd.parent = parent;
				tcd.parent.childCategories.Add(tcd);
				tcd.ResolveReferences();
				tcd.PostLoad();
				DefDatabase<ThingCategoryDef>.Add(tcd);
			}
			return tcd;
		}

		public static void Init()
		{
			CreateCategory(Category.Terraform);
			CreateCategory(Category.FromTerrain);
			CreateCategory(Category.Terrain);
			CreateCategory(Category.FromRock);
			CreateCategory(Category.Rocks);
			CreateCategory(Category.FromMineral);
			CreateCategory(Category.Minerals);
			CreateCategory(Category.FromFlora);
			CreateCategory(Category.Flora);
			CreateCategory(Category.Biome);
			CreateCategory(Category.Filth);
			CreateCategory(Category.Farbe);
			PopulateCategoryBiome();
			PopulateCategoryColor();
		}

		public static void PopulateCategoryBiome()
		{
			foreach (BiomeDef biome in DefDatabase<BiomeDef>.AllDefs)
			{
				ThingDef t = DefDatabase<ThingDef>.GetNamed(_Text.TBI_ + biome.defName, false);
				if (t == null)
				{
					t = new ThingDef();
					t.defName = _Text.TBI_ + biome.defName;
					t.label = biome.label;

					var l = (List<BiomePlantRecord>)biome.GetMemberValue("wildPlants");
					List<ThingDef> lBiomePlants = new List<ThingDef>();
					foreach (BiomePlantRecord r in l)
						lBiomePlants.Add(r.plant);

					t.description = biome.defName + "\nPlants:" + Helper.ListToString(lBiomePlants) + "\nTerrains:" + Helper.ListToString(biome.AllTerrains());
					//ThingDef test = lBiomePlants.First();
					//if (test != null)
					//{
					//}
					t.graphicData = new GraphicData();
					t.graphicData.color = Color.white;

					MethodInfo shortHashGiver = typeof(ShortHashGiver).GetMethod(name: "GiveShortHash", bindingAttr: BindingFlags.NonPublic | BindingFlags.Static) ?? throw new ArgumentNullException();
					Type type = typeof(ThingDef);
					shortHashGiver.Invoke(obj: null, parameters: new object[] { t, type });

					t.ResolveReferences();
					t.PostLoad();
					DefDatabase<ThingDef>.Add(t);
				}
				AddToThingCategory(Biome, t);
			}
			Biome.ResolveReferences();
			Biome.PostLoad();
		}

		public static void PopulateCategoryColor()
		{
			dicColors = new Dictionary<string, Color>();
			dicColors.Add(_Color.White, new Color(1, 1, 1));
			dicColors.Add(_Color.LightGray, new Color(0.82f, 0.824f, 0.831f));
			dicColors.Add(_Color.Gray, new Color(0.714f, 0.718f, 0.733f));
			dicColors.Add(_Color.DarkGray, new Color(0.506f, 0.51f, 0.525f));
			dicColors.Add(_Color.Graphite, new Color(0.345f, 0.345f, 0.353f));
			dicColors.Add(_Color.Black, new Color(0, 0, 0));
			dicColors.Add(_Color.NavyBlue, new Color(0, 0.082f, 0.267f));
			dicColors.Add(_Color.DarkBlue, new Color(0.137f, 0.235f, 0.486f));
			dicColors.Add(_Color.RoyalBlue, new Color(0.157f, 0.376f, 0.678f));
			dicColors.Add(_Color.Blue, new Color(0.004f, 0.42f, 0.718f));
			dicColors.Add(_Color.PureBlue, new Color(0, 0, 1));
			dicColors.Add(_Color.LightBlue, new Color(0.129f, 0.569f, 0.816f));
			dicColors.Add(_Color.SkyBlue, new Color(0.58f, 0.757f, 0.91f));
			dicColors.Add(_Color.Maroon, new Color(0.373f, 0, 0.125f));
			dicColors.Add(_Color.Burgundy, new Color(0.478f, 0.153f, 0.255f));
			dicColors.Add(_Color.DarkRed, new Color(0.545f, 0, 0));
			dicColors.Add(_Color.Red, new Color(0.624f, 0.039f, 0.055f));
			dicColors.Add(_Color.PureRed, new Color(1, 0, 0));
			dicColors.Add(_Color.LightRed, new Color(0.784f, 0.106f, 0.216f));
			dicColors.Add(_Color.HotPink, new Color(0.863f, 0.345f, 0.631f));
			dicColors.Add(_Color.Pink, new Color(0.969f, 0.678f, 0.808f));
			dicColors.Add(_Color.DarkPurple, new Color(0.251f, 0.157f, 0.384f));
			dicColors.Add(_Color.Purple, new Color(0.341f, 0.176f, 0.561f));
			dicColors.Add(_Color.LightPurple, new Color(0.631f, 0.576f, 0.784f));
			dicColors.Add(_Color.Teal, new Color(0.11f, 0.576f, 0.592f));
			dicColors.Add(_Color.Turquoise, new Color(0.027f, 0.51f, 0.58f));
			dicColors.Add(_Color.DarkBrown, new Color(0.282f, 0.2f, 0.125f));
			dicColors.Add(_Color.Brown, new Color(0.388f, 0.204f, 0.102f));
			dicColors.Add(_Color.LightBrown, new Color(0.58f, 0.353f, 0.196f));
			dicColors.Add(_Color.Tawny, new Color(0.784f, 0.329f, 0.098f));
			dicColors.Add(_Color.BlazeOrange, new Color(0.941f, 0.29f, 0.141f));
			dicColors.Add(_Color.Orange, new Color(0.949f, 0.369f, 0.133f));
			dicColors.Add(_Color.LightOrange, new Color(0.973f, 0.58f, 0.133f));
			dicColors.Add(_Color.Gold, new Color(0.824f, 0.624f, 0.055f));
			dicColors.Add(_Color.YellowGold, new Color(1, 0.761f, 0.051f));
			dicColors.Add(_Color.Yellow, new Color(1, 0.859f, 0.004f));
			dicColors.Add(_Color.DarkYellow, new Color(0.953f, 0.886f, 0.227f));
			dicColors.Add(_Color.Chartreuse, new Color(0.922f, 0.91f, 0.067f));
			dicColors.Add(_Color.LightYellow, new Color(1, 0.91f, 0.51f));
			dicColors.Add(_Color.DarkGreen, new Color(0, 0.345f, 0.149f));
			dicColors.Add(_Color.Green, new Color(0.137f, 0.663f, 0.29f));
			dicColors.Add(_Color.PureGreen, new Color(0, 1, 0));
			dicColors.Add(_Color.LimeGreen, new Color(0.682f, 0.82f, 0.208f));
			dicColors.Add(_Color.LightGreen, new Color(0.541f, 0.769f, 0.537f));
			dicColors.Add(_Color.DarkOlive, new Color(0.255f, 0.282f, 0.149f));
			dicColors.Add(_Color.Olive, new Color(0.451f, 0.463f, 0.294f));
			dicColors.Add(_Color.OliveDrab, new Color(0.357f, 0.337f, 0.263f));
			dicColors.Add(_Color.FoilageGreen, new Color(0.482f, 0.498f, 0.443f));
			dicColors.Add(_Color.Tan, new Color(0.718f, 0.631f, 0.486f));
			dicColors.Add(_Color.Beige, new Color(0.827f, 0.741f, 0.545f));
			dicColors.Add(_Color.Khaki, new Color(0.933f, 0.835f, 0.678f));
			dicColors.Add(_Color.Peach, new Color(0.996f, 0.859f, 0.733f));
			dicColors.Add(_Color.Cream, new Color(0.996f, 0.929f, 0.812f));

			foreach (string key in dicColors.Keys)
			{
				ThingDef t = DefDatabase<ThingDef>.GetNamed(key, false);
				if (t == null)
				{
					t = new ThingDef();
					t.defName = key;
					t.label = TRMod.OPTION_KeyedLanguange ? key.Translate().ToString() : key.Replace(_Text.TColor_, "");
					t.description = _Text.Color + t.label;
					t.uiIconColor = dicColors[key];
					t.graphicData = new GraphicData();
					t.graphicData.color = dicColors[key];

					MethodInfo shortHashGiver = typeof(ShortHashGiver).GetMethod(name: "GiveShortHash", bindingAttr: BindingFlags.NonPublic | BindingFlags.Static) ?? throw new ArgumentNullException();
					Type type = typeof(ThingDef);
					shortHashGiver.Invoke(obj: null, parameters: new object[] { t, type });

					t.ResolveReferences();
					t.PostLoad();
					DefDatabase<ThingDef>.Add(t);
				}
				AddToThingCategory(Farbe, t);
			}
			Farbe.ResolveReferences();
			Farbe.PostLoad();
		}
		public static void PopulateCategoryFTerrain(ThingCategoryDef baseCategory)
		{
			foreach (ThingDef tui in baseCategory.childThingDefs)
			{
				string name = tui.defName.Replace(_Text.T_, "");
				ThingDef t = DefDatabase<ThingDef>.GetNamed(_Text.FromTerrain_ + name, false);
				if (t == null)
				{
					t = new ThingDef();
					t.defName = _Text.FromTerrain_ + name;
					t.label = tui.label;
					t.description = tui.description;
					t.graphicData = new GraphicData();
					t.graphicData.color = Color.white;

					MethodInfo shortHashGiver = typeof(ShortHashGiver).GetMethod(name: "GiveShortHash", bindingAttr: BindingFlags.NonPublic | BindingFlags.Static) ?? throw new ArgumentNullException();
					Type type = typeof(ThingDef);
					shortHashGiver.Invoke(obj: null, parameters: new object[] { t, type });

					t.ResolveReferences();
					t.PostLoad();
					DefDatabase<ThingDef>.Add(t);
				}
				AddToThingCategory(FromTerrain, t);
			}
			FromTerrain.ResolveReferences();
			FromTerrain.PostLoad();
		}
		public static void UpdateRootCategories()
		{
			ThingCategoryDef.Named(_ThingCategory.BuildingMisc).ResolveReferences();
			ThingCategoryDef.Named(_ThingCategory.BuildingMisc).PostLoad();
			Terraform.ResolveReferences();
			Terraform.PostLoad();
			ThingCategoryDefOf.Root.ResolveReferences();
			ThingCategoryDefOf.Root.PostLoad();
		}
	}
}
