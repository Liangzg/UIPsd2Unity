using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace EditorTool.PsdExport
{
	[CustomEditor(typeof(PsdSetting))]
	public class PsdSettingEditor : Editor
	{
		protected PsdSetting m_PsdSetting;

		public void OnEnable()
		{
			m_PsdSetting = (PsdSetting)target;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

            //			EditorGUILayout.BeginHorizontal();

            //			if (GUILayout.Button("Psd Path"))
            //			{
            //                string savePath = EditorUtility.SaveFolderPanel("Default psd file path", m_PsdSetting.PsdPath, string.Empty);
            //			    m_PsdSetting.PsdPath = savePath.Substring(Application.dataPath.Length + 1);
            //			}
            //			m_PsdSetting.PsdPath = GUILayout.TextArea(m_PsdSetting.PsdPath);

            //			EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label("GUI", GUILayout.MaxWidth(100));
            SerializedProperty guiType = serializedObject.FindProperty("curGUIType");
            guiType.enumValueIndex = EditorGUILayout.Popup(guiType.enumValueIndex, new[] {"NGUI", "UGUI"});

            string scriptDefine = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (guiType.enumValueIndex == 0)
		    {
		        if (!scriptDefine.ToLower().Contains("ngui"))
		        {
		            scriptDefine += ";NGUI";
		            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup , scriptDefine);
		        }
		    }else if (guiType.enumValueIndex == 1)
		    {
		        if (scriptDefine.ToLower().Contains("ngui"))
		        {
		            scriptDefine = scriptDefine.Replace("NGUI", "");
                    scriptDefine = scriptDefine.Replace(";;", ";");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, scriptDefine);
                }
            }
            EditorGUILayout.EndHorizontal();

            /////Default Import Path
            this.onPathSetting("m_DefaultImportPath" , "Default Import");

            //  NGUI Atlas Path
		    if (guiType.enumValueIndex == 0)
		    {
                this.onPathSetting("mAtlaImportPath" , "Atlas Folder");
		    }

            //  Templet Import Path
            this.onPathSetting("mTempletImportPath" , "Templet Folder");

            EditorGUILayout.Foldout(true, "Custom Asset Type");
            List<int> rmList = new List<int>();
            SerializedProperty assetDirArr = serializedObject.FindProperty("assetDirArr");
		    for (int i = 0; i < assetDirArr.arraySize; i++)
		    {
		        SerializedProperty element = assetDirArr.GetArrayElementAtIndex(i);

                GUILayout.BeginHorizontal();
                SerializedProperty keyProperty = element.FindPropertyRelative("key");
                keyProperty.stringValue = GUILayout.TextField(keyProperty.stringValue, GUILayout.Width(100));

                SerializedProperty folderProperty = element.FindPropertyRelative("folder");
                folderProperty.stringValue = GUILayout.TextField(folderProperty.stringValue);
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    string savePath = EditorUtility.SaveFolderPanel("Save Path",
                        string.IsNullOrEmpty(folderProperty.stringValue) ? Application.dataPath : folderProperty.stringValue,
                        string.Empty);
                    if (savePath.StartsWith(Application.dataPath))
                        folderProperty.stringValue = savePath.Substring(Application.dataPath.Length + 1);
                }

                if (GUILayout.Button("X", GUILayout.Width(25)))
                    rmList.Add(i);
                GUILayout.EndHorizontal();
		    }
            
            rmList.Sort();

		    for (int i = rmList.Count - 1; i >= 0; i--)
		    {
                 assetDirArr.DeleteArrayElementAtIndex(rmList[i]);
		    }

		    if (GUILayout.Button("+"))
		    {
		        assetDirArr.InsertArrayElementAtIndex(assetDirArr.arraySize);

                SerializedProperty element = assetDirArr.GetArrayElementAtIndex(assetDirArr.arraySize - 1);
                SerializedProperty keyProperty = element.FindPropertyRelative("key");
                keyProperty.stringValue = "";

                SerializedProperty folderProperty = element.FindPropertyRelative("folder");
		        folderProperty.stringValue = "";
		    }


            this.textSetting();

            serializedObject.ApplyModifiedProperties();
		}

        /// <summary>
        /// 文本设置
        /// </summary>
	    private void textSetting()
	    {
            EditorGUILayout.Foldout(true, "Text Setting");

//            GUILayout.BeginHorizontal();
//            SerializedProperty defaultTextSize = serializedObject.FindProperty("DefaultTextSize");
//            GUILayout.Label("Text Size" , GUILayout.MaxWidth(100));
//            string newSize = GUILayout.TextField(Convert.ToString(defaultTextSize.intValue));
//            defaultTextSize.intValue = Convert.ToInt32(string.IsNullOrEmpty(newSize) ? "20" : newSize);
//            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            SerializedProperty defaultFont = serializedObject.FindProperty("defaultFont");
            if (GUILayout.Button("Font" , GUILayout.MaxWidth(100)))
            {
                string path = EditorUtility.OpenFilePanel("select file",
                                    Path.Combine(Application.dataPath, defaultFont.stringValue),
                                    string.Empty);
                var startLen = Mathf.Min(Application.dataPath.Length + 1, path.Length);
                defaultFont.stringValue = path.Substring(startLen);
            }
            GUILayout.TextField(defaultFont.stringValue);
            GUILayout.EndHorizontal();
        }


	    private void onPathSetting(string propertyName , string btnTitle)
	    {
	         EditorGUILayout.BeginHorizontal();

            SerializedProperty mImportPath = serializedObject.FindProperty(propertyName);
            if (GUILayout.Button(btnTitle, GUILayout.MaxWidth(100)))
            {
                var path = EditorUtility.SaveFolderPanel("Select Import path",
                                                        Path.Combine(Application.dataPath, mImportPath.stringValue), string.Empty);

                if (path.StartsWith(Application.dataPath))
                {
                    var startLen = Mathf.Min(Application.dataPath.Length + 1, path.Length);
                    mImportPath.stringValue = path.Substring(startLen);
                }
                else
                {
                    Debug.LogWarning("Not support path out of Application.dataPath.");
                }
            }
            mImportPath.stringValue = GUILayout.TextArea(mImportPath.stringValue);
            EditorGUILayout.EndHorizontal();
	    }
       

	}
}
