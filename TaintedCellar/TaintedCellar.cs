using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.ObjectModel;
using xTile.Tiles;

namespace TaintedCellar
{
    public class TaintedCellar : Mod
    {
        private static string modPath = "";

        public static CellarConfig ModConfig { get; private set; }

        public TaintedCellar()
        {
            base.\u002Ector();
        }

        public virtual void Entry(params object[] objects)
        {
            TaintedCellar.TaintedCellar.modPath = this.get_PathOnDisk();
            TaintedCellar.TaintedCellar.ModConfig = (CellarConfig)ConfigExtensions.InitializeConfig<CellarConfig>((M0)new CellarConfig(), this.get_BaseConfigPath());
            GameEvents.add_UpdateTick(new EventHandler(TaintedCellar.TaintedCellar.Event_UpdateTick));
            LocationEvents.add_CurrentLocationChanged(new EventHandler<EventArgsCurrentLocationChanged>(TaintedCellar.TaintedCellar.Event_CurrentLocationChanged));
        }

        private static void Event_UpdateTick(object sender, EventArgs e)
        {
            if (((List<GameLocation>)Game1.locations).Count < 47)
                return;
            if (Game1.getLocationFromName("TaintedCellarMap") != null)
            {
                GameEvents.remove_UpdateTick(new EventHandler(TaintedCellar.TaintedCellar.Event_UpdateTick));
            }
            else
            {
                GameLocation gameLocation = new GameLocation(TaintedCellar.TaintedCellar.LoadMap(Path.Combine(TaintedCellar.TaintedCellar.modPath, "TaintedCellarMap.xnb")), "TaintedCellarMap");
                ((Map)gameLocation.map).GetTileSheet("Ztainted_cellar").set_ImageSource("..\\mods\\TaintedCellar\\Ztainted_cellar");
                ((Map)gameLocation.map).LoadTileSheets((IDisplayDevice)Game1.mapDisplayDevice);
                gameLocation.isOutdoors = (__Null)0;
                gameLocation.isFarm = (__Null)1;
                ((List<GameLocation>)Game1.locations).Add(gameLocation);
                int num1 = (TaintedCellar.TaintedCellar.ModConfig.flipCellarEntrance ? 69 : 57) + TaintedCellar.TaintedCellar.ModConfig.xPositionOffset;
                int num2 = 12 + TaintedCellar.TaintedCellar.ModConfig.yPositionOffset;
                gameLocation.setTileProperty(3, 3, "Buildings", "Action", "Warp " + (object)num1 + " " + (object)num2 + " Farm");
                GameEvents.remove_UpdateTick(new EventHandler(TaintedCellar.TaintedCellar.Event_UpdateTick));
            }
        }

        private static void Event_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (TaintedCellar.TaintedCellar.ModConfig.onlyUnlockAfterFinalHouseUpgrade && ((Farmer)Game1.player).get_HouseUpgradeLevel() != 2 || e.get_NewLocation() != Game1.getFarm())
                return;
            Farm farm = Game1.getFarm();
            ((Map)((GameLocation)farm).map).AddTileSheet(new TileSheet("Zpaths_objects_cellar", (Map)((GameLocation)farm).map, "..\\mods\\TaintedCellar\\Zpaths_objects_cellar", new Size(32, 68), new Size(16, 16)));
            ((Map)((GameLocation)farm).map).LoadTileSheets((IDisplayDevice)Game1.mapDisplayDevice);
            if (TaintedCellar.TaintedCellar.ModConfig.flipCellarEntrance)
            {
                TaintedCellar.TaintedCellar.PatchMap((GameLocation)farm, TaintedCellar.TaintedCellar.CellarRightSideEdits((GameLocation)farm));
                int num1 = 68 + TaintedCellar.TaintedCellar.ModConfig.xPositionOffset;
                int num2 = 69 + TaintedCellar.TaintedCellar.ModConfig.xPositionOffset;
                int num3 = 11 + TaintedCellar.TaintedCellar.ModConfig.yPositionOffset;
                int num4 = 12 + TaintedCellar.TaintedCellar.ModConfig.yPositionOffset;
                ((GameLocation)farm).setTileProperty(num1, num3, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
                ((GameLocation)farm).setTileProperty(num1, num4, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
            }
            if (!TaintedCellar.TaintedCellar.ModConfig.flipCellarEntrance)
            {
                TaintedCellar.TaintedCellar.PatchMap((GameLocation)farm, TaintedCellar.TaintedCellar.CellarLeftSideEdits((GameLocation)farm));
                int num1 = 57 + TaintedCellar.TaintedCellar.ModConfig.xPositionOffset;
                int num2 = 58 + TaintedCellar.TaintedCellar.ModConfig.xPositionOffset;
                int num3 = 11 + TaintedCellar.TaintedCellar.ModConfig.yPositionOffset;
                int num4 = 12 + TaintedCellar.TaintedCellar.ModConfig.yPositionOffset;
                ((GameLocation)farm).setTileProperty(num2, num3, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
                ((GameLocation)farm).setTileProperty(num2, num4, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
            }
          ((GameLocation)farm).setTileProperty(68, 11, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
            ((GameLocation)farm).setTileProperty(68, 12, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
            ((IDictionary<string, PropertyValue>)((Component)((Map)((GameLocation)farm).map).GetTileSheet("Zpaths_objects_cellar")).get_Properties()).Add("@TileIndex@1865@Passable", new PropertyValue(true));
            ((IDictionary<string, PropertyValue>)((Component)((Map)((GameLocation)farm).map).GetTileSheet("Zpaths_objects_cellar")).get_Properties()).Add("@TileIndex@1897@Passable", new PropertyValue(true));
            ((IDictionary<string, PropertyValue>)((Component)((Map)((GameLocation)farm).map).GetTileSheet("Zpaths_objects_cellar")).get_Properties()).Add("@TileIndex@1866@Passable", new PropertyValue(true));
            ((IDictionary<string, PropertyValue>)((Component)((Map)((GameLocation)farm).map).GetTileSheet("Zpaths_objects_cellar")).get_Properties()).Add("@TileIndex@1898@Passable", new PropertyValue(true));
            LocationEvents.remove_CurrentLocationChanged(new EventHandler<EventArgsCurrentLocationChanged>(TaintedCellar.TaintedCellar.Event_CurrentLocationChanged));
        }

        private static Map LoadMap(string filePath)
        {
            Path.GetExtension(filePath);
            Map map = new ContentManager((IServiceProvider)new GameServiceContainer(), Path.GetDirectoryName(filePath)).Load<Map>(Path.GetFileNameWithoutExtension(filePath));
            if (map == null)
                throw new FileLoadException();
            return map;
        }

        private static List<Tile> CellarRightSideEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.getTileSheetIndex("Zpaths_objects_cellar", ((Map)gl.map).get_TileSheets());
            int x1 = 68 + TaintedCellar.TaintedCellar.ModConfig.xPositionOffset;
            int x2 = 69 + TaintedCellar.TaintedCellar.ModConfig.xPositionOffset;
            int y1 = 11 + TaintedCellar.TaintedCellar.ModConfig.yPositionOffset;
            int y2 = 12 + TaintedCellar.TaintedCellar.ModConfig.yPositionOffset;
            return new List<Tile>()
      {
        new Tile(1, x1, y1, 1864, tileSheetIndex),
        new Tile(1, x2, y1, 1865, tileSheetIndex),
        new Tile(1, x1, y2, 1896, tileSheetIndex),
        new Tile(1, x2, y2, 1897, tileSheetIndex)
      };
        }

        private static List<Tile> CellarLeftSideEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.getTileSheetIndex("Zpaths_objects_cellar", ((Map)gl.map).get_TileSheets());
            int x1 = 57 + TaintedCellar.TaintedCellar.ModConfig.xPositionOffset;
            int x2 = 58 + TaintedCellar.TaintedCellar.ModConfig.xPositionOffset;
            int y1 = 11 + TaintedCellar.TaintedCellar.ModConfig.yPositionOffset;
            int y2 = 12 + TaintedCellar.TaintedCellar.ModConfig.yPositionOffset;
            return new List<Tile>()
      {
        new Tile(1, x1, y1, 1866, tileSheetIndex),
        new Tile(1, x2, y1, 1867, tileSheetIndex),
        new Tile(1, x1, y2, 1898, tileSheetIndex),
        new Tile(1, x2, y2, 1899, tileSheetIndex)
      };
        }

        private static void PatchMap(GameLocation gl, List<Tile> tileArray)
        {
            foreach (Tile tile in tileArray)
            {
                if (tile.tileIndex < 0)
                {
                    gl.removeTile(tile.x, tile.y, ((Component)((Map)gl.map).get_Layers()[tile.l]).get_Id());
                    ((bool[,])gl.waterTiles)[tile.x, tile.y] = false;
                    using (List<LargeTerrainFeature>.Enumerator enumerator = ((List<LargeTerrainFeature>)gl.largeTerrainFeatures).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            LargeTerrainFeature current = enumerator.Current;
                            // ISSUE: explicit reference operation
                            // ISSUE: cast to a reference type
                            // ISSUE: explicit reference operation
                            // ISSUE: explicit reference operation
                            // ISSUE: cast to a reference type
                            // ISSUE: explicit reference operation
                            if ((double)(^ (Vector2 &) @current.tilePosition).X == (double)tile.x && (double)(^ (Vector2 &) @current.tilePosition).Y == (double)tile.y)
              {
                                ((List<LargeTerrainFeature>)gl.largeTerrainFeatures).Remove(current);
                                break;
                            }
                        }
                    }
                }
                else if (((Map)gl.map).get_Layers()[tile.l].get_Tiles().get_Item(tile.x, tile.y) == null || ((Component)((Map)gl.map).get_Layers()[tile.l].get_Tiles().get_Item(tile.x, tile.y).get_TileSheet()).get_Id() != Tile.getTileSheetName(tile.tileSheet, ((Map)gl.map).get_TileSheets()))
                    ((Map)gl.map).get_Layers()[tile.l].get_Tiles().set_Item(tile.x, tile.y, (Tile)new StaticTile(((Map)gl.map).get_Layers()[tile.l], ((Map)gl.map).get_TileSheets()[tile.tileSheet], (BlendMode)0, tile.tileIndex));
                else
                    gl.setMapTileIndex(tile.x, tile.y, tile.tileIndex, ((Component)((Map)gl.map).get_Layers()[tile.l]).get_Id());
            }
        }
    }
}
