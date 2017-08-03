/* Copyright (c) 2017 LiangZG. See license.txt for your rights */

using PhotoshopFile.Text;
using UnityEngine;

namespace PhotoshopFile
{
    public class ObjectBasedEffect : LayerInfo
    {
        public override string Key
        {
            get { return "lfx2"; }
        }

        public DynVal Descriptor;

        public GradientEffect Gradient { get; private set; }

        public ObjectBasedEffect(PsdBinaryReader reader, int dataLength)
        {
            uint version = reader.ReadUInt32();
            uint descriptorVersion = reader.ReadUInt32();
            Descriptor = DynVal.ReadDescriptor(reader);

            DynVal grad = Descriptor.FindDynVal("Grad");
            if (grad != null)
            {
                Gradient = new GradientEffect();
                Gradient.ParseDescriptor(grad);
            }
        }

        protected override void WriteData(PsdBinaryWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}