using System;
using UnityEngine;

namespace EditorTool.PsdExport
{
    public class FontBinder : ABinder
    {
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            UILabel lab = gObj.GetComponent<UILabel>();
            lab.fontSize = Convert.ToInt32(args);
        }
    }
}