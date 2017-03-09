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
            
            GUI.color = Color.green;
            if (GUILayout.Button("Build"))
            {
                XmlDocument doc = new XmlDocument();
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                doc.AppendChild(dec);

                root.BuildPanel(doc , root.gameObject);

                string genDir = Path.Combine(Application.dataPath, ToolConst.GenLogFolder);
                if (! Directory.Exists(genDir)) Directory.CreateDirectory(genDir);

                string filePath = Path.Combine(genDir, root.name + ".xml");
                doc.Save(filePath);

                Debug.Log("<color=#2fd95b>Build Success !</color>");
            }
            GUI.color = Color.white;
        }

        
    }
}