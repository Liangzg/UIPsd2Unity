using System.Collections.Generic;
using PhotoshopFile.Text;
using UnityEngine;

namespace PhotoshopFile
{
    public class GradientEffect : EffectBase
    {

        public class Transparency
        {

            public ushort Opacity { get; private set; }

            public Color MinColor { get; private set; }

            public Color MaxColor { get; private set; }
            public Transparency(PsdBinaryReader reader)
            {
                uint localColorStop = reader.ReadUInt32();
                uint midpointColorStop = reader.ReadUInt32();

                Opacity = reader.ReadUInt16();
                ushort expansionCount = reader.ReadUInt16();
                ushort interpolation = reader.ReadUInt16();
                ushort length = reader.ReadUInt16();
                ushort gradientMode = reader.ReadUInt16();
                uint randomNumSeed = reader.ReadUInt32();
                ushort showFlag = reader.ReadUInt16();
                ushort usingVectorColorFlag = reader.ReadUInt16();
                uint roughnessFactor = reader.ReadUInt16();
                ushort colorModel = reader.ReadUInt16();

                MinColor = reader.ReadPSDColor(16, true);
                MaxColor = reader.ReadPSDColor(16, true);

                ushort dummy = reader.ReadUInt16();
            }
        }


        public class ColorStop
        {
            public Color ActualColor { get; private set; }

            public List<Transparency> Trsps { get; private set; }

            public ColorStop(PsdBinaryReader reader)
            {
                uint localColorStop = reader.ReadUInt32();
                uint midpointColorStop = reader.ReadUInt32();
                ushort followColorMode = reader.ReadUInt16();

                ActualColor = reader.ReadPSDColor(16 , true);

                ushort numTransparency = reader.ReadUInt16();
                Trsps = new List<Transparency>(numTransparency);
                for (int j = 0; j < numTransparency; j++)
                {
                    Trsps.Add(new Transparency(reader));
                }
            }
        }

        
        public List<ColorStop> ColorStops { get; private set; }
         
        public GradientEffect(PsdBinaryReader r, string key)
        {
            m_key = key;

            uint version = r.ReadUInt32();

            //Is gradient reversed
            bool isReversed = r.ReadBoolean();
            bool isDithered = r.ReadBoolean();

            string gradientName = r.ReadUnicodeString();

            ushort numColors = r.ReadUInt16();
            ColorStops = new List<ColorStop>(numColors);
            for (int i = 0; i < numColors; i++)
            {
                ColorStops.Add(new ColorStop(r));
            }

        }
    }
}