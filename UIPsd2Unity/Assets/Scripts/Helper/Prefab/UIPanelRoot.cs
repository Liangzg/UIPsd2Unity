using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Assets.Editor.UI;

namespace UIHelper
{
    public class UIPanelRoot : MonoBehaviour
    {

        public string ScriptName;

        public string FilePath;

        public int Depth = -1;
        public GameObject RelactiveRoot;
        public string relativePath;


	    // Use this for initialization
	    void Start () {
	
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
                Depth++;
            }
            buf.Reverse();
            relativePath = string.Join(".", buf.ToArray());
            return relativePath;
        }

        public void BuildPanel(GameObject root)
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
                childPanel.BuildPanel(root);

                buf.AppendFormat("\t\t{{field=\"{0}\",path=\"{1}\",src = LuaScript}},\n",
                             string.IsNullOrEmpty(childPanel.ScriptName) ? childPanel.gameObject.name : childPanel.ScriptName,
                             childPanel.relativePath);
            }
            
            UIGenFlag[] genFlags = this.GetComponentsInChildren<UIGenFlag>();
            
            foreach (UIGenFlag gen in genFlags)
            {
                gen.initRelativeHierarchy(this.RelactiveRoot);
                bool canGen = true;
                foreach (UIPanelRoot childPanel in childRoots)
                {
                    if (gen.Depth > childPanel.Depth)
                    {
                        canGen = false;
                        break;
                    }
                }
                if (canGen)
                    buf.AppendLine(formatExport(gen));
            }
            
            this.writeScriptFile(buf.ToString());
        }


        private void writeScriptFile(string viewPanel)
        {
            string path = Path.Combine(Application.dataPath, this.FilePath);
            string folder = Path.GetDirectoryName(path);
  
            if (!Directory.Exists(folder))  Directory.CreateDirectory(folder);

            string tempPath = Path.Combine(Application.dataPath, ToolConst.LuaPanelTempletPath);
            string fileText = File.ReadAllText(tempPath);

            fileText = fileText.Replace("#SCRIPTNAME#", Path.GetFileNameWithoutExtension(path));
            fileText = fileText.Replace("#WIDGETS#", viewPanel);

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

    }

}
