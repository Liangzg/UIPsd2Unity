using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UIHelper
{
    /// <summary>
    /// UIPrefab 结点标志
    /// </summary>
    public class UIGenFlag : MonoBehaviour
    {
        /// <summary>
        /// 用于查询的全局ID
        /// </summary>
        public string Uri;
        /// <summary>
        /// 程序代码使用的字段名称
        /// </summary>
        public string Field;
        /// <summary>
        /// 脚本类型
        /// </summary>
        public string ScriptType;

        /// <summary>
        /// 层次深度
        /// </summary>
        public int Depth = -1;
        
	    // Use this for initialization
	    void Start ()
	    {
	        
	    }
	
	    // Update is called once per frame
	    void Update () {
	
	    }

        public string initRelativeHierarchy(GameObject root)
        {
            Depth = 0;
            Transform trans = this.transform;
            List<string> buf = new List<string>();
            while (trans && !trans.gameObject.Equals(root))
            {
                buf.Add(trans.name);
                trans = trans.parent;
                Depth ++;
            }
            buf.Reverse();

            if (string.IsNullOrEmpty(ScriptType))
                ScriptType = FindWidgetTypes()[0];

            return string.Join(".", buf.ToArray());
        }


        public List<string> FindWidgetTypes()
        {
            List<string> types = new List<string>();

            UIRect[] rectArr = this.gameObject.GetComponents<UIRect>();
            foreach (UIRect rect in rectArr)
                types.Add(rect.GetType().FullName);

            UIWidgetContainer[] container = this.gameObject.GetComponents<UIWidgetContainer>();
            foreach (UIWidgetContainer widget in container)
                types.Add(widget.GetType().FullName);

            UIInput input = this.gameObject.GetComponent<UIInput>();
            if(input)   types.Add(input.GetType().FullName);

            if (types.Count <= 0)
                types.Add("Transform");
            return types;
        }

    }

}

