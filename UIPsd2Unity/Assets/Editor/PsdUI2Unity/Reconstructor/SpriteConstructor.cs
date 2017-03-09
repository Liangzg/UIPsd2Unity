using PhotoshopFile;
using UnityEditor;
using UnityEngine;

namespace EditorTool.PsdExport
{
	public class SpriteConstructor : IPsdConstructor
	{
		/// <summary>
		/// Convenience factory function for creating a blank GameObject
		/// </summary>
		/// <param name="name">Name of game object</param>
		/// <param name="parent">Parent GameObject, if any</param>
		/// <returns>A new GameObject</returns>
		public static GameObject GOFactory(string name, GameObject parent)
		{
			GameObject spriteGO = new GameObject(name);
			Transform spriteT = spriteGO.transform;

			if (parent != null)
			{
				spriteT.SetParent(parent.transform);
				spriteGO.layer = parent.layer;
				spriteGO.tag = parent.tag;
			}

			spriteT.localPosition = Vector3.zero;
			spriteT.localScale = Vector3.one;

			return spriteGO;
		}

		public string MenuName { get { return "Unity Sprites"; } }
        
        public Layer layer { get; set; }

        public bool CanBuild(GameObject hierarchySelection) { return true; }

		public GameObject CreateGameObject(string name, GameObject parent)
		{
			return GOFactory(name, parent);
		}

		public void AddComponents(int layerIndex, GameObject imageObject, Sprite sprite, TextureImporterSettings settings)
		{
			var spr = imageObject.AddComponent<SpriteRenderer>();
			spr.sprite = sprite;
			// If setting the sorting order is as simple as setting a sorting number,
			// do it here. Note that UiImgConstructor is more complex because of how
			// Unity UI uses the hierarchy position for sorting
			spr.sortingOrder = layerIndex;
		}

		public void HandleGroupOpen(GameObject groupParent) { }

		public void HandleGroupClose(GameObject groupParent) { }

		public Vector3 GetLayerPosition(Rect layerSize, Vector2 layerPivot, float pixelsToUnitSize)
		{
			Vector3 layerPos = PsdBuilder.CalculateLayerPosition(layerSize, layerPivot);
			// Because this constructor is working with Unity sprites,
			// scale the layer position by the pixels to unit size
			layerPos /= pixelsToUnitSize;

			return layerPos;
		}

		public Vector3 GetGroupPosition(GameObject groupRoot, SpriteAlignment alignment)
		{
			Transform t = groupRoot.transform;

			// Find all the SpriteRenderers that are children of the layer group
			var spriteList = t.GetComponentsInChildren<SpriteRenderer>();

			Vector3 min = new Vector3(float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue);

			// From the SpriteRenderers, find the size of
			// the layer group by the bounds of the renderers
			foreach (var sprite in spriteList)
			{
				var bounds = sprite.bounds;
				min = Vector3.Min(min, bounds.min);
				max = Vector3.Max(max, bounds.max);
			}

			// Calculate the position for this layer group
			Vector2 pivot = PsdBuilder.GetPivot(alignment);
			Vector3 pos = Vector3.zero;
			pos.x = Mathf.Lerp(min.x, max.x, pivot.x);
			pos.y = Mathf.Lerp(min.y, max.y, pivot.y);
			return pos;
		}
	}
}