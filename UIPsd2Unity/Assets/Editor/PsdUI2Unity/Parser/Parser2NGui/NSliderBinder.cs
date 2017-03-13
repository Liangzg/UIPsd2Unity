using System;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
    public class NSliderBinder : ABinder
    {
#if NGUI
        public override void StartBinding(GameObject mainObj, string args, string layerName)
        {
            try
            {
                UISlider slider = LayerWordBinder.swapComponent<UISlider>(mainObj);

                UISprite imgBg = LayerWordBinder.findChildComponent<UISprite>(mainObj, "background");
                imgBg.type = UIBasicSprite.Type.Sliced;

                UISprite imgFill = LayerWordBinder.findChildComponent<UISprite>(mainObj, "fill");
                imgFill.transform.localPosition -= imgBg.transform.localPosition;
                imgFill.type = UIBasicSprite.Type.Sliced;
                slider.foregroundWidget = imgFill;

                UISprite imgHandle = LayerWordBinder.findChildComponent<UISprite>(mainObj, "handle");
                if (imgHandle != null)
                {
                    slider.thumb = imgHandle.transform;
                    slider.thumb.localPosition -= imgBg.transform.localPosition;
                    imgFill.width = imgBg.width - imgHandle.width;
                }
                

                LayerWordBinder.NGUICopySprite(imgBg.gameObject, mainObj, true);
                slider.backgroundWidget = mainObj.GetComponent<UIWidget>();
                slider.value = 0.2f;
                NGUITools.AddWidgetCollider(mainObj);
            }
            catch (Exception)
            {
                Debug.LogError(string.Format("[异常Slider:{0}] 请检查是否存在（被隐藏）background和fill组！ ", layerName));
            }

        }
#endif
    }
}