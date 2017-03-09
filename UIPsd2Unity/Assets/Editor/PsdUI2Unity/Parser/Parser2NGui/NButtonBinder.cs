using UnityEngine;

namespace EditorTool.PsdExport
{
    public class NButtonBinder:ABinder
    {
#if NGUI
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            
            UIButton button = LayerWordBinder.swapComponent<UIButton>(gObj);
            NGUITools.AddWidgetCollider(gObj);

            UISprite imgBtn = gObj.GetComponent<UISprite>();
            if(imgBtn == null)
                 imgBtn = LayerWordBinder.findChildComponent<UISprite>(gObj, "imgBtn");

            if(imgBtn != null)  button.tweenTarget = imgBtn.gameObject;
        }
#endif
    }

}