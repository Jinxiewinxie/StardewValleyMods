// Decompiled with JetBrains decompiler
// Type: WonderfulFarmLife.Tile
// Assembly: WonderfulFarmLife, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 811ABA56-734C-41D0-915D-FE94C07965A0
// Assembly location: C:\Program Files (x86)\GalaxyClient\Games\Stardew Valley\Mods\WonderfulFarmLife\WonderfulFarmLife.dll

using System.Collections.ObjectModel;
using xTile.Layers;
using xTile.ObjectModel;
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
        if (((Component) tileSheets[index]).get_Id().Equals(tsn))
          return index;
      }
      return 0;
    }

    public static string getTileSheetName(int tsi, ReadOnlyCollection<TileSheet> tileSheets)
    {
      if (tsi >= tileSheets.Count)
        return ((Component) tileSheets[0]).get_Id();
      return ((Component) tileSheets[tsi]).get_Id();
    }

    public static int getLayerIndex(string ln, ReadOnlyCollection<Layer> layers)
    {
      for (int index = 0; index < layers.Count; ++index)
      {
        if (((Component) layers[index]).get_Id().Equals(ln))
          return index;
      }
      return 0;
    }

    public static string getLayerName(int li, ReadOnlyCollection<Layer> layers)
    {
      if (li >= layers.Count)
        return ((Component) layers[0]).get_Id();
      return ((Component) layers[li]).get_Id();
    }
  }
}
