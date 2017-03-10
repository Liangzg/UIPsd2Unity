/*
The MIT License (MIT)

Copyright (c) 2013 Banbury

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.IO;
using System.Reflection;
using PhotoshopFile;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using Object = UnityEngine.Object;

namespace EditorTool.PsdExport
{
	public class PSDEditorWindow : EditorWindow
	{
		private static string[] _sortingLayerNames;

		private PsdExportSettings settings;
		private PsdFileInfo fileInfo;

		private Texture2D image;
		private int importCount;

		private bool imageLoaded;

		private bool isDragging;

		private int extIndex = -1;
		private UiImgConstructor uiConstructor;
		private SpriteConstructor spriteConstructor;
		private IPsdConstructor[] constructorExtensions = null;
		private GUIContent[] constructorNames;
		private GUIContent extLabel = new GUIContent("Create with Extension");

		public Texture2D Image
		{
			get { return image; }
			set
			{
				image = value;
				LoadImage();
			}
		}

		#region Static/Menus
		private static PSDEditorWindow GetPSDEditor()
		{
			var wnd = GetWindow<PSDEditorWindow>();
			wnd.Setup();

			wnd.Show();

			return wnd;
		}
#if NGUI
        [MenuItem("NGUI/PSD Importer")]
#else
        [MenuItem("PSD/PSD Importer")]
#endif
        static void ImportPsdWindow()
		{
			var window = GetPSDEditor();

			if (PsdAssetSelected || true)
			{
//				window.Image = (Texture2D)Selection.objects[0];
				EditorUtility.SetDirty(window);
			}
			else
			{
				var psdPath = EditorUtility.OpenFilePanel("请选择PSD文件", PsdSetting.Instance.PsdPath, "psd");
				if (string.IsNullOrEmpty(psdPath))
				{
					return;
				}

				window.LoadPsdFile(psdPath);
				EditorUtility.SetDirty(window);
			}
		}

#if NGUI
	    [MenuItem("NGUI/Build Folder Atlas")]
	    private static void BuildFolderAltas()
	    {
	        UIAtlasFolderMaker.Instance.OnBuild();
	    }
#endif

        public static bool PsdAssetSelected
		{
			get
			{
				Object[] arr = Selection.objects;

				if (arr.Length != 1)
					return false;

				string assetPath = AssetDatabase.GetAssetPath(arr[0]);
				return assetPath.ToUpper().EndsWith(".PSD");
			}
		}
#endregion

		void OnEnable()
		{
			BuildConstructorExtensions();
			SetupSortingLayerNames();
			if (image != null)
				LoadImage();
		}

		void BuildConstructorExtensions()
		{
			uiConstructor = new UiImgConstructor();
			spriteConstructor = new SpriteConstructor();

			Type targetType = typeof(IPsdConstructor);
			Type sprType = typeof(SpriteConstructor);
			Type uiType = typeof(UiImgConstructor);
//		    Type textType = typeof (TextConstructor);
			Type[] constructorTypes = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(s => s.GetTypes())
				.Where(p => targetType.IsAssignableFrom(p) && p.IsClass &&
						!(p == sprType || p == uiType ))
				.ToArray();

			List<IPsdConstructor> constructors = new List<IPsdConstructor>();
			List<GUIContent> constructNames = new List<GUIContent>();

			foreach (Type t in constructorTypes)
			{
				IPsdConstructor constructor = Activator.CreateInstance(t) as IPsdConstructor;
				if (constructor == null)
					continue;

				constructors.Add(constructor);
				constructNames.Add(new GUIContent(constructor.MenuName));
			}

			constructorExtensions = constructors.ToArray();
			constructorNames = constructNames.ToArray();
		}

		void SetupSortingLayerNames()
		{
			if (_sortingLayerNames == null)
			{
				Type internalEditorUtilityType = typeof(InternalEditorUtility);
				PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
				_sortingLayerNames = (string[])sortingLayersProperty.GetValue(null, new object[0]);
			}
		}

		private bool LoadImage()
		{
			settings = new PsdExportSettings(image);
			showImportSettings = !settings.HasMetaData;

			bool valid = (settings.Psd != null);
			if (valid)
			{
				// Parse the layer info
				fileInfo = new PsdFileInfo(settings.Psd);
				settings.LoadLayers(fileInfo);
			}
			return valid;
		}

		private bool LoadPsdFile(string psdPath)
		{
			settings = new PsdExportSettings(psdPath);

			showImportSettings = !settings.HasMetaData;

			bool valid = (settings.Psd != null);
			if (valid)
			{
				// Parse the layer info
				fileInfo = new PsdFileInfo(settings.Psd);
				settings.LoadLayers(fileInfo);
			}
			return valid;
		}

#region GUI Styles
		private bool styleIsSetup = false;
		private GUIStyle styleHeader, styleLabelLeft, styleBoldFoldout, styleLayerSelected, styleLayerNormal;
		private Texture2D icnFolder, icnTexture , icnText;

		void SetupStyles()
		{
			if (styleIsSetup)
				return;
			
			icnFolder = EditorGUIUtility.FindTexture("Folder Icon");
			icnTexture = EditorGUIUtility.FindTexture("Texture Icon");
		    icnText = EditorGUIUtility.FindTexture("TextAsset Icon");

			styleHeader = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Bold
			};

			styleLabelLeft = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleLeft,
				padding = new RectOffset(0, 0, 0, 0)
			};

			styleBoldFoldout = new GUIStyle(EditorStyles.foldout)
			{
				fontStyle = FontStyle.Bold
			};

			styleLayerSelected = new GUIStyle(GUI.skin.box)
			{
				margin = new RectOffset(0, 0, 0, 0),
				padding = new RectOffset(0, 0, 0, 0),
				contentOffset = new Vector2(0, 0)
			};

			styleLayerNormal = new GUIStyle();

			styleIsSetup = true;
		}

	    private PSDLayerGroupInfo rootGroup;
		private Dictionary<PSDLayerGroupInfo, Rect> groupRects;
#endregion

		public void Setup()
		{
			titleContent = new GUIContent("PSD Importer")
			{
				image = EditorGUIUtility.FindTexture("Texture Icon")
			};
			EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyItem;
		}

		public void OnDestroy()
		{
			EditorApplication.hierarchyWindowItemOnGUI -= HandleHierarchyItem;
		}

		public void OnSelectionChange()
		{
			Repaint();
		}

		public void OnGUI()
		{
			SetupStyles();

			imageLoaded = (settings != null && settings.Psd != null);

			// Draw the layers, store where the layer groups are
			groupRects = DrawPsdLayers();

			DrawImportField();

			DrawImportSettings();

			DrawCreateEntry();

			ProcessMouse();
		}

#region Mouse/Drag Drop processing
		protected void ProcessMouse()
		{
			// No group layer rects to process, don't do anything
			if (groupRects == null)
				return;

			var evt = Event.current;

			// Automatically Reject non mouse button 1 events
			if (evt.button != 0 || !evt.isMouse)
				return;

			switch (evt.type)
			{
				case EventType.MouseDown:
					break;
				case EventType.MouseDrag:
					HandleBeginDrag(evt);
					break;
				case EventType.MouseUp:
					// Clear drag data, if any on mouseup
					DragAndDrop.PrepareStartDrag();
					isDragging = false;
					HandleGroupSelect(evt);
					break;
			}
		}

		private void HandleGroupSelect(Event evt)
		{
			if (scrollViewRect.Contains(evt.mousePosition) == false)
				return;

			bool didLayerClick = false;

			foreach (KeyValuePair<PSDLayerGroupInfo, Rect> keypair in groupRects)
			{
				var targetRect = keypair.Value;
				targetRect.x += scrollViewRect.x - scrollPos.x;
				targetRect.y += scrollViewRect.y - scrollPos.y;

				if (targetRect.Contains(evt.mousePosition))
				{
					selectedGroup = (selectedGroup == keypair.Key) ? null : keypair.Key;
					Repaint();
					didLayerClick = true;
				}
			}

			if (!didLayerClick)
			{
				selectedGroup = null;
				Repaint();
			}
		}

		private void HandleBeginDrag(Event evt)
		{
			if (scrollViewRect.Contains(evt.mousePosition) == false)
				return;

			foreach (KeyValuePair<PSDLayerGroupInfo, Rect> keypair in groupRects)
			{
				var targetRect = keypair.Value;
				targetRect.x += scrollViewRect.x - scrollPos.x;
				targetRect.y += scrollViewRect.y - scrollPos.y;

				if (targetRect.Contains(evt.mousePosition))
				{
					selectedGroup = keypair.Key;
					Repaint();

					DragAndDrop.PrepareStartDrag();

					DragAndDrop.paths = new string[0];
					DragAndDrop.objectReferences = new Object[0];

					DragAndDrop.StartDrag("Create PSD Group");
					evt.Use();

					isDragging = true;
					return;
				}
			}
		}

		private void HandleHierarchyItem(int instanceId, Rect selectionRect)
		{
			if (!isDragging || selectedGroup == null)
				return;
			
			if (selectionRect.Contains(Event.current.mousePosition))
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Link;

				if (Event.current.type == EventType.DragPerform)
				{
					var obj = EditorUtility.InstanceIDToObject(instanceId);
					GameObject targetObject = (GameObject) obj;

					if (targetObject == null)
						return;

					DragAndDrop.AcceptDrag();
					isDragging = false;

					var canvas = targetObject.GetComponentInParent<Canvas>();
					bool isUi = (canvas != null);

					if (isUi)
					{
						PsdBuilder.BuildUiImages(targetObject, selectedGroup, settings, fileInfo, createAlign);
					}
					else
					{
						PsdBuilder.BuildSprites(targetObject, selectedGroup, settings, fileInfo, createAlign);
					}
				}
			}
		}
#endregion

#region PSD Layer display functions
		private Vector2 scrollPos = Vector2.zero;
		private Rect scrollViewRect;

		private Dictionary<PSDLayerGroupInfo, Rect> DrawPsdLayers()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				EditorGUILayout.LabelField("Layers", styleHeader);
			}

			// Headers
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				float labelSize = (position.width - (colPivot + colSize));
				labelSize = Mathf.Max(labelSize, 145f);

				using (new EditorGUILayout.HorizontalScope(GUILayout.Width(labelSize)))
				{
					//GUILayout.FlexibleSpace();
					GUILayout.Label("Name");
					//GUILayout.FlexibleSpace();
				}

				using (new EditorGUILayout.HorizontalScope(GUILayout.Width(colSize)))
				{
					//GUILayout.FlexibleSpace();
					GUILayout.Label("Size");
					//GUILayout.FlexibleSpace();
				}

				using (new EditorGUILayout.HorizontalScope(GUILayout.Width(colPivot)))
				{
					//GUILayout.FlexibleSpace();
					GUILayout.Label("Pivot");
					//GUILayout.FlexibleSpace();
				}
			}

			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

			Dictionary<PSDLayerGroupInfo, Rect> groupRects = null;

			if (imageLoaded)
			{
				groupRects = DisplayLayers();
			}
			else
			{
				EditorGUILayout.HelpBox("No PSD file loaded", MessageType.Error, true);
			}

			EditorGUILayout.EndScrollView();
			scrollViewRect = GUILayoutUtility.GetLastRect();

			// Draw import button only when image is loaded
			if (imageLoaded)
			{
				string importBtnText = string.Format("Import {0} Layer(s)", importCount);
				if (GUILayout.Button(importBtnText, GUILayout.Height(25f)))
					ImportLayers();
			}

			// End layers box
			GUILayout.Box(GUIContent.none, GUILayout.Height(4f), GUILayout.ExpandWidth(true));

			if (!imageLoaded)
				return null;
			return groupRects;
		}

		private Dictionary<PSDLayerGroupInfo, Rect> DisplayLayers()
		{
			Dictionary<PSDLayerGroupInfo, Rect> groupRects = new Dictionary<PSDLayerGroupInfo, Rect>();

			int groupDepth = 0;
			int groupVisibleMask = 1;
			int groupOpenMask = 1;

			PsdFile psd = settings.Psd;
            // Loop backwards through the layers to display them in the expected order
            for (int i = psd.Layers.Count - 1; i >= 0; i--)
			{
				Layer layer = psd.Layers[i];

				//var instanceInfo = fileInfo.GetInstancedLayer(i);
				//if (instanceInfo != null && doDebug)
				//	Debug.LogFormat("Layer {0}, index {1}, is instance of {2}", layer.Name, i, instanceInfo.instanceLayer);

				// Layer set seems to appear in the photoshop layers
				// no idea what it does but doesn't seem to be relevant
				if (layer.Name.Equals("</Layer set>"))
					continue;

				// Try to get the group of this layer
				var groupInfo = fileInfo.GetGroupByLayerIndex(i);
				bool inGroup = groupInfo != null;

				bool startGroup = false;
				bool closeGroup = false;

				if (inGroup)
				{
					closeGroup = groupInfo.start == i;
					startGroup = groupInfo.end == i;
				}

				// If start of group, indent
				if (startGroup)
				{
					groupDepth++;

					// Save the mask info
					groupVisibleMask |= ((groupInfo.visible ? 1 : 0) << groupDepth);
					groupOpenMask |= ((groupInfo.opened ? 1 : 0) << groupDepth);
				}
				// If exiting a layer group, unindent and continue to next layer
				if (closeGroup)
				{
					// Reset mask info when closing group
					groupVisibleMask &= ~(1 << groupDepth);
					groupOpenMask &= ~(1 << groupDepth);

					groupDepth--;
					continue;
				}

				bool parentVisible = true;

				// If layer group content...
				if (inGroup)
				{
					bool parentOpen = true;
					parentVisible = true;

					for (int parentMask = groupDepth - 1; parentMask > 0; parentMask--)
					{
						bool isOpen = (groupOpenMask & (1 << parentMask)) > 0;
						bool isVisible = (groupVisibleMask & (1 << parentMask)) > 0;

						parentOpen &= isOpen;
						parentVisible &= isVisible;
					}

					bool skipLayer = !groupInfo.opened || !parentOpen;
					bool disableLayer = !groupInfo.visible || !parentVisible;

					if (startGroup)
					{
						skipLayer = !parentOpen;
						disableLayer = !parentVisible;
					}

					// Skip contents if group folder closed
					if (skipLayer)
					{
						SetHiddenLayer(i, groupInfo.visible && parentVisible);
						continue;
					}

					// If not visible, disable the row
					if (disableLayer)
						GUI.enabled = false;

					// Set parentVisible for settings override in DrawLayerEntry
					parentVisible = !disableLayer;
				}

				if (startGroup)
				{
					var rect = DrawLayerGroupStart(groupInfo, i, groupDepth - 1);
				    groupInfo.depth = groupDepth;
                    groupRects.Add(groupInfo, rect);
				}
				else
				{
					DrawLayerEntry(layer, i, groupDepth, parentVisible);
                }

				GUI.enabled = true;

			} // End layer loop

			importCount = PSDExporter.GetExportCount(settings, fileInfo);
			return groupRects;
		}

		private void SetHiddenLayer(int layerIndex, bool parentVisible)
		{
			if (settings.layerSettings.ContainsKey(layerIndex) == false)
			{
				settings.layerSettings.Add(layerIndex, new PsdExportSettings.LayerSetting()
				{
					doExport = true,
					layerIndex = layerIndex,
					pivot = settings.Pivot
				});
			}
			settings.layerSettings[layerIndex].doExport = fileInfo.LayerVisibility[layerIndex] && parentVisible;
		}

		private const float colSize = 60f;
		private const float colPivot = 95f;
		private const float colVisible = 15f;
		private const float indentSize = 20f;

		private bool DrawLayerEntry(Layer layer, int layerIndex, int indentLevel, bool parentVisible)
		{
			var drawStyle = styleLayerNormal;

			if (selectedGroup != null && selectedGroup.ContainsLayer(layerIndex))
				drawStyle = styleLayerSelected;

			EditorGUILayout.BeginHorizontal(drawStyle);

			// Draw layer visibility toggle
			bool visToggle = EditorGUILayout.Toggle(fileInfo.LayerVisibility[layerIndex],
													GUILayout.Width(colVisible));

			float indentAmount = indentLevel*indentSize;
			GUILayout.Space(indentAmount);

			float labelSize = (position.width - indentAmount - (colPivot + colSize) - 50f);
			labelSize = Mathf.Max(labelSize, 125f);

			// Draw the layer name
			GUIContent layerDisplay = new GUIContent()
			{
				image = layer.IsText ? icnText : icnTexture,
				text = layer.Name
			};
			EditorGUILayout.LabelField(layerDisplay, styleLabelLeft,
											GUILayout.Width(labelSize),
											GUILayout.ExpandWidth(true));

			fileInfo.LayerVisibility[layerIndex] = visToggle;

            // If layer visible, show layer export settings
            if (settings.layerSettings.ContainsKey(layerIndex)) {
			    var layerSetting = settings.layerSettings[layerIndex];
			    layerSetting.doExport = visToggle && parentVisible;
			    if (layerSetting.doExport)
			    {
				    layerSetting.scaleBy = (PSDExporter.ScaleDown)EditorGUILayout.EnumPopup(layerSetting.scaleBy,
																			    GUILayout.Width(colSize));
				    layerSetting.pivot = (SpriteAlignment)EditorGUILayout.EnumPopup(layerSetting.pivot,
																			    GUILayout.Width(colPivot));
				    settings.layerSettings[layerIndex] = layerSetting;
			    }
            }

			EditorGUILayout.EndHorizontal();

			return visToggle;
		}

		private PSDLayerGroupInfo selectedGroup;

		private Rect DrawLayerGroupStart(PSDLayerGroupInfo groupInfo,
										int layerIndex, int indentLevel)
		{
			GUIStyle style = styleLayerNormal;
			if (selectedGroup != null && selectedGroup.ContainsLayer(layerIndex))
				style = styleLayerSelected;

			EditorGUILayout.BeginHorizontal(style);

			// Draw group visibility toggle
			bool visToggle = EditorGUILayout.Toggle(groupInfo.visible, GUILayout.Width(colVisible));

			float indentAmount = indentLevel * indentSize;
			GUILayout.Space(indentAmount);

			// Draw the layer group name
			GUIContent groupDisplay = new GUIContent()
			{
				image = icnFolder,
				text = groupInfo.name
			};
			groupInfo.opened = EditorGUILayout.Foldout(groupInfo.opened, GUIContent.none);

			var foldoutRect = GUILayoutUtility.GetLastRect();
			foldoutRect.xMin += 13f;
			foldoutRect.width = 250f;
			EditorGUI.LabelField(foldoutRect, groupDisplay);

			GUILayout.FlexibleSpace();

			// Save the data into group info and file info
			groupInfo.visible = visToggle;
			fileInfo.LayerVisibility[layerIndex] = visToggle;

			EditorGUILayout.EndHorizontal();
			Rect groupRect = GUILayoutUtility.GetLastRect();
			return groupRect;
		}
#endregion

#region Import UI
		private void DrawImportField()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				string psdString = "PSD File";
				if (imageLoaded)
					psdString = settings.Filename;

				EditorGUI.BeginChangeCheck();
				var img = (Texture2D)EditorGUILayout.ObjectField(psdString, image,
															typeof(Texture2D), false,
															GUILayout.ExpandWidth(true));
				bool changed = EditorGUI.EndChangeCheck();
				if (changed)
					Image = img;
			}
		}

		private bool showImportSettings;

		private void DrawImportSettings()
		{
			showImportSettings = EditorGUILayout.Foldout(showImportSettings, "Import Settings", styleBoldFoldout);
			if (!showImportSettings || !imageLoaded)
				return;

			EditorGUI.BeginChangeCheck();

			GUILayout.Label("Source Image Scale");
			settings.ScaleBy = GUILayout.Toolbar(settings.ScaleBy, new string[] { "1X", "2X", "4X" });

			settings.AutoReImport = EditorGUILayout.Toggle("Auto Re-Import", settings.AutoReImport);

			settings.PixelsToUnitSize = EditorGUILayout.FloatField("Pixels To Unit Size", settings.PixelsToUnitSize);
			if (settings.PixelsToUnitSize <= 0)
			{
				EditorGUILayout.HelpBox("Pixels To Unit Size should be greater than 0.", MessageType.Warning);
			}

			settings.PackingTag = EditorGUILayout.TextField("Packing Tag", settings.PackingTag);

			// Default pivot
			var newPivot = (SpriteAlignment)EditorGUILayout.EnumPopup("Default Pivot", settings.Pivot);
			// When pivot changed, change the other layer settings as well
			if (newPivot != settings.Pivot)
			{
				List<int> changeLayers = new List<int>();
				foreach (var layerKeyPair in settings.layerSettings)
				{
					if (layerKeyPair.Value.pivot == settings.Pivot)
					{
						changeLayers.Add(layerKeyPair.Value.layerIndex);
					}
				}
				foreach (int changeLayer in changeLayers)
				{
					settings.layerSettings[changeLayer].pivot = newPivot;
				}
				settings.Pivot = newPivot;
			}

			if (settings.Pivot == SpriteAlignment.Custom)
			{
				settings.PivotVector = EditorGUILayout.Vector2Field("Custom Pivot", settings.PivotVector);
			}

			// Path picker
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Import Path", GUILayout.Width(EditorGUIUtility.labelWidth)))
				PickExportPath();
			GUI.enabled = false;
			EditorGUILayout.TextField(GUIContent.none, settings.ExportPath);
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
			}

			if (GUILayout.Button("Save Import Settings"))
				settings.SaveMetaData();
		}

		private void PickExportPath()
		{
			string path = EditorUtility.SaveFolderPanel("Export Path", 
				Path.Combine(Application.dataPath, settings.ExportPath), "");

			if (string.IsNullOrEmpty(path))
			{
				settings.ExportPath = PsdSetting.Instance.DefaultImportPath;
			}
			else
			{
				int inPath = path.IndexOf(Application.dataPath);
				if (inPath < 0 || Application.dataPath.Length == path.Length)
					path = "";
				else
					path = path.Substring(Application.dataPath.Length + 1);

				settings.ExportPath = path;
			}
		}

		private void ImportLayers()
		{
			PSDExporter.Export(settings, fileInfo);

#if NGUI
            //打开NGUI图集
            UIAtlasFolderMaker.Instance.OnBuild();
#endif
        }
#endregion

#region Sprite creation
		private bool showCreateSprites = true;
		private SpriteAlignment createAlign = SpriteAlignment.Center;

		private void DrawCreateEntry()
		{
			showCreateSprites = EditorGUILayout.Foldout(showCreateSprites, "Sprite Creation", styleBoldFoldout);

			if (!showCreateSprites || !imageLoaded)
				return;

			float labelWidth = EditorGUIUtility.labelWidth;
			using (new EditorGUILayout.HorizontalScope(GUILayout.Height(30f)))
			{
				MessageType boxType = MessageType.Warning;
				string boxMsg = "Select a Layer Group";

				if (selectedGroup != null)
				{
					boxMsg = string.Format("Creating: {0}", selectedGroup.name);
					boxType = MessageType.Info;
				}

				EditorGUILayout.HelpBox(boxMsg, boxType);

				using (new EditorGUILayout.VerticalScope(GUILayout.Width(165f)))
				{
					GUILayout.FlexibleSpace();
					EditorGUIUtility.labelWidth = 65f;
					createAlign = (SpriteAlignment)EditorGUILayout.EnumPopup("Alignment", createAlign);
					GUILayout.FlexibleSpace();	
				}
			}

			GUILayout.Space(5f);

			bool createSprites = false;
			bool createUiImgs = false;
            bool createAllUI = false;

//			EditorGUIUtility.labelWidth = labelWidth;
//			using (new EditorGUILayout.HorizontalScope())
//			{
//				if (selectedGroup == null)
//					GUI.enabled = false;
//
//				const float buttonHeight = 50f;
//
//				using (new EditorGUILayout.VerticalScope())
//				{
//					GUILayout.Label("2D Sprites", styleHeader);
//					createSprites = GUILayout.Button("Create 2D Sprites",
//													GUILayout.Height(buttonHeight),
//													GUILayout.ExpandHeight(false));
//				}
//
//				using (new EditorGUILayout.VerticalScope())
//				{
//					bool selectionOk = Selection.activeGameObject != null;
//					string btnText = "Select UI Root";
//					if (selectionOk)
//					{
//						selectionOk = uiConstructor.CanBuild(Selection.activeGameObject);
//						if (selectionOk)
//							btnText = "Create UI Images";
//					}
//
//					GUI.enabled = selectionOk && selectedGroup != null;
//
//					GUILayout.Label("UI Images", styleHeader);
//					createUiImgs = GUILayout.Button(btnText,
//													GUILayout.Height(buttonHeight),
//													GUILayout.ExpandHeight(false));
//				}
//				GUI.enabled = true;
//			}


            using (new EditorGUILayout.VerticalScope())
            {
                bool selectionOk = Selection.activeGameObject != null;
                string btnText = "Select Build Root";
                if (selectionOk)
                {
                    selectionOk = uiConstructor.CanBuild(Selection.activeGameObject);
                    if (selectionOk)
                        btnText = "Create UI Panel";
                }

                createAllUI = GUILayout.Button(btnText,
                                                GUILayout.Height(30),
                                                GUILayout.ExpandHeight(false));

                GUILayout.Space(5);
            }

            if (createSprites)
				PsdBuilder.BuildPsd(Selection.activeGameObject, selectedGroup, settings, fileInfo, createAlign, spriteConstructor);
			if (createUiImgs)
				PsdBuilder.BuildPsd(Selection.activeGameObject, selectedGroup, settings, fileInfo, createAlign, uiConstructor);
		    if (createAllUI)
		    {
		        GameObject root = PsdBuilder.BuildPsdRoot(Selection.activeGameObject, settings, fileInfo, createAlign, uiConstructor);
		        Selection.activeGameObject = root;
		    }
                

//            DrawCreateExtensions();
		}

        

		private void DrawCreateExtensions()
		{
			if (constructorExtensions.Length == 0)
				return;

			GUI.enabled = selectedGroup != null;

			bool doCreate;

			using (new EditorGUILayout.HorizontalScope(GUILayout.Height(30f)))
			{
				extIndex = EditorGUILayout.Popup(extLabel, extIndex, constructorNames);

				bool canBuild = false;
				GUIContent createBtn = GUIContent.none;
				if (extIndex > -1 && extIndex < constructorExtensions.Length)
				{
					createBtn = constructorNames[extIndex];
					canBuild = constructorExtensions[extIndex].CanBuild(Selection.activeGameObject);
				}

				GUI.enabled = canBuild && selectedGroup != null;
				doCreate = GUILayout.Button(createBtn, GUILayout.ExpandHeight(true));
			}

			if (doCreate)
			{
				IPsdConstructor constructor = constructorExtensions[extIndex];
				PsdBuilder.BuildPsd(Selection.activeGameObject, selectedGroup, settings, fileInfo, createAlign, constructor);
			}

			GUI.enabled = true;
		}
#endregion
	}
}