using UnityEngine;

namespace EditorTool.PsdExport
{
    public class NTempletBinder : ABinder
    {
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
#if NGUI
            GameObject templet = NTempletHelper.GetTempletPrefab(args);
            templet.name = gObj.name;
            templet.transform.SetParent(gObj.transform.parent);
            templet.transform.localPosition = gObj.transform.localPosition;
            templet.transform.localRotation = gObj.transform.localRotation;
            templet.transform.localScale = gObj.transform.localScale;

            foreach (Transform childTrans in gObj.transform)
            {
                childTrans.SetParent(templet.transform);
            }

            UISprite rootSprite = gObj.GetComponent<UISprite>();
            UIWidget destSprite = templet.GetComponent<UIWidget>();
            if (rootSprite != null && destSprite != null)
            {
                destSprite.depth = rootSprite.depth;
                foreach (Transform childTrans in templet.transform)
                {
                    UIWidget childWidget = childTrans.GetComponent<UIWidget>();
                    if (childWidget == null) return;
                    childWidget.depth += rootSprite.depth;
                }
                destSprite.width = rootSprite.width;
                destSprite.height = rootSprite.height;
            }
#endif
            
        }

        public override void ExitBinding(GameObject gObj, string args, string layerName)
        {
//            GameObject.DestroyImmediate(gObj);
        }
    }
}