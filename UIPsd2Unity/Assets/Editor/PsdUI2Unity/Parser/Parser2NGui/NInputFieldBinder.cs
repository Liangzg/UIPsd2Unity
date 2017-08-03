using System;
using UnityEngine;

namespace EditorTool.PsdExport
{
    public class NInputFieldBinder : ABinder
    {
#if NGUI
        public override void StartBinding(GameObject mainObj, string args, string layerName)
        {
            try
            {
                UISprite bgTrans = LayerWordBinder.findChildComponent<UISprite>(mainObj, "background" , "bg");
                bgTrans.type = UIBasicSprite.Type.Sliced;

                UILabel text = LayerWordBinder.findChildComponent<UILabel>(mainObj, "holder" , "ho");
                text.transform.localPosition -= bgTrans.transform.localPosition;
                text.pivot = UIWidget.Pivot.Left ;
             
                LayerWordBinder.NGUICopySprite(bgTrans.gameObject , mainObj , true);
                NGUITools.AddWidgetCollider(mainObj);

                UIInput inputField = LayerWordBinder.swapComponent<UIInput>(mainObj);
                inputField.label = text;
            }
            catch (Exception)
            {
                Debug.LogError(string.Format("[异常InputField:{0}] 请检查是否存在（被隐藏）background和holder组！ ", layerName));
            }

        }
#endif
    }
}