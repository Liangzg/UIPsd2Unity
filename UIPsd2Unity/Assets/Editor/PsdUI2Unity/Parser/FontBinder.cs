using System;
using UnityEngine;

namespace EditorTool.PsdExport
{
    public class FontBinder : ABinder
    {
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            UILabel lab = gObj.GetComponent<UILabel>();
            string[] argArr = args.Split(',');
            try
            {
                lab.fontSize = Convert.ToInt32(argArr[0]); //fontSize
            }
            catch (Exception)
            {
                Debug.LogError(layerName);
                throw;
            }
        }



    }
}