using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace subjectnerdagreement.psdexport
{
	public class PSDPostProcessor : AssetPostprocessor
	{
		static List<Texture2D> reimports = new List<Texture2D>();

		static void OnPostprocessAllAssets(string[] modified,
											string[] deleted,
											string[] moved,
											string[] movedFrom)
		{
			reimports.Clear();

			foreach (var modPath in modified)
			{
				if (modPath.ToLower().EndsWith(".psd") == false)
					continue;
				var target = AssetDatabase.LoadAssetAtPath<Texture2D>(modPath);
				reimports.Add(target);
			}

			// This is kind of a hacky way of doing this,
			// If there are PSDs that want to be reimported
			// We have to do it outside of this function because
			// AssetDatabase cannot handle new images created
			// from within this function
			if (reimports.Count > 0)
			{
				// So instead, hook into the editor update function
				// and fire the import from there
				EditorApplication.update += ExecuteAutoImport;
			}
		}

		private static void ExecuteAutoImport()
		{
			// This function only fires once, unhook from update function immediately
			EditorApplication.update -= ExecuteAutoImport;

			for (int i = 0; i < reimports.Count; i++)
			{
				var target = reimports[i];
				var exportSettings = new PsdExportSettings(target);
				if (exportSettings.AutoReImport)
				{
					PsdFileInfo psdInfo = new PsdFileInfo(exportSettings.Psd);
					exportSettings.LoadLayers(psdInfo);
					PSDExporter.Export(exportSettings, psdInfo);
				}
			}

			reimports.Clear();
		}
	}
}
