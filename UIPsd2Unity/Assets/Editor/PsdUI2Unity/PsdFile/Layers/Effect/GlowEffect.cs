/* Copyright (c) 2017 LiangZG. See license.txt for your rights */
using System.ComponentModel;
using System.Xml.Serialization;
using PhotoshopFile.Text;
using UnityEngine;

namespace PhotoshopFile
{
    [Description("oglw,iglw")]
    public class GlowEffect : EffectBase
    {
        [XmlAttribute()]
        public uint Intensity;
        [XmlAttributeAttribute()]
        public bool UseGlobalAngle;
        [XmlAttributeAttribute()]
        public bool Inner { get { return Key.StartsWith("i"); } }

        public byte Unknown;
        public Color UnknownColor;


        public GlowEffect(PsdBinaryReader r, string key)
        {
            m_key = key;
            uint version = r.ReadUInt32(); //two version specifications?!?
            this.Blur = r.ReadUInt32();
            this.Intensity = r.ReadUInt32();
            this.Color = r.ReadPSDColor(16, true); //Inner color (no alpha)
            this.BlendModeKey = this.ReadBlendKey(r);
            this.Enabled = r.ReadBoolean();
            this.Opacity = r.ReadByte();

            switch (version)
            {
                case 0:
                    
                    break;
                case 2:
                    //TODO!
                    if (this.Inner)
                        this.Unknown = r.ReadByte();
                    this.UnknownColor = r.ReadPSDColor(16, true); //unknown color(no alpha)
//                    byte[] Data = r.ReadBytes((int)r.BytesToEnd);
                    break;
            }
        }
    }

}