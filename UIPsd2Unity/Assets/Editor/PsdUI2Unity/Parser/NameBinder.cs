using System.Collections.Generic;
using UnityEngine;

namespace EditorTool.PsdExport
{
    public class NameBinder : ABinder
    {
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            if (args == "_root_")
            {
#if NGUI
                UIPanel rootPanel  = LayerWordBinder.swapComponent<UIPanel>(gObj);
                rootPanel.depth = 1;
                return;
#endif
            }
            gObj.name = args;
        }

        public override void ExitBinding(GameObject g, string args, string layerName)
        {
            //添加同级名称限制
            renameSame(g);
        }

        /// <summary>
        /// 重命名同级结点中相同的结点
        /// </summary>
        /// <param name="g"></param>
        private void renameSame(GameObject g)
        {
            Dictionary<string , int> nameMap = new Dictionary<string, int>();
            for (int i = 0 , max = g.transform.childCount; i < max; i++)
            {
                Transform childTrans = g.transform.GetChild(i);
                string childName = childTrans.name;
                if (!nameMap.ContainsKey(childName))
                    nameMap[childName] = 0;
                else
                {
                    nameMap[childName] = nameMap[childName] + 1;
                    childTrans.name = childName + nameMap[childName];
                }

                if(childTrans.childCount > 0)
                    renameSame(childTrans.gameObject);
            }
        }
    }
}