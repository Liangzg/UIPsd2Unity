using System.Collections.Generic;
using System.Linq;
using PhotoshopFile;

namespace EditorTool.PsdExport
{
	/// <summary>
	/// Stores preparsed information about the layers in a PSD file
	/// </summary>
	public class PsdFileInfo
	{
		public class InstancedLayerInfo
		{
			public int instanceLayer;
			public List<int> duplicateLayers;
		}

		public PSDLayerGroupInfo[] LayerGroups { get; protected set; }

		/// <summary>
		/// Layer visibility data, indexed by layer
		/// </summary>
		public bool[] LayerVisibility { get; protected set; }
		/// <summary>
		/// A list of layer index that are regular PS layers
		/// </summary>
		public int[] LayerIndices { get; protected set; }

		public float width
		{
			get { return m_Width; }
		}
		protected float m_Width;

		public float height
		{
			get { return m_Height; }
		}
		protected float m_Height;

		public PsdFileInfo(PsdFile psd)
		{
			m_Width = psd.BaseLayer.Rect.width;
			m_Height = psd.BaseLayer.Rect.height;

			List<int> layerIndices = new List<int>();
			List<PSDLayerGroupInfo> layerGroups = new List<PSDLayerGroupInfo>();
			List<PSDLayerGroupInfo> openGroupStack = new List<PSDLayerGroupInfo>();
			List<bool> layerVisibility = new List<bool>();
			// Reverse loop through layers to get the layers in the
			// same way they are displayed in Photoshop
			for (int i = psd.Layers.Count - 1; i >= 0; i--)
			{
				Layer layer = psd.Layers[i];

				layerVisibility.Add(layer.Visible);

				// Get the section info for this layer
				var secInfo = layer.AdditionalInfo
					.Where(info => info.GetType() == typeof(LayerSectionInfo))
					.ToArray();
				// Section info is basically layer group info
				bool isOpen = false;
				bool isGroup = false;
				bool closeGroup = false;
				if (secInfo.Any())
				{
					foreach (var layerSecInfo in secInfo)
					{
						LayerSectionInfo info = (LayerSectionInfo)layerSecInfo;
						isOpen = info.SectionType == LayerSectionType.OpenFolder;
						isGroup = info.SectionType == LayerSectionType.ClosedFolder | isOpen;
						closeGroup = info.SectionType == LayerSectionType.SectionDivider;
						if (isGroup || closeGroup)
							break;
					}
				}

				if (isGroup)
				{
					// Open a new layer group info, because we're iterating
					// through layers backwards, this layer number is the last logical layer
					openGroupStack.Add(new PSDLayerGroupInfo(layer.Name, i, layer.Visible, isOpen));
				}
				else if (closeGroup)
				{
					// Set the start index of the last LayerGroupInfo
					var closeInfo = openGroupStack.Last();
					closeInfo.start = i;
					// Add it to the layerGroup list
					layerGroups.Add(closeInfo);
					// And remove it from the open group stack 
					openGroupStack.RemoveAt(openGroupStack.Count - 1);
				}
				else
				{
					// Normal layer
					layerIndices.Add(i);
					// look for instances	
					if (layer.Name.Contains(" Copy"))
					{
					}
				}
			} // End layer loop

			// Since loop was reversed...
			layerVisibility.Reverse();
			LayerVisibility = layerVisibility.ToArray();

			LayerIndices = layerIndices.ToArray();

			LayerGroups = layerGroups.ToArray();
		}

		public InstancedLayerInfo GetInstancedLayer(int layerindex)
		{
			return null;
		}

		public PSDLayerGroupInfo GetGroupByLayerIndex(int layerIndex)
		{
			List<PSDLayerGroupInfo> candidates = new List<PSDLayerGroupInfo>();
			// Might be a nested layer group
			foreach (var layerGroupInfo in LayerGroups)
			{
				if (layerGroupInfo.ContainsLayer(layerIndex))
					candidates.Add(layerGroupInfo);
			}
			return candidates.OrderBy(info => info.end - info.start).FirstOrDefault();
		}

		public PSDLayerGroupInfo GetGroupByStartIndex(int startIndex)
		{
			return LayerGroups.FirstOrDefault(info => info.end == startIndex);
		}
	}
}