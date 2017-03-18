using System;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
    public class NToggleBinder : ABinder
    {
#if NGUI
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            try
            {
                UIToggle toggleCom = LayerWordBinder.swapComponent<UIToggle>(gObj);
                toggleCom.@group = 1;

                Transform bgTrans = LayerWordBinder.findChildComponent<Transform>(gObj, "background");
                UIWidget bgWidge = bgTrans.GetComponent<UIWidget>();

                Transform imgMarkTrans = LayerWordBinder.findChildComponent<Transform>(gObj, "checkmark");
                UIWidget imgMark = LayerWordBinder.swapComponent<UIWidget>(imgMarkTrans.gameObject);
                imgMark.depth = bgWidge.depth + 1;
                imgMark.SetDimensions(bgWidge.width , bgWidge.height);

                toggleCom.activeSprite = imgMark;

                Vector3 orginPos = bgTrans.localPosition;
                NHelper.TransformOffsetParent(gObj.transform , bgTrans, orginPos);
                gObj.transform.localPosition = orginPos;

                NGUITools.AddWidgetCollider(gObj);
            }
            catch (Exception)
            {
                Debug.LogError(string.Format("[异常Toggle:{0}] 请检查是否存在（被隐藏）background组！ ", layerName));
            }

        }
#endif
    }
}