namespace PondWithBridge.Framework
{
    /// <summary>Defines an override to apply to a tile position.</summary>
    public class Tile
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The layer index to modify.</summary>
        public int LayerIndex;

        /// <summary>The X tile coordinate.</summary>
        public int X;

        /// <summary>The Y tile coordinate.</summary>
        public int Y;

        /// <summary>The tile ID in the tilesheet.</summary>
        public int TileIndex;

        /// <summary>The layer name to modify.</summary>
        public string LayerName { get; }

        /// <summary>The tilesheet index to modify.</summary>
        public int Tilesheet;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="layerIndex">The layer index to modify.</param>
        /// <param name="x">The X tile coordinate.</param>
        /// <param name="y">The Y tile coordinate.</param>
        /// <param name="tileIndex">The tilesheet index to modify.</param>
        /// <param name="tilesheet">The tilesheet index to modify.</param>
        public Tile(int layerIndex, int x, int y, int tileIndex, int tilesheet = 1)
        {
            this.LayerIndex = layerIndex;
            this.X = x;
            this.Y = y;
            this.TileIndex = tileIndex;
            this.Tilesheet = tilesheet;

            switch (layerIndex)
            {
                case 0:
                    this.LayerName = "Back";
                    break;
                case 1:
                    this.LayerName = "Buildings";
                    break;
                case 2:
                    this.LayerName = "Paths";
                    break;
                case 3:
                    this.LayerName = "Front";
                    break;
                case 4:
                    this.LayerName = "AlwaysFront";
                    break;
            }
        }
    }
}