using System.IO;
using PhotoshopFile;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
	public class UiImgConstructor : IPsdConstructor
	{
		public string MenuName { get { return "Unity UI"; } }

        public Layer layer { get; set; }

        public bool CanBuild(GameObject hierarchySelection)
		{
#if NGUI
            UIPanel parentCanvas = hierarchySelection.GetComponentInParent<UIPanel>();
            return parentCanvas != null;
#else
            Canvas parentCanvas = hierarchySelection.GetComponentInParent<Canvas>();
			return parentCanvas != null;
#endif
        }

		public GameObject CreateGameObject(string name, GameObject parent)
		{
		    GameObject go = SpriteConstructor.GOFactory(name, parent);
#if !NGUI
            // Unity UI objects need Rect Transforms,
            // add the component after creating the base object
            RectTransform rectTrans = go.AddComponent<RectTransform>();
            rectTrans.sizeDelta = Vector2.zero;
#endif
            return go;
		}

		public void AddComponents(int layerIndex, GameObject imageObject, Sprite sprite, TextureImporterSettings settings)
		{
#if NGUI
		    var uiImg = imageObject.AddComponent<UISprite>();
		    string sprPath = PSDExporter.GetLayerFilename(this.layer);
		    if (sprPath == null)
		    {
                Debug.LogWarning("Cant find sprite path . layer name :" +  this.layer.Name);
		        return;
		    }
		    string spriteName = Path.GetFileNameWithoutExtension(sprPath);
		    NAtlasHelper.UIAtlasData atlas = NAtlasHelper.FindSprite(spriteName);
		    if (atlas != null)
                uiImg.atlas = atlas.mainAtlas;		        
		    else
		    {
		        uiImg.width = (int) layer.Rect.width;
		        uiImg.height = (int) layer.Rect.height;
		    }
            uiImg.spriteName = spriteName;
		    uiImg.depth = layerIndex;
            uiImg.MakePixelPerfect();
            uiImg.transform.SetAsFirstSibling();
//
//            Vector2 sprPivot = PsdBuilder.GetPivot(settings);
//            uiImg.pivot = NGUIMath.GetPivot(sprPivot);
#else
            var uiImg = imageObject.AddComponent<Image>();

			uiImg.sprite = sprite;
			uiImg.SetNativeSize();
			uiImg.rectTransform.SetAsFirstSibling();

			Vector2 sprPivot = PsdBuilder.GetPivot(settings);
			uiImg.rectTransform.pivot = sprPivot;
#endif
        }

		public void HandleGroupOpen(GameObject groupParent) { }

		public void HandleGroupClose(GameObject groupParent)
		{
			// Because Unity UI ordering is dependent on
			// the hierarchy order, reposition the layer group
			// when it is closed
			groupParent.transform.SetAsFirstSibling();
		}

		public Vector3 GetLayerPosition(Rect layerSize, Vector2 layerPivot, float pixelsToUnitSize)
		{
			return PsdBuilder.CalculateLayerPosition(layerSize, layerPivot);
		}

		public Vector3 GetGroupPosition(GameObject groupRoot, SpriteAlignment alignment)
		{
		    return PsdBuilder.CalculateGroupPosition(groupRoot , alignment);

		}

	}
}