/* Copyright (c) 2017 LiangZG. See license.txt for your rights */
using System.Collections.Generic;

namespace PhotoshopFile
{
    public class MetadataSetting : LayerInfo
    {

        public class Metadata
        {
            public string Key { get; private set; }
            public Metadata(PsdBinaryReader reader)
            {
                string signature = reader.ReadAsciiChars(4);
                if (signature != "8BIM")
                    throw new PsdInvalidException("Could not read LayerInfo due to signature mismatch." + signature);

                Key = reader.ReadAsciiChars(4);
                bool sheetCopy = reader.ReadBoolean();
                byte[] padding = reader.ReadBytes(3);
                uint length = reader.ReadUInt32();
                byte[] datas = reader.ReadBytes((int)length);
            }
        }

        public List<Metadata> Metadatas; 

        public override string Key
        {
            get { return "shmd"; }
        }


        public MetadataSetting(PsdBinaryReader reader)
        {
            uint metadataCount = reader.ReadUInt32();
            Metadatas = new List<Metadata>((int)metadataCount);
            for (int i = 0; i < metadataCount; i++)
            {
                Metadatas.Add(new Metadata(reader));
            }
        }

        protected override void WriteData(PsdBinaryWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}