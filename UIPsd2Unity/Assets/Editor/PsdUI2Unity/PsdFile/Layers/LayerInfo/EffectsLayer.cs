/* Copyright (c) 2017 LiangZG. See license.txt for your rights */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using PhotoshopFile;
using PhotoshopFile.Text;

namespace PhotoshopFile
{
    [Description("lrFX")]
    public class EffectsLayer : LayerInfo
    {
        public ushort Version { get; private set; }

        private string mKey;

        private Dictionary<string, LayerInfo> _resources = new Dictionary<string, LayerInfo>();

        public override string Key
        {
            get { return mKey; }
        }

        public ShadowEffect DropShadow
        {
            get { return (ShadowEffect)GetEffect(EffectTypes.dsdw); }
        }
        
        public bool IsDropShadow { get { return enableEffect(EffectTypes.dsdw); } }

        public ShadowEffect InnerShadow
        {
            get { return (ShadowEffect)GetEffect(EffectTypes.isdw);}
        }

        public bool IsInnerShadow { get { return enableEffect(EffectTypes.isdw);} }

        public GlowEffect OuterGlow
        {
            get { return (GlowEffect)GetEffect(EffectTypes.oglw);}
        }

        public bool IsOuterGlow { get { return enableEffect(EffectTypes.oglw); } }

        public GlowEffect InterGlow
        {
            get { return (GlowEffect)GetEffect(EffectTypes.iglw); }
        }

        public bool IsInterGlow { get { return enableEffect(EffectTypes.iglw);} }

        public BevelEffect Bevel
        {
            get { return (BevelEffect)GetEffect(EffectTypes.bevl);}
        }

        public bool IsBevel { get { return enableEffect(EffectTypes.bevl); } }

        public LayerInfo SolidFill
        {
            get { return GetEffect(EffectTypes.sofi); }
        }

        public GradientEffect Gradient;

        public EffectsLayer(PsdBinaryReader reader , string key)
        {
            mKey = key;
            Version = reader.ReadUInt16();

            ushort count = reader.ReadUInt16();
            
            for (int i = 0; i < count; i++)
            {
                LayerInfo li = EffectLayerFactory.Load(reader);
                _resources[li.Key] = li;
            }
        }


        public LayerInfo GetEffect(EffectTypes eType)
        {
            string eTypeKey = Enum.GetName(typeof (EffectTypes), eType);
            if (!_resources.ContainsKey(eTypeKey)) return null;
            return _resources[eTypeKey];
        }

        public bool enableEffect(EffectTypes eType)
        {
            LayerInfo effect = GetEffect(eType);
            if (effect == null) return false;
            return ((EffectBase) effect).Enabled;
        }

        protected override void WriteData(PsdBinaryWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}