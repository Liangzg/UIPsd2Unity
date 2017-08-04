using System;
using System.IO;
using System.Text;
using System.Xml;
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
            if (root == null) return;

            EditorGUIUtility.labelWidth = 120;
            Transform parentTrans = root.transform.parent;
            bool isRoot = false;
            if (parentTrans)
            {
                UIPanelRoot parentRoot = parentTrans.GetComponentInParent<UIPanelRoot>();
                if (parentRoot)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Field"));
                else
                {
//                    GUI.color = Color.yellow;
//                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Controller"));
                    isRoot = true;
//                    GUI.color = Color.white;
                }             
            }

            SerializedProperty requirePath = serializedObject.FindProperty("LuaRequirePath");
//            requirePath.stringValue = EditorGUILayout.TextField("LuaRequirePath" , requirePath.stringValue);
            if(!isRoot)
                EditorGUILayout.PropertyField(requirePath);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("FilePath", GUILayout.Width(120)))
            {

                string selectFilePath = EditorUtility.SaveFilePanel("Select Folder", Path.Combine(Application.dataPath, root.FilePath),
                    "RootViewName" , "lua");
                root.FilePath = selectFilePath.Replace(Application.dataPath, "/").Replace("//" , "");
                root.Field = Path.GetFileNameWithoutExtension(selectFilePath);

                string requireLuaPath = root.FilePath.Replace("LuaFramework/Lua/", "");
                serializedObject.FindProperty("FilePath").stringValue = root.FilePath;
                serializedObject.FindProperty("Field").stringValue = root.Field;
                requirePath.stringValue = requireLuaPath.Replace(".lua", "").Replace("/" , ".");
            }
            GUILayout.TextArea(root.FilePath);
            EditorGUILayout.EndHorizontal();
            
            GUI.color = Color.green;
            if (isRoot && GUILayout.Button("Build") )
            {
                if (PrefabUtility.GetPrefabType(root.gameObject) != PrefabType.PrefabInstance)
                {
                    EditorUtility.DisplayDialog("提示", "请先存储UI为Prefab\n注意格式为（xxxPanel.prefab）", "OK");
                    return;
                }

                XmlDocument doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                doc.AppendChild(dec);

                root.BuildPanel(doc , root.gameObject , true);

                string genDir = Path.Combine(Application.dataPath, ToolConst.GenLogFolder);
                if (! Directory.Exists(genDir)) Directory.CreateDirectory(genDir);

                string filePath = Path.Combine(genDir, root.name + ".xml");
                doc.Save(filePath);

                Debug.Log("<color=#2fd95b>Build Success !</color>");
                EditorUtility.DisplayDialog("恭喜", "Build Success ！", "OK");
            }
            GUI.color = Color.white;

            serializedObject.ApplyModifiedProperties();
        }

        
    }
}