//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

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
	/// Add a new sprite to the atlas, given the texture it's coming from and the packed rect within the atlas.
	/// </summary>

	static public UISpriteData AddSprite (List<UISpriteData> sprites, UIAtlasMaker.SpriteEntry se)
	{
		// See if this sprite already exists
		foreach (UISpriteData sp in sprites)
		{
			if (sp.name == se.name)
			{
				sp.CopyFrom(se);
				return sp;
			}
		}

		UISpriteData sprite = new UISpriteData();
		sprite.CopyFrom(se);
		sprites.Add(sprite);
		return sprite;
	}

	/// <summary>
	/// Create a list of sprites using the specified list of textures.
	/// </summary>

	static public List<UIAtlasMaker.SpriteEntry> CreateSprites (List<Texture> textures)
	{
		List<UIAtlasMaker.SpriteEntry> list = new List<UIAtlasMaker.SpriteEntry>();

		foreach (Texture tex in textures)
		{
			Texture2D oldTex = NGUIEditorTools.ImportTexture(tex, true, false, true);
			if (oldTex == null) oldTex = tex as Texture2D;
			if (oldTex == null) continue;

			// If we aren't doing trimming, just use the texture as-is
			if (!NGUISettings.atlasTrimming && !NGUISettings.atlasPMA)
			{
				UIAtlasMaker.SpriteEntry sprite = new UIAtlasMaker.SpriteEntry();
				sprite.SetRect(0, 0, oldTex.width, oldTex.height);
				sprite.tex = oldTex;
				sprite.name = oldTex.name;
				sprite.temporaryTexture = false;
				list.Add(sprite);
				continue;
			}

			// If we want to trim transparent pixels, there is more work to be done
			Color32[] pixels = oldTex.GetPixels32();

			int xmin = oldTex.width;
			int xmax = 0;
			int ymin = oldTex.height;
			int ymax = 0;
			int oldWidth = oldTex.width;
			int oldHeight = oldTex.height;

			// Find solid pixels
			if (NGUISettings.atlasTrimming)
			{
				for (int y = 0, yw = oldHeight; y < yw; ++y)
				{
					for (int x = 0, xw = oldWidth; x < xw; ++x)
					{
						Color32 c = pixels[y * xw + x];

						if (c.a != 0)
						{
							if (y < ymin) ymin = y;
							if (y > ymax) ymax = y;
							if (x < xmin) xmin = x;
							if (x > xmax) xmax = x;
						}
					}
				}
			}
			else
			{
				xmin = 0;
				xmax = oldWidth - 1;
				ymin = 0;
				ymax = oldHeight - 1;
			}

			int newWidth  = (xmax - xmin) + 1;
			int newHeight = (ymax - ymin) + 1;

			if (newWidth > 0 && newHeight > 0)
			{
				UIAtlasMaker.SpriteEntry sprite = new UIAtlasMaker.SpriteEntry();
				sprite.x = 0;
				sprite.y = 0;
				sprite.width = oldTex.width;
				sprite.height = oldTex.height;

				// If the dimensions match, then nothing was actually trimmed
				if (!NGUISettings.atlasPMA && (newWidth == oldWidth && newHeight == oldHeight))
				{
					sprite.tex = oldTex;
					sprite.name = oldTex.name;
					sprite.temporaryTexture = false;
				}
				else
				{
					// Copy the non-trimmed texture data into a temporary buffer
					Color32[] newPixels = new Color32[newWidth * newHeight];

					for (int y = 0; y < newHeight; ++y)
					{
						for (int x = 0; x < newWidth; ++x)
						{
							int newIndex = y * newWidth + x;
							int oldIndex = (ymin + y) * oldWidth + (xmin + x);
							if (NGUISettings.atlasPMA) newPixels[newIndex] = NGUITools.ApplyPMA(pixels[oldIndex]);
							else newPixels[newIndex] = pixels[oldIndex];
						}
					}

					// Create a new texture
					sprite.name = oldTex.name;
					sprite.SetTexture(newPixels, newWidth, newHeight);

					// Remember the padding offset
					sprite.SetPadding(xmin, ymin, oldWidth - newWidth - xmin, oldHeight - newHeight - ymin);
				}
				list.Add(sprite);
			}
		}
		return list;
	}

	/// <summary>
	/// Release all temporary textures created for the sprites.
	/// </summary>

	static public void ReleaseSprites (List<UIAtlasMaker.SpriteEntry> sprites)
	{
		foreach (UIAtlasMaker.SpriteEntry se in sprites) se.Release();
		Resources.UnloadUnusedAssets();
	}

	/// <summary>
	/// Replace the sprites within the atlas.
	/// </summary>

	static public void ReplaceSprites (UIAtlas atlas, List<UIAtlasMaker.SpriteEntry> sprites)
	{
		// Get the list of sprites we'll be updating
		List<UISpriteData> spriteList = atlas.spriteList;
		List<UISpriteData> kept = new List<UISpriteData>();

		// Run through all the textures we added and add them as sprites to the atlas
		for (int i = 0; i < sprites.Count; ++i)
		{
			UIAtlasMaker.SpriteEntry se = sprites[i];
			UISpriteData sprite = AddSprite(spriteList, se);
			kept.Add(sprite);
		}

		// Remove unused sprites
		for (int i = spriteList.Count; i > 0; )
		{
			UISpriteData sp = spriteList[--i];
			if (!kept.Contains(sp)) spriteList.RemoveAt(i);
		}

		// Sort the sprites so that they are alphabetical within the atlas
		atlas.SortAlphabetically();
		atlas.MarkAsChanged();
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
	/// Extract sprites from the atlas, adding them to the list.
	/// </summary>

	static public void ExtractSprites (UIAtlas atlas , List<UIAtlasMaker.SpriteEntry> finalSprites)
	{
		ShowProgress(0f);

		// Make the atlas texture readable
		Texture2D tex = NGUIEditorTools.ImportTexture(atlas.texture, true, true, false);

		if (tex != null)
		{
			Color32[] pixels = null;
			int width = tex.width;
			int height = tex.height;
			List<UISpriteData> sprites = atlas.spriteList;
			float count = sprites.Count;
			int index = 0;

			foreach (UISpriteData es in sprites)
			{
				ShowProgress((index++) / count);

				bool found = false;

				foreach (UIAtlasMaker.SpriteEntry fs in finalSprites)
				{
					if (es.name == fs.name)
					{
						fs.CopyBorderFrom(es);
						found = true;
						break;
					}
				}

				if (!found)
				{
					if (pixels == null) pixels = tex.GetPixels32();
					UIAtlasMaker.SpriteEntry sprite = ExtractSprite(es, pixels, width, height);
					if (sprite != null) finalSprites.Add(sprite);
				}
			}
		}

		// The atlas no longer needs to be readable
		NGUIEditorTools.ImportTexture(NGUISettings.atlas.texture, false, false, !NGUISettings.atlas.premultipliedAlpha);
		ShowProgress(1f);
	}

	/// <summary>
	/// Combine all sprites into a single texture and save it to disk.
	/// </summary>

	static public bool UpdateTexture (UIAtlas atlas, List<UIAtlasMaker.SpriteEntry> sprites)
	{
		// Get the texture for the atlas
		Texture2D tex = atlas.texture as Texture2D;
		string oldPath = (tex != null) ? AssetDatabase.GetAssetPath(tex.GetInstanceID()) : "";
		string newPath = NGUIEditorTools.GetSaveableTexturePath(atlas);

		// Clear the read-only flag in texture file attributes
		if (System.IO.File.Exists(newPath))
		{
#if !UNITY_4_1 && !UNITY_4_0 && !UNITY_3_5
			if (!AssetDatabase.IsOpenForEdit(newPath))
			{
				Debug.LogError(newPath + " is not editable. Did you forget to do a check out?");
				return false;
			}
#endif
			System.IO.FileAttributes newPathAttrs = System.IO.File.GetAttributes(newPath);
			newPathAttrs &= ~System.IO.FileAttributes.ReadOnly;
			System.IO.File.SetAttributes(newPath, newPathAttrs);
		}

		bool newTexture = (tex == null || oldPath != newPath);

		if (newTexture)
		{
			// Create a new texture for the atlas
			tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		}
		else
		{
			// Make the atlas readable so we can save it
			tex = NGUIEditorTools.ImportTexture(oldPath, true, false, false);
		}

		// Pack the sprites into this texture
		if (PackTextures(tex, sprites))
		{
			byte[] bytes = tex.EncodeToPNG();
			System.IO.File.WriteAllBytes(newPath, bytes);
			bytes = null;

			// Load the texture we just saved as a Texture2D
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			tex = NGUIEditorTools.ImportTexture(newPath, false, true, !atlas.premultipliedAlpha);

			// Update the atlas texture
			if (newTexture)
			{
				if (tex == null) Debug.LogError("Failed to load the created atlas saved as " + newPath);
				else atlas.spriteMaterial.mainTexture = tex;
				ReleaseSprites(sprites);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			}
			return true;
		}
		else
		{
			if (!newTexture) NGUIEditorTools.ImportTexture(oldPath, false, true, !atlas.premultipliedAlpha);

			//Debug.LogError("Operation canceled: The selected sprites can't fit into the atlas.\n" +
			//	"Keep large sprites outside the atlas (use UITexture), and/or use multiple atlases instead.");

			EditorUtility.DisplayDialog("Operation Canceled", "The selected sprites can't fit into the atlas.\n" +
					"Keep large sprites outside the atlas (use UITexture), and/or use multiple atlases instead", "OK");
			return false;
		}
	}

	/// <summary>
	/// Show a progress bar.
	/// </summary>

	static public void ShowProgress (float val)
	{
		EditorUtility.DisplayProgressBar("Updating", "Updating the atlas, please wait...", val);
	}

	/// <summary>
	/// Update the sprites within the texture atlas, preserving the sprites that have not been selected.
	/// </summary>

	void UpdateAtlas (List<Texture> textures, bool keepSprites)
	{
		// Create a list of sprites using the collected textures
		List<UIAtlasMaker.SpriteEntry> sprites = CreateSprites(textures);

		if (sprites.Count > 0)
		{
			// Extract sprites from the atlas, filling in the missing pieces
			if (keepSprites) ExtractSprites(NGUISettings.atlas, sprites);

			// NOTE: It doesn't seem to be possible to undo writing to disk, and there also seems to be no way of
			// detecting an Undo event. Without either of these it's not possible to restore the texture saved to disk,
			// so the undo process doesn't work right. Because of this I'd rather disable it altogether until a solution is found.

			// The ability to undo this action is always useful
			//NGUIEditorTools.RegisterUndo("Update Atlas", UISettings.atlas, UISettings.atlas.texture, UISettings.atlas.material);

			// Update the atlas
			UpdateAtlas(NGUISettings.atlas , sprites);
		}
		else if (!keepSprites)
		{
			UpdateAtlas(NGUISettings.atlas , sprites);
		}
	}

	/// <summary>
	/// Update the sprite atlas, keeping only the sprites that are on the specified list.
	/// </summary>

	static public void UpdateAtlas (UIAtlas atlas, List<UIAtlasMaker.SpriteEntry> sprites)
	{
		if (sprites.Count > 0)
		{
			// Combine all sprites into a single texture and save it
			if (UpdateTexture(atlas, sprites))
			{
				// Replace the sprites within the atlas
				ReplaceSprites(atlas, sprites);
			}

			// Release the temporary textures
			ReleaseSprites(sprites);
			EditorUtility.ClearProgressBar();
			return;
		}
		else
		{
			atlas.spriteList.Clear();
			string path = NGUIEditorTools.GetSaveableTexturePath(atlas);
			atlas.spriteMaterial.mainTexture = null;
			if (!string.IsNullOrEmpty(path)) AssetDatabase.DeleteAsset(path);
		}

		atlas.MarkAsChanged();
		Selection.activeGameObject = (NGUISettings.atlas != null) ? NGUISettings.atlas.gameObject : null;
		EditorUtility.ClearProgressBar();
	}

	/// <summary>
	/// Draw the UI for this tool.
	/// </summary>

	public void OnBuild ()
	{
		NGUISettings.atlasPadding = 1;
	    NGUISettings.atlasTrimming = true;       

		if (NGUISettings.atlas != null)
		{
			Material mat = NGUISettings.atlas.spriteMaterial;
			if (mat != null)
			{
				Shader shader = mat.shader;
				if (shader != null)
					NGUISettings.atlasPMA = false;
			}
		}

		NGUISettings.unityPacking = true;
        NGUISettings.trueColorAtlas = true;

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
            createAtlas(atlasPath);
            UpdateAtlas(textures, false);
        }

        //根目录
        string rootfolderName = Path.GetFileNameWithoutExtension(rootFolderPath + ".xx");
        string atlasRootPath = Path.Combine(PsdSetting.Instance.AtlasImportFolder, rootfolderName + ".prefab");
        atlasRootPath = atlasRootPath.Replace("\\", "/");

        List<Texture> rootTextures = GetSelectedTextures(rootFolderPath, false);
        createAtlas(atlasRootPath);
        UpdateAtlas(rootTextures, false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }


    private void createAtlas(string path)
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
        NGUISettings.atlas = go.GetComponent<UIAtlas>();
    }
}
