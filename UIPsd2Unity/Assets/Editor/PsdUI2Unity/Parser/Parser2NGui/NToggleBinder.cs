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
            NGUITools.AddWidgetCollider(gObj);

            UISprite imgBtn = LayerWordBinder.findChildComponent<UISprite>(gObj, "background");
            btn.tweenTarget = imgBtn.gameObject;

            UISprite imgMark = LayerWordBinder.findChildComponent<UISprite>(gObj, "checkmark");
            toggleCom.activeSprite = imgMark;
        }
#endif
    }
}