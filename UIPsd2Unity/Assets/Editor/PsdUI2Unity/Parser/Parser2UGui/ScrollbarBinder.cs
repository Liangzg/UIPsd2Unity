using UnityEngine;
using UnityEngine.UI;

namespace subjectnerdagreement.psdexport
{
    public class ScrollbarBinder : ABinder
    {
        public void StartBinding(GameObject mainObj, string args, string layerName)
        {
            Image oldImg = LayerWordBinder.findChildComponent<Image>(mainObj, "background");
            LayerWordBinder.copyRectTransform(oldImg.gameObject, mainObj, true);

            GameObject sliderArea = LayerWordBinder.CreateUIObject("Sliding Area", mainObj);
            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
            sliderAreaRect.sizeDelta = new Vector2(-20, -20);
            sliderAreaRect.anchorMin = Vector2.zero;
            sliderAreaRect.anchorMax = Vector2.one;

            RectTransform handleRect = LayerWordBinder.findChildComponent<RectTransform>(mainObj, "handle");
            handleRect.transform.SetParent(sliderArea.transform);
            handleRect.sizeDelta = new Vector2(20, 20);
            handleRect.anchoredPosition = Vector2.zero;

            Scrollbar scrollbar = LayerWordBinder.swapComponent<Scrollbar>(mainObj);
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleRect.GetComponent<Image>();
        }

    }
}