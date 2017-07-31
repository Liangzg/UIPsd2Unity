
using System;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if !SLUA_STANDALONE
using UnityEngine;
#endif

namespace EditorTool.PsdExport
{
    [Serializable]
    public class AssetDir
    {
        [SerializeField]
        public string key = string.Empty;
        [SerializeField]
        public string folder = string.Empty;
    }


    public enum GUIType { NGUI , UGUI}

	public class PsdSetting : ScriptableObject
	{
		private const string DEFAULT_IMPORT_PATH = "UI";
	    private const string SETTING_PATH = "Assets/Scripts/Editor/PsdUI2Unity/_Setting/psdsetting.asset";

		public string PsdPath = "";

	    public GUIType curGUIType = GUIType.NGUI;

		public string DefaultImportPath
		{
			get
			{
				if (string.IsNullOrEmpty(m_DefaultImportPath))
					return DEFAULT_IMPORT_PATH;
				return m_DefaultImportPath;
			}
		}

	    public string AtlasImportFolder
	    {
	        get { return string.Concat("Assets/", mAtlaImportPath); }
	    }

	    public string DefaultFontPath
	    {
	        get { return "Assets/" + defaultFont; }
	    }

        public string TempletImportFolder { get { return string.Concat("Assets/", mTempletImportPath); } }

		[SerializeField]
		protected string m_DefaultImportPath;

        [SerializeField]
	    protected string mAtlaImportPath;

        [SerializeField]
	    protected string mTempletImportPath;
        
	    public AssetDir[] assetDirArr;

        #region ------------默认的文本字体--------------------
        /// <summary>
        /// 默认文本的字符大小
        /// </summary>
        [SerializeField]
	    public int DefaultTextSize = 20;
        /// <summary>
        /// 默认字体
        /// </summary>
        [SerializeField]
	    public string defaultFont;

        #endregion

        private static PsdSetting _instance = null;
		public static PsdSetting Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = AssetDatabase.LoadAssetAtPath<PsdSetting>(SETTING_PATH);
#if UNITY_EDITOR
					if (_instance == null)
					{
						_instance = PsdSetting.CreateInstance<PsdSetting>();
					    string dirPath = Path.GetDirectoryName(SETTING_PATH);
					    if (!Directory.Exists(dirPath))
					        Directory.CreateDirectory(dirPath);
						AssetDatabase.CreateAsset(_instance, SETTING_PATH);
					}
#endif
				}

				return _instance;
			}
		}

#if UNITY_EDITOR && !SLUA_STANDALONE
#if NGUI
        [MenuItem("NGUI/Psd Importer Setting" , false , 30)]
#else
        [MenuItem("PSD/Setting")]
#endif
        public static void Open()

		{
			Selection.activeObject = Instance;
		}
#endif


        public string GetAssetFolder(string assetType)
	    {
            if (assetDirArr == null)
            {
                Debug.LogError("提示：请您先为资源定义类型及保存目录！！");
                return DefaultImportPath;
            }
	        foreach (AssetDir assetDir in assetDirArr)
	        {
	            if (assetDir.key == assetType)
	            {
	                return assetDir.folder;
	            }
	        }
	        return DefaultImportPath;
	    }


	    public void insertAssetMapFirst(string key, string path)
	    {
            AssetDir dir = new AssetDir()
            {
                key = key,
                folder = path
            };

            AssetDir[] newAssetDirs = new AssetDir[assetDirArr.Length + 1];
            Array.Copy(assetDirArr, 0 ,  newAssetDirs, 1 , assetDirArr.Length);
            newAssetDirs[0] = dir;
            assetDirArr = newAssetDirs;
        }

        /// <summary>
        /// 设置PSD GUI环境域
        /// </summary>
        /// <param name="guiType"></param>
        public void SetPsdDefine(int guiType)
	    {
            string scriptDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (guiType == 0)
            {
                if (!scriptDefine.ToLower().Contains("ngui"))
                {
                    scriptDefine += ";NGUI";
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, scriptDefine);
                }
            }
            else if (guiType == 1)
            {
                if (scriptDefine.ToLower().Contains("ngui"))
                {
                    scriptDefine = scriptDefine.Replace("NGUI", "");
                    scriptDefine = scriptDefine.Replace(";;", ";");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, scriptDefine);
                }
            }
        }
	}
}
