using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UIHelper
{
    [CustomEditor(typeof(UIGenFlag))]
    public class UIGenFlagEditor : Editor
    {

        

        public override void OnInspectorGUI()
        {
            UIGenFlag genFlag = target as UIGenFlag;

            EditorGUIUtility.labelWidth = 120;
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("Uri"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Field"));

            SerializedProperty st = serializedObject.FindProperty("ScriptType");
            string[] typeStrArr = genFlag.FindWidgetTypes().ToArray();
            int selectIndex = 0;
            if (genFlag.ScriptType != null)
            {
                for (int i = 0; i < typeStrArr.Length; i++)
                {
                    if (typeStrArr[i] == genFlag.ScriptType)
                        selectIndex = i;
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Script Type" , GUILayout.Width(EditorGUIUtility.labelWidth));
            selectIndex = EditorGUILayout.Popup(selectIndex, typeStrArr);
            st.stringValue = typeStrArr[selectIndex];
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }


    }
}