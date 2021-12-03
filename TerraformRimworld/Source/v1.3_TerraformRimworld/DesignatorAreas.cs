using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TerraformRimworld
{
	#region designator Area Add Overhanging Mountains

	public class Designator_AreaOvermountain : Designator_Area
	{
		public static List<IntVec3> noThingCells = new List<IntVec3>();

		public Designator_AreaOvermountain()
		{
			this.defaultLabel = TRMod.isGerman ? "Überhängendes Gebierge" : "Overhanging mountains";
			this.defaultDesc = "";
			this.icon = ContentFinder<Texture2D>.Get("addMountainRoof", true);
			this.soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			this.soundDragChanged = null;
			this.soundSucceeded = SoundDefOf.Designate_PlanAdd;
			this.useMouseIcon = true;
		}

		public override bool DragDrawMeasurements
		{
			get
			{
				return true;
			}
		}

		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}
		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}

			return true;
		}

		public override bool CanRemainSelected()
		{
			Find.PlaySettings.showRoofOverlay = false;
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (TRMod.OPTION_InstantConstruction)
			{
				base.Map.roofGrid.SetRoof(c, RoofDefOf.RoofRockThick);
			}
			else
			{
				AcceptanceReport ar = new AcceptanceReport("This function only works if 'Instant Construction' is enabled.");
				Messages.Message(ar.Reason, MessageTypeDefOf.RejectInput, false);
			}
		}

		public override void SelectedUpdate()
		{
			Find.PlaySettings.showRoofOverlay = true;
			GenUI.RenderMouseoverBracket();
			base.Map.areaManager.NoRoof.MarkForDraw();
			base.Map.areaManager.BuildRoof.MarkForDraw();
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
		}
	}

	#endregion designator Area Add Overhanging Mountains

	#region designator Area destroy

	public class Designator_AreaNoThings : Designator_Area
	{
		public static List<IntVec3> noThingCells = new List<IntVec3>();

		public Designator_AreaNoThings()
		{
			this.defaultLabel = TRMod.isGerman ? "Zerstöre" : "Destroy";
			this.defaultDesc = "";
			this.icon = ContentFinder<Texture2D>.Get("destroy", true);
			this.soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			this.soundDragChanged = null;
			this.soundSucceeded = SoundDefOf.Designate_PlanAdd;
			this.useMouseIcon = true;
		}

		public override bool DragDrawMeasurements
		{
			get
			{
				return true;
			}
		}

		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}
		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}

			return true;
		}

		public override bool CanRemainSelected()
		{
			Find.PlaySettings.showRoofOverlay = false;
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			if (TRMod.OPTION_InstantConstruction)
			{
				base.Map.roofGrid.SetRoof(c, null);
				Helper.ClearCell(c, base.Map);

				Designator_Cancel cancel = new Designator_Cancel();
				cancel.DesignateSingleCell(c);
			}
			else
			{
				AcceptanceReport ar = new AcceptanceReport("This function only works if 'Instant Construction' is enabled.");
				Messages.Message(ar.Reason, MessageTypeDefOf.RejectInput, false);
			}
		}

		public override void SelectedUpdate()
		{
			Find.PlaySettings.showRoofOverlay = true;
			GenUI.RenderMouseoverBracket();
			base.Map.areaManager.NoRoof.MarkForDraw();
			base.Map.areaManager.BuildRoof.MarkForDraw();
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
		}
	}

	#endregion designator Area destroy

	#region designator Area Remove Overhanging Mountains

	public class Designator_AreaNoRockyRoof : Designator_Area
	{
		private static List<IntVec3> justAddedCells = new List<IntVec3>();

		public Designator_AreaNoRockyRoof()
		{
			this.defaultLabel = TRMod.isGerman ? "Überhängendes Gebierge Entfernen" : "Remove overhanging mountains";
			this.defaultDesc = "";
			this.icon = ContentFinder<Texture2D>.Get("removeRockyRoof", true);
			this.hotKey = KeyBindingDefOf.Misc5;
			this.soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
			this.soundDragChanged = null;
			this.soundSucceeded = SoundDefOf.Designate_PlanAdd;
			this.useMouseIcon = true;
		}

		public override bool DragDrawMeasurements
		{
			get
			{
				return true;
			}
		}

		public override int DraggableDimensions
		{
			get
			{
				return 2;
			}
		}
		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(base.Map))
			{
				return false;
			}
			if (c.Fogged(base.Map))
			{
				return false;
			}
			RoofDef roofDef = base.Map.roofGrid.RoofAt(c);
			bool flag = base.Map.areaManager.NoRoof[c];
			return !flag;
		}

		public override bool CanRemainSelected()
		{
			Find.PlaySettings.showRoofOverlay = false;
			return true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			base.Map.areaManager.NoRoof[c] = true;
			Designator_AreaNoRockyRoof.justAddedCells.Add(c);
		}

		public override void SelectedUpdate()
		{
			Find.PlaySettings.showRoofOverlay = true;
			GenUI.RenderMouseoverBracket();
			base.Map.areaManager.NoRoof.MarkForDraw();
			base.Map.areaManager.BuildRoof.MarkForDraw();
		}

		protected override void FinalizeDesignationSucceeded()
		{
			base.FinalizeDesignationSucceeded();
			for (int i = 0; i < Designator_AreaNoRockyRoof.justAddedCells.Count; i++)
			{
				base.Map.areaManager.BuildRoof[Designator_AreaNoRockyRoof.justAddedCells[i]] = false;
			}
			Designator_AreaNoRockyRoof.justAddedCells.Clear();
		}
	}

	public class JobDriver_RemoveThickRoof : JobDriver_AffectRoof
	{
		private static List<IntVec3> removedRoofs = new List<IntVec3>();

		protected override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.ClosestTouch;
			}
		}

		protected override void DoEffect()
		{
			JobDriver_RemoveThickRoof.removedRoofs.Clear();
			base.Map.roofGrid.SetRoof(base.Cell, null);
			JobDriver_RemoveThickRoof.removedRoofs.Add(base.Cell);
			RoofCollapseCellsFinder.CheckCollapseFlyingRoofs(JobDriver_RemoveThickRoof.removedRoofs, base.Map, true, false);
			JobDriver_RemoveThickRoof.removedRoofs.Clear();
		}

		protected override bool DoWorkFailOn()
		{
			return !base.Cell.Roofed(base.Map);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !this.Map.areaManager.NoRoof[this.Cell]);
			foreach (Toil t in base.MakeNewToils())
			{
				yield return t;
			}
		}
	}

	public class WorkGiver_RemoveThickRoof : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.ClosestTouch;
			}
		}

		public override bool Prioritized
		{
			get
			{
				return true;
			}
		}
		public override float GetPriority(Pawn pawn, TargetInfo t)
		{
			IntVec3 cell = t.Cell;
			int num = 0;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = cell + GenAdj.AdjacentCells[i];
				if (c.InBounds(t.Map))
				{
					Building edifice = c.GetEdifice(t.Map);
					if (edifice != null && edifice.def.holdsRoof)
					{
						return -60f;
					}
					if (c.Roofed(pawn.Map))
					{
						num++;
					}
				}
			}
			return (float)(-(float)Mathf.Min(num, 3));
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			if (!pawn.Map.areaManager.NoRoof[c])
			{
				return false;
			}
			if (!c.Roofed(pawn.Map))
			{
				return false;
			}
			if (c.IsForbidden(pawn))
			{
				return false;
			}
			LocalTargetInfo target = c;
			ReservationLayerDef ceiling = ReservationLayerDefOf.Ceiling;
			return pawn.CanReserve(target, 1, -1, ceiling, forced);
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			return new Job(JobDefOf.RemoveThickRoof, c, c);
		}

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			return pawn.Map.areaManager.NoRoof.ActiveCells;
		}
	}
	#endregion designator Area Remove Overhanging Mountains
}
