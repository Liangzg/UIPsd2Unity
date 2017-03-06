using System;
using System.Collections.Generic;

namespace PhotoshopFile
{
	public class Matrix2D
	{
		public double M11;
		public double M12;
		public double M13;
		public double M21;
		public double M22;
		public double M23;
		public Matrix2D()
		{ }
		public Matrix2D(PsdBinaryReader r)
		{
			this.M11 = r.ReadDouble();
			this.M12 = r.ReadDouble();
			this.M13 = r.ReadDouble();
			this.M21 = r.ReadDouble();
			this.M22 = r.ReadDouble();
			this.M23 = r.ReadDouble();
		}
	}

	public class LayerTextType : LayerInfo
	{
		public class FontInfo
		{
			public ushort Mark;
			public uint FontType;
			public string FontName;
			public string FontFamilyName;
			public string FontStyleName;
			public ushort Script;
			public List<uint> DesignVectors;

			public FontInfo()
			{ }
			public FontInfo(PsdBinaryReader r)
			{
				Mark = r.ReadUInt16();
				FontType = r.ReadUInt32();
				FontName = r.ReadPascalString(2);
				FontFamilyName = r.ReadPascalString(2);
				FontStyleName = r.ReadPascalString(2);
				Script = r.ReadUInt16();

				ushort NumDesignAxesVectors = r.ReadUInt16();
				DesignVectors = new List<uint>();
				for (int vectorNum = 0; vectorNum < NumDesignAxesVectors; vectorNum++)
					DesignVectors.Add(r.ReadUInt32());
			}
		}

		public override string Key
		{
			get { return "tySh"; }
		}

		public ushort version;
		public Matrix2D transform;
		public List<FontInfo> fontInfos;

		public LayerTextType()
		{
			
		}

		public LayerTextType(PsdBinaryReader reader)
		{
			version = reader.ReadUInt16(); // 1 = Photoshop 5.0
			transform = new Matrix2D(reader);

			//Font info:
			// fontVersion
			reader.ReadUInt16(); // 6 = Photoshop 5.0
			ushort faceCount = reader.ReadUInt16();
			fontInfos = new List<FontInfo>();
			for (int i = 0; i < faceCount; i++)
				fontInfos.Add(new FontInfo(reader));

			//TODO: make classes of styles as well...
			ushort styleCount = reader.ReadUInt16();
			for (int i = 0; i < styleCount; i++)
			{
				// mark
				reader.ReadUInt16();
				// faceMark
				reader.ReadUInt16();
				// size
				reader.ReadUInt32();
				// tracking
				reader.ReadUInt32();
				// kerning
				reader.ReadUInt32();
				// leading
				reader.ReadUInt32();
				// baseShift
				reader.ReadUInt32();

				// autoKern
				reader.ReadByte();
				
				if (version <= 5)
					// extra
					reader.ReadByte();

				// rotate
				reader.ReadByte();
			}

			//Text information
			// type
			reader.ReadUInt16();
			// scalingFactor
			reader.ReadUInt32();
			// characterCount
			reader.ReadUInt32();
			// horizontalPlacement
			reader.ReadUInt32();
			// verticalPlacement
			reader.ReadUInt32();
			// selectStart
			reader.ReadUInt32();
			// selectEnd
			reader.ReadUInt32();

			ushort lineCount = reader.ReadUInt16();
			for (int i = 0; i < lineCount; i++)
			{
				// characterCountLine
				reader.ReadUInt32();
				// orientation
				reader.ReadUInt16();
				// alignment
				reader.ReadUInt16();
				// doubleByteChar
				reader.ReadUInt16();
				// style
				reader.ReadUInt16();
			}

			// colorSpace
			reader.ReadUInt16();
			for (int i = 0; i < 4; i++)
				reader.ReadUInt16(); //Color compensation
			// antiAlias
			reader.ReadByte();
		}

		protected override void WriteData(PsdBinaryWriter writer)
		{
			throw new NotImplementedException("LayerTextType not implemented!");
		}
	}
}
