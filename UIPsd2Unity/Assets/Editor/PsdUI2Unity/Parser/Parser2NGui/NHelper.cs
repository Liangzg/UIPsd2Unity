using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;

namespace subjectnerdagreement.psdexport
{
    public class NAtlasHelper
    {
        
        private static List<UIAtlasData> atlasPool = new List<UIAtlasData>(); 

        public static void LoadAllAtlas(string atlasFolderPath)
        {
            if (!atlasFolderPath.StartsWith("Assets"))
                atlasFolderPath = string.Concat("Assets/", atlasFolderPath);
            string[] files = AssetDatabase.FindAssets("t:GameObject", new[] {atlasFolderPath});

            foreach (string fileGUID in files)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(fileGUID);
                GameObject gObj = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                UIAtlas atlas = gObj.GetComponent<UIAtlas>();

                if (atlas == null)  continue;
                atlasPool.Add(new UIAtlasData(filePath , atlas));
           }
        }



        public static UIAtlasData FindSprite(string spriteName)
        {
            foreach (UIAtlasData atlasData in atlasPool)
            {
                if (atlasData[spriteName] == null)  continue;
                
                return atlasData;
            }

            return null;
        }


        #region -----------------封装的Atlas数据-------------------------
        public class UIAtlasData
                {
                    public string FilePath;

                    private UIAtlas uiAtlas;

                    private Dictionary<string , UISpriteData> cache = new Dictionary<string, UISpriteData>();

                    public UIAtlasData(string filePath)
                    {
                        FilePath = filePath;

                        GameObject gObj = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                        uiAtlas = gObj.GetComponent<UIAtlas>();
                
                        if (uiAtlas == null)
                            Debug.LogWarning("Cant find UIAtlas !  Path is " + FilePath);
                    }

                    public UIAtlasData(string filePath, UIAtlas atlas)
                    {
                        FilePath = filePath;
                        uiAtlas = atlas;
                    }

                    public UIAtlas mainAtlas { get { return uiAtlas;} }

                    public UISpriteData this[string spriteName]
                    {
                        get
                        {
                            if (cache.ContainsKey(spriteName))
                                return cache[spriteName];

                            UISpriteData _spriteData = null;
                            foreach (UISpriteData spriteData in uiAtlas.spriteList)
                            {
                                if (spriteData.name != spriteName)  continue;
                                cache[spriteName] = spriteData;
                                _spriteData = spriteData;
                                break;
                            }

                            return _spriteData;
                        }
                    }
                }
        #endregion
        
    }


    public class NTempletHelper
    {
        private static Dictionary<string , string> templetNameAndPaths = new Dictionary<string, string>(); 
        public static void LoadAllTemplet(string folderPath)
        {
            string[] templetGUIDArr = AssetDatabase.FindAssets("t:GameObject", new[] {folderPath});
            foreach (string fileGUID in templetGUIDArr)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(fileGUID);
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                templetNameAndPaths[fileName.ToLower()] = filePath;
            }
        }

        public static GameObject GetTempletPrefab(string templetName)
        {
            if (!templetNameAndPaths.ContainsKey(templetName)) return null;

            string filePath = templetNameAndPaths[templetName];
            GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
            return GameObject.Instantiate(prefabObj);
        }
    }
}