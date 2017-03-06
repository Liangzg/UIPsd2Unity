using UnityEngine;
using UnityEngine.UI;

namespace subjectnerdagreement.psdexport
{
    public class ButtonBinder:ABinder
    {
        public void StartBinding(GameObject gObj, string args, string layerName)
        {
            Button button = LayerWordBinder.swapComponent<Button>(gObj);
            Image imgBtn = LayerWordBinder.findChildComponent<Image>(gObj, "imgBtn");
            button.targetGraphic = imgBtn;
        }

    }
}