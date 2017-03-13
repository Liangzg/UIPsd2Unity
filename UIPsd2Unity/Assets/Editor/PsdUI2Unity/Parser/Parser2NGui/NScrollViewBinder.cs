using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
    public class NScrollViewBinder : ABinder
    {
#if NGUI
        public override void StartBinding(GameObject mainObj, string args, string layerName)
        {
            UIPanel panelView = LayerWordBinder.swapComponent<UIPanel>(mainObj);
            panelView.clipping = UIDrawCall.Clipping.SoftClip;
            UIPanel parentPanel = mainObj.transform.GetComponentInParent<UIPanel>();
            panelView.depth = parentPanel ? parentPanel.depth + 5 : 1;

            UIScrollView scrollview = LayerWordBinder.swapComponent<UIScrollView>(mainObj);
           
            UISprite bgRectTrans = LayerWordBinder.findChildComponent<UISprite>(mainObj , "background");
            if (bgRectTrans != null)
            {
//                panelView.baseClipRegion = new Vector4(bgRectTrans.transform.localPosition.x , bgRectTrans.transform.localPosition.y ,
//                                                       bgRectTrans.width , bgRectTrans.height);
                Vector3 ogrinPos = bgRectTrans.transform.localPosition;
                panelView.baseClipRegion = new Vector4(0 , 0 , bgRectTrans.width , bgRectTrans.height);

                //更新子结点的坐标
                NHelper.TransformOffset(panelView.transform , ogrinPos, false);
                panelView.transform.localPosition = ogrinPos;
            }

            GameObject viewportGObj = LayerWordBinder.CreateUIObject("viewport", mainObj);
            LayerWordBinder.CreateUIObject("content", viewportGObj);

            UIScrollBar hbar = LayerWordBinder.findChildComponent<UIScrollBar>(mainObj, "hbar");
            if (hbar != null)
            {
                scrollview.horizontalScrollBar = hbar;
                scrollview.movement = UIScrollView.Movement.Horizontal;
            }

            UIScrollBar vbar = LayerWordBinder.findChildComponent<UIScrollBar>(mainObj, "vbar");
            if (vbar != null)
            {
                scrollview.verticalScrollBar = hbar;
                scrollview.movement = UIScrollView.Movement.Vertical;
            }
        }


        public override void ExitBinding(GameObject g, string args, string layerName)
        {
            foreach (Transform childTrans in g.transform)
            {
                if (childTrans.name.StartsWith("item"))
                {
                    reducePosition(childTrans);
                }
                LayerWordBinder.swapComponent<UIDragScrollView>(childTrans.gameObject);
                NGUITools.AddWidgetCollider(childTrans.gameObject);
            }
        }

        /// <summary>
        /// 缩小滚动项的位置
        /// </summary>
        /// <param name="root"></param>
        private void reducePosition(Transform root)
        {
            int x = int.MaxValue;
            int y = int.MaxValue;
            foreach (Transform child in root)
            {
                Vector3 locPos = child.localPosition;
                x = locPos.x < x ? (int)locPos.x : x;
                y = locPos.y < y ? (int) locPos.y : y;
            }

            Vector3 offset = new Vector3(x , y , 0);
            foreach (Transform childTrans in root)
                childTrans.localPosition -= offset;

            root.localPosition += offset;
        }
#endif
    }
}