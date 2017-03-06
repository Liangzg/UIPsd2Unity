using System;
using System.Collections.Generic;
using PhotoshopFile;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace subjectnerdagreement.psdexport
{
	public class PsdBuilder
	{
		#region convenience functions
		public static void BuildUiImages(GameObject root, PSDLayerGroupInfo group,
									PsdExportSettings settings, PsdFileInfo fileInfo,
									SpriteAlignment createAlign)
		{
			BuildPsd(root, group, settings, fileInfo,
					createAlign, new UiImgConstructor());
		}

		public static void BuildSprites(GameObject root, PSDLayerGroupInfo group,
										PsdExportSettings settings, PsdFileInfo fileInfo,
										SpriteAlignment createAlign)
		{
			BuildPsd(root, group, settings, fileInfo,
					createAlign, new SpriteConstructor());
		}
		#endregion

		#region General handler
		public static void BuildPsd(GameObject root, PSDLayerGroupInfo group,
									PsdExportSettings settings, PsdFileInfo fileInfo,
									SpriteAlignment align, IPsdConstructor initConstructor)
		{
			// Run the export on non exported layers
//			PSDExporter.Export(settings, fileInfo, false);

			// Find all the layers being exported
			var exportLayers = PSDExporter.GetExportLayers(settings, fileInfo);

			// Stores the root object for each encountered group
			Dictionary<PSDLayerGroupInfo, GameObject> groupHeaders = new Dictionary<PSDLayerGroupInfo, GameObject>();

			// Store the last parent, for traversal
			GameObject lastParent = root;
			GameObject rootBase = root;

			int groupVisibleMask = 1;
			int groupDepth = 0;

			// Loop through all the layers of the PSD file
			// backwards so they appear in the expected order
			// Going through all the layers, and not just the exported layers
			// so that the groups can be setup
			for (int i = group.end; i >= group.start; i--)
			{
			    IPsdConstructor constructor = initConstructor;

                // Skip if layer is hidden
                if (fileInfo.LayerVisibility[i] == false)
					continue;

				var groupInfo = fileInfo.GetGroupByLayerIndex(i);
				bool inGroup = groupInfo != null;

				// Skip if layer belongs to a hidden group
				if (inGroup && groupInfo.visible == false)
					continue;
 

                // When inside a group...
                if (inGroup)
				{
					// Inverted because starting backwards
					bool startGroup = groupInfo.end == i;
					bool closeGroup = groupInfo.start == i;

					// Go up or down group depths
					if (startGroup)
					{
						groupDepth++;
						groupVisibleMask |= ((groupInfo.visible ? 1 : 0) << groupDepth);
					}
					if (closeGroup)
					{
						// Reset group visible flag when closing group
						groupVisibleMask &= ~(1 << groupDepth);
						groupDepth--;
					}

					// First, check if parents of this group is visible in the first place
					bool parentVisible = true;
					for (int parentMask = groupDepth - 1; parentMask > 0; parentMask--)
					{
						bool isVisible = (groupVisibleMask & (1 << parentMask)) > 0;
						parentVisible &= isVisible;
					}
					// Parents not visible, continue to next layer
					if (!parentVisible)
						continue;

					// Finally, check if layer being processed is start/end of group
					if (startGroup || closeGroup)
					{
                        // If start or end of the group, call HandleGroupObject
                        // which creates the group layer object and assignment of lastParent
                        HandleGroupObject(groupInfo, groupHeaders,
										startGroup, constructor, ref lastParent);

						// A bunch of book keeping needs to be done at the start of a group
						if (startGroup)
						{
							// If this is the start of the group being constructed
							// store as the rootBase
							if (i == group.end)
							{
								rootBase = lastParent;
							}
						}

						// Start or end group doesn't have visible sprite object, skip to next layer
						continue;
					}
				} // End processing of group start/end

				// If got to here, processing a visual layer

				// Skip if the export layers list doesn't contain this index
				if (exportLayers.Contains(i) == false)
					continue;

				// If got here and root base hasn't been set, that's a problem
				if (rootBase == null)
				{
					throw new Exception("Trying to create image layer before root base has been set");
				}

                // Get layer info
                Layer layer = settings.Psd.Layers[i];
                if (layer.IsText)
                {
                    constructor = new TextConstructor();
                }
                constructor.layer = layer;

                // Create the game object for the sprite
                string layerName = Utility.PinYinHelper.Convert(layer.Name);
				GameObject spriteObject = constructor.CreateGameObject(layerName, lastParent);
				// Reparent created object to last parent
				if (lastParent != null)
					spriteObject.transform.SetParent(lastParent.transform, false);

				Vector2 spritePivot = GetPivot(SpriteAlignment.Center);
				if (layer.IsText)
				{
                    constructor.AddComponents(i , spriteObject , null , null);
				}
				else
				{
#if NGUI
                    // Add components to the sprite object for the visuals
                    constructor.AddComponents(i, spriteObject, null, null);
#else
                    // Retrieve sprite from asset database
                    string sprPath = PSDExporter.GetLayerFilename(settings, i);

                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(sprPath);

					// Get the pivot settings for the sprite
					TextureImporter sprImporter = (TextureImporter)AssetImporter.GetAtPath(sprPath);
					TextureImporterSettings sprSettings = new TextureImporterSettings();
					sprImporter.ReadTextureSettings(sprSettings);
					sprImporter = null;

					// Add components to the sprite object for the visuals
					constructor.AddComponents(i, spriteObject, sprite, sprSettings);

					// Reposition the sprite object according to PSD position
					spritePivot = GetPivot(sprSettings);
#endif
                }
				
				Vector3 layerPos = constructor.GetLayerPosition(layer.Rect, spritePivot, settings.PixelsToUnitSize);
				// reverse y axis
				layerPos.y = fileInfo.height - layerPos.y;
                layerPos -= new Vector3(fileInfo.width , fileInfo.height) * 0.5f;

				// Scaling factor, if sprites were scaled down
				float posScale = 1f;
				switch (settings.ScaleBy)
				{
					case 1:
						posScale = 0.5f;
						break;
					case 2:
						posScale = 0.25f;
						break;
				}
				layerPos *= posScale;

				// Sprite position is based on root object position initially
				Transform spriteT = spriteObject.transform;
#if NGUI
                spriteT.localPosition = layerPos;
#else
                RectTransform rectTrans = spriteObject.GetComponent<RectTransform>();
			    rectTrans.anchoredPosition3D = layerPos;
#endif
			} // End layer loop

		    parseComponent(root);
		} // End BuildPsd()

	    private static void parseComponent(GameObject root)
	    {
	        string layerName = root.name;
	        List<Word> parseWords = WordParser.BindingParse(root , layerName);

	        int childCount = root.transform.childCount;
	        for (int i = childCount - 1; i >= 0; i--)
	        {
	            Transform childTrans = root.transform.GetChild(i);
                parseComponent(childTrans.gameObject);
            }

            WordParser.exitBindingParse(parseWords , root , layerName);
        }

	    public static GameObject BuildPsdRoot(GameObject root, PsdExportSettings settings, PsdFileInfo fileInfo,
	        SpriteAlignment align, IPsdConstructor constructor)
	    {
            GameObject spriteObject = constructor.CreateGameObject(settings.Filename, root);
	        spriteObject.name += "[name:_root_]";
            LayerWordBinder.registWord();

            PSDLayerGroupInfo rootGroup = new PSDLayerGroupInfo(settings.Filename, settings.Psd.Layers.Count - 1, true, true);
            rootGroup.start = 0;
            
            BuildPsd(spriteObject, rootGroup, settings, fileInfo, align, constructor);

            return spriteObject;
        }

        private static void HandleGroupObject(PSDLayerGroupInfo groupInfo,
									Dictionary<PSDLayerGroupInfo, GameObject> groupHeaders,
									bool startGroup, IPsdConstructor constructor,
									ref GameObject lastParent)
		{
			if (startGroup)
			{
				string groupName = Utility.PinYinHelper.Convert(groupInfo.name);
				GameObject groupRoot = constructor.CreateGameObject(groupName, lastParent);

				constructor.HandleGroupOpen(groupRoot);

                lastParent = groupRoot;
				groupHeaders.Add(groupInfo, groupRoot);
				return;
			}

			// If not startGroup, closing group
			var header = groupHeaders[groupInfo].transform;
			if (header.parent != null)
			{
				constructor.HandleGroupClose(lastParent);

				lastParent = groupHeaders[groupInfo].transform.parent.gameObject;
			}
			else
			{
				lastParent = null;
			}
		}
#endregion

#region Public APIs

		public static Vector3 CalculateLayerPosition(Rect layerSize, Vector2 layerPivot)
		{
			Vector3 layerPos = Vector3.zero;
			layerPos.x = ((layerSize.width * layerPivot.x) + layerSize.x);
			layerPos.y = ((layerSize.height * layerPivot.y) + layerSize.y);
			return layerPos;
		}


        public static Vector3 CalculateGroupPosition(GameObject groupRoot, SpriteAlignment alignment)
        {

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

#if NGUI
            var tList = groupRoot.GetComponentsInChildren<UIWidget>();
            foreach (var rectTransform in tList)
            {
                if (rectTransform.gameObject == groupRoot)
                    continue;

                var rectSize = new Vector2(rectTransform.width, rectTransform.height);
                var rectPivot = NGUIMath.GetPivotOffset(rectTransform.pivot);

                var calcMin = rectTransform.cachedTransform.position;
                calcMin.x -= rectSize.x * rectPivot.x;
                calcMin.y -= rectSize.y * rectPivot.y;

                var calcMax = calcMin + new Vector3(rectSize.x, rectSize.y);

                min = Vector2.Min(min, calcMin);
                max = Vector2.Max(max, calcMax);
            }
#else
            var tList = groupRoot.GetComponentsInChildren<RectTransform>();
            foreach (var rectTransform in tList)
            {
                if (rectTransform.gameObject == groupRoot)
                    continue;

                var rectSize = rectTransform.sizeDelta;
                var rectPivot = rectTransform.pivot;

                var calcMin = rectTransform.position;
                calcMin.x -= rectSize.x * rectPivot.x;
                calcMin.y -= rectSize.y * rectPivot.y;

                var calcMax = calcMin + new Vector3(rectSize.x, rectSize.y);

                min = Vector2.Min(min, calcMin);
                max = Vector2.Max(max, calcMax);
            }
#endif

            Vector2 pivot = PsdBuilder.GetPivot(alignment);
            Vector3 pos = Vector3.zero;
            pos.x = Mathf.Lerp(min.x, max.x, pivot.x);
            pos.y = Mathf.Lerp(min.y, max.y, pivot.y);
            return pos;
        }
        public static Vector2 GetPivot(SpriteAlignment spriteAlignment)
		{
			Vector2 pivot = new Vector2(0.5f, 0.5f);
			if (spriteAlignment == SpriteAlignment.TopLeft ||
				spriteAlignment == SpriteAlignment.LeftCenter ||
				spriteAlignment == SpriteAlignment.BottomLeft)
			{
				pivot.x = 0f;
			}
			if (spriteAlignment == SpriteAlignment.TopRight ||
				spriteAlignment == SpriteAlignment.RightCenter ||
				spriteAlignment == SpriteAlignment.BottomRight)
			{
				pivot.x = 1;
			}
			if (spriteAlignment == SpriteAlignment.TopLeft ||
				spriteAlignment == SpriteAlignment.TopCenter ||
				spriteAlignment == SpriteAlignment.TopRight)
			{
				pivot.y = 1;
			}
			if (spriteAlignment == SpriteAlignment.BottomLeft ||
				spriteAlignment == SpriteAlignment.BottomCenter ||
				spriteAlignment == SpriteAlignment.BottomRight)
			{
				pivot.y = 0;
			}
			return pivot;
		}

		public static Vector2 GetPivot(TextureImporterSettings sprSettings)
		{
			SpriteAlignment align = (SpriteAlignment) sprSettings.spriteAlignment;
			if (align == SpriteAlignment.Custom)
				return sprSettings.spritePivot;
			return GetPivot(align);
		}

#endregion
	}
}