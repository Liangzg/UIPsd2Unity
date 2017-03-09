using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PhotoshopFile;
using UnityEditor;
using UnityEngine;

namespace EditorTool.PsdExport
{
	/// <summary>
	/// Defines settings for PSDExporter to use
	/// </summary>
	public class PsdExportSettings
	{
		private const string TagImport1 = "ImportX1";
		private const string TagImport2 = "ImportX2";
		private const string TagImport4 = "ImportX4";
		private const string TagImportAnchor = "ImportAnchor";
		private const string TagImportPTU = "ImportPTU|";
		private const string TagImportPack = "ImportPackTag|";
		private const string TagExportPath = "ExportPath|";
		private const string TagExportAuto = "ExportAuto";

		/// <summary>
		/// Defines export settings for each layer
		/// </summary>
		public class LayerSetting
		{
			public int layerIndex;
			public bool doExport;
			public PSDExporter.ScaleDown scaleBy;
			public SpriteAlignment pivot;
		}

		/// <summary>
		/// The list of layer export settings
		/// </summary>
		public Dictionary<int, LayerSetting> layerSettings;

		/// <summary>
		/// The PsdFile type for the image this setting references
		/// </summary>
		public PsdFile Psd { get; protected set; }
		/// <summary>
		/// Filename of the PSD
		/// </summary>
		public string Filename { get; protected set; }
		/// <summary>
		/// Full filename of the PSD
		/// </summary>
		public string FullFileName { get; protected set; }
		/// <summary>
		/// Unity Texture2D reference to the PSD
		/// </summary>
		public Texture2D Image { get; protected set; }

		/// <summary>
		/// Pixels to Unit Size when exported to Unity sprites
		/// </summary>
		public float PixelsToUnitSize { get; set; }
		/// <summary>
		/// The packing tag to assign to the exported sprites
		/// </summary>
		public string PackingTag { get; set; }
		/// <summary>
		/// The scale of the PSD file relative to exported sprites
		/// </summary>
		public int ScaleBy { get; set; }
		/// <summary>
		/// The default pivot point for the Unity sprites
		/// </summary>
		public Vector2 PivotVector { get; set; }

		public string ExportPath { get; set; }

		public bool AutoReImport { get; set; }

		public bool HasMetaData { get; protected set; }

		private SpriteAlignment _pivot;
		public SpriteAlignment Pivot
		{
			get { return _pivot; }
			set
			{
				_pivot = value;
				if (_pivot == SpriteAlignment.Custom)
					return;

				Vector2 pivotCustom = new Vector2(0.5f, 0.5f);
				if (_pivot == SpriteAlignment.TopCenter ||
				    _pivot == SpriteAlignment.TopLeft ||
				    _pivot == SpriteAlignment.TopRight)
				{
					pivotCustom.y = 1;
				}
				if (_pivot == SpriteAlignment.BottomCenter ||
				    _pivot == SpriteAlignment.BottomLeft ||
				    _pivot == SpriteAlignment.BottomRight)
				{
					pivotCustom.y = 0f;
				}

				if (_pivot == SpriteAlignment.LeftCenter ||
				    _pivot == SpriteAlignment.TopLeft ||
				    _pivot == SpriteAlignment.BottomLeft)
				{
					pivotCustom.x = 0f;
				}
				if (_pivot == SpriteAlignment.RightCenter ||
				    _pivot == SpriteAlignment.TopRight ||
				    _pivot == SpriteAlignment.BottomRight)
				{
					pivotCustom.x = 1f;
				}
				PivotVector = pivotCustom;
			}
		}

		public PsdExportSettings(Texture2D image)
		{
			string path = AssetDatabase.GetAssetPath(image);
			if (!path.ToUpper().EndsWith(".PSD"))
				return;

			FullFileName = path;
			ExportPath = PsdSetting.Instance.DefaultImportPath;

			Psd = new PsdFile(path, Encoding.Default);
			Filename = Path.GetFileNameWithoutExtension(path);
			Image = image;

			ScaleBy = 0;
			Pivot = SpriteAlignment.Center;
			PixelsToUnitSize = 100f;

			LoadMetaData();
		}

		public PsdExportSettings(string path)
		{
			if (!path.ToUpper().EndsWith(".PSD"))
				return;

			FullFileName = path;
			ExportPath = PsdSetting.Instance.DefaultImportPath;

			Psd = new PsdFile(path, Encoding.Default);
			Filename = Path.GetFileNameWithoutExtension(path);

			ScaleBy = 0;
			Pivot = SpriteAlignment.Center;
			PixelsToUnitSize = 100f;
		}

		private void LoadMetaData()
		{
			AutoReImport = false;
			string[] pivotNameStrings = Enum.GetNames(typeof(SpriteAlignment));
			Array pivotNameVals = Enum.GetValues(typeof(SpriteAlignment));

			HasMetaData = false;
			string[] labels = AssetDatabase.GetLabels(Image);
			foreach (var label in labels)
			{
				if (label.Equals(TagImport1))
					ScaleBy = 0;
				if (label.Equals(TagImport2))
					ScaleBy = 1;
				if (label.Equals(TagImport4))
					ScaleBy = 2;

				if (label.StartsWith(TagImportAnchor))
				{
					string pivotType = label.Substring(12);
					if (pivotType.StartsWith("Custom"))
					{
						// Get the coordinates value inside the string "[]"
						//string values = pivotType.Substring(pivotType.IndexOf("["),
						//									pivotType.IndexOf("]"));
						//string[] vals = values.Split(',');
						//PivotVector = new Vector2(float.Parse(vals[0]), float.Parse(vals[1]));
						PivotVector = new Vector2(0.5f, 0.5f);
						Pivot = SpriteAlignment.Custom;
					}
					else
					{
						// Find by enum
						for (int i = 0; i < pivotNameStrings.Length; i++)
						{
							if (pivotType == pivotNameStrings[i])
								Pivot = (SpriteAlignment)pivotNameVals.GetValue(i);
						}
					}
				} // End import anchor if

				if (label.StartsWith(TagImportPTU))
				{
					string ptuVal = label.Substring(TagImportPTU.Length);
					PixelsToUnitSize = Single.Parse(ptuVal);
					HasMetaData = true;
				}

				if (label.StartsWith(TagImportPack))
				{
					string packTag = label.Substring(TagImportPack.Length);
					PackingTag = packTag;
				}

				if (label.StartsWith(TagExportPath))
				{
					string exportPath = label.Substring(TagExportPath.Length);
					ExportPath = exportPath;
				}

				if (label.StartsWith(TagExportAuto))
					AutoReImport = true;
			} // End label loop
		}

		public void SaveMetaData()
		{
			int tagCount = AutoReImport ? 6 : 5;
			string[] labels = new string[tagCount];

			if (ScaleBy == 0)
				labels[0] = TagImport1;
			if (ScaleBy == 1)
				labels[0] = TagImport2;
			if (ScaleBy == 2)
				labels[0] = TagImport4;

			labels[1] = TagImportAnchor + Pivot.ToString();
			if (Pivot == SpriteAlignment.Custom)
			{
				labels[1] = TagImportAnchor + "Custom[" + PivotVector.x + "," + PivotVector.y + "]";
			}

			labels[2] = TagImportPTU + PixelsToUnitSize;
			labels[3] = TagImportPack + PackingTag;
			labels[4] = TagExportPath + ExportPath;

			if (AutoReImport)
				labels[5] = TagExportAuto;

			AssetDatabase.SetLabels(Image, labels);
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Setup the layer settings using a PsdFileInfo,
		/// tries to read settings from file labels if available
		/// </summary>
		/// <param name="fileInfo"></param>
		public void LoadLayers(PsdFileInfo fileInfo)
		{
			layerSettings = new Dictionary<int, LayerSetting>();
			foreach (var layerIndex in fileInfo.LayerIndices)
			{
				string layerName = Psd.Layers[layerIndex].Name;
				bool visible = fileInfo.LayerVisibility[layerIndex];
				LoadLayerSetting(layerName, layerIndex, visible);
			}
		}

		private void LoadLayerSetting(string layerName, int layerIndex, bool visible)
		{
			LayerSetting setting = new LayerSetting
			{
				doExport = visible,
				layerIndex = layerIndex,
				pivot = Pivot,
				scaleBy = PSDExporter.ScaleDown.Default
			};

			string layerPath = GetLayerPath(layerName);
			Sprite layerSprite = (Sprite)AssetDatabase.LoadAssetAtPath(layerPath, typeof(Sprite));
			if (layerSprite != null)
			{
				// Layer import size is stored via tags
				string[] labels = AssetDatabase.GetLabels(layerSprite);
				foreach (var label in labels)
				{
					if (label.Equals(TagImport1))
						setting.scaleBy = PSDExporter.ScaleDown.Default;
					if (label.Equals(TagImport2))
						setting.scaleBy = PSDExporter.ScaleDown.Half;
					if (label.Equals(TagImport4))
						setting.scaleBy = PSDExporter.ScaleDown.Quarter;
				} // End label loop

				// Anchor is determined by import settings

				// Get the texture importer for the asset
				TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(layerPath);
				// Read out the texture import settings so import pivot point can be changed
				TextureImporterSettings importSetting = new TextureImporterSettings();
				textureImporter.ReadTextureSettings(importSetting);

				setting.pivot = (SpriteAlignment) importSetting.spriteAlignment;
			} // End layer settings loading

			layerSettings.Add(layerIndex, setting);
		}

		public void SaveLayerMetaData()
		{
			foreach (var keypair in layerSettings)
			{
				SaveLayerSetting(keypair.Value);
			}
		}

		private void SaveLayerSetting(LayerSetting setting)
		{
			// Not exporting, don't save layer settings
			if (!setting.doExport)
				return;

			// Get the asset
			var layer = Psd.Layers[setting.layerIndex];
			string layerPath = GetLayerPath(layer.Name);
			var layerSprite = AssetDatabase.LoadAssetAtPath(layerPath, typeof(Sprite));

			if (layerSprite == null)
				return;

			// Write out the labels for the layer settings
			// layer settings is just import size
			string[] labels = new string[1];

			if (setting.scaleBy == PSDExporter.ScaleDown.Default)
				labels[0] = TagImport1;
			if (setting.scaleBy == PSDExporter.ScaleDown.Half)
				labels[0] = TagImport2;
			if (setting.scaleBy == PSDExporter.ScaleDown.Quarter)
				labels[0] = TagImport4;

			// Write the label for the texture
			AssetDatabase.SetLabels(layerSprite, labels);

			// Set the alignment for the texture

			// Get the texture importer for the asset
			TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(layerPath);
			// Read out the texture import settings so import pivot point can be changed
			TextureImporterSettings importSetting = new TextureImporterSettings();
			textureImporter.ReadTextureSettings(importSetting);

			// Set the alignment
			importSetting.spriteAlignment = (int) setting.pivot;

			// And write settings into the importer
			textureImporter.SetTextureSettings(importSetting);

			EditorUtility.SetDirty(layerSprite);
			AssetDatabase.WriteImportSettingsIfDirty(layerPath);
			AssetDatabase.ImportAsset(layerPath, ImportAssetOptions.ForceUpdate);
		}

		public static string GetLayerPath(string layerName)
		{
//			string assetPath = FullFileName;
//			string directoryPath = Path.GetDirectoryName(assetPath);
//			
//			string layerFile = Path.GetFileNameWithoutExtension(assetPath);
//			layerFile = string.Format("{0}_{1}.png", layerFile, layerName);

//			string layerPath = Path.Combine(directoryPath, layerFile);

            string[] exportNameAndPath = ImageBinder.GetTextureExportPath(layerName);
            string basePath = string.Format("Assets/{0}", exportNameAndPath[1]);
			if (!Directory.Exists(basePath))
			{
				Directory.CreateDirectory(basePath);
			}
			string imgPath = string.Format("{0}.png", exportNameAndPath[0]);
            string layerPath = Path.Combine(basePath, imgPath);
			return layerPath;
		}
	}
}