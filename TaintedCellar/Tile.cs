using System.Collections.ObjectModel;
using StardewModdingAPI;
using xTile.Tiles;

namespace TaintedCellar
{
    public class Tile
    {
        public int TileSheet;
        public int LayerIndex;
        public int X;
        public int Y;
        public int TileIndex;
        public string LayerName;

        public Tile(int layerIndex, int x, int y, int tileIndex)
        {
            this.LayerIndex = layerIndex;
            this.X = x;
            this.Y = y;
            this.TileIndex = tileIndex;
        }

        public Tile(int layerIndex, int x, int y, int tileIndex, int tileSheet)
        {
            this.LayerIndex = layerIndex;
            this.X = x;
            this.Y = y;
            this.TileIndex = tileIndex;
            this.TileSheet = tileSheet;
        }

        public static int GetTileSheetIndex(string tileSheetName, ReadOnlyCollection<TileSheet> tileSheets)
        {
            for (int index = 0; index < tileSheets.Count; ++index)
            {
                if (tileSheets[index].Id.Equals(tileSheetName))
                    return index;
            }
            return 0;
        }

        public static string GetTileSheetName(int tileSheetIndex, ReadOnlyCollection<TileSheet> tileSheets)
        {
            if (tileSheetIndex < tileSheets.Count)
                return tileSheets[tileSheetIndex].Id;
            Log.Error("tileSheetIndex out of range");
            return "";
        }
    }
}
