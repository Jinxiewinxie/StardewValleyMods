using System.Collections.ObjectModel;
using xTile.Layers;
using xTile.Tiles;

namespace WonderfulFarmLife
{
    public class Tile
    {
        public int layerIndex;
        public int x;
        public int y;
        public int tileIndex;
        public int tileSheetIndex;
        public string layer;
        public string tileSheet;

        public Tile(int layerIndex, int x, int y, int tileIndex, int tileSheetIndex = -1, string tileSheet = "")
        {
            this.layerIndex = layerIndex;
            this.x = x;
            this.y = y;
            this.tileIndex = tileIndex;
            this.tileSheetIndex = tileSheetIndex;
            this.tileSheet = tileSheet;
        }

        public Tile(string layer, int x, int y, int tileIndex, int tileSheetIndex = -1, string tileSheet = "")
        {
            this.layer = layer;
            this.x = x;
            this.y = y;
            this.tileIndex = tileIndex;
            this.tileSheetIndex = tileSheetIndex;
            this.tileSheet = tileSheet;
        }

        public static int getTileSheetIndex(string tsn, ReadOnlyCollection<TileSheet> tileSheets)
        {
            for (int index = 0; index < tileSheets.Count; ++index)
            {
                if (tileSheets[index].Id.Equals(tsn))
                    return index;
            }
            return 0;
        }

        public static string getTileSheetName(int tsi, ReadOnlyCollection<TileSheet> tileSheets)
        {
            if (tsi >= tileSheets.Count)
                return tileSheets[0].Id;
            return tileSheets[tsi].Id;
        }

        public static int getLayerIndex(string ln, ReadOnlyCollection<Layer> layers)
        {
            for (int index = 0; index < layers.Count; ++index)
            {
                if (layers[index].Id.Equals(ln))
                    return index;
            }
            return 0;
        }

        public static string getLayerName(int li, ReadOnlyCollection<Layer> layers)
        {
            if (li >= layers.Count)
                return layers[0].Id;
            return layers[li].Id;
        }
    }
}
