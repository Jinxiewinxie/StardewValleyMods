using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using WonderfulFarmLife.Framework.Config;
using WonderfulFarmLife.Framework.Constants;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace WonderfulFarmLife
{
    /// <summary>The main entry class for the mod.</summary>
    internal class WonderfulFarmLife : Mod
    {
        /*********
        ** Properties
        *********/
        private bool PetBowlFilled;

        private bool FarmSheetPatched;

        private ModConfig Config;

        /// <summary>The layout data.</summary>
        private DataModel LayoutConfig;

        /// <summary>The default tilesheet for tile overrides that don't specify one.</summary>
        private string DefaultTilesheet;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            this.Config = helper.ReadConfig<ModConfig>();
            this.LayoutConfig = helper.ReadJsonFile<DataModel>("data.json");
            if (!this.LayoutConfig.Tilesheets.ContainsKey("default"))
                throw new KeyNotFoundException("The required 'default' tilesheet isn't specified in data.json.");
            this.DefaultTilesheet = this.LayoutConfig.Tilesheets["default"];

            // hook up events
            SaveEvents.AfterLoad += this.ReceiveAfterLoad;
            LocationEvents.CurrentLocationChanged += this.Event_CurrentLocationChanged;
            TimeEvents.DayOfMonthChanged += this.Event_DayOfMonthChanged;
            ControlEvents.MouseChanged += this.Event_MouseChanged;
            ControlEvents.ControllerButtonPressed += this.Event_ControllerButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The event invoked after the player loads a saved game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ReceiveAfterLoad(object sender, EventArgs e)
        {
            // get farm data
            Farm farm = Game1.getFarm();
            FarmType farmType = (FarmType)Game1.whichFarm;

            // get layouts
            if (!this.LayoutConfig.Layouts.ContainsKey(farmType))
            {
                this.Monitor.Log($"The {farmType} farm isn't supported by the mod.", LogLevel.Warn);
                return;
            }
            LayoutConfig[] layouts = this.LayoutConfig
                .Layouts[farmType]
                .Where(p => p.ConfigFlag == null || this.GetConfigFlag(p.ConfigFlag))
                .ToArray();

            // resize tilesheet
            TileSheet tileSheet = farm.map.GetTileSheet("untitled tile sheet");
            tileSheet.SheetSize = new Size(tileSheet.SheetSize.Width, tileSheet.SheetSize.Height + 44);

            // apply layouts
            foreach (LayoutConfig layout in layouts)
            {
                // override tiles
                if (layout.Tiles != null)
                    this.Apply(farm, layout.Tiles.SelectMany(p => p.GetOverrides(this.LayoutConfig.Tilesheets, this.DefaultTilesheet)));

                // tile properties
                if (layout.TileProperties != null)
                {
                    foreach (TileProperty property in layout.TileProperties.SelectMany(p => p.GetProperties(this.LayoutConfig.Tilesheets, this.DefaultTilesheet)))
                        farm.setTileProperty(property.X, property.Y, property.Layer.ToString(), property.Key, property.Value);
                }

                // tilesheet properties
                if (layout.TileIndexProperties != null)
                {
                    foreach (TileIndexProperty property in layout.TileIndexProperties.SelectMany(p => p.GetProperties(this.LayoutConfig.Tilesheets, this.DefaultTilesheet)))
                        farm.map.GetTileSheet(property.Tilesheet).Properties.Add(property.Key, new PropertyValue(property.Value));
                }
            }
        }

        private void Event_SecondUpdateTick(object sender, EventArgs e)
        {
            Farm farm = Game1.getFarm();

            TileSheet tileSheet = farm.map.GetTileSheet("untitled tile sheet");
            var dictionary = this.Helper.Reflection.GetPrivateValue<Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice, "m_tileSheetTextures");
            Texture2D targetTexture = dictionary[tileSheet];
            int num = 1100;
            Dictionary<int, int> spriteOverrides = new Dictionary<int, int>();
            for (int key = 0; key < num; ++key)
                spriteOverrides.Add(key, 1975 + key);
            if (targetTexture != null)
                dictionary[tileSheet] = this.PatchTexture(targetTexture, Game1.currentSeason + "_wonderful.png", spriteOverrides, 16, 16);
            this.FarmSheetPatched = true;
            GameEvents.SecondUpdateTick -= this.Event_SecondUpdateTick;
        }

        private void Event_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            Farm farm = Game1.getFarm();

            if (e.NewLocation != farm)
                return;

            TileSheet tileSheet = farm.map.GetTileSheet("untitled tile sheet");

            if (this.Config.RemoveShippingBin)
                this.Helper.Reflection.GetPrivateField<TemporaryAnimatedSprite>(farm, "shippingBinLid").SetValue(null);
            var dictionary = this.Helper.Reflection.GetPrivateValue<Dictionary<TileSheet, Texture2D>>(Game1.mapDisplayDevice, "m_tileSheetTextures");
            Texture2D targetTexture = dictionary[tileSheet];
            int num = 1100;
            Dictionary<int, int> spriteOverrides = new Dictionary<int, int>();
            for (int key = 0; key < num; ++key)
                spriteOverrides.Add(key, 1975 + key);
            if (targetTexture != null)
                dictionary[tileSheet] = this.PatchTexture(targetTexture, Game1.currentSeason + "_wonderful.png", spriteOverrides, 16, 16);
            if (!this.FarmSheetPatched)
                GameEvents.SecondUpdateTick += this.Event_SecondUpdateTick;
        }

        private void Event_DayOfMonthChanged(object sender, EventArgs e)
        {
            if (!this.PetBowlFilled)
                return;

            Farm farm = Game1.getFarm();

            List<Pet> pets = this.findPets();
            if (pets == null)
                return;

            foreach (Pet pet in pets)
                pet.friendshipTowardFarmer = Math.Min(1000, pet.friendshipTowardFarmer + 6);

            farm.setMapTileIndex(52, 7, 2201, "Buildings");
            farm.setMapTileIndex(53, 7, 2202, "Buildings");
            this.PetBowlFilled = false;
        }

        private void Event_MouseChanged(object sender, EventArgsMouseStateChanged e)
        {
            if (Game1.hasLoadedGame)
                return;

            if (e.NewState.RightButton == ButtonState.Pressed && e.PriorState.RightButton != ButtonState.Pressed)
                this.CheckForAction();
            if (e.NewState.LeftButton != ButtonState.Pressed || e.PriorState.LeftButton == ButtonState.Pressed)
                return;
            this.ChangeTileOnClick();
            this.CheckForAction();
        }

        private void Event_ControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
        {
            if (Game1.hasLoadedGame || e.ButtonPressed != Buttons.A)
                return;
            this.CheckForAction();
        }


        /// <summary>Get the value of a config flag.</summary>
        /// <param name="name">The name of the config flag.</param>
        private bool GetConfigFlag(string name)
        {
            // get property
            PropertyInfo property = this.Config
                .GetType()
                .GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            // validate property
            if (property == null)
            {
                this.Monitor.Log($"The '{name}' config setting doesn't exist, assuming false.", LogLevel.Warn);
                return false;
            }
            if (property.PropertyType != typeof(bool))
            {
                this.Monitor.Log($"The '{name}' config setting isn't a bool flag, assuming false.", LogLevel.Warn);
                return false;
            }

            // check flag value
            return (bool)property.GetValue(this.Config);
        }

        private void ChangeTileOnClick()
        {
            if ((Game1.player.CurrentTool as WateringCan)?.WaterLeft > 0)
                return;

            Farm farm = Game1.getFarm();

            Vector2 vector2 = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize;
            if (!Utility.tileWithinRadiusOfPlayer((int)vector2.X, (int)vector2.Y, 1, Game1.player))
                vector2 = Game1.player.GetGrabTile();
            if (farm.getTileIndexAt((int)vector2.X, (int)vector2.Y, "Buildings") == 2201 || farm.getTileIndexAt((int)vector2.X, (int)vector2.Y, "Buildings") == 2202)
            {
                farm.setMapTileIndex(52, 7, 2204, "Buildings");
                farm.setMapTileIndex(53, 7, 2205, "Buildings");
                this.PetBowlFilled = true;
            }
        }

        private void CheckForAction()
        {
            if (Game1.player.UsingTool || Game1.numberOfSelectedItems != -1 || Game1.activeClickableMenu != null)
                return;


            Vector2 vector2 = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize;
            if (!Utility.tileWithinRadiusOfPlayer((int)vector2.X, (int)vector2.Y, 1, Game1.player))
                vector2 = Game1.player.GetGrabTile();

            xTile.Tiles.Tile tile = Game1.currentLocation.map.GetLayer("Buildings").PickTile(new Location((int)vector2.X * Game1.tileSize, (int)vector2.Y * Game1.tileSize), Game1.viewport.Size);
            PropertyValue propertyValue = null;
            tile?.Properties.TryGetValue("Action", out propertyValue);
            if (propertyValue != null)
            {
                if (propertyValue == "NewShippingBin")
                {
                    ItemGrabMenu itemGrabMenu = new ItemGrabMenu(null, true, false, Utility.highlightShippableObjects, this.shipItem, "", null, true, true, false);
                    itemGrabMenu.initializeUpperRightCloseButton();
                    itemGrabMenu.setBackgroundTransparency(false);
                    itemGrabMenu.setDestroyItemOnClick(true);
                    itemGrabMenu.initializeShippingBin();
                    Game1.activeClickableMenu = itemGrabMenu;
                    Game1.playSound("shwip");
                    if (Game1.player.facingDirection == 1)
                        Game1.player.Halt();
                    Game1.player.showCarrying();
                }
                if (propertyValue == "TelescopeMessage")
                {
                    Random random = new Random();
                    List<string> stringList = new List<string>
                    {
                        "I wish Neil DeGrasse Tyson was here.",
                        "I call this star mine... and that one, oh, and that one too.",
                        "Astronomy compels the soul to look upward, and leads us from this world to another.",
                        "Be glad of life, because it gives you the chance to love and to work and to play and to look up at the stars.",
                        "The sky is the ultimate art gallery just above us.",
                        "'Stop acting so small. You are the universe in estatic motion.' - Rumi",
                        "The universe doesn't give you what you ask for with your thoughts, it gives you what you demand with your actions.",
                        "The darkest nights produce the brightest stars.",
                        "'there wouldn't be a sky full of stars if we were all meant to wish on the same one.' - Frances Clark",
                        "Stars can't shine without darkness.",
                        "I have loved the stars too fondly to be fearful of the night.",
                        "I know nothing with any certainty, but the sight of the stars makes me dream."
                    };
                    Game1.drawObjectDialogue(stringList[random.Next(stringList.Count)]);
                }
            }
        }

        private void shipItem(Item i, Farmer who)
        {
            if (i == null)
                return;

            Farm farm = Game1.getFarm();

            farm.shippingBin.Add(i);
            if (i is Object)
                DelayedAction.playSoundAfterDelay("Ship", 0);
            farm.lastItemShipped = i;
            who.removeItemFromInventory(i);
            if (Game1.player.ActiveObject == null)
            {
                Game1.player.showNotCarrying();
                Game1.player.Halt();
            }
        }

        private List<Pet> findPets()
        {
            if (!Game1.player.hasPet())
                return null;

            List<Pet> pets =
                Game1.getFarm().characters.OfType<Pet>()
                .Concat(Utility.getHomeOfFarmer(Game1.player).characters.OfType<Pet>())
                .ToList();

            return pets.Any()
                ? pets
                : null;
        }

        /// <summary>Clear tiles from the map, which removes the tile, tile properties, and spawned objects on all layers (e.g. before placing custom tiles).</summary>
        /// <param name="location">The game location to patch.</param>
        /// <param name="areas">The tile areas to clear.</param>
        private void Clear(Farm location, IEnumerable<Rectangle> areas)
        {
            foreach (Rectangle area in areas)
            {
                // clear tiles
                foreach (Layer layer in location.map.Layers)
                {
                    for (int x = area.X; x <= area.Right; x++)
                    {
                        for (int y = area.Y; y <= area.Bottom; y++)
                        {
                            location.removeTile(x, y, layer.Id);
                            location.waterTiles[x, y] = false;
                        }
                    }
                }

                // clear spawned objects
                foreach (Vector2 tile in location.terrainFeatures.Keys)
                {
                    if (area.Contains((int)tile.X, (int)tile.Y))
                        location.terrainFeatures.Remove(tile);
                }
                location.largeTerrainFeatures.RemoveAll(feature => area.Contains((int)feature.tilePosition.X, (int)feature.tilePosition.Y));
                location.resourceClumps.RemoveAll(clump => area.Intersects(clump.getBoundingBox(clump.tile)));

            }
        }

        /// <summary>Apply tile overrides to the map.</summary>
        /// <param name="location">The game location to patch.</param>
        /// <param name="tiles">The tile overrides to apply.</param>
        private void Apply(Farm location, IEnumerable<TileOverride> tiles)
        {
            foreach (TileOverride tile in tiles)
            {
                // reset tile
                if (tile.TileID == null)
                {
                    location.removeTile(tile.X, tile.Y, tile.LayerName);
                    location.waterTiles[tile.X, tile.Y] = false;

                    foreach (LargeTerrainFeature feature in location.largeTerrainFeatures)
                    {
                        if (feature.tilePosition.X == tile.X && feature.tilePosition.Y == tile.Y)
                        {
                            location.largeTerrainFeatures.Remove(feature);
                            break;
                        }
                    }
                }

                // overwrite tile
                else
                {
                    Layer layer = location.map.GetLayer(tile.LayerName);
                    var layerTile = layer.Tiles[tile.X, tile.Y];
                    if (layerTile == null || layerTile.TileSheet.Id != tile.Tilesheet)
                    {
                        var tilesheet = location.map.GetTileSheet(tile.Tilesheet);
                        layer.Tiles[tile.X, tile.Y] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tile.TileID.Value);
                    }
                    else
                        location.setMapTileIndex(tile.X, tile.Y, tile.TileID.Value, layer.Id);
                }
            }
        }

        private Texture2D PatchTexture(Texture2D targetTexture, string overridingTexturePath, Dictionary<int, int> spriteOverrides, int gridWidth, int gridHeight)
        {
            int bottom = this.GetSourceRect(spriteOverrides.Values.Max(), targetTexture, gridWidth, gridHeight).Bottom;
            if (bottom > targetTexture.Height)
            {
                Color[] data1 = new Color[targetTexture.Width * targetTexture.Height];
                targetTexture.GetData(data1);
                Color[] data2 = new Color[targetTexture.Width * bottom];
                Array.Copy(data1, data2, data1.Length);
                targetTexture = new Texture2D(Game1.graphics.GraphicsDevice, targetTexture.Width, bottom);
                targetTexture.SetData(data2);
            }
            using (FileStream fileStream = File.Open(Path.Combine(this.Helper.DirectoryPath, "overrides", overridingTexturePath), FileMode.Open))
            {
                Texture2D texture = Texture2D.FromStream(Game1.graphics.GraphicsDevice, fileStream);
                foreach (KeyValuePair<int, int> spriteOverride in spriteOverrides)
                {
                    Color[] data = new Color[gridWidth * gridHeight];
                    texture.GetData(0, this.GetSourceRect(spriteOverride.Key, texture, gridWidth, gridHeight), data, 0, data.Length);
                    targetTexture.SetData(0, this.GetSourceRect(spriteOverride.Value, targetTexture, gridWidth, gridHeight), data, 0, data.Length);
                }
            }
            return targetTexture;
        }

        private Rectangle GetSourceRect(int index, Texture2D texture, int gridWidth, int gridHeight)
        {
            return new Rectangle(index % (texture.Width / gridWidth) * gridWidth, index / (texture.Width / gridWidth) * gridHeight, gridWidth, gridHeight);
        }
    }
}
