using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PhotoshopFile.Text;
using UnityEngine;

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

        public string Text{get;private set;}

        public double FontSize{get;private set;}

        public string FontName{get;private set;}

        public bool FauxBold{get;private set;}

        public bool FauxItalic{get;private set;}

        public bool Underline   {   get;private set; }

        public Color FillColor  {get;private set;}
        
        public double OutlineWidth  { get; private set; }

       public bool StrokeFlag { get; private set; }

        public Color StrokeColor { get; private set; }

        public FontStyle Style { get; private set; }

        public int FontBaseline { get; private set; }

        public bool Strikethrough { get; private set; }

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
            ushort wrapVersion = reader.ReadUInt16(); //2 bytes, =1. For Photoshop 6.0.

            // DescriptorVersion
            uint wrapDescriptorVersion = reader.ReadUInt32();

            DynVal warpDescriptor = DynVal.ReadDescriptor(reader);

            //            double left = reader.ReadDouble();
            //            double top = reader.ReadDouble();
            //            double right = reader.ReadDouble();
            //            double bottom = reader.ReadDouble();

            byte[] datas = reader.ReadBytes(32);

            engineData = (Dictionary<string, object>)TxtDescriptor.Children.Find(c => c.Name == "EngineData").Value;
            StylesheetReader = new TdTaStylesheetReader(engineData);

            Dictionary<string, object> d = StylesheetReader.GetStylesheetDataFromLongestRun();
            Text = StylesheetReader.Text;
            FontName = TdTaParser.getString(StylesheetReader.getFontSet()[(int)TdTaParser.query(d, "Font")], "Name$");
            FontSize = (double)TdTaParser.query(d, "FontSize");

            if (d.ContainsKey("FauxBold"))
                FauxBold = TdTaParser.getBool(d, "FauxBold");
            if (d.ContainsKey("FauxItalic"))
                FauxItalic = TdTaParser.getBool(d, "FauxItalic");
            if (d.ContainsKey("Underline"))
                Underline = TdTaParser.getBool(d, "Underline");
            if (d.ContainsKey("StyleRunAlignment"))
            {
                int styleRunAlignment = (int)TdTaParser.query(d, "StyleRunAlignment");//No idea what this maps to.
            }

            FillColor = Color.black;
            if (d.ContainsKey("FillColor"))
                FillColor = TdTaParser.getColor(d, "FillColor");
            if (d.ContainsKey("OutlineWidth"))
                OutlineWidth = (double)TdTaParser.query(d, "OutlineWidth");
            if (d.ContainsKey("StrokeFlag"))
                StrokeFlag = TdTaParser.getBool(d, "StrokeFlag");
            if (d.ContainsKey("StrokeColor"))
                StrokeColor = TdTaParser.getColor(d, "StrokeColor");

            if (d.ContainsKey("Strikethrough"))
                Strikethrough = TdTaParser.getBool(d, "Strikethrough");
            if (d.ContainsKey("FontBaseline"))
                FontBaseline = TdTaParser.getIntger(d, "FontBaseline");

            //Fix newlines
            try
            {
                //Remove MT
                if (FontName.EndsWith("MT")) FontName = FontName.Substring(0, FontName.Length - 2);
                //Remove -Bold, -Italic, -BoldItalic
                if (FontName.EndsWith("-Bold", StringComparison.OrdinalIgnoreCase)) Style |= FontStyle.Bold;
                if (FontName.EndsWith("-Italic", StringComparison.OrdinalIgnoreCase)) Style |= FontStyle.Italic;
                if (FontName.EndsWith("-BoldItalic", StringComparison.OrdinalIgnoreCase)) Style |= FontStyle.Bold | FontStyle.Italic;
                //Remove from FontName
                FontName = new Regex("\\-(Bold|Italic|BoldItalic)$", RegexOptions.IgnoreCase | RegexOptions.IgnoreCase).Replace(FontName, "");
                //Remove PS
                if (FontName.EndsWith("PS")) FontName = FontName.Substring(0, FontName.Length - 2);
                //Find font family

                if (FauxBold) Style |= FontStyle.Bold;
                if (FauxItalic) Style |= FontStyle.Italic;
                //                    if (underline) style |= FontStyle.Underline;
                //                    if (strikethrough) style |= FontStyle.Strikeout;

            }
            finally
            {

            }
        }

        protected override void WriteData(PsdBinaryWriter writer)
        {
            writer.Write(Data);
        }
    }
}