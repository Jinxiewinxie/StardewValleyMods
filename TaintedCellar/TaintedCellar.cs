using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace TaintedCellar
{
    /// <summary>The main entry class called by SMAPI.</summary>
    public class TaintedCellar : Mod
    {
        /*********
        ** Properties
        *********/
        /// <summary>The mod configuration.</summary>
        private CellarConfig Config;


        /*********
        ** Properties
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<CellarConfig>();

            SaveEvents.AfterLoad += this.OnAfterLoad;
            LocationEvents.CurrentLocationChanged += this.OnCurrentLocationChanged;
        }

        /*********
        ** Private methods
        *********/
        /****
        ** Events
        ****/
        /// <summary>The method called after the player loads the world.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnAfterLoad(object sender, EventArgs e)
        {
            this.AddLocation();
        }

        /// <summary>The method called when the player enters a new area.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (this.Config.OnlyUnlockAfterFinalHouseUpgrade && Game1.player.HouseUpgradeLevel < 3 || e.NewLocation != Game1.getFarm())
                return;

            this.PatchMap(Game1.getFarm());
            LocationEvents.CurrentLocationChanged -= this.OnCurrentLocationChanged;
        }


        /****
        ** Methods
        ****/
        /// <summary>Add the cellar location to the world.</summary>
        private void AddLocation()
        {
            GameLocation location = new GameLocation(this.Helper.Content.Load<Map>(@"assets\TaintedCellarMap.xnb"), "TaintedCellarMap")
            {
                isOutdoors = false,
                isFarm = true
            };

            int entranceX = (this.Config.FlipCellarEntrance ? 69 : 57) + this.Config.XPositionOffset;
            int entranceY = 12 + this.Config.YPositionOffset;
            location.setTileProperty(3, 3, "Buildings", "Action", $"Warp {entranceX} {entranceY} Farm");

            Game1.locations.Add(location);
        }

        /// <summary>Patch the farm map to add the cellar entrance.</summary>
        private void PatchMap(Farm farm)
        {
            farm.map.AddTileSheet(new TileSheet("Zpaths_objects_cellar", farm.map, this.Helper.Content.GetActualAssetKey(@"assets\Zpaths_objects_cellar.xnb"), new Size(32, 68), new Size(16, 16)));
            farm.map.LoadTileSheets(Game1.mapDisplayDevice);
            if (this.Config.FlipCellarEntrance)
            {
                this.PatchMap(farm, this.GetCellarRightSideEdits());
                int entranceX = 68 + this.Config.XPositionOffset;
                int entranceY1 = 11 + this.Config.YPositionOffset;
                int entranceY2 = 12 + this.Config.YPositionOffset;
                farm.setTileProperty(entranceX, entranceY1, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
                farm.setTileProperty(entranceX, entranceY2, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
            }
            else
            {
                this.PatchMap(farm, this.GetCellarLeftSideEdits());
                int entranceX = 58 + this.Config.XPositionOffset;
                int entranceY1 = 11 + this.Config.YPositionOffset;
                int entranceY2 = 12 + this.Config.YPositionOffset;
                farm.setTileProperty(entranceX, entranceY1, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
                farm.setTileProperty(entranceX, entranceY2, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
            }
            farm.setTileProperty(68, 11, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");
            farm.setTileProperty(68, 12, "Buildings", "Action", "Warp 3 4 TaintedCellarMap");

            var properties = farm.map.GetTileSheet("Zpaths_objects_cellar").Properties;
            foreach (int tileID in new[] { 1865, 1897, 1866, 1898 })
                properties.Add($"@TileIndex@{tileID}@Passable", new PropertyValue(true));
        }

        /// <summary>Get the tiles to change for the right-side cellar entrance.</summary>
        private Tile[] GetCellarRightSideEdits()
        {
            string tilesheet = "Zpaths_objects_cellar";
            int x1 = 68 + this.Config.XPositionOffset;
            int x2 = 69 + this.Config.XPositionOffset;
            int y1 = 11 + this.Config.YPositionOffset;
            int y2 = 12 + this.Config.YPositionOffset;
            return new[]
            {
                new Tile(1, x1, y1, 1864, tilesheet),
                new Tile(1, x2, y1, 1865, tilesheet),
                new Tile(1, x1, y2, 1896, tilesheet),
                new Tile(1, x2, y2, 1897, tilesheet)
            };
        }

        /// <summary>Get the tiles to change for the right-side cellar entrance.</summary>
        private Tile[] GetCellarLeftSideEdits()
        {
            string tilesheet = "Zpaths_objects_cellar";
            int x1 = 57 + this.Config.XPositionOffset;
            int x2 = 58 + this.Config.XPositionOffset;
            int y1 = 11 + this.Config.YPositionOffset;
            int y2 = 12 + this.Config.YPositionOffset;
            return new[]
            {
                new Tile(1, x1, y1, 1866, tilesheet),
                new Tile(1, x2, y1, 1867, tilesheet),
                new Tile(1, x1, y2, 1898, tilesheet),
                new Tile(1, x2, y2, 1899, tilesheet)
            };
        }

        /// <summary>Apply a set of map overrides to the farm map.</summary>
        /// <param name="farm">The farm to patch.</param>
        /// <param name="tiles">The tile overrides to apply.</param>
        private void PatchMap(Farm farm, Tile[] tiles)
        {
            foreach (Tile tile in tiles)
            {
                if (tile.TileIndex < 0)
                {
                    farm.removeTile(tile.X, tile.Y, farm.map.Layers[tile.LayerIndex].Id);
                    farm.waterTiles[tile.X, tile.Y] = false;

                    foreach (LargeTerrainFeature feature in farm.largeTerrainFeatures)
                    {
                        if (feature.tilePosition.X == tile.X && feature.tilePosition.Y == tile.Y)
                            farm.largeTerrainFeatures.Remove(feature);
                    }
                }
                else
                {
                    Layer layer = farm.map.Layers[tile.LayerIndex];
                    xTile.Tiles.Tile mapTile = layer.Tiles[tile.X, tile.Y];
                    if (mapTile == null || mapTile.TileSheet.Id != tile.Tilesheet)
                        layer.Tiles[tile.X, tile.Y] = new StaticTile(layer, farm.map.GetTileSheet(tile.Tilesheet), 0, tile.TileIndex);
                    else
                        farm.setMapTileIndex(tile.X, tile.Y, tile.TileIndex, layer.Id);
                }
            }
        }
    }
}