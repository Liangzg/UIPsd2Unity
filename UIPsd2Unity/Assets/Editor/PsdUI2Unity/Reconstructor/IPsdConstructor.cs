using PhotoshopFile;
using UnityEditor;
using UnityEngine;

namespace subjectnerdagreement.psdexport
{
	public interface IPsdConstructor
	{
		/// <summary>
		/// Name to display in a dropdown menu
		/// </summary>
		string MenuName { get; }

        Layer layer { get; set; }

        /// <summary>
        /// Returns if the constructor can build in the given hierarchy selection
        /// </summary>
        /// <param name="hierarchySelection"></param>
        /// <returns></returns>
        bool CanBuild(GameObject hierarchySelection);

		/// <summary>
		/// A factory function for creating a game object.
		/// Can usually use SpriteConstructor.GOFactory.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		GameObject CreateGameObject(string name, GameObject parent);

		/// <summary>
		/// Add the components that make up an image
		/// </summary>
		/// <param name="layerIndex">Layer depth/sorting order</param>
		/// <param name="imageObject">Parent object of the image</param>
		/// <param name="sprite">The Unity sprite to display on this image</param>
		/// <param name="settings"></param>
		void AddComponents(int layerIndex, GameObject imageObject, Sprite sprite, TextureImporterSettings settings);

		/// <summary>
		/// Do something when a layer group starts
		/// </summary>
		/// <param name="groupParent"></param>
		void HandleGroupOpen(GameObject groupParent);

		/// <summary>
		/// Do something when a layer group closes
		/// </summary>
		/// <param name="groupParent"></param>
		void HandleGroupClose(GameObject groupParent);

		/// <summary>
		/// Calculate the position of an object in the layer space
		/// Usually uses PsdBuilder.CalculateLayerPosition
		/// </summary>
		/// <param name="layerSize">Pixel size of the layer</param>
		/// <param name="layerPivot">Pivot point within the layer</param>
		/// <param name="pixelsToUnitSize">Pixels to Unit Size in the import settings</param>
		/// <returns></returns>
		Vector3 GetLayerPosition(Rect layerSize, Vector2 layerPivot, float pixelsToUnitSize);

		/// <summary>
		/// Calculate the position of a Layer group, given the GameObject
		/// that is the root of the layer group.
		/// </summary>
		/// <param name="groupRoot"></param>
		/// <param name="alignment"></param>
		/// <returns></returns>
		Vector3 GetGroupPosition(GameObject groupRoot, SpriteAlignment alignment);
	}
}