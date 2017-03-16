#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine.UI;

namespace UIHelper
{
    public class UIPanelRoot : MonoBehaviour
    {

        public string Field;
        /// <summary>
        /// Lua加载文件的路径
        /// </summary>
        public string LuaRequirePath;
        public string FilePath = "";
        /// <summary>
        /// 控制逻辑Ctrl名称
        /// </summary>
        public string Controller;

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

            
            UIPanelRoot[] roots = this.GetComponentsInChildren<UIPanelRoot>();
            List<UIPanelRoot> childRoots = new List<UIPanelRoot>();
            foreach (UIPanelRoot childPanel in roots)
            {
                if (childPanel.gameObject.Equals(this.gameObject)) continue;

                childRoots.Add(childPanel);
                childPanel.BuildPanel(doc, root);
            }

            List<UIGenFlag> childFlags = new List<UIGenFlag>();
            findBuildComponent(this.gameObject , childFlags);

            string templetPath = root == this.gameObject
                                ? ToolConst.LuaPanelTempletPath
                                : ToolConst.LuaSubPanelTempletPath;
            this.writeScriptFile(childRoots ,childFlags , templetPath);

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

        private void writeScriptFile(List<UIPanelRoot> panelRoots, List<UIGenFlag> flags , string templetPath)
        {
            string path = Path.Combine(Application.dataPath, this.FilePath);
            string folder = Path.GetDirectoryName(path);

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string tempPath = Path.Combine(Application.dataPath, templetPath);
            Dictionary<string, string> localWidgets = new Dictionary<string, string>();
            string fileText = readLocalFile(path , localWidgets);

            StringBuilder buf = new StringBuilder();
            string src = "src=";
            string pathFormat = ",path=";
            foreach (UIPanelRoot childPanel in panelRoots)
            {
                string formatPanel = string.Format("\t\t{{field=\"{0}\",path=\"{1}\",src=LuaScript , requirePath=\"{2}\"}},",
                             string.IsNullOrEmpty(childPanel.Field) ? childPanel.gameObject.name : childPanel.Field,
                             childPanel.relativePath,
                             string.IsNullOrEmpty(childPanel.LuaRequirePath) ? childPanel.FilePath : childPanel.LuaRequirePath);
                string[] formatArr = formatPanel.Split(new[] { pathFormat }, StringSplitOptions.None);
                if (localWidgets.ContainsKey(formatArr[0])) localWidgets.Remove(formatArr[0]);

                buf.AppendLine(formatPanel);
            }

                      
            foreach (UIGenFlag flag in flags)
            {
                string format = formatExport(flag);
                string[] formatArr = format.Split(new[] {pathFormat}, StringSplitOptions.None);
                if (localWidgets.ContainsKey(formatArr[0]))
                {
                    string value = localWidgets[formatArr[0]];
                    if (flag.ScriptType == typeof(UIButton).FullName)
                    {
                        format = this.replace(format, value, "onClick");
                    }else if (flag.ScriptType == typeof (UIToggle).FullName)
                    {
                        format = this.replace(format, value, "onChange");
                    }else if (flag.ScriptType == typeof (UIInput).FullName)
                    {
                        format = this.replace(format, value, "onChange");
                        format = this.replace(format, value, "onSubmit");
                    }
                    localWidgets.Remove(formatArr[0]);
                }
                buf.AppendLine(format);
            }

            if (localWidgets.Count > 0)
            {
                buf.AppendLine("\t\t---custom extendsion");
                foreach (string value in localWidgets.Values)
                    buf.AppendLine(value);
            }

            if (string.IsNullOrEmpty(fileText))
            {
                fileText = File.ReadAllText(tempPath);
            }
            fileText = fileText.Replace("#SCRIPTNAME#", Path.GetFileNameWithoutExtension(path));
            fileText = fileText.Replace("#WIDGETS#", buf.ToString());
            if (!string.IsNullOrEmpty(this.Controller))
                fileText = fileText.Replace("#SCRIPTCTRL#", this.Controller);

            File.WriteAllText(path , fileText);
        }

        private string replace(string src, string dest, string key)
        {
            int si = src.IndexOf(key);
            int di = dest.IndexOf(key);
            if (si < 0 || di < 0) return src;

            int endSi = src.IndexOf(",", si);
            string srcChunk = src.Substring(si + 1, endSi - si - 2);
            
            int endDi = dest.IndexOf(",", di);
            string destChunk = dest.Substring(di + 1, endDi - di - 2);

            return src.Replace(srcChunk, destChunk);
        }

        /// <summary>
        /// 读取本地已存在的文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string readLocalFile(string filePath , Dictionary<string, string> localWidgets)
        {            
            Dictionary<string , string> localFile = new Dictionary<string, string>();
            if (File.Exists(filePath))
            {
                string fileText = File.ReadAllText(filePath);
                fileText = fileText.Replace("\r\n", "\n");
                string[] fileLinearArr = fileText.Split('\n');
                bool start = false;
                string startWidgets = "widgets = {";
                string endStr = "}";
                string path = ",path=";
                int startIndex = 0;
                int endIndex = 0;

                for (int i = 0; i < fileLinearArr.Length; i++)
                {
                    if (fileLinearArr[i].Contains(startWidgets))
                    {
                        startIndex = i;
                        start = true;
                    }
                    if (start && fileLinearArr[i].Trim().Equals(endStr))
                    {
                        endIndex = i;
                        break;
                    }
                    
                    if (fileLinearArr[i].Contains(path))
                    {
                        string[] kv = fileLinearArr[i].Split(new []{ path } , StringSplitOptions.None);
                        localFile[kv[0]] = fileLinearArr[i];                        
                    }
                }


                string[] newText = new string[fileLinearArr.Length - (endIndex - startIndex) + 2];
                Array.Copy(fileLinearArr ,  0 , newText, 0 , startIndex + 1);
                Array.Copy(fileLinearArr , endIndex , newText , startIndex + 2, fileLinearArr.Length - endIndex);
                newText[startIndex + 1] = "#WIDGETS#";
                return string.Join("\r\n" , newText);
            }
            return null;
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
                buf.Append("src=LuaButton, onClick = function (gObj)  --[===[todo click]===]  end ");

            }else if (genFlag.ScriptType == typeof (UIToggle).FullName)
            {
                buf.Append("src=LuaToggle , onChange = function (toggle) --[===[todo toggle.onchange]===] end");
            }else if (genFlag.ScriptType == typeof (UIPanel).FullName)
            {
                buf.Append("src=LuaPanel");
            }else if (genFlag.ScriptType == typeof (UIInput).FullName)
            {
                buf.Append("src=LuaInput, onChange = function (input) --[===[todo input change]===]  end , onSubmit = function (input) --[===[todo input onSubmit]===]  end");
            }else 
            {
                buf.AppendFormat("src=\"{0}\"", genFlag.ScriptType);
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
            ele.SetAttribute("field", this.Field);
            ele.SetAttribute("filePath", this.FilePath);
            ele.SetAttribute("luaRequirePath", this.LuaRequirePath);
            ele.SetAttribute("hierarchy", string.IsNullOrEmpty(relativePath) ? this.gameObject.name : relativePath);
            if (!string.IsNullOrEmpty(this.Controller))         ele.SetAttribute("ctrlName", this.Controller);  

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
            this.Field = ele.GetAttribute("field");
            this.FilePath = ele.GetAttribute("filePath");
            this.LuaRequirePath = ele.GetAttribute("luaRequirePath");
            if (string.IsNullOrEmpty(LuaRequirePath))
            {
                LuaRequirePath = this.FilePath.Replace(".lua", "").Replace("/", ".");
            }
            this.Controller = ele.GetAttribute("ctrlName");

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
