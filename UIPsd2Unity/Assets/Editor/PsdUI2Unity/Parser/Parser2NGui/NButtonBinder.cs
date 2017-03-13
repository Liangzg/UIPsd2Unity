using System;
using UnityEngine;

namespace EditorTool.PsdExport
{
    public class NButtonBinder:ABinder
    {
#if NGUI
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            try
            {
                //需求添加，用于导出Lua识别类型
                UIButton btn = LayerWordBinder.swapComponent<UIButton>(gObj);
                btn.enabled = false;

                UIButtonScale button = LayerWordBinder.swapComponent<UIButtonScale>(gObj);
                LayerWordBinder.swapComponent<UIEventListener>(gObj);
                button.pressed = Vector3.one * 0.8f;
                //            UIButton button = LayerWordBinder.swapComponent<UIButton>(gObj);
                //            button.pressed = Color.white;
                //            button.hover = Color.white;

                UISprite imgBtn = gObj.GetComponent<UISprite>();
                if(imgBtn == null)
                     imgBtn = LayerWordBinder.findChildComponent<UISprite>(gObj, "background");

                if (imgBtn != null)
                {
                    //button.tweenTarget = imgBtn.gameObject;
                    Vector3 originPos = imgBtn.transform.localPosition;
                    NHelper.TransformOffset(gObj.transform, imgBtn.transform.localPosition, true);
                    gObj.transform.localPosition = originPos;
                }

                NGUITools.AddWidgetCollider(gObj);
            }
            catch (Exception)
            {
                Debug.LogError(string.Format("[异常Button:{0}] 请检查是否存在（被隐藏）background组！ ", layerName));
            }

        }
#endif
    }

}