using UnityEngine;
using UnityEngine.UI;

namespace subjectnerdagreement.psdexport
{
    public class NScrollbarBinder : ABinder
    {
#if NGUI
        public override void StartBinding(GameObject mainObj, string args, string layerName)
        {
            UISprite background = LayerWordBinder.findChildComponent<UISprite>(mainObj, "background");
            background.type = UIBasicSprite.Type.Sliced;

            UISprite foreground = LayerWordBinder.findChildComponent<UISprite>(mainObj, "handle");
            foreground.transform.localPosition = Vector3.zero;
            foreground.width = background.width - 10;
            foreground.height = background.height - 10;
            foreground.type = UIBasicSprite.Type.Sliced;
            NGUITools.AddWidgetCollider(foreground.gameObject);

            UIScrollBar scrollbar = LayerWordBinder.swapComponent<UIScrollBar>(mainObj);
            LayerWordBinder.NGUICopySprite(background.gameObject , mainObj , true);
            NGUITools.AddWidgetCollider(mainObj);

            scrollbar.backgroundWidget = mainObj.GetComponent<UISprite>();
            scrollbar.foregroundWidget = foreground;
            scrollbar.value = 0.1f;
            scrollbar.barSize = 0.2f;
        }
#endif
    }
}