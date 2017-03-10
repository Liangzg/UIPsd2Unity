using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
    public class NToggleBinder : ABinder
    {
#if NGUI
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            UIToggle toggleCom = LayerWordBinder.swapComponent<UIToggle>(gObj);
            toggleCom.group = 1;
            UIButton btn = gObj.AddComponent<UIButton>();
            

            UISprite imgBtn = LayerWordBinder.findChildComponent<UISprite>(gObj, "background");
            btn.tweenTarget = imgBtn.gameObject;

            UISprite imgMark = LayerWordBinder.findChildComponent<UISprite>(gObj, "checkmark");
            toggleCom.activeSprite = imgMark;

            Vector3 orginPos = imgBtn.transform.localPosition;
            NHelper.TransformOffset(gObj.transform , orginPos , true);
            gObj.transform.localPosition = orginPos;

            NGUITools.AddWidgetCollider(gObj);
        }
#endif
    }
}