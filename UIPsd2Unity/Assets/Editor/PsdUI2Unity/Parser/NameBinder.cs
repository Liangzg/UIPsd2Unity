using UnityEngine;

namespace EditorTool.PsdExport
{
    public class NameBinder : ABinder
    {
        public override void StartBinding(GameObject gObj, string args, string layerName)
        {
            if (args == "_root_")
            {
#if NGUI
                UIPanel rootPanel  = LayerWordBinder.swapComponent<UIPanel>(gObj);
                rootPanel.depth = 1;
                return;
#endif
            }
            gObj.name = args;
        }

    }
}