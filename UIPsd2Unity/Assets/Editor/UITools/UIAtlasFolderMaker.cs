using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EditorTool.PsdExport;

/// <summary>
/// Atlas maker lets you create atlases from a bunch of small textures. It's an alternative to using the external Texture Packer.
/// </summary>

public class UIAtlasFolderMaker {
	static private UIAtlasFolderMaker instance;

    Vector2 mScroll = Vector2.zero;
	UIAtlas mLastAtlas;

    private static List<UIAtlas> atlasList = new List<UIAtlas>();

    public static UIAtlasFolderMaker Instance
    {
        get
        {
            if(instance == null)   
                instance = new UIAtlasFolderMaker();
            return instance;
        }
    }


	/// <summary>
	/// Helper function that retrieves the list of currently selected textures.
	/// </summary>

	List<Texture> GetSelectedTextures (string path , bool loopChild)
	{
		List<Texture> textures = new List<Texture>();

        string[] textGUIDArr = Directory.GetFiles(path, "*.*",
            loopChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        textGUIDArr = textGUIDArr.Where(s=>s.EndsWith("png") || s.EndsWith("jpg")).ToArray();

	    foreach (string filePath in textGUIDArr)
	    {
	        string relativePath = filePath.Replace(Application.dataPath, "Assets");
	        Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(relativePath);
            if (tex == null || tex.name == "Font Texture" || (tex.width > 512 && tex.height > 512)) continue;
            textures.Add(tex);
	    }
        
		return textures;
	}

	/// <summary>
	/// Used to sort the sprites by pixels used
	/// </summary>

	static int Compare (UIAtlasMaker.SpriteEntry a, UIAtlasMaker.SpriteEntry b)
	{
		// A is null b is not b is greater so put it at the front of the list
		if (a == null && b != null) return 1;

		// A is not null b is null a is greater so put it at the front of the list
		if (a != null && b == null) return -1;

		// Get the total pixels used for each sprite
		int aPixels = a.width * a.height;
		int bPixels = b.width * b.height;

		if (aPixels > bPixels) return -1;
		else if (aPixels < bPixels) return 1;
		return 0;
	}

	/// <summary>
	/// Pack all of the specified sprites into a single texture, updating the outer and inner rects of the sprites as needed.
	/// </summary>

	static bool PackTextures (Texture2D tex, List<UIAtlasMaker.SpriteEntry> sprites)
	{
		Texture2D[] textures = new Texture2D[sprites.Count];
		Rect[] rects;

#if UNITY_3_5 || UNITY_4_0
		int maxSize = 4096;
#else
		int maxSize = SystemInfo.maxTextureSize;
#endif

#if UNITY_ANDROID || UNITY_IPHONE
		maxSize = Mathf.Min(maxSize, NGUISettings.allow4096 ? 4096 : 2048);
#endif
		if (NGUISettings.unityPacking)
		{
			for (int i = 0; i < sprites.Count; ++i) textures[i] = sprites[i].tex;
			rects = tex.PackTextures(textures, NGUISettings.atlasPadding, maxSize);
		}
		else
		{
			sprites.Sort(Compare);
			for (int i = 0; i < sprites.Count; ++i) textures[i] = sprites[i].tex;
			rects = UITexturePacker.PackTextures(tex, textures, 4, 4, NGUISettings.atlasPadding, maxSize);
		}

		for (int i = 0; i < sprites.Count; ++i)
		{
			Rect rect = NGUIMath.ConvertToPixels(rects[i], tex.width, tex.height, true);

			// Apparently Unity can take the liberty of destroying temporary textures without any warning
			if (textures[i] == null) return false;

			// Make sure that we don't shrink the textures
			if (Mathf.RoundToInt(rect.width) != textures[i].width) return false;

			UIAtlasMaker.SpriteEntry se = sprites[i];
			se.x = Mathf.RoundToInt(rect.x);
			se.y = Mathf.RoundToInt(rect.y);
			se.width = Mathf.RoundToInt(rect.width);
			se.height = Mathf.RoundToInt(rect.height);
		}
		return true;
	}

	/// <summary>
	/// Extract the specified sprite from the atlas texture.
	/// </summary>

	static UIAtlasMaker.SpriteEntry ExtractSprite (UISpriteData es, Color32[] oldPixels, int oldWidth, int oldHeight)
	{
		int xmin = Mathf.Clamp(es.x, 0, oldWidth);
		int ymin = Mathf.Clamp(es.y, 0, oldHeight);
		int xmax = Mathf.Min(xmin + es.width, oldWidth - 1);
		int ymax = Mathf.Min(ymin + es.height, oldHeight - 1);
		int newWidth = Mathf.Clamp(es.width, 0, oldWidth);
		int newHeight = Mathf.Clamp(es.height, 0, oldHeight);

		if (newWidth == 0 || newHeight == 0) return null;

		Color32[] newPixels = new Color32[newWidth * newHeight];

		for (int y = 0; y < newHeight; ++y)
		{
			int cy = ymin + y;
			if (cy > ymax) cy = ymax;

			for (int x = 0; x < newWidth; ++x)
			{
				int cx = xmin + x;
				if (cx > xmax) cx = xmax;

				int newIndex = (newHeight - 1 - y) * newWidth + x;
				int oldIndex = (oldHeight - 1 - cy) * oldWidth + cx;

				newPixels[newIndex] = oldPixels[oldIndex];
			}
		}

		// Create a new sprite
		UIAtlasMaker.SpriteEntry sprite = new UIAtlasMaker.SpriteEntry();
		sprite.CopyFrom(es);
		sprite.SetRect(0, 0, newWidth, newHeight);
		sprite.SetTexture(newPixels, newWidth, newHeight);
		return sprite;
	}

	/// <summary>
	/// Update the sprites within the texture atlas, preserving the sprites that have not been selected.
	/// </summary>

	void UpdateAtlas (UIAtlas atlas , List<Texture> textures, bool keepSprites)
	{
		// Create a list of sprites using the collected textures
		List<UIAtlasMaker.SpriteEntry> sprites = UIAtlasMaker.CreateSprites(textures);

		if (sprites.Count > 0)
		{
			// Extract sprites from the atlas, filling in the missing pieces
			if (keepSprites) UIAtlasMaker.ExtractSprites(atlas, sprites);

			// NOTE: It doesn't seem to be possible to undo writing to disk, and there also seems to be no way of
			// detecting an Undo event. Without either of these it's not possible to restore the texture saved to disk,
			// so the undo process doesn't work right. Because of this I'd rather disable it altogether until a solution is found.

			// The ability to undo this action is always useful
			//NGUIEditorTools.RegisterUndo("Update Atlas", UISettings.atlas, UISettings.atlas.texture, UISettings.atlas.material);

			// Update the atlas
			UIAtlasMaker.UpdateAtlas(atlas , sprites);
		}
		else if (!keepSprites)
		{
            UIAtlasMaker.UpdateAtlas(atlas , sprites);
		}
	}


	/// <summary>
	/// Draw the UI for this tool.
	/// </summary>

	public void OnBuild ()
	{
		NGUISettings.atlasPadding = 1;
	    NGUISettings.atlasTrimming = true; 
		NGUISettings.atlasPMA = false;
		NGUISettings.unityPacking = false;
	    NGUISettings.forceSquareAtlas = false;
        NGUISettings.trueColorAtlas = true;

        atlasList.Clear();
        string rootFolderPath = Path.Combine(Application.dataPath, PsdSetting.Instance.DefaultImportPath);
	    rootFolderPath = rootFolderPath.Replace("\\", "/");
	    string[] childFolderArr = System.IO.Directory.GetDirectories(rootFolderPath);
	    foreach (string folderDir in childFolderArr)
	    {
            string dir = folderDir.Replace("\\", "/");
	        string folderName = Path.GetFileNameWithoutExtension(dir + ".xx");
            string atlasPath = Path.Combine(PsdSetting.Instance.AtlasImportFolder, folderName + ".prefab");
	        atlasPath = atlasPath.Replace("\\", "/");
            
            List<Texture> textures = GetSelectedTextures(folderDir , true);
            UIAtlas atlas = createAtlas(atlasPath);
            atlasList.Add(atlas);
            UpdateAtlas(atlas , textures, false);
            
        }

        //根目录
        string rootfolderName = Path.GetFileNameWithoutExtension(rootFolderPath + ".xx");
        string atlasRootPath = Path.Combine(PsdSetting.Instance.AtlasImportFolder, rootfolderName + ".prefab");
        atlasRootPath = atlasRootPath.Replace("\\", "/");

        List<Texture> rootTextures = GetSelectedTextures(rootFolderPath, false);
        UIAtlas atlasRoot = createAtlas(atlasRootPath);
        atlasList.Add(atlasRoot);
        UpdateAtlas(atlasRoot, rootTextures, false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }


    private UIAtlas createAtlas(string path)
    {
        NGUISettings.currentPath = System.IO.Path.GetDirectoryName(path);
        GameObject go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        string matPath = path.Replace(".prefab", ".mat");

        // Try to load the material
        Material mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;

        // If the material doesn't exist, create it
        if (mat == null)
        {
            Shader shader = Shader.Find(NGUISettings.atlasPMA ? "Unlit/Premultiplied Colored" : "Unlit/Transparent Colored");
            mat = new Material(shader);

            // Save the material
            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // Load the material so it's usable
            mat = AssetDatabase.LoadAssetAtPath(matPath, typeof(Material)) as Material;
        }

        // Create a new prefab for the atlas
        Object prefab = (go != null) ? go : PrefabUtility.CreateEmptyPrefab(path);

        // Create a new game object for the atlas
        string atlasName = path.Replace(".prefab", "");
        atlasName = atlasName.Substring(path.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
        go = new GameObject(atlasName);
        go.AddComponent<UIAtlas>().spriteMaterial = mat;

        // Update the prefab
        PrefabUtility.ReplacePrefab(go, prefab);
        GameObject.DestroyImmediate(go);
                
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        // Select the atlas
        go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        return go.GetComponent<UIAtlas>();
    }
}
