using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace TaintedCellar
{
    public class TaintedCellar : Mod
    {
        private static string ModPath = "";

        public static CellarConfig ModConfig { get; private set; }

        public virtual void Entry(params object[] objects)
        {
            TaintedCellar.ModPath = this.PathOnDisk;
            ModConfig = new CellarConfig().InitializeConfig(this.BaseConfigPath);
            GameEvents.UpdateTick += TaintedCellar.Event_UpdateTick;
            LocationEvents.CurrentLocationChanged += TaintedCellar.Event_CurrentLocationChanged;
        }

        private static void Event_UpdateTick(object sender, EventArgs e)
        {
            if (Game1.locations.Count < 47)
                return;
            if (Game1.getLocationFromName("TaintedCellarMap") != null)
                GameEvents.UpdateTick -= TaintedCellar.Event_UpdateTick;
            else
            {
                GameLocation gameLocation = new GameLocation(TaintedCellar.LoadMap(Path.Combine(TaintedCellar.ModPath, "TaintedCellarMap.xnb")), "TaintedCellarMap");
                gameLocation.map.GetTileSheet("Ztainted_cellar").ImageSource = "..\\mods\\TaintedCellar\\Ztainted_cellar";
                gameLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
                gameLocation.isOutdoors = false;
                gameLocation.isFarm = true;
                Game1.locations.Add(gameLocation);
                int entranceX = (TaintedCellar.ModConfig.flipCellarEntrance ? 69 : 57) + TaintedCellar.ModConfig.xPositionOffset;
                int entranceY = 12 + TaintedCellar.ModConfig.yPositionOffset;
                gameLocation.setTileProperty(3, 3, "Buildings", "Action", $"Warp {entranceX} {entranceY} Farm");
                GameEvents.UpdateTick -= TaintedCellar.Event_UpdateTick;
            }
        }

        private static void Event_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (TaintedCellar.ModConfig.onlyUnlockAfterFinalHouseUpgrade && Game1.player.HouseUpgradeLevel != 2 || e.NewLocation != Game1.getFarm())
                return;
            Farm farm = Game1.getFarm();
            farm.map.AddTileSheet(new TileSheet("Zpaths_objects_cellar", farm.map, "..\\mods\\TaintedCellar\\Zpaths_objects_cellar", new Size(32, 68), new Size(16, 16)));
            farm.map.LoadTileSheets(Game1.mapDisplayDevice);
            if (TaintedCellar.ModConfig.flipCellarEntrance)
            {
                TaintedCellar.PatchMap(farm, TaintedCellar.CellarRightSideEdits(farm));
                int entranceX = 68 + TaintedCellar.ModConfig.xPositionOffset;
                int entranceY1 = 11 + TaintedCellar.ModConfig.yPositionOffset;
                int entranceY2 = 12 + TaintedCellar.ModConfig.yPositionOffset;
                farm.setTileProperty(entranceX, entranceY1, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
                farm.setTileProperty(entranceX, entranceY2, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
            }
            else
            {
                TaintedCellar.PatchMap(farm, TaintedCellar.CellarLeftSideEdits(farm));
                int entranceX = 58 + TaintedCellar.ModConfig.xPositionOffset;
                int entranceY1 = 11 + TaintedCellar.ModConfig.yPositionOffset;
                int entranceY2 = 12 + TaintedCellar.ModConfig.yPositionOffset;
                farm.setTileProperty(entranceX, entranceY1, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
                farm.setTileProperty(entranceX, entranceY2, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
            }
            farm.setTileProperty(68, 11, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
            farm.setTileProperty(68, 12, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");

            var properties = farm.map.GetTileSheet("Zpaths_objects_cellar").Properties;
            foreach (int tileID in new[] { 1865, 1897, 1866, 1898 })
                properties.Add($"@TileIndex@{tileID}@Passable", new PropertyValue(true));

            LocationEvents.CurrentLocationChanged -= TaintedCellar.Event_CurrentLocationChanged;
        }

        private static Map LoadMap(string filePath)
        {
            Map map = new ContentManager(new GameServiceContainer(), Path.GetDirectoryName(filePath)).Load<Map>(Path.GetFileNameWithoutExtension(filePath));
            if (map == null)
                throw new FileLoadException();
            return map;
        }

        private static List<Tile> CellarRightSideEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("Zpaths_objects_cellar", gl.map.TileSheets);
            int x1 = 68 + TaintedCellar.ModConfig.xPositionOffset;
            int x2 = 69 + TaintedCellar.ModConfig.xPositionOffset;
            int y1 = 11 + TaintedCellar.ModConfig.yPositionOffset;
            int y2 = 12 + TaintedCellar.ModConfig.yPositionOffset;
            return new List<Tile>
            {
                new Tile(1, x1, y1, 1864, tileSheetIndex),
                new Tile(1, x2, y1, 1865, tileSheetIndex),
                new Tile(1, x1, y2, 1896, tileSheetIndex),
                new Tile(1, x2, y2, 1897, tileSheetIndex)
            };
        }

        private static List<Tile> CellarLeftSideEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("Zpaths_objects_cellar", gl.map.TileSheets);
            int x1 = 57 + TaintedCellar.ModConfig.xPositionOffset;
            int x2 = 58 + TaintedCellar.ModConfig.xPositionOffset;
            int y1 = 11 + TaintedCellar.ModConfig.yPositionOffset;
            int y2 = 12 + TaintedCellar.ModConfig.yPositionOffset;
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
                if (tile.TileIndex < 0)
                {
                    gl.removeTile(tile.X, tile.Y, gl.map.Layers[tile.LayerIndex].Id);
                    gl.waterTiles[tile.X, tile.Y] = false;

                    foreach (LargeTerrainFeature feature in gl.largeTerrainFeatures)
                    {
                        if (feature.tilePosition.X == tile.X && feature.tilePosition.Y == tile.Y)
                            gl.largeTerrainFeatures.Remove(feature);
                    }
                }
                else if (gl.map.Layers[tile.LayerIndex].Tiles[tile.X, tile.Y] == null || (gl.map.Layers[tile.LayerIndex].Tiles[tile.X, tile.Y].TileSheet).Id != Tile.GetTileSheetName(tile.TileSheet, gl.map.TileSheets))
                    gl.map.Layers[tile.LayerIndex].Tiles[tile.X, tile.Y] = new StaticTile(gl.map.Layers[tile.LayerIndex], gl.map.TileSheets[tile.TileSheet], 0, tile.TileIndex);
                else
                    gl.setMapTileIndex(tile.X, tile.Y, tile.TileIndex, gl.map.Layers[tile.LayerIndex].Id);
            }
        }
    }
}
