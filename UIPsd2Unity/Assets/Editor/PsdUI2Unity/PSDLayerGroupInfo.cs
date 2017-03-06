namespace subjectnerdagreement.psdexport
{
	/// <summary>
	/// Data on PSD layer groups
	/// </summary>
	public class PSDLayerGroupInfo
	{
		/// <summary>
		/// Layer group name
		/// </summary>
		public string name;
		/// <summary>
		/// The last layer number contained in this layer group
		/// </summary>
		public int end;
		/// <summary>
		/// The first layer number contained by this layer group
		/// </summary>
		public int start;
		/// <summary>
		/// If this layer group is visible
		/// </summary>
		public bool visible;
		/// <summary>
		/// If this layer group is expanded
		/// </summary>
		public bool opened;

	    public int depth;

		public PSDLayerGroupInfo(string name, int end, bool visible, bool opened)
		{
			this.name = name;
			this.end = end;
			this.visible = visible;
			this.opened = opened;

			start = -1;
		}

		public bool ContainsLayer(int layerIndex)
		{
			return (layerIndex <= end) && (layerIndex >= start);
		}
	}
}