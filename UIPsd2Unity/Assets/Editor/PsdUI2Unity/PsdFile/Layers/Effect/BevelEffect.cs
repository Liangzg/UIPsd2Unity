/* Copyright (c) 2017 LiangZG. See license.txt for your rights */
using System.ComponentModel;
using System.Xml.Serialization;
using PhotoshopFile.Text;
using UnityEngine;

namespace PhotoshopFile
{
    [Description("bevl"), Category("Effect")]
    public class BevelEffect : EffectBase
    {
        [XmlAttribute()]
        public uint Angle;
        [XmlAttributeAttribute()]
        public uint Strength;

        [XmlAttributeAttribute()]
        public string ShadowBlendModeKey;

        public Color ShadowColor;

        [XmlAttributeAttribute()]
        public byte BevelStyle;
        [XmlAttributeAttribute()]
        public byte ShadowOpacity;

        [XmlAttributeAttribute()]
        public bool UseGlobalAngle;
        [XmlAttributeAttribute()]
        public bool Inverted;

        public byte Unknown1;
        public byte Unknown2;
        public ushort Unknown3;
        public ushort Unknown4;

        public BevelEffect()
        {
        }

        public BevelEffect(PsdBinaryReader r, string key)
        {
            m_key = key;

            uint version = r.ReadUInt32();
            this.Angle = r.ReadUInt32();
            this.Strength = r.ReadUInt32();
            this.Blur = r.ReadUInt32();

            this.BlendModeKey = this.ReadBlendKey(r);
            this.ShadowBlendModeKey = this.ReadBlendKey(r);

            this.Color = r.ReadPSDColor(16, true);
            this.ShadowColor = r.ReadPSDColor(16, true);

            this.BevelStyle = r.ReadByte();
            this.Opacity = r.ReadByte();
            this.ShadowOpacity = r.ReadByte();

            this.Enabled = r.ReadBoolean();
            this.UseGlobalAngle = r.ReadBoolean();
            this.Inverted = r.ReadBoolean();

            switch (version)
            {
                case 0:
                    break;
                case 2:
                    Color someColor = r.ReadPSDColor(16, true);
                    Color someColor2 = r.ReadPSDColor(16, true);
                    break;
            }
        }

    }



}