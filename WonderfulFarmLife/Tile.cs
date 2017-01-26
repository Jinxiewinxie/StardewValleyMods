using System.Collections.ObjectModel;
using WonderfulFarmLife.Constants;
using xTile.Tiles;

namespace WonderfulFarmLife
{
    /// <summary>Encapsulates interaction with a map tile.</summary>
    internal class Tile
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The tile layer in the tilesheet.</summary>
        public readonly TileLayer Layer;

        /// <summary>The name of the <see cref="Layer"/>.</summary>
        public readonly string LayerName;

        /// <summary>The tile's X coordinate on the map.</summary>
        public readonly int X;

        /// <summary>The tile's Y coordinate on the map.</summary>
        public readonly int Y;

        /// <summary>The tile's index in the map's tile array.</summary>
        public readonly int TileIndex;

        /// <summary>The tilesheet name which has the tile.</summary>
        public readonly string Tilesheet;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance for an empty tile.</summary>
        /// <param name="layer">The tile layer in the tilesheet.</param>
        /// <param name="x">The tile's X coordinate on the map.</param>
        /// <param name="y">The tile's Y coordinate on the map.</param>
        public Tile(TileLayer layer, int x, int y)
            : this(layer, x, y, -1, null) { }

        /// <summary>Construct an instance for a sprite tile.</summary>
        /// <param name="layer">The tile layer in the tilesheet.</param>
        /// <param name="x">The tile's X coordinate on the map.</param>
        /// <param name="y">The tile's Y coordinate on the map.</param>
        /// <param name="tileIndex">The tile's index in the map's tile array.</param>
        /// <param name="tilesheet">The tilesheet name which has the tile.</param>
        public Tile(TileLayer layer, int x, int y, int tileIndex, string tilesheet)
        {
            this.Layer = layer;
            this.LayerName = layer.ToString();
            this.X = x;
            this.Y = y;
            this.TileIndex = tileIndex;
            this.Tilesheet = tilesheet;
        }

        /// <summary>Get a tilesheet index from its name.</summary>
        /// <param name="name">The tilesheet name.</param>
        /// <param name="tileSheets">The current tilesheets.</param>
        public static int GetTileSheetIndex(string name, ReadOnlyCollection<TileSheet> tileSheets)
        {
            for (int i = 0; i < tileSheets.Count; i++)
            {
                if (tileSheets[i].Id.Equals(name))
                    return i;
            }
            return 0;
        }
    }
}
