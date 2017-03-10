using UnityEngine;
using System.Collections.Generic;

namespace EditorTool.PsdExport
{
    public class ImageBinder : ABinder
    {
        public void StartBinding(GameObject gObj, string args, string layerName)
        {
            
        }

        public void ExitBinding(GameObject g, string args, string layerName)
        {
            
        }

        public static string[] GetTextureExportPath(string layerName)
        {
            List<Word> words = WordParser.ParseLayerName(layerName);
            PsdSetting setting = PsdSetting.Instance;
            foreach (Word word in words)
            {
                if (word.TypeAndParams.ContainsKey("img"))
                {
                    string paramStr = word.TypeAndParams["img"];
                    string[] imgInfo = paramStr.Split('#'); //asset type

                    string assetFolder = setting.GetAssetFolder(imgInfo[0]);
                    string assetName = assetFolder == setting.DefaultImportPath ? paramStr : paramStr.Substring(imgInfo[0].Length + 1);
                    return new[] { assetName, assetFolder };                   
                }
                if (word.TypeAndParams.ContainsKey("tmpt"))
                {
                    //模板文件不导出
                    return null;
                }
            }

            return new[]
            {
                layerName,
                setting.DefaultImportPath
            };
        }
    }
}