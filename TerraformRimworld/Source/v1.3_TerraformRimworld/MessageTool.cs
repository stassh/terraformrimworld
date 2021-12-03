using RimWorld;
using Verse;

namespace TerraformRimworld
{
	public static class MessageTool
	{
		public static void Show(string info, MessageTypeDef mt = null)
		{
			Messages.Message(info, mt ?? MessageTypeDefOf.SilentInput, false);
		}

		public static void ShowInDebug(string info)
		{
			if (Prefs.DevMode)
				Show(info);
		}
		public static void ShowDialog(string s, bool doRestart = false)
		{
			if (doRestart)
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(s, delegate
				{
					GameDataSaveLoader.SaveGame("autosave_last");
					GenCommandLine.Restart();
				}, false, null));
			}
			else
			{
				Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(s, null, false, null));
			}
		}
	}
}
