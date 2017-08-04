using System;
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

            Color textColor = layerText.FillColor;
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
                //竖屏
                text.overflowMethod = layer.Rect.width < layer.Rect.height ? UILabel.Overflow.ResizeHeight : UILabel.Overflow.ResizeFreely;

                if (PsdSetting.Instance.DefaultFontPath.EndsWith(".ttf"))
                    text.trueTypeFont = AssetDatabase.LoadAssetAtPath<Font>(PsdSetting.Instance.DefaultFontPath);
                else
                    text.trueTypeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

                NGUISettings.ambigiousFont = text.trueTypeFont;
                text.depth = layerIndex;
                text.fontStyle = layerText.Style;
                if (text.fontStyle == FontStyle.Bold)   text.spacingX = (int)layerText.FontSize / 4;

                text.fontSize = layerText.FontBaseline != 0 ? (int)layerText.FontSize / 2 : (int)layerText.FontSize;
                text.transform.SetAsFirstSibling();

                if (layer.BaseEffect != null)
                {
                    GradientEffect gradient = layer.BaseEffect.Gradient;
                    if (gradient != null)
                    {
                        text.applyGradient = true;
                        textColor = Color.white;
                        text.gradientTop = gradient.TopColor;
                        text.gradientBottom = gradient.BottomColor;
                    }
                }

                if (layer.Effects != null)
                {
                    EffectsLayer effectLayer = layer.Effects;
                    if (effectLayer.IsDropShadow)
                    {
                        text.effectStyle = UILabel.Effect.Shadow;
                        text.effectDistance = new Vector2(2 , 2);
                        text.effectColor = effectLayer.DropShadow.Color;
                    }
                    if (effectLayer.IsOuterGlow)
                    {
                        text.effectStyle = UILabel.Effect.Outline;
                        text.effectColor = effectLayer.OuterGlow.Color;
                    }
                }

                if (layerText.Underline)
                {
                    text.text = string.Format("[u]{0}[/u]", layerText.Text);
                }else if (layerText.Strikethrough)
                    text.text = string.Format("[s]{0}[/s]", layerText.Text);
                else if (layerText.FontBaseline == 1)
                    text.text = string.Format("[sub]{0}[/sub]", layerText.Text);
                else if (layerText.FontBaseline == 2)
                    text.text = string.Format("[sup]{0}[/sup]", layerText.Text);
                else
                    text.text = layerText.Text;
                text.color = textColor;

                int width = text.overflowMethod == UILabel.Overflow.ResizeHeight
                    ? (int)Math.Max(text.fontSize, layer.Rect.width)
                    : (int)layer.Rect.width;
                int height = text.overflowMethod == UILabel.Overflow.ResizeHeight
                    ? (int)layer.Rect.height
                    : (int)Math.Max(text.fontSize, layer.Rect.height);
                text.SetDimensions(width, (int)height);

                if (text.overflowMethod == UILabel.Overflow.ClampContent)
                {
                    NGUIEditorTools.RegisterUndo("Snap Dimensions", text);
                    NGUIEditorTools.RegisterUndo("Snap Dimensions", text.transform);
                    text.MakePixelPerfect();
                }

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