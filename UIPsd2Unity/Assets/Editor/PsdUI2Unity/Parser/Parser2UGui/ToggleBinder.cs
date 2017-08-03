using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
    public class ToggleBinder : ABinder
    {
        public void StartBinding(GameObject gObj, string args, string layerName)
        {
            Toggle toggle = LayerWordBinder.swapComponent<Toggle>(gObj);

            Image imgBtn = LayerWordBinder.findChildComponent<Image>(gObj, "background" , "bg");
            toggle.image = imgBtn;

            Image imgMark = LayerWordBinder.findChildComponent<Image>(gObj, "checkmark" , "cm");
            toggle.graphic = imgMark;
            toggle.isOn = true;
        }

    }
}