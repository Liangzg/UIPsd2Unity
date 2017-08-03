using System;
using UnityEngine;

namespace EditorTool.PsdExport
{
    public class NScrollViewBinder : ABinder
    {
#if NGUI
        Vector3 ogrinPos = Vector3.zero;
        public override void StartBinding(GameObject mainObj, string args, string layerName)
        {
            try
            {
                UIPanel panelView = LayerWordBinder.swapComponent<UIPanel>(mainObj);
                panelView.clipping = UIDrawCall.Clipping.SoftClip;
                UIPanel parentPanel = mainObj.transform.GetComponentInParent<UIPanel>();
                panelView.depth = parentPanel ? parentPanel.depth + 5 : 1;

                UIScrollView scrollview = LayerWordBinder.swapComponent<UIScrollView>(mainObj);
                ogrinPos = Vector3.zero;
                UISprite bgRectTrans = LayerWordBinder.findChildComponent<UISprite>(mainObj , "background" , "bg");
                if (bgRectTrans != null)
                {
    //                panelView.baseClipRegion = new Vector4(bgRectTrans.transform.localPosition.x , bgRectTrans.transform.localPosition.y ,
    //                                                       bgRectTrans.width , bgRectTrans.height);
                   
                    panelView.baseClipRegion = new Vector4(0 , 0 , bgRectTrans.width , bgRectTrans.height);

                    ogrinPos = bgRectTrans.transform.localPosition;
                }

                GameObject viewportGObj = LayerWordBinder.CreateUIObject("viewport", mainObj);
                //LayerWordBinder.CreateUIObject("content", viewportGObj);

                UIScrollBar hbar = LayerWordBinder.findChildComponent<UIScrollBar>(mainObj, "hbar" , "hb");
                if (hbar != null)
                {
                    scrollview.horizontalScrollBar = hbar;
                    scrollview.movement = UIScrollView.Movement.Horizontal;
                }

                UIScrollBar vbar = LayerWordBinder.findChildComponent<UIScrollBar>(mainObj, "vbar" , "vb");
                if (vbar != null)
                {
                    scrollview.verticalScrollBar = hbar;
                    scrollview.movement = UIScrollView.Movement.Vertical;
                }
            }
            catch (Exception)
            {
                Debug.LogError(string.Format("[异常ScrollView:{0}] 请检查是否存在（被隐藏）background组！ ", layerName));
            }

        }


        public override void ExitBinding(GameObject g, string args, string layerName)
        {
            foreach (Transform childTrans in g.transform)
            {
                if (childTrans.name.StartsWith("item"))
                {
                    reducePosition(childTrans);
                    LayerWordBinder.swapComponent<UIDragScrollView>(childTrans.gameObject);
                    NGUITools.AddWidgetCollider(childTrans.gameObject);
                }

            }

            //更新子结点的坐标
            NHelper.TransformOffset(g.transform, ogrinPos, false);
            g.transform.localPosition = ogrinPos;
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
                x = Mathf.Abs(locPos.x) < x ? (int)locPos.x : x;
                y = Mathf.Abs(locPos.y) < y ? (int)locPos.y : y;
            }

            Vector3 offset = new Vector3(x , y , 0);
            foreach (Transform childTrans in root)
                childTrans.localPosition -= offset;

            root.localPosition += offset;
        }
#endif
    }
}