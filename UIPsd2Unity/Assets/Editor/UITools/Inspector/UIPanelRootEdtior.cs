using System.IO;
using UnityEditor;
using UnityEngine;

namespace UIHelper
{
    [CustomEditor(typeof(UIPanelRoot))]
    public class UIPanelRootEdtior : Editor
    {

        public override void OnInspectorGUI()
        {
            UIPanelRoot root = target as UIPanelRoot;

            EditorGUIUtility.labelWidth = 120;
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("ScriptName"));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("FilePath", GUILayout.Width(120)))
            {

                string selectFilePath = EditorUtility.SaveFilePanel("Select Folder", Path.Combine(Application.dataPath, root.FilePath),
                    "RootViewName" , "lua");
                root.FilePath = selectFilePath.Replace(Application.dataPath, "/").Replace("//" , "");
                root.ScriptName = Path.GetFileNameWithoutExtension(selectFilePath);

                serializedObject.FindProperty("FilePath").stringValue = root.FilePath;
                serializedObject.FindProperty("ScriptName").stringValue = root.ScriptName;
            }
            GUILayout.TextArea(root.FilePath);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Build"))
            {
                root.BuildPanel(root.gameObject);
                Debug.Log("<color=#2fd95b>Build Success !</color>");
            }  
        }
    }
}