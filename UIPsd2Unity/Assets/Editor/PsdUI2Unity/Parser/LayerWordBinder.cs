////////////////////////////////////////////////
/// auth: liangzg
/// mail: game.liangzg@foxmail.com
///////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
    /// <summary>
    /// 词法绑定 
    /// </summary>
    public class LayerWordBinder
    {
        private static Dictionary<string , IBinding> importParser = new Dictionary<string, IBinding>();

        public static void registWord()
        {
            // 参数绑定解析
            importParser["name"] = new NameBinder();
            importParser["img"] = new ImageBinder();
            importParser["ignore"] = new IgnoreBinder();
            importParser["font"] = new FontBinder();
            // 脚本组件
            if (PsdSetting.Instance.curGUIType == GUIType.UGUI)
            {
                importParser["button"] = new ButtonBinder();
                importParser["toggle"] = new ToggleBinder();
                importParser["slider"] = new SliderBinder();
                importParser["scrollview"] = new ScrollViewBinder();
                importParser["scrollbar"] = new ScrollbarBinder();
                importParser["inputfield"] = new InputFieldBinder();  
                              
            }else if (PsdSetting.Instance.curGUIType == GUIType.NGUI)
            {
                importParser["button"] = new NButtonBinder();
                importParser["toggle"] = new NToggleBinder();
                importParser["slider"] = new NSliderBinder();
                importParser["scrollview"] = new NScrollViewBinder();
                importParser["scrollbar"] = new NScrollbarBinder();
                importParser["inputfield"] = new NInputFieldBinder();
                importParser["texture"] = new NTextureBinder();
                importParser["tmpt"] = new NTempletBinder();
            }

#if NGUI
            NAtlasHelper.LoadAllAtlas(PsdSetting.Instance.AtlasImportFolder);
#endif
            NTempletHelper.LoadAllTemplet(PsdSetting.Instance.TempletImportFolder);
        }


        public static IBinding GetParser(string key)
        {
            key = key.ToLower();
            if (importParser.ContainsKey(key)) return importParser[key];
            return null;
        }


        public static T swapComponent<T>(GameObject gObj) where T : Component
        {
            T instance = gObj.GetComponent<T>();
            if (instance == null)
            {
                instance = gObj.AddComponent<T>();
            }
            return instance;
        }

        public static T findChildComponent<T>(GameObject gObj, string hierarchy)
            where T : Component
        {
            Transform destTrans = null;
            foreach (Transform childTrans in gObj.transform)
            {
                if (childTrans.name.StartsWith(hierarchy))
                {
                    destTrans = childTrans;
                    break;
                }
            }

            //平级没有，才查找子结点
            if (destTrans == null)
            {
                foreach (Transform childTrans in gObj.transform)
                {
                   if (childTrans.childCount > 0)
                    {
                        T childCom = findChildComponent<T>(childTrans.gameObject, hierarchy);
                        if (childCom) return childCom;
                    }
                }
                return null;
            }

            return destTrans.GetComponent<T>();
        }

        public static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
#if !NGUI
            go.AddComponent<RectTransform>();
#endif
            SetParentAndAlign(go, parent);
            return go;
        }
        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        public static void copyRectTransform(GameObject src, GameObject dest , bool copyImage = false)
        {
            RectTransform srt = src.GetComponent<RectTransform>();
            RectTransform drt = dest.GetComponent<RectTransform>();

            drt.anchoredPosition = srt.anchoredPosition;
            drt.anchorMin = srt.anchorMin;
            drt.anchorMax = srt.anchorMax;
            drt.pivot = srt.pivot;

            drt.localScale = Vector3.one;

            if (copyImage)
            {
                Image imgRoot = src.GetComponent<Image>();
                Image destImg = dest.AddComponent<Image>();
                destImg.sprite = imgRoot.sprite;
                destImg.SetNativeSize();

                GameObject.DestroyImmediate(src);                
            }
        }


#if NGUI
        public static void NGUICopySprite(GameObject src, GameObject dest, bool copyImage = false)
        {
            dest.transform.localPosition = src.transform.localPosition;

            if (copyImage)
            {
                UISprite imgRoot = src.GetComponent<UISprite>();
                UISprite destImg = dest.AddComponent<UISprite>();

                destImg.atlas = imgRoot.atlas;
                destImg.spriteName = imgRoot.spriteName;
                destImg.type = imgRoot.type;
                destImg.depth = imgRoot.depth;

                destImg.MakePixelPerfect();

                GameObject.DestroyImmediate(src);
            }
        }
#endif

        public static void offsetAnchorPosition(RectTransform rootTrans , Vector3 offset)
        {
            foreach (RectTransform childTrans in rootTrans)
            {
                if (childTrans.childCount > 0)
                    offsetAnchorPosition(childTrans , offset);
                childTrans.anchoredPosition3D -= offset;
            }
        }

        public static Vector3 findMinPosition(RectTransform rootTrans , Vector3 minPosition , bool isY)
        {
            foreach (RectTransform childTrans in rootTrans)
            {
                if (isY)
                {
                    if (childTrans.anchoredPosition3D.y < minPosition.y)
                        minPosition = childTrans.anchoredPosition3D;
                }
                else
                {
                    if (childTrans.anchoredPosition3D.x < minPosition.x)
                        minPosition = childTrans.anchoredPosition3D;
                }
            }

            return minPosition;
        }
    }



}
