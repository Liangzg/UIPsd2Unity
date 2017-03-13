#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace UIHelper
{
    public class UIPanelRoot : MonoBehaviour
    {

        public string ScriptName;
        /// <summary>
        /// Lua加载文件的路径
        /// </summary>
        public string LuaRequirePath;
        public string FilePath = "";

        public int Depth = -1;
        public GameObject RelactiveRoot;
        public string relativePath;

        public string initRelativeHierarchy(GameObject root)
        {
            Depth = 0;
            Transform trans = this.transform;
            List<string> buf = new List<string>();
            while (trans && !trans.gameObject.Equals(root))
            {
                buf.Add(trans.name);
                trans = trans.parent;
                Depth++;
            }
            buf.Reverse();
            relativePath = string.Join(".", buf.ToArray());
            return relativePath;
        }

        public void BuildPanel(XmlDocument doc , GameObject root)
        {
            this.initRelativeHierarchy(root);

            if (string.IsNullOrEmpty(FilePath))
            {
                Debug.LogError("请先配置PanelRoot导出的脚本文件路径! Hierarchy:" + relativePath);
                return;
            }

            StringBuilder buf = new StringBuilder();
            UIPanelRoot[] roots = this.GetComponentsInChildren<UIPanelRoot>();
            List<UIPanelRoot> childRoots = new List<UIPanelRoot>();
            foreach (UIPanelRoot childPanel in roots)
            {
                if (childPanel.gameObject.Equals(this.gameObject))  continue;
                
                childRoots.Add(childPanel);
                childPanel.BuildPanel(doc , root);

                buf.AppendFormat("\t\t{{field=\"{0}\",path=\"{1}\",src = LuaScript}},\n",
                             string.IsNullOrEmpty(childPanel.LuaRequirePath) ? childPanel.FilePath : childPanel.LuaRequirePath,
                             childPanel.relativePath);
            }
            
            List<UIGenFlag> childFlags = new List<UIGenFlag>();
            findBuildComponent(this.gameObject , childFlags);
            
            this.writeScriptFile(buf , childFlags);

#if UNITY_EDITOR
            //保存记录
            this.serializePanelRoot(doc , childFlags);
#endif
        }


        private void findBuildComponent(GameObject gObj, List<UIGenFlag> childFlags)
        {
            UIGenFlag flag = gObj.GetComponent<UIGenFlag>();
            if (flag)   childFlags.Add(flag);

            foreach (Transform childTrans in gObj.transform)
            {
                UIPanelRoot subRoot = childTrans.GetComponent<UIPanelRoot>();
                if (subRoot)    continue;

                if (childTrans.childCount > 0)
                    findBuildComponent(childTrans.gameObject , childFlags);
                else
                {
                    flag = childTrans.GetComponent<UIGenFlag>();
                    if (flag) childFlags.Add(flag);
                }
            }
        }

        private void writeScriptFile(StringBuilder buf , List<UIGenFlag> flags)
        {
            foreach (UIGenFlag flag in flags)
                buf.AppendLine(formatExport(flag));

            string path = Path.Combine(Application.dataPath, this.FilePath);
            string folder = Path.GetDirectoryName(path);
  
            if (!Directory.Exists(folder))  Directory.CreateDirectory(folder);

            string tempPath = Path.Combine(Application.dataPath, ToolConst.LuaPanelTempletPath);
            string fileText = File.ReadAllText(tempPath);

            fileText = fileText.Replace("#SCRIPTNAME#", Path.GetFileNameWithoutExtension(path));
            fileText = fileText.Replace("#WIDGETS#", buf.ToString());

            File.WriteAllText(path , fileText);
        }

      
        private string formatExport(UIGenFlag genFlag)
        {
            StringBuilder buf = new StringBuilder();
            buf.AppendFormat("\t\t{{field=\"{0}\",path=\"{1}\",", 
                             string.IsNullOrEmpty(genFlag.Field) ? genFlag.gameObject.name : genFlag.Field,
                             genFlag.initRelativeHierarchy(this.gameObject) );

            if (genFlag.ScriptType == typeof(UILabel).FullName)
            {
                buf.Append("src=LuaText");
            }else if (genFlag.ScriptType == typeof(UISprite).FullName)
            {
                buf.Append("src=LuaImage");
            }else if (genFlag.ScriptType == typeof(UIButton).FullName)
            {
                buf.Append("src=LuaButton, onClick = function ()  --[===[todo click]===]  end ");

            }else if (genFlag.ScriptType == typeof (UIToggle).FullName)
            {
                buf.Append("src=LuaToggle , onChange = function () --[===[todo toggle.onchange]===] end");
            }else if (genFlag.ScriptType == typeof (UIPanel).FullName)
            {
                buf.Append("src=LuaPanel");
            }else if (genFlag.ScriptType == typeof (UIInput).FullName)
            {
                buf.Append("src=LuaInput, onChange = function () --[===[todo input change]===]  end , onSubmit = function () --[===[todo input onSubmit]===]  end");
            }else 
            {
                buf.AppendFormat("src=\"{0}\",", genFlag.ScriptType);
            }
            buf.Append("},");

            return buf.ToString();
        }

        /// <summary>
        /// 序列化UIPanelRoot的配置数据
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public void serializePanelRoot(XmlDocument doc , List<UIGenFlag> flags)
        {
            XmlNode rootNodes = doc.SelectSingleNode("panelRoots");
            if (rootNodes == null)
            {
                rootNodes = doc.CreateElement("panelRoots");
                doc.AppendChild(rootNodes);
            }
            XmlElement ele = doc.CreateElement("panelRoot");
            ele.SetAttribute("filePath", this.FilePath);
            ele.SetAttribute("hierarchy", string.IsNullOrEmpty(relativePath) ? this.gameObject.name : relativePath);
            
            foreach (UIGenFlag flag in flags)
            {
                XmlElement flagEle = doc.CreateElement("flag");
                flag.serializeFlag(flagEle);
                ele.AppendChild(flagEle);
            }
            rootNodes.AppendChild(ele);
        }

        /// <summary>
        /// 反序列化记录数据
        /// </summary>
        /// <param name="doc"></param>
        public void deserializePanelRoot(XmlElement ele)
        {
            this.FilePath = ele.GetAttribute("filePath");

            foreach (XmlElement childEle in ele.ChildNodes)
            {
                string hierarchy = childEle.GetAttribute("hierarchy");
                hierarchy = hierarchy.Replace(".", "/");
                Transform trans = this.transform.FindChild(hierarchy);
                if (trans == null)  continue;

                UIGenFlag flag = trans.GetComponent<UIGenFlag>();
                if (flag == null)
                    flag = trans.gameObject.AddComponent<UIGenFlag>();
                flag.deserializeFlag(childEle);
            }
        }

    }

}

#endif
