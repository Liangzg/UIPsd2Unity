using System;
using System.Linq;
using System.Text;
using PhotoshopFile;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EditorTool.PsdExport
{
    public class TextConstructor : IPsdConstructor
    {
        public string MenuName {  get { return "Unity Text"; } }

        public Layer layer { get; set; }

        public bool CanBuild(GameObject hierarchySelection)
        {
            return true;
        }

        public GameObject CreateGameObject(string name, GameObject parent)
        {
            return SpriteConstructor.GOFactory(name, parent);
        }

        public void AddComponents(int layerIndex, GameObject spriteObject, Sprite sprite, TextureImporterSettings settings)
        {
            var layerText = layer.LayerText;

            float a = ((layerText.FillColor & 0xFF000000U) >> 24) / 255f;
            float r = ((layerText.FillColor & 0xFF0000U) >> 16) / 255f;
            float g = ((layerText.FillColor & 0xFF00U) >> 8) / 255f;
            float b = (layerText.FillColor & 0xFFU) / 255f;

            Color textColor = new Color(r, g, b, a);
            if (PsdSetting.Instance.curGUIType == GUIType.UGUI)
            {
                Text text = spriteObject.AddComponent<Text>();
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;

                if (PsdSetting.Instance.DefaultFontPath.EndsWith(".ttf"))
                    text.font = AssetDatabase.LoadAssetAtPath<Font>(PsdSetting.Instance.DefaultFontPath);
                
                text.fontStyle = GetFontStyle(layerText);
                text.fontSize = (int)layerText.FontSize;
                text.rectTransform.SetAsFirstSibling();
                text.rectTransform.sizeDelta = new Vector2(layer.Rect.width, layer.Rect.height);
                text.text = layerText.Text.Replace("\r\n", "\n").Replace("\r", "\n");
                text.color = textColor;               
            }else if (PsdSetting.Instance.curGUIType == GUIType.NGUI)
            {
#if NGUI
                UILabel text = spriteObject.AddComponent<UILabel>();
                text.overflowMethod = UILabel.Overflow.ShrinkContent;

                if (PsdSetting.Instance.DefaultFontPath.EndsWith(".ttf"))
                    text.trueTypeFont = AssetDatabase.LoadAssetAtPath<Font>(PsdSetting.Instance.DefaultFontPath);
                else
                    text.trueTypeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

                NGUISettings.ambigiousFont = text.trueTypeFont;
                text.depth = layerIndex;
                text.fontStyle = GetFontStyle(layerText);
                text.fontSize = (int)layerText.FontSize;
                text.transform.SetAsFirstSibling();
                text.SetDimensions((int)layer.Rect.width, (int)layer.Rect.height);
                text.text = layerText.Text.Replace("\r\n", "\n").Replace("\r", "");
                if (layer.Rect.width < layer.Rect.height)
                {
                    char[] charArr = text.text.ToCharArray();
                    StringBuilder buf = new StringBuilder();
                    foreach (char c in charArr)
                        buf.Append(c).AppendLine();
                    text.text = buf.ToString().TrimEnd();
                }
                text.color = textColor;
                text.MakePixelPerfect();
#endif
            }
        }


        public static FontStyle GetFontStyle(LayerText layerText)
        {
            FontStyle style = FontStyle.Normal;
            if (layerText.FauxBold)
                style |= FontStyle.Bold;
            if (layerText.FauxItalic)
                style |= FontStyle.Italic;
            return style;
        }

        public void HandleGroupOpen(GameObject groupParent)
        {

        }

        public void HandleGroupClose(GameObject groupParent)
        {

        }

        public Vector3 GetLayerPosition(Rect layerSize, Vector2 layerPivot, float pixelsToUnitSize)
        {
            return PsdBuilder.CalculateLayerPosition(layerSize, layerPivot);
        }

        public Vector3 GetGroupPosition(GameObject groupRoot, SpriteAlignment alignment)
        {
            return PsdBuilder.CalculateGroupPosition(groupRoot, alignment);
        }



    }
}