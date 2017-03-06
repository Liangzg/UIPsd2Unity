using System;
using System.Collections.Generic;
using PhotoshopFile.Text;

namespace PhotoshopFile
{
    public class LayerText : LayerInfo
    {
        public override string Key
        {
            get { return "TySh"; }
        }

        public byte[] Data { get; private set; }

        public Matrix2D Transform;
        public DynVal TxtDescriptor;
        public TdTaStylesheetReader StylesheetReader;
        public Dictionary<string, object> engineData;
        public Boolean isTextHorizontal
        {
            get
            {
                DynVal dynVal = TxtDescriptor.Children.Find(c => c.Name.Equals("Orientation", StringComparison.InvariantCultureIgnoreCase));
                return dynVal != null && ((string)dynVal.Value).Equals("Orientation.Horizontal", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public string Text
        {
            get;
            private set;
        }

        public double FontSize
        {
            get;
            private set;
        }

        public string FontName
        {
            get;
            private set;
        }

        public bool FauxBold
        {
            get;
            private set;
        }

        public bool FauxItalic
        {
            get;
            private set;
        }

        public bool Underline
        {
            get;
            private set;
        }

        public uint FillColor
        {
            get;
            private set;
        }

        public double OutlineWidth
        {
            get;
            private set;
        }

        public uint StrokeColor
        {
            get;
            private set;
        }

        public LayerText()
        {

        }

        public LayerText(PsdBinaryReader psdReader, int dataLength)
        {
            Data = psdReader.ReadBytes((int)dataLength);
            var reader = new PsdBinaryReader(new System.IO.MemoryStream(Data), psdReader);

            // PhotoShop version
            reader.ReadUInt16();

            Transform = new Matrix2D(reader);

            // TextVersion
            reader.ReadUInt16(); //2 bytes, =50. For Photoshop 6.0.

            // DescriptorVersion
            reader.ReadUInt32(); //4 bytes,=16. For Photoshop 6.0.

            TxtDescriptor = DynVal.ReadDescriptor(reader); //Text descriptor

            // WarpVersion
            reader.ReadUInt16(); //2 bytes, =1. For Photoshop 6.0.

            engineData = (Dictionary<string, object>)TxtDescriptor.Children.Find(c => c.Name == "EngineData").Value;
            StylesheetReader = new TdTaStylesheetReader(engineData);

            Dictionary<string, object> d = StylesheetReader.GetStylesheetDataFromLongestRun();
            Text = StylesheetReader.Text;
            FontName = TdTaParser.getString(StylesheetReader.getFontSet()[(int)TdTaParser.query(d, "Font")], "Name$");
            FontSize = (double)TdTaParser.query(d, "FontSize");

            try
            {
                FauxBold = TdTaParser.getBool(d, "FauxBold");
            }
            catch (KeyNotFoundException)
            {
                FauxBold = false;
            }

            try
            {
                FauxItalic = TdTaParser.getBool(d, "FauxItalic");
            }
            catch (KeyNotFoundException)
            {
                FauxItalic = false;
            }

            try
            {
                Underline = TdTaParser.getBool(d, "Underline");
            }
            catch (KeyNotFoundException)
            {
                Underline = false;
            }

            FillColor = TdTaParser.getColor(d, "FillColor");

            try
            {
                OutlineWidth = (double)TdTaParser.query(d, "OutlineWidth");
            }
            catch (KeyNotFoundException)
            {
                OutlineWidth = 0f;
            }

            try
            {
                StrokeColor = TdTaParser.getColor(d, "StrokeColor");
            }
            catch (KeyNotFoundException)
            {
                StrokeColor = 0;
            }
        }

        protected override void WriteData(PsdBinaryWriter writer)
        {
            writer.Write(Data);
        }
    }
}