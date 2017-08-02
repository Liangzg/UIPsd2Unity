/* Copyright (c) 2017 LiangZG. See license.txt for your rights */
using System.ComponentModel;
using System.Xml.Serialization;
using PhotoshopFile.Text;
using UnityEngine;

namespace PhotoshopFile
{
    [Description("dsdw,isdw")]
    public class ShadowEffect : EffectBase
    {
        [XmlAttribute()]
        public uint Intensity;
        [XmlAttributeAttribute()]
        public uint Angle;
        [XmlAttributeAttribute()]
        public uint Distance;
        [XmlAttributeAttribute()]
        public bool UseGlobalAngle;
        [XmlAttributeAttribute()]

        public Color NativeColor;
        public bool Inner { get { return m_key.StartsWith("i"); } }

        public ShadowEffect(PsdBinaryReader r, string key)
        {
            m_key = key;
            int version = r.ReadInt32();
            this.Blur = r.ReadUInt32();
            this.Intensity = r.ReadUInt32();
            this.Angle = r.ReadUInt32();
            this.Distance = r.ReadUInt32();
            this.Color = r.ReadPSDColor(16, true);
            this.BlendModeKey = this.ReadBlendKey(r);
            this.Enabled = r.ReadBoolean();
            this.UseGlobalAngle = r.ReadBoolean();
            this.Opacity = r.ReadByte();
            NativeColor = r.ReadPSDColor(16, true);
        }

        public Color ShadowColor
        {
            get
            {
                return new Color(Color.r , Color.g , Color.b , this.Opacity / 255.0f);
            }
        }
    }

}