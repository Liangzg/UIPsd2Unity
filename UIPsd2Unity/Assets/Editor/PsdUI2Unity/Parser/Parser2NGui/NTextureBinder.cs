using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTool.PsdExport
{
    public class NTextureBinder : ABinder
    {
#if NGUI
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            UITexture mTexture = LayerWordBinder.swapComponent<UITexture>(gObj);
            UISprite sprte = gObj.GetComponent<UISprite>();
            mTexture.mainTexture = this.findBigTexture(sprte.spriteName);

            mTexture.depth = sprte.depth;
            mTexture.width = sprte.width;
            mTexture.height = sprte.height;

            mTexture.shader = Shader.Find("Unlit/Transparent Colored");
            GameObject.DestroyImmediate(sprte);
        }


        private Texture findBigTexture(string imgName)
        {
            string defaultPath = PsdSetting.Instance.DefaultImportPath;
            string bigTextDir = null;
            foreach (AssetDir assetDir in PsdSetting.Instance.assetDirArr)
            {
                bigTextDir = assetDir.folder;
                if (!assetDir.folder.StartsWith(defaultPath))
                {
                    string[] textureArr = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/" + bigTextDir });
                    foreach (string texGUID in textureArr)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(texGUID);
                        string fileName = Path.GetFileNameWithoutExtension(path);
                        if (fileName == imgName)
                        {
                            return AssetDatabase.LoadAssetAtPath<Texture>(path);
                        }
                    }
                }
            }
            return null;
        }
#endif
    }
}