using UnityEngine;
using UnityEngine.UI;

namespace subjectnerdagreement.psdexport
{
    public class SliderBinder : ABinder
    {
        public void StartBinding(GameObject mainObj, string args, string layerName)
        {
            Slider slider = LayerWordBinder.swapComponent<Slider>(mainObj);

            Image imgBg = LayerWordBinder.findChildComponent<Image>(mainObj, "background");
            imgBg.type = Image.Type.Sliced;
            slider.targetGraphic = imgBg;

            RectTransform sliderBgTrans = slider.targetGraphic.GetComponent<RectTransform>();
            RectTransform mainRectTrans = mainObj.GetComponent<RectTransform>();
            mainRectTrans.sizeDelta = sliderBgTrans.sizeDelta;
            mainRectTrans.anchoredPosition3D = sliderBgTrans.anchoredPosition3D;

            sliderBgTrans.anchorMin = new Vector2(0, 0.25f);
            sliderBgTrans.anchorMax = new Vector2(1, 0.75f);
            sliderBgTrans.sizeDelta = Vector2.zero;
            sliderBgTrans.anchoredPosition3D = Vector3.zero;

            slider.fillRect = LayerWordBinder.findChildComponent<RectTransform>(mainObj, "fill");
            if (slider.fillRect)
            {
                GameObject fillRoot = LayerWordBinder.CreateUIObject("fillArea", mainObj);
                fillRoot.transform.localScale = Vector3.one;
                fillRoot.transform.localPosition = Vector3.zero;
                RectTransform rectTrans = fillRoot.GetComponent<RectTransform>();
                rectTrans.anchorMin = new Vector2(0, 0.25f);
                rectTrans.anchorMax = new Vector2(1, 0.75f);
                rectTrans.pivot = Vector2.one * 0.5f;
                rectTrans.sizeDelta = new Vector2(-20, 0);

                slider.fillRect.transform.SetParent(fillRoot.transform);
                slider.fillRect.anchoredPosition = Vector3.zero;
                Image fillImage = slider.fillRect.GetComponent<Image>();
                fillImage.type = Image.Type.Sliced;

                slider.fillRect.anchorMax = new Vector2(0, 1);
                slider.fillRect.sizeDelta = new Vector2(10, 0);
            }

            slider.handleRect = LayerWordBinder.findChildComponent<RectTransform>(mainObj, "handle");
            if (slider.handleRect)
            {
                GameObject handleArea = LayerWordBinder.CreateUIObject("HandleSlideArea", mainObj);
                handleArea.transform.localScale = Vector3.one;
                handleArea.transform.localPosition = Vector3.zero;

                RectTransform rectTrans = handleArea.GetComponent<RectTransform>();
                rectTrans.anchorMin = Vector2.zero;
                rectTrans.anchorMax = Vector2.one;
                rectTrans.pivot = Vector2.one * 0.5f;
                rectTrans.sizeDelta = new Vector2(-20, 0);

                slider.handleRect.transform.SetParent(handleArea.transform);
                slider.handleRect.anchoredPosition = Vector3.zero;
                slider.handleRect.anchorMax = new Vector2(0, 1);
                slider.handleRect.sizeDelta = new Vector2(20, 0);
            }
        }

    }
}