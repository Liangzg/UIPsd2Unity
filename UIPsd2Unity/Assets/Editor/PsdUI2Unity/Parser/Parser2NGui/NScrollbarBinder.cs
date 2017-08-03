using System;
using UnityEngine;

namespace EditorTool.PsdExport
{
    public class NScrollbarBinder : ABinder
    {
#if NGUI
        public override void StartBinding(GameObject mainObj, string args, string layerName)
        {
            try
            {
                UISprite background = LayerWordBinder.findChildComponent<UISprite>(mainObj, "background" , "bg");
                background.type = UIBasicSprite.Type.Sliced;

                UISprite foreground = LayerWordBinder.findChildComponent<UISprite>(mainObj, "handle" , "ha");
                foreground.transform.localPosition = Vector3.zero;
                foreground.width = background.width - 10;
                foreground.height = background.height - 10;
                foreground.type = UIBasicSprite.Type.Sliced;
            

                UIScrollBar scrollbar = LayerWordBinder.swapComponent<UIScrollBar>(mainObj);
                LayerWordBinder.NGUICopySprite(background.gameObject , mainObj , true);
                NGUITools.AddWidgetCollider(mainObj);

                scrollbar.backgroundWidget = mainObj.GetComponent<UISprite>();
                scrollbar.foregroundWidget = foreground;
                scrollbar.value = 0.1f;
                scrollbar.barSize = 0.2f;

                NGUITools.AddWidgetCollider(foreground.gameObject);
            }
            catch (Exception)
            {
                Debug.LogError(string.Format("[异常Scrollbar:{0}] 请检查是否存在（被隐藏）background和handle组！ ", layerName));
            }

        }
#endif
    }
}