using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
    public class ScrollViewBinder : ABinder
    {
        public void StartBinding(GameObject mainObj, string args, string layerName)
        {
            ScrollRect scrollview = LayerWordBinder.swapComponent<ScrollRect>(mainObj);
            RectTransform viewRectTrans = scrollview.GetComponent<RectTransform>();
            RectTransform bgRectTrans = LayerWordBinder.findChildComponent<RectTransform>(mainObj, "background");
            viewRectTrans.anchoredPosition3D = bgRectTrans.anchoredPosition3D;
            viewRectTrans.sizeDelta = bgRectTrans.sizeDelta;

            LayerWordBinder.offsetAnchorPosition(viewRectTrans, bgRectTrans.anchoredPosition3D);


            GameObject viewportGObj = LayerWordBinder.CreateUIObject("viewport", mainObj);
            // Make viewport fill entire scroll view.
            RectTransform viewportRT = viewportGObj.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.sizeDelta = Vector2.zero;
            viewportRT.pivot = Vector2.up;

            Mask viewportMask = viewportGObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            Image viewportImage = viewportGObj.AddComponent<Image>();
            viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            viewportImage.type = Image.Type.Sliced;

            GameObject contentGObj = LayerWordBinder.CreateUIObject("content", viewportGObj);
            // Make context match viewpoprt width and be somewhat taller.
            // This will show the vertical scrollbar and not the horizontal one.
            RectTransform contentRT = contentGObj.GetComponent<RectTransform>();
            ContentSizeFitter sizeFitter = contentGObj.AddComponent<ContentSizeFitter>();

            contentRT.anchorMin = Vector2.up;
            contentRT.anchorMax = Vector2.one;
            contentRT.sizeDelta = new Vector2(0, 300);
            contentRT.pivot = Vector2.up;

            scrollview.viewport = viewportRT;
            scrollview.content = contentRT;

            Scrollbar hbar = LayerWordBinder.findChildComponent<Scrollbar>(mainObj, "hbar");
            if (hbar != null)
            {
                scrollview.horizontalScrollbar = hbar;
                scrollview.horizontal = true;
                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            Scrollbar vbar = LayerWordBinder.findChildComponent<Scrollbar>(mainObj, "vbar");
            if (vbar != null)
            {
                scrollview.verticalScrollbar = hbar;
                scrollview.vertical = true;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            RectTransform cell = LayerWordBinder.findChildComponent<RectTransform>(mainObj, "cell");
            if (cell)
            {
                Vector3 minPosition = LayerWordBinder.findMinPosition(cell, Vector3.one * float.MaxValue, hbar);
                cell.anchoredPosition3D = minPosition;
                LayerWordBinder.offsetAnchorPosition(cell, minPosition);
            }
        }
    }
}