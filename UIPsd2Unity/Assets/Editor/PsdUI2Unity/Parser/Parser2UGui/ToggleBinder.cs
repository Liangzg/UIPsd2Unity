using UnityEngine;
using UnityEngine.UI;

namespace subjectnerdagreement.psdexport
{
    public class ToggleBinder : ABinder
    {
        public void StartBinding(GameObject gObj, string args, string layerName)
        {
            Toggle toggle = LayerWordBinder.swapComponent<Toggle>(gObj);

            Image imgBtn = LayerWordBinder.findChildComponent<Image>(gObj, "background");
            toggle.image = imgBtn;

            Image imgMark = LayerWordBinder.findChildComponent<Image>(gObj, "checkmark");
            toggle.graphic = imgMark;
            toggle.isOn = true;
        }

    }
}