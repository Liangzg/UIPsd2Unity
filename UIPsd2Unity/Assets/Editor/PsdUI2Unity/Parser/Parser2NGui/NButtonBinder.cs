using UnityEngine;

namespace EditorTool.PsdExport
{
    public class NButtonBinder:ABinder
    {
#if NGUI
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            
            UIButton button = LayerWordBinder.swapComponent<UIButton>(gObj);
            

            UISprite imgBtn = gObj.GetComponent<UISprite>();
            if(imgBtn == null)
                 imgBtn = LayerWordBinder.findChildComponent<UISprite>(gObj, "background");

            if (imgBtn != null)
            {
                button.tweenTarget = imgBtn.gameObject;
                Vector3 originPos = imgBtn.transform.localPosition;
                NHelper.TransformOffset(gObj.transform, imgBtn.transform.localPosition, true);
                gObj.transform.localPosition = originPos;
            }

            NGUITools.AddWidgetCollider(gObj);
        }
#endif
    }

}