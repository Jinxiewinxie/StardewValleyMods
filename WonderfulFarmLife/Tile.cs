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

        /// <summary>The tilesheet index which has the tile.</summary>
        public int TileSheetIndex;

        /// <summary>The tilesheet name which has the tile.</summary>
        public string TileSheet;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="layer">The tile layer in the tilesheet.</param>
        /// <param name="x">The tile's X coordinate on the map.</param>
        /// <param name="y">The tile's Y coordinate on the map.</param>
        /// <param name="tileIndex">The tile's index in the map's tile array.</param>
        /// <param name="tileSheetIndex">The tilesheet index which has the tile.</param>
        /// <param name="tileSheet">The tilesheet name which has the tile.</param>
        public Tile(TileLayer layer, int x, int y, int tileIndex, int tileSheetIndex = -1, string tileSheet = "")
        {
            this.Layer = layer;
            this.LayerName = layer.ToString();
            this.X = x;
            this.Y = y;
            this.TileIndex = tileIndex;
            this.TileSheetIndex = tileSheetIndex;
            this.TileSheet = tileSheet;
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

        /// <summary>Get a tilesheet name from its index.</summary>
        /// <param name="index">The tilesheet index.</param>
        /// <param name="tileSheets">The current tilesheets.</param>
        public static string GetTileSheetName(int index, ReadOnlyCollection<TileSheet> tileSheets)
        {
            return index < tileSheets.Count
                ? tileSheets[index].Id
                : tileSheets[0].Id;
        }
    }
}
