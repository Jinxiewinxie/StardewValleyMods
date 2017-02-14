using StardewModdingAPI;
using System.Collections.ObjectModel;
using xTile.ObjectModel;
using xTile.Tiles;

namespace TaintedCellar
{
    public class Tile
    {
        public int tileSheet = 0;
        public int l;
        public int x;
        public int y;
        public int tileIndex;
        public string layer;

        public Tile(int l, int x, int y, int tileIndex)
        {
            this.l = l;
            this.x = x;
            this.y = y;
            this.tileIndex = tileIndex;
        }

        public Tile(int l, int x, int y, int tileIndex, int tileSheet)
        {
            this.l = l;
            this.x = x;
            this.y = y;
            this.tileIndex = tileIndex;
            this.tileSheet = tileSheet;
        }

        public static int getTileSheetIndex(string tileSheetName, ReadOnlyCollection<TileSheet> tileSheets)
        {
            for (int index = 0; index < tileSheets.Count; ++index)
            {
                if (((Component)tileSheets[index]).get_Id().Equals(tileSheetName))
                    return index;
            }
            return 0;
        }

        public static string getTileSheetName(int tileSheetIndex, ReadOnlyCollection<TileSheet> tileSheets)
        {
            if (tileSheetIndex < tileSheets.Count)
                return ((Component)tileSheets[tileSheetIndex]).get_Id();
            Log.Error((object)"tileSheetIndex out of range");
            return "";
        }
    }
}
