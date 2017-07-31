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
            

            if (argArr.Length == 1) return;

            for (int i = 1; i < argArr.Length; i++)
            {
                string[] paramArr = argArr[i].Split(':');
                if (paramArr[0] == "ol")
                {
                    lab.effectStyle = UILabel.Effect.Outline;
                    lab.effectColor = NGUIText.ParseColor(paramArr[1], 0);
                }else if (paramArr[0] == "sd")
                {
                    lab.effectStyle = UILabel.Effect.Shadow;
                    lab.effectColor = NGUIText.ParseColor(paramArr[1], 0);
                }
            }
        }



    }
}