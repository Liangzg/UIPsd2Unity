using UnityEngine;
using UnityEngine.UI;

namespace subjectnerdagreement.psdexport
{
    public class InputFieldBinder : ABinder
    {
        public void StartBinding(GameObject mainObj, string args, string layerName)
        {
            Transform bgTrans = LayerWordBinder.findChildComponent<Transform>(mainObj, "background");
            LayerWordBinder.copyRectTransform(bgTrans.gameObject, mainObj, true);

            Text holderText = LayerWordBinder.findChildComponent<Text>(mainObj, "holder");
            if (holderText)
            {
                RectTransform holderTrans = holderText.GetComponent<RectTransform>();
                holderTrans.anchorMin = Vector2.zero;
                holderTrans.anchorMax = Vector2.one;
                holderTrans.sizeDelta = Vector2.zero;
                holderTrans.offsetMin = new Vector2(10, 6);
                holderTrans.offsetMax = new Vector2(-10, -7);
            }

            GameObject textObj = LayerWordBinder.CreateUIObject("text", mainObj);
            Text text = textObj.AddComponent<Text>();
            text.text = "";
            text.supportRichText = false;
            if (holderText)
            {
                text.alignment = holderText.alignment;
                text.fontSize = holderText.fontSize - 1;
                text.font = holderText.font;
            }

            RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;
            textRectTransform.offsetMin = new Vector2(10, 6);
            textRectTransform.offsetMax = new Vector2(-10, -7);

            InputField inputField = LayerWordBinder.swapComponent<InputField>(mainObj);
            inputField.textComponent = text;
            inputField.placeholder = holderText;
        }

    }
}