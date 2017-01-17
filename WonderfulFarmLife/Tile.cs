using System.Collections.ObjectModel;
using xTile.Layers;
using xTile.Tiles;

namespace WonderfulFarmLife
{
    /// <summary>Encapsulates interaction with a map tile.</summary>
    internal class Tile
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The map layer index which has this tile.</summary>
        public int LayerIndex;

        /// <summary>The tile's X coordinate on the map.</summary>
        public int X;

        /// <summary>The tile's Y coordinate on the map.</summary>
        public int Y;

        /// <summary>The tile's index in the map's tile array.</summary>
        public int TileIndex;

        /// <summary>The tilesheet index which has the tile.</summary>
        public int TileSheetIndex;

        /// <summary>The layer name which has the tile.</summary>
        public string LayerName;

        /// <summary>The tilesheet name which has the tile.</summary>
        public string TileSheet;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="layerIndex">The map layer index which has this tile.</param>
        /// <param name="x">The tile's X coordinate on the map.</param>
        /// <param name="y">The tile's Y coordinate on the map.</param>
        /// <param name="tileIndex">The tile's index in the map's tile array.</param>
        /// <param name="tileSheetIndex">The tilesheet index which has the tile.</param>
        /// <param name="tileSheet">The tilesheet name which has the tile.</param>
        public Tile(int layerIndex, int x, int y, int tileIndex, int tileSheetIndex = -1, string tileSheet = "")
        {
            this.LayerIndex = layerIndex;
            this.X = x;
            this.Y = y;
            this.TileIndex = tileIndex;
            this.TileSheetIndex = tileSheetIndex;
            this.TileSheet = tileSheet;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="layerName">The map layer index which has this tile.</param>
        /// <param name="x">The tile's X coordinate on the map.</param>
        /// <param name="y">The tile's Y coordinate on the map.</param>
        /// <param name="tileIndex">The tile's index in the map's tile array.</param>
        /// <param name="tileSheetIndex">The tilesheet index which has the tile.</param>
        /// <param name="tileSheet">The tilesheet name which has the tile.</param>
        public Tile(string layerName, int x, int y, int tileIndex, int tileSheetIndex = -1, string tileSheet = "")
        {
            this.LayerName = layerName;
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

        /// <summary>Get a layer index from its name.</summary>
        /// <param name="name">The layer name.</param>
        /// <param name="layers">The current map layers.</param>
        public static int GetLayerIndex(string name, ReadOnlyCollection<Layer> layers)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].Id.Equals(name))
                    return i;
            }
            return 0;
        }

        /// <summary>Get a layer name from its index.</summary>
        /// <param name="index">The layer index.</param>
        /// <param name="layers">The current map layers.</param>
        public static string GetLayerName(int index, ReadOnlyCollection<Layer> layers)
        {
            return index < layers.Count
                ? layers[index].Id
                : layers[0].Id;
        }
    }
}
