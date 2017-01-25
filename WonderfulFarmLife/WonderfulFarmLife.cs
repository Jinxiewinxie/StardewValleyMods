using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using WonderfulFarmLife.Constants;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace WonderfulFarmLife
{
    internal class WonderfulFarmLife : Mod
    {
        public bool PetBowlFilled;
        public bool FarmSheetPatched;
        public int TickCount = 0;
        public ModConfig Config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            GameEvents.UpdateTick += this.Event_UpdateTick;
            LocationEvents.CurrentLocationChanged += this.Event_CurrentLocationChanged;
            TimeEvents.DayOfMonthChanged += this.Event_DayOfMonthChanged;
            ControlEvents.MouseChanged += this.Event_MouseChanged;
            ControlEvents.ControllerButtonPressed += this.Event_ControllerButtonPressed;
        }

        private void Event_UpdateTick(object sender, EventArgs e)
        {
            Farm farm = Game1.getFarm();
            if (!Game1.hasLoadedGame || Game1.currentLocation != farm)
                return;

            TileSheet tileSheet = farm.map.TileSheets[Tile.GetTileSheetIndex("untitled tile sheet", farm.map.TileSheets)];
            tileSheet.SheetSize = new Size(tileSheet.SheetSize.Width, tileSheet.SheetSize.Height + 44);
            List<ResourceClump> resourceClumps = farm.resourceClumps;

            foreach (ResourceClump clump in farm.resourceClumps)
            {
                if (clump.occupiesTile(71, 13) || clump.occupiesTile(72, 13) || clump.occupiesTile(71, 14) || clump.occupiesTile(72, 14))
                    resourceClumps.Remove(clump);
            }
            if (this.Config.ShowFarmStand)
            {
                this.PatchMap(farm, this.FarmStandEdits(farm));
                farm.setTileProperty(74, 15, "Buildings", "Action", "NewShippingBin");
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2058@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2083@Passable", new PropertyValue(true));
            }
            if (this.Config.EditPath)
                this.PatchMap(farm, this.PathEdits(farm));
            if (this.Config.RemovePathAlltogether)
            {
                this.PatchMap(farm, this.PathEdits(farm));
                this.PatchMap(farm, this.RemovePathEdits(farm));
            }
            if (this.Config.RemoveShippingBin)
                this.PatchMap(farm, this.RemoveShippingBinEdits(farm));
            if (this.Config.ShowPatio)
            {
                this.PatchMap(farm, this.PatioEdits(farm));
                farm.setTileProperty(68, 6, "Buildings", "Action", "kitchen");
                farm.setTileProperty(69, 6, "Buildings", "Action", "kitchen");
            }
            if (this.Config.ShowBinClutter)
            {
                this.PatchMap(farm, this.YardGardenEditsAndBinClutter(farm));
                farm.setTileProperty(75, 4, "Buildings", "Action", "Jukebox");
                farm.setTileProperty(75, 5, "Buildings", "Action", "Jukebox");
            }
            if (this.Config.AddDogHouse)
            {
                this.PatchMap(farm, this.DogHouseEdits(farm));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2718@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2719@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2720@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2721@Passable", new PropertyValue(true));
            }
            if (this.Config.AddGreenHouseArch)
                this.PatchMap(farm, this.GreenHouseArchEdits(farm));
            if (this.Config.ShowPicnicBlanket)
                this.PatchMap(farm, this.PicnicBlanketEdits(farm));
            if (this.Config.ShowPicnicTable)
                this.PatchMap(farm, this.PicnicAreaTableEdits(farm));
            if (this.Config.ShowTreeSwing)
            {
                this.PatchMap(farm, this.TreeSwingEdits(farm));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2944@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2969@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2941@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2942@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2966@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2967@Passable", new PropertyValue(true));
            }
            if (this.Config.AddStoneBridge)
                this.PatchMap(farm, this.StoneBridgeEdits(farm));
            if (this.Config.AddTelescopeArea)
            {
                this.PatchMap(farm, this.TelescopeEdits(farm));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2619@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2620@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2621@Passable", new PropertyValue(true));
                farm.setTileProperty(30, 2, "Buildings", "Action", "TelescopeMessage");
            }
            if (this.Config.ShowMemorialArea)
                this.PatchMap(farm, this.MemorialArea(farm));
            if (this.Config.ShowMemorialAreaArch)
                this.PatchMap(farm, this.MemorialAreaArch(farm));
            if (this.Config.UsingTSE)
                this.PatchMap(farm, this.TegoFixes(farm));
            GameEvents.UpdateTick -= this.Event_UpdateTick;
        }

        private void Event_SecondUpdateTick(object sender, EventArgs e)
        {
            Farm farm = Game1.getFarm();

            TileSheet tileSheet = farm.map.TileSheets[Tile.GetTileSheetIndex("untitled tile sheet", farm.map.TileSheets)];
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

            TileSheet tileSheet = farm.map.TileSheets[Tile.GetTileSheetIndex("untitled tile sheet", farm.map.TileSheets)];

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

        private List<Tile> PathEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
            {
            new Tile(TileLayer.Back, 70, 15, 2232, tileSheetIndex),
            new Tile(TileLayer.Back, 71, 15, 2232, tileSheetIndex),
            new Tile(TileLayer.Back, 72, 15, 2232, tileSheetIndex),
            new Tile(TileLayer.Back, 73, 15, 2232, tileSheetIndex),
            new Tile(TileLayer.Back, 69, 16, 200, tileSheetIndex),
            new Tile(TileLayer.Back, 70, 16, 179, tileSheetIndex),
            new Tile(TileLayer.Back, 71, 16, 205, tileSheetIndex),
            new Tile(TileLayer.Back, 72, 16, 179, tileSheetIndex),
            new Tile(TileLayer.Back, 73, 16, 205, tileSheetIndex),
            new Tile(TileLayer.Back, 72, 17, 227, tileSheetIndex),
            new Tile(TileLayer.Back, 73, 17, 227, tileSheetIndex),
            new Tile(TileLayer.Back, 74, 17, 227, tileSheetIndex),
            new Tile(TileLayer.Back, 75, 17, 624, tileSheetIndex)
            };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> GreenHouseArchEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
            {
            new Tile(TileLayer.AlwaysFront, 25, 10, 2626, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 26, 10, 2627, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 27, 10, 2628, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 28, 10, 2629, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 29, 10, 2630, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 30, 10, 2631, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 31, 10, 2632, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 32, 10, 2633, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 33, 10, 2635, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 24, 11, 2650, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 25, 11, 2651, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 26, 11, 2652, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 27, 11, 2653, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 28, 11, 2654, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 29, 11, 2655, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 30, 11, 2656, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 31, 11, 2657, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 32, 11, 2658, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 33, 11, 2659, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 24, 12, 2675, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 25, 12, 2676, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 26, 12, 2677, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 27, 12, 2678, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 28, 12, 2679, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 29, 12, 2680, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 30, 12, 2681, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 31, 12, 2682, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 32, 12, 2683, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 33, 12, 2684, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 24, 13, 2700, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 25, 13, 2701, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 26, 13, 2702, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 27, 13, 2703, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 28, 13, 2704, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 29, 13, 2705, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 30, 13, 2706, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 25, 14, 2726, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 26, 14, 2727, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 27, 14, 2728, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 28, 14, 2729, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 29, 14, 2730, tileSheetIndex),
            new Tile(TileLayer.AlwaysFront, 30, 14, 2731, tileSheetIndex),
            new Tile(TileLayer.Front, 25, 15, 2751, tileSheetIndex),
            new Tile(TileLayer.Front, 26, 15, 2752, tileSheetIndex),
            new Tile(TileLayer.Front, 27, 15, 2753, tileSheetIndex),
            new Tile(TileLayer.Front, 28, 15, 2754, tileSheetIndex),
            new Tile(TileLayer.Front, 29, 15, 2755, tileSheetIndex),
            new Tile(TileLayer.Front, 30, 15, 2756, tileSheetIndex),
            new Tile(TileLayer.Buildings, 25, 16, 2776, tileSheetIndex),
            new Tile(TileLayer.Buildings, 26, 16, 2777, tileSheetIndex),
            new Tile(TileLayer.Buildings, 30, 16, 2781, tileSheetIndex)
            };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> TegoFixes(GameLocation gl)
        {
            Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
            {
            new Tile(TileLayer.Front, 58, 5, -1),
            new Tile(TileLayer.Front, 59, 5, -1),
            new Tile(TileLayer.Front, 60, 5, -1),
            new Tile(TileLayer.Front, 61, 5, -1),
            new Tile(TileLayer.Front, 62, 5, -1),
            new Tile(TileLayer.Front, 63, 5, -1),
            new Tile(TileLayer.Front, 64, 5, -1),
            new Tile(TileLayer.Front, 65, 5, -1),
            new Tile(TileLayer.Front, 66, 5, -1),
            new Tile(TileLayer.Front, 67, 5, -1),
            new Tile(TileLayer.Front, 72, 5, -1),
            new Tile(TileLayer.Front, 77, 5, -1)
            };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> RemoveShippingBinEdits(GameLocation gl)
        {
            Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Front, 71, 13, -1),
        new Tile(TileLayer.Front, 72, 13, -1),
        new Tile(TileLayer.Buildings, 71, 14, -1),
        new Tile(TileLayer.Buildings, 72, 14, -1)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> RemovePathEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Back, 75, 17, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 76, 17, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 77, 17, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 78, 17, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 79, 17, 227, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> TelescopeEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Back, 16, 4, 312, tileSheetIndex),
        new Tile(TileLayer.Back, 17, 4, 313, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 4, 314, tileSheetIndex),
        new Tile(TileLayer.Back, 16, 5, 337, tileSheetIndex),
        new Tile(TileLayer.Back, 17, 5, 338, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 5, 339, tileSheetIndex),
        new Tile(TileLayer.Back, 16, 6, 598, tileSheetIndex),
        new Tile(TileLayer.Back, 17, 6, 599, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 6, 587, tileSheetIndex),
        new Tile(TileLayer.Back, 16, 7, 587, tileSheetIndex),
        new Tile(TileLayer.Back, 17, 7, 587, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 7, 226, tileSheetIndex),
        new Tile(TileLayer.Back, 16, 8, 587, tileSheetIndex),
        new Tile(TileLayer.Back, 17, 8, 587, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 7, 587, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 4, 312, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 5, 337, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 8, 565, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 3, 444, tileSheetIndex),
        new Tile(TileLayer.Buildings, 16, 3, -1),
        new Tile(TileLayer.Buildings, 17, 3, -1),
        new Tile(TileLayer.Buildings, 18, 3, -1),
        new Tile(TileLayer.Buildings, 15, 4, 469, tileSheetIndex),
        new Tile(TileLayer.Buildings, 16, 4, -1),
        new Tile(TileLayer.Buildings, 17, 4, -1),
        new Tile(TileLayer.Buildings, 18, 4, 416, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 5, 494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 16, 5, -1),
        new Tile(TileLayer.Buildings, 17, 5, -1),
        new Tile(TileLayer.Buildings, 18, 5, 441, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 6, 519, tileSheetIndex),
        new Tile(TileLayer.Buildings, 16, 6, -1),
        new Tile(TileLayer.Buildings, 17, 6, -1),
        new Tile(TileLayer.Buildings, 18, 6, 466, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 7, 540, tileSheetIndex),
        new Tile(TileLayer.Buildings, 16, 7, -1),
        new Tile(TileLayer.Buildings, 17, 7, -1),
        new Tile(TileLayer.Buildings, 18, 7, 491, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 8, -1),
        new Tile(TileLayer.Buildings, 18, 8, 516, tileSheetIndex),
        new Tile(TileLayer.Buildings, 18, 9, 541, tileSheetIndex),
        new Tile(TileLayer.Back, 32, 4, 467, tileSheetIndex),
        new Tile(TileLayer.Back, 33, 4, 468, tileSheetIndex),
        new Tile(TileLayer.Back, 32, 5, 492, tileSheetIndex),
        new Tile(TileLayer.Back, 33, 5, 493, tileSheetIndex),
        new Tile(TileLayer.Back, 32, 6, 346, tileSheetIndex),
        new Tile(TileLayer.Back, 32, 7, 695, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 2, 277, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 2, 277, tileSheetIndex),
        new Tile(TileLayer.Back, 20, 2, 278, tileSheetIndex),
        new Tile(TileLayer.Back, 21, 2, 377, tileSheetIndex),
        new Tile(TileLayer.Back, 21, 1, 352, tileSheetIndex),
        new Tile(TileLayer.Back, 21, 0, 352, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 4, 402, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 5, 402, tileSheetIndex),
        new Tile(TileLayer.Back, 20, 3, 200, tileSheetIndex),
        new Tile(TileLayer.Back, 21, 3, 201, tileSheetIndex),
        new Tile(TileLayer.Back, 22, 3, 201, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 3, 201, tileSheetIndex),
        new Tile(TileLayer.Back, 24, 3, 203, tileSheetIndex),
        new Tile(TileLayer.Back, 20, 4, 225, tileSheetIndex),
        new Tile(TileLayer.Back, 21, 4, 226, tileSheetIndex),
        new Tile(TileLayer.Back, 22, 4, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 4, 488, tileSheetIndex),
        new Tile(TileLayer.Back, 24, 4, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 20, 5, 250, tileSheetIndex),
        new Tile(TileLayer.Back, 21, 5, 251, tileSheetIndex),
        new Tile(TileLayer.Back, 22, 5, 251, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 5, 251, tileSheetIndex),
        new Tile(TileLayer.Back, 24, 5, 253, tileSheetIndex),
        new Tile(TileLayer.Back, 22, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 24, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 25, 0, 151, tileSheetIndex),
        new Tile(TileLayer.Back, 26, 0, 150, tileSheetIndex),
        new Tile(TileLayer.Back, 22, 1, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 1, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 24, 1, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 25, 1, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 26, 1, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 22, 2, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 2, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 24, 2, 151, tileSheetIndex),
        new Tile(TileLayer.Back, 25, 2, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 26, 2, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 27, 1, 200, tileSheetIndex),
        new Tile(TileLayer.Back, 28, 1, 201, tileSheetIndex),
        new Tile(TileLayer.Back, 29, 1, 201, tileSheetIndex),
        new Tile(TileLayer.Back, 30, 1, 201, tileSheetIndex),
        new Tile(TileLayer.Back, 31, 1, 201, tileSheetIndex),
        new Tile(TileLayer.Back, 32, 1, 203, tileSheetIndex),
        new Tile(TileLayer.Back, 27, 2, 225, tileSheetIndex),
        new Tile(TileLayer.Back, 28, 2, 1125, tileSheetIndex),
        new Tile(TileLayer.Back, 29, 2, 1126, tileSheetIndex),
        new Tile(TileLayer.Back, 30, 2, 1127, tileSheetIndex),
        new Tile(TileLayer.Back, 31, 2, 1128, tileSheetIndex),
        new Tile(TileLayer.Back, 32, 2, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 27, 3, 225, tileSheetIndex),
        new Tile(TileLayer.Back, 28, 3, 1150, tileSheetIndex),
        new Tile(TileLayer.Back, 29, 3, 1151, tileSheetIndex),
        new Tile(TileLayer.Back, 30, 3, 1152, tileSheetIndex),
        new Tile(TileLayer.Back, 31, 3, 1153, tileSheetIndex),
        new Tile(TileLayer.Back, 32, 3, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 27, 4, 225, tileSheetIndex),
        new Tile(TileLayer.Back, 28, 4, 1175, tileSheetIndex),
        new Tile(TileLayer.Back, 29, 4, 1176, tileSheetIndex),
        new Tile(TileLayer.Back, 30, 4, 1177, tileSheetIndex),
        new Tile(TileLayer.Back, 31, 4, 1178, tileSheetIndex),
        new Tile(TileLayer.Back, 27, 5, 250, tileSheetIndex),
        new Tile(TileLayer.Back, 28, 5, 251, tileSheetIndex),
        new Tile(TileLayer.Back, 29, 5, 251, tileSheetIndex),
        new Tile(TileLayer.Back, 30, 5, 251, tileSheetIndex),
        new Tile(TileLayer.Back, 31, 5, 230, tileSheetIndex),
        new Tile(TileLayer.Back, 27, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 28, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 29, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 30, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 31, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 32, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 33, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 34, 0, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 25, 4, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 26, 4, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 25, 5, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 26, 5, 175, tileSheetIndex),
        new Tile(TileLayer.Buildings, 32, 4, 419, tileSheetIndex),
        new Tile(TileLayer.Buildings, 32, 5, 444, tileSheetIndex),
        new Tile(TileLayer.Buildings, 19, 6, 467, tileSheetIndex),
        new Tile(TileLayer.Buildings, 20, 6, 467, tileSheetIndex),
        new Tile(TileLayer.Buildings, 21, 6, 468, tileSheetIndex),
        new Tile(TileLayer.Buildings, 22, 6, 467, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 6, 468, tileSheetIndex),
        new Tile(TileLayer.Buildings, 24, 6, 467, tileSheetIndex),
        new Tile(TileLayer.Buildings, 25, 6, 468, tileSheetIndex),
        new Tile(TileLayer.Buildings, 26, 6, 446, tileSheetIndex),
        new Tile(TileLayer.Buildings, 27, 6, 468, tileSheetIndex),
        new Tile(TileLayer.Buildings, 28, 6, 467, tileSheetIndex),
        new Tile(TileLayer.Buildings, 29, 6, 468, tileSheetIndex),
        new Tile(TileLayer.Buildings, 30, 6, 467, tileSheetIndex),
        new Tile(TileLayer.Buildings, 31, 6, 468, tileSheetIndex),
        new Tile(TileLayer.Buildings, 32, 6, 469, tileSheetIndex),
        new Tile(TileLayer.Buildings, 19, 7, 492, tileSheetIndex),
        new Tile(TileLayer.Buildings, 20, 7, 493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 21, 7, 493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 22, 7, 492, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 7, 493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 24, 7, 492, tileSheetIndex),
        new Tile(TileLayer.Buildings, 25, 7, 493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 26, 7, 492, tileSheetIndex),
        new Tile(TileLayer.Buildings, 27, 7, 493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 28, 7, 492, tileSheetIndex),
        new Tile(TileLayer.Buildings, 29, 7, 493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 30, 7, 492, tileSheetIndex),
        new Tile(TileLayer.Buildings, 31, 7, 493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 32, 7, 494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 19, 8, 517, tileSheetIndex),
        new Tile(TileLayer.Buildings, 20, 8, 518, tileSheetIndex),
        new Tile(TileLayer.Buildings, 21, 8, 518, tileSheetIndex),
        new Tile(TileLayer.Buildings, 22, 8, 517, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 8, 518, tileSheetIndex),
        new Tile(TileLayer.Buildings, 24, 8, 517, tileSheetIndex),
        new Tile(TileLayer.Buildings, 25, 8, 518, tileSheetIndex),
        new Tile(TileLayer.Buildings, 26, 8, 517, tileSheetIndex),
        new Tile(TileLayer.Buildings, 27, 8, 518, tileSheetIndex),
        new Tile(TileLayer.Buildings, 28, 8, 448, tileSheetIndex),
        new Tile(TileLayer.Buildings, 29, 8, 518, tileSheetIndex),
        new Tile(TileLayer.Buildings, 30, 8, 517, tileSheetIndex),
        new Tile(TileLayer.Buildings, 31, 8, 518, tileSheetIndex),
        new Tile(TileLayer.Buildings, 32, 8, 519, tileSheetIndex),
        new Tile(TileLayer.Buildings, 19, 9, 542, tileSheetIndex),
        new Tile(TileLayer.Buildings, 20, 9, 543, tileSheetIndex),
        new Tile(TileLayer.Buildings, 21, 9, 542, tileSheetIndex),
        new Tile(TileLayer.Buildings, 22, 9, 543, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 9, 542, tileSheetIndex),
        new Tile(TileLayer.Buildings, 24, 9, 543, tileSheetIndex),
        new Tile(TileLayer.Buildings, 25, 9, 542, tileSheetIndex),
        new Tile(TileLayer.Buildings, 26, 9, 543, tileSheetIndex),
        new Tile(TileLayer.Buildings, 27, 9, 542, tileSheetIndex),
        new Tile(TileLayer.Buildings, 28, 9, 543, tileSheetIndex),
        new Tile(TileLayer.Buildings, 29, 9, 542, tileSheetIndex),
        new Tile(TileLayer.Buildings, 30, 9, 542, tileSheetIndex),
        new Tile(TileLayer.Buildings, 31, 9, 543, tileSheetIndex),
        new Tile(TileLayer.Buildings, 32, 9, 544, tileSheetIndex),
        new Tile(TileLayer.Buildings, 17, 0, -1),
        new Tile(TileLayer.Buildings, 18, 0, -1),
        new Tile(TileLayer.Buildings, 19, 0, -1),
        new Tile(TileLayer.Buildings, 17, 1, -1),
        new Tile(TileLayer.Buildings, 18, 1, -1),
        new Tile(TileLayer.Buildings, 19, 1, -1),
        new Tile(TileLayer.Buildings, 18, 2, -1),
        new Tile(TileLayer.Buildings, 20, 0, -1),
        new Tile(TileLayer.Buildings, 21, 0, -1),
        new Tile(TileLayer.Buildings, 20, 1, -1),
        new Tile(TileLayer.Buildings, 21, 1, -1),
        new Tile(TileLayer.Buildings, 20, 2, -1),
        new Tile(TileLayer.Buildings, 21, 2, -1),
        new Tile(TileLayer.Buildings, 17, 2, -1),
        new Tile(TileLayer.Buildings, 19, 2, -1),
        new Tile(TileLayer.Buildings, 19, 3, -1),
        new Tile(TileLayer.Buildings, 22, 0, -1),
        new Tile(TileLayer.Buildings, 22, 1, -1),
        new Tile(TileLayer.Buildings, 22, 2, -1),
        new Tile(TileLayer.Buildings, 23, 1, -1),
        new Tile(TileLayer.Buildings, 23, 2, -1),
        new Tile(TileLayer.Buildings, 25, 1, -1),
        new Tile(TileLayer.Buildings, 25, 2, -1),
        new Tile(TileLayer.Buildings, 26, 1, -1),
        new Tile(TileLayer.Buildings, 30, 3, -1),
        new Tile(TileLayer.Buildings, 32, 2, -1),
        new Tile(TileLayer.Buildings, 32, 3, -1),
        new Tile(TileLayer.Buildings, 33, 0, -1),
        new Tile(TileLayer.Buildings, 33, 1, -1),
        new Tile(TileLayer.Buildings, 19, 4, -1),
        new Tile(TileLayer.Buildings, 25, 4, -1),
        new Tile(TileLayer.Buildings, 26, 4, -1),
        new Tile(TileLayer.Buildings, 27, 4, -1),
        new Tile(TileLayer.Buildings, 28, 4, -1),
        new Tile(TileLayer.Buildings, 29, 4, -1),
        new Tile(TileLayer.Buildings, 30, 4, -1),
        new Tile(TileLayer.Buildings, 31, 4, -1),
        new Tile(TileLayer.Buildings, 19, 5, -1),
        new Tile(TileLayer.Buildings, 20, 5, -1),
        new Tile(TileLayer.Buildings, 21, 5, -1),
        new Tile(TileLayer.Buildings, 22, 5, -1),
        new Tile(TileLayer.Buildings, 23, 5, -1),
        new Tile(TileLayer.Buildings, 24, 5, -1),
        new Tile(TileLayer.Buildings, 25, 5, -1),
        new Tile(TileLayer.Buildings, 26, 5, -1),
        new Tile(TileLayer.Buildings, 27, 5, -1),
        new Tile(TileLayer.Buildings, 28, 5, -1),
        new Tile(TileLayer.Buildings, 29, 5, -1),
        new Tile(TileLayer.Buildings, 30, 5, -1),
        new Tile(TileLayer.Buildings, 31, 5, -1),
        new Tile(TileLayer.Buildings, 23, 0, -1),
        new Tile(TileLayer.Buildings, 24, 0, -1),
        new Tile(TileLayer.Buildings, 25, 0, -1),
        new Tile(TileLayer.Buildings, 24, 1, -1),
        new Tile(TileLayer.Buildings, 24, 2, -1),
        new Tile(TileLayer.Buildings, 27, 0, -1),
        new Tile(TileLayer.Buildings, 28, 0, -1),
        new Tile(TileLayer.Front, 29, 0, -1),
        new Tile(TileLayer.Buildings, 28, 1, -1),
        new Tile(TileLayer.Buildings, 28, 2, -1),
        new Tile(TileLayer.Buildings, 29, 0, -1),
        new Tile(TileLayer.Buildings, 30, 0, -1),
        new Tile(TileLayer.Buildings, 29, 1, -1),
        new Tile(TileLayer.Buildings, 30, 1, -1),
        new Tile(TileLayer.Front, 30, 0, -1),
        new Tile(TileLayer.Front, 30, 1, -1),
        new Tile(TileLayer.Buildings, 31, 0, -1),
        new Tile(TileLayer.Buildings, 32, 0, -1),
        new Tile(TileLayer.Buildings, 31, 1, -1),
        new Tile(TileLayer.Buildings, 32, 1, -1),
        new Tile(TileLayer.Buildings, 31, 2, -1),
        new Tile(TileLayer.Buildings, 31, 3, -1),
        new Tile(TileLayer.Buildings, 21, 3, 2594, tileSheetIndex),
        new Tile(TileLayer.Buildings, 22, 3, 2595, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 3, 2596, tileSheetIndex),
        new Tile(TileLayer.Buildings, 20, 4, 2618, tileSheetIndex),
        new Tile(TileLayer.Buildings, 21, 4, 2619, tileSheetIndex),
        new Tile(TileLayer.Buildings, 22, 4, 2620, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 4, 2621, tileSheetIndex),
        new Tile(TileLayer.Buildings, 24, 4, 2622, tileSheetIndex),
        new Tile(TileLayer.Front, 20, 0, 2518, tileSheetIndex),
        new Tile(TileLayer.Front, 21, 0, 2519, tileSheetIndex),
        new Tile(TileLayer.Front, 22, 0, 2520, tileSheetIndex),
        new Tile(TileLayer.Front, 23, 0, 2521, tileSheetIndex),
        new Tile(TileLayer.Front, 24, 0, 2522, tileSheetIndex),
        new Tile(TileLayer.Front, 20, 1, 2543, tileSheetIndex),
        new Tile(TileLayer.Front, 21, 1, 2544, tileSheetIndex),
        new Tile(TileLayer.Front, 22, 1, 2545, tileSheetIndex),
        new Tile(TileLayer.Front, 23, 1, 2546, tileSheetIndex),
        new Tile(TileLayer.Front, 24, 1, 2547, tileSheetIndex),
        new Tile(TileLayer.Front, 20, 2, 2568, tileSheetIndex),
        new Tile(TileLayer.Front, 21, 2, 2569, tileSheetIndex),
        new Tile(TileLayer.Front, 22, 2, 2570, tileSheetIndex),
        new Tile(TileLayer.Front, 23, 2, 2571, tileSheetIndex),
        new Tile(TileLayer.Front, 24, 2, 2572, tileSheetIndex),
        new Tile(TileLayer.Front, 20, 3, 2593, tileSheetIndex),
        new Tile(TileLayer.Front, 24, 3, 2597, tileSheetIndex),
        new Tile(TileLayer.Front, 30, 1, 2266, tileSheetIndex),
        new Tile(TileLayer.Buildings, 30, 2, 2291, tileSheetIndex),
        new Tile(TileLayer.Buildings, 30, 5, 117, tileSheetIndex),
        new Tile(TileLayer.Buildings, 31, 5, 113, tileSheetIndex),
        new Tile(TileLayer.Front, 31, 4, 88, tileSheetIndex),
        new Tile(TileLayer.Front, 32, 4, 89, tileSheetIndex),
        new Tile(TileLayer.Front, 32, 5, 114, tileSheetIndex),
        new Tile(TileLayer.Buildings, 33, 3, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 17, 0, 307, tileSheetIndex),
        new Tile(TileLayer.Buildings, 18, 0, 257, tileSheetIndex),
        new Tile(TileLayer.Buildings, 19, 0, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 20, 0, 126, tileSheetIndex),
        new Tile(TileLayer.Buildings, 21, 0, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 22, 0, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 0, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 24, 0, 307, tileSheetIndex),
        new Tile(TileLayer.Buildings, 25, 0, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 27, 0, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 28, 0, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 29, 0, 63, tileSheetIndex),
        new Tile(TileLayer.Buildings, 30, 0, 64, tileSheetIndex),
        new Tile(TileLayer.Buildings, 31, 0, 65, tileSheetIndex),
        new Tile(TileLayer.Buildings, 32, 0, 257, tileSheetIndex),
        new Tile(TileLayer.Buildings, 33, 1, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 33, 2, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 12, 2, 21, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> MemorialArea(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Back, 3, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 10, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 11, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 7, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 3, 8, 2300, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 8, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 8, 2302, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 8, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 10, 8, 2400, tileSheetIndex),
        new Tile(TileLayer.Back, 11, 8, 2401, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 8, 2401, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 8, 2401, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 8, 2280, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 8, 838, tileSheetIndex),
        new Tile(TileLayer.Back, 3, 9, 2331, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 9, 153, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 9, 2327, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 9, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 9, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 9, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 9, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 10, 9, 2303, tileSheetIndex),
        new Tile(TileLayer.Back, 11, 9, 226, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 9, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 9, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 9, 2305, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 9, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 9, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 20, 9, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 21, 9, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 22, 9, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 3, 10, 2350, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 10, 2351, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 10, 2352, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 10, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 10, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 10, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 10, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 10, 10, 2328, tileSheetIndex),
        new Tile(TileLayer.Back, 11, 10, 226, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 10, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 10, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 10, 2330, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 10, 2300, tileSheetIndex),
        new Tile(TileLayer.Back, 20, 10, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 21, 10, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 22, 10, 2302, tileSheetIndex),
        new Tile(TileLayer.Back, 3, 11, 2300, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 11, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 11, 2302, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 11, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 11, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 11, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 11, 2283),
        new Tile(TileLayer.Back, 10, 11, 2353),
        new Tile(TileLayer.Back, 11, 11, 226, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 11, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 11, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 11, 2355, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 11, 2303, tileSheetIndex),
        new Tile(TileLayer.Back, 20, 11, 226, tileSheetIndex),
        new Tile(TileLayer.Back, 21, 11, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 22, 11, 2305, tileSheetIndex),
        new Tile(TileLayer.Back, 3, 12, 2475, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 12, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 12, 2327, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 12, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 12, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 12, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 12, 2283),
        new Tile(TileLayer.Back, 10, 12, 2378),
        new Tile(TileLayer.Back, 11, 12, 2379),
        new Tile(TileLayer.Back, 12, 12, 2379),
        new Tile(TileLayer.Back, 13, 12, 2379),
        new Tile(TileLayer.Back, 14, 12, 2380),
        new Tile(TileLayer.Back, 19, 12, 2328),
        new Tile(TileLayer.Back, 20, 12, 226),
        new Tile(TileLayer.Back, 21, 12, 227),
        new Tile(TileLayer.Back, 22, 12, 2330),
        new Tile(TileLayer.Back, 3, 13, 2350, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 13, 2351, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 13, 2352, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 13, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 13, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 13, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 13, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 10, 13, 2278, tileSheetIndex),
        new Tile(TileLayer.Back, 11, 13, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 13, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 13, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 13, 2302, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 13, 2328),
        new Tile(TileLayer.Back, 20, 13, 226),
        new Tile(TileLayer.Back, 21, 13, 227),
        new Tile(TileLayer.Back, 22, 13, 2330),
        new Tile(TileLayer.Back, 3, 14, 2300, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 14, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 14, 2302, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 14, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 14, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 14, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 14, 2283),
        new Tile(TileLayer.Back, 10, 14, 2353),
        new Tile(TileLayer.Back, 11, 14, 226, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 14, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 14, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 14, 2355, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 14, 2328),
        new Tile(TileLayer.Back, 20, 14, 226),
        new Tile(TileLayer.Back, 21, 14, 227),
        new Tile(TileLayer.Back, 22, 14, 2330),
        new Tile(TileLayer.Back, 3, 15, 2475, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 15, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 15, 2327, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 15, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 15, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 15, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 15, 2283),
        new Tile(TileLayer.Back, 10, 15, 2353),
        new Tile(TileLayer.Back, 11, 15, 226, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 15, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 15, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 15, 2355, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 15, 2328),
        new Tile(TileLayer.Back, 20, 15, 226),
        new Tile(TileLayer.Back, 21, 15, 227),
        new Tile(TileLayer.Back, 22, 15, 2330),
        new Tile(TileLayer.Back, 3, 16, 2350, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 16, 2351, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 16, 2352, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 16, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 16, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 16, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 16, 2283, tileSheetIndex),
        new Tile(TileLayer.Back, 10, 16, 2378),
        new Tile(TileLayer.Back, 11, 16, 2379),
        new Tile(TileLayer.Back, 12, 16, 2379),
        new Tile(TileLayer.Back, 13, 16, 2379),
        new Tile(TileLayer.Back, 14, 16, 2380),
        new Tile(TileLayer.Back, 19, 16, 2475),
        new Tile(TileLayer.Back, 20, 16, 226),
        new Tile(TileLayer.Back, 21, 16, 227),
        new Tile(TileLayer.Back, 22, 16, 2330),
        new Tile(TileLayer.Back, 3, 17, 2300, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 17, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 17, 2302, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 17, 2300, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 17, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 17, 2302, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 17, 2300, tileSheetIndex),
        new Tile(TileLayer.Back, 10, 17, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 11, 17, 2302, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 17, 2300, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 17, 2301, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 17, 2302, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 17, 2350),
        new Tile(TileLayer.Back, 20, 17, 2351),
        new Tile(TileLayer.Back, 21, 17, 2351),
        new Tile(TileLayer.Back, 22, 17, 2352),
        new Tile(TileLayer.Back, 3, 18, 2325, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 18, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 18, 2327, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 18, 2325, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 18, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 18, 2327, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 18, 2325, tileSheetIndex),
        new Tile(TileLayer.Back, 10, 18, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 11, 18, 2327, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 18, 2325, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 18, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 18, 2327, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 18, 2283),
        new Tile(TileLayer.Back, 20, 18, 2283),
        new Tile(TileLayer.Back, 21, 18, 2283),
        new Tile(TileLayer.Back, 22, 18, 2283),
        new Tile(TileLayer.Back, 3, 19, 2350, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 19, 2351, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 19, 2352, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 19, 2350, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 19, 2351, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 19, 2352, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 19, 2350, tileSheetIndex),
        new Tile(TileLayer.Back, 10, 19, 2351, tileSheetIndex),
        new Tile(TileLayer.Back, 11, 19, 2352, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 19, 2350, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 19, 2351, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 19, 2352, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 19, 2283),
        new Tile(TileLayer.Back, 20, 19, 2283),
        new Tile(TileLayer.Back, 21, 19, 2283),
        new Tile(TileLayer.Back, 22, 19, 2283),
        new Tile(TileLayer.Back, 3, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 4, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 5, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 6, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 7, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 8, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 9, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 10, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 11, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 12, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 13, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 14, 20, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 20, 838, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 20, 838, tileSheetIndex),
        new Tile(TileLayer.Back, 19, 20, 2715),
        new Tile(TileLayer.Back, 20, 20, 2715),
        new Tile(TileLayer.Back, 21, 20, 2715),
        new Tile(TileLayer.Back, 22, 20, 2715),
        new Tile(TileLayer.Back, 23, 20, 2715),
        new Tile(TileLayer.Buildings, 3, 19, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 4, 19, 2495, tileSheetIndex),
        new Tile(TileLayer.Buildings, 5, 19, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 6, 19, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 7, 19, 2495, tileSheetIndex),
        new Tile(TileLayer.Buildings, 8, 19, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 9, 19, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 10, 19, 2495, tileSheetIndex),
        new Tile(TileLayer.Buildings, 11, 19, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 12, 19, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 13, 19, 2495, tileSheetIndex),
        new Tile(TileLayer.Buildings, 14, 19, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 19, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 18, 19, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 19, 19, 2495, tileSheetIndex),
        new Tile(TileLayer.Buildings, 20, 19, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 21, 19, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 22, 19, 2495, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 19, 2496, tileSheetIndex),
        new Tile(TileLayer.Front, 3, 18, 2419, tileSheetIndex),
        new Tile(TileLayer.Front, 4, 18, 2420, tileSheetIndex),
        new Tile(TileLayer.Front, 5, 18, 2421, tileSheetIndex),
        new Tile(TileLayer.Front, 6, 18, 2419, tileSheetIndex),
        new Tile(TileLayer.Front, 7, 18, 2420, tileSheetIndex),
        new Tile(TileLayer.Front, 8, 18, 2421, tileSheetIndex),
        new Tile(TileLayer.Front, 9, 18, 2419, tileSheetIndex),
        new Tile(TileLayer.Front, 10, 18, 2420, tileSheetIndex),
        new Tile(TileLayer.Front, 11, 18, 2421, tileSheetIndex),
        new Tile(TileLayer.Front, 12, 18, 2419, tileSheetIndex),
        new Tile(TileLayer.Front, 13, 18, 2420, tileSheetIndex),
        new Tile(TileLayer.Front, 14, 18, 2421, tileSheetIndex),
        new Tile(TileLayer.Front, 15, 18, 2421, tileSheetIndex),
        new Tile(TileLayer.Front, 18, 18, 2419, tileSheetIndex),
        new Tile(TileLayer.Front, 19, 18, 2420, tileSheetIndex),
        new Tile(TileLayer.Front, 20, 18, 2421, tileSheetIndex),
        new Tile(TileLayer.Front, 21, 18, 2419, tileSheetIndex),
        new Tile(TileLayer.Front, 22, 18, 2420, tileSheetIndex),
        new Tile(TileLayer.Front, 23, 18, 2421, tileSheetIndex),
        new Tile(TileLayer.Front, 15, 7, 2419, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 8, 2444, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 9, 2469, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 10, 2419, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 11, 2444, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 11, 2469, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 12, 2494, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 7, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 8, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 9, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 10, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 11, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 11, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 12, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 13, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 14, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 15, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 16, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 17, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 18, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 15, 19, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 9, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 10, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 11, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 11, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 12, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 13, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 14, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 15, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 16, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 17, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 18, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 18, 19, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 9, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 10, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 11, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 11, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 12, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 13, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 14, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 15, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 16, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 17, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 18, 812, tileSheetIndex),
        new Tile(TileLayer.Back, 23, 19, 812, tileSheetIndex),
        new Tile(TileLayer.Front, 18, 9, 2421, tileSheetIndex),
        new Tile(TileLayer.Buildings, 18, 10, 2446, tileSheetIndex),
        new Tile(TileLayer.Buildings, 18, 11, 2471, tileSheetIndex),
        new Tile(TileLayer.Buildings, 18, 12, 2471, tileSheetIndex),
        new Tile(TileLayer.Buildings, 18, 13, 2496, tileSheetIndex),
        new Tile(TileLayer.Front, 23, 9, 2421, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 10, 2446, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 11, 2471, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 12, 2421, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 13, 2446, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 14, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 23, 15, 2421, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 16, 2446, tileSheetIndex),
        new Tile(TileLayer.Buildings, 23, 17, 2471, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> MemorialAreaArch(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.AlwaysFront, 15, 16, 2388, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 16, 16, 2389, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 17, 16, 2390, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 18, 16, 2391, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 15, 17, 2413, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 16, 17, 2414, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 17, 17, 2415, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 18, 17, 2416, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 15, 18, 2438, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 16, 18, 2439, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 17, 18, 2440, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 18, 18, 2441, tileSheetIndex),
        new Tile(TileLayer.Front, 15, 19, 2463, tileSheetIndex),
        new Tile(TileLayer.Front, 18, 19, 2466, tileSheetIndex),
        new Tile(TileLayer.Buildings, 15, 20, 2488, tileSheetIndex),
        new Tile(TileLayer.Buildings, 18, 20, 2491, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> PicnicBlanketEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Back, 33, 45, 2992, tileSheetIndex),
        new Tile(TileLayer.Back, 34, 45, 2993, tileSheetIndex),
        new Tile(TileLayer.Back, 35, 45, 2994, tileSheetIndex),
        new Tile(TileLayer.Back, 36, 45, 2995, tileSheetIndex),
        new Tile(TileLayer.Back, 37, 45, 2996, tileSheetIndex),
        new Tile(TileLayer.Back, 38, 45, 2997, tileSheetIndex),
        new Tile(TileLayer.Back, 33, 46, 3017, tileSheetIndex),
        new Tile(TileLayer.Back, 34, 46, 3018, tileSheetIndex),
        new Tile(TileLayer.Back, 35, 46, 3019, tileSheetIndex),
        new Tile(TileLayer.Back, 36, 46, 3020, tileSheetIndex),
        new Tile(TileLayer.Back, 37, 46, 3021, tileSheetIndex),
        new Tile(TileLayer.Back, 38, 46, 3022, tileSheetIndex),
        new Tile(TileLayer.Back, 33, 47, 3042, tileSheetIndex),
        new Tile(TileLayer.Back, 34, 47, 3043, tileSheetIndex),
        new Tile(TileLayer.Back, 35, 47, 3044, tileSheetIndex),
        new Tile(TileLayer.Back, 36, 47, 3045, tileSheetIndex),
        new Tile(TileLayer.Back, 37, 47, 3046, tileSheetIndex),
        new Tile(TileLayer.Back, 38, 47, 3047, tileSheetIndex),
        new Tile(TileLayer.Back, 33, 48, 3067, tileSheetIndex),
        new Tile(TileLayer.Back, 34, 48, 3068, tileSheetIndex),
        new Tile(TileLayer.Back, 35, 48, 3069, tileSheetIndex),
        new Tile(TileLayer.Back, 36, 48, 3070, tileSheetIndex),
        new Tile(TileLayer.Back, 37, 48, 3071, tileSheetIndex),
        new Tile(TileLayer.Back, 38, 48, 3072, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> StoneBridgeEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Back, 40, 48, -1),
        new Tile(TileLayer.Back, 41, 48, -1),
        new Tile(TileLayer.Back, 40, 49, -1),
        new Tile(TileLayer.Back, 41, 49, -1),
        new Tile(TileLayer.Buildings, 39, 49, -1),
        new Tile(TileLayer.Buildings, 40, 49, -1),
        new Tile(TileLayer.Buildings, 41, 49, -1),
        new Tile(TileLayer.Buildings, 42, 49, -1),
        new Tile(TileLayer.Back, 40, 50, -1),
        new Tile(TileLayer.Back, 41, 50, -1),
        new Tile(TileLayer.Buildings, 39, 50, -1),
        new Tile(TileLayer.Buildings, 40, 50, -1),
        new Tile(TileLayer.Buildings, 41, 50, -1),
        new Tile(TileLayer.Buildings, 42, 50, -1),
        new Tile(TileLayer.Back, 40, 51, -1),
        new Tile(TileLayer.Back, 41, 51, -1),
        new Tile(TileLayer.Back, 40, 52, -1),
        new Tile(TileLayer.Back, 41, 52, -1),
        new Tile(TileLayer.Back, 40, 53, -1),
        new Tile(TileLayer.Back, 41, 53, -1),
        new Tile(TileLayer.Back, 40, 54, -1),
        new Tile(TileLayer.Back, 41, 54, -1),
        new Tile(TileLayer.Back, 40, 55, -1),
        new Tile(TileLayer.Back, 41, 55, -1),
        new Tile(TileLayer.Back, 40, 56, -1),
        new Tile(TileLayer.Back, 41, 56, -1),
        new Tile(TileLayer.Back, 40, 57, -1),
        new Tile(TileLayer.Back, 41, 57, -1),
        new Tile(TileLayer.Buildings, 41, 57, -1),
        new Tile(TileLayer.Buildings, 42, 57, -1),
        new Tile(TileLayer.Back, 40, 58, -1),
        new Tile(TileLayer.Back, 41, 58, -1),
        new Tile(TileLayer.Buildings, 39, 58, -1),
        new Tile(TileLayer.Buildings, 40, 58, -1),
        new Tile(TileLayer.Buildings, 41, 58, -1),
        new Tile(TileLayer.Back, 39, 59, -1),
        new Tile(TileLayer.Back, 40, 59, -1),
        new Tile(TileLayer.Back, 41, 59, -1),
        new Tile(TileLayer.Back, 42, 59, -1),
        new Tile(TileLayer.Buildings, 39, 48, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 48, 2714, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 48, 2714, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 48, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 49, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 49, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 49, 2738, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 49, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 50, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 50, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 50, 2738, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 50, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 51, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 51, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 51, 2738, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 51, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 52, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 52, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 52, 2738, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 52, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 53, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 53, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 53, 2738, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 53, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 54, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 54, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 54, 2738, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 54, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 55, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 55, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 55, 2738, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 55, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 56, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 56, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 56, 2738, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 56, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 57, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 57, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 57, 2738, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 57, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 58, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 58, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 58, 2738, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 58, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 39, 59, 2740, tileSheetIndex),
        new Tile(TileLayer.Back, 40, 59, 2713, tileSheetIndex),
        new Tile(TileLayer.Back, 41, 59, 2713, tileSheetIndex),
        new Tile(TileLayer.Buildings, 42, 59, 2740, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 48, 2999, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 49, 3024, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 50, 3024, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 51, 3024, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 52, 3024, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 53, 3024, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 54, 3024, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 55, 3024, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 56, 3024, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 57, 3024, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 58, 3024, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 38, 59, 3049, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> PicnicAreaTableEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Buildings, 36, 44, 2970, tileSheetIndex),
        new Tile(TileLayer.Buildings, 37, 44, 2971, tileSheetIndex),
        new Tile(TileLayer.Front, 37, 42, 2921, tileSheetIndex),
        new Tile(TileLayer.Front, 36, 43, 2945, tileSheetIndex),
        new Tile(TileLayer.Front, 37, 43, 2946, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> TreeSwingEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Back, 32, 42, 433, tileSheetIndex),
        new Tile(TileLayer.Back, 33, 42, 433, tileSheetIndex),
        new Tile(TileLayer.Buildings, 32, 43, 2941, tileSheetIndex),
        new Tile(TileLayer.Buildings, 33, 43, 2942, tileSheetIndex),
        new Tile(TileLayer.Buildings, 34, 43, 2943, tileSheetIndex),
        new Tile(TileLayer.Buildings, 35, 43, 2944, tileSheetIndex),
        new Tile(TileLayer.Buildings, 32, 42, 2916, tileSheetIndex),
        new Tile(TileLayer.Buildings, 33, 42, 2917, tileSheetIndex),
        new Tile(TileLayer.Buildings, 32, 44, 2966, tileSheetIndex),
        new Tile(TileLayer.Buildings, 33, 44, 2967, tileSheetIndex),
        new Tile(TileLayer.Buildings, 34, 44, 2968, tileSheetIndex),
        new Tile(TileLayer.Buildings, 35, 44, 2969, tileSheetIndex),
        new Tile(TileLayer.Front, 34, 42, 2918, tileSheetIndex),
        new Tile(TileLayer.Front, 35, 42, 2919, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 32, 38, 2816, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 33, 38, 2817, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 34, 38, 2818, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 35, 38, 2819, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 36, 38, 2820, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 32, 39, 2841, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 33, 39, 2842, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 34, 39, 2843, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 35, 39, 2844, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 36, 39, 2845, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 32, 40, 2866, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 33, 40, 2867, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 34, 40, 2868, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 35, 40, 2869, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 36, 40, 2870, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 32, 41, 2891, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 33, 41, 2892, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 34, 41, 2893, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 35, 41, 2894, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 36, 41, 2895, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 34, 42, 2918, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 35, 42, 2919, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 36, 42, 2920, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> DogHouseEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Back, 51, 5, 838, tileSheetIndex),
        new Tile(TileLayer.Back, 52, 5, 1203, tileSheetIndex),
        new Tile(TileLayer.Back, 53, 5, 1203, tileSheetIndex),
        new Tile(TileLayer.Back, 54, 5, 838, tileSheetIndex),
        new Tile(TileLayer.Back, 51, 6, 838, tileSheetIndex),
        new Tile(TileLayer.Back, 52, 6, 838, tileSheetIndex),
        new Tile(TileLayer.Back, 53, 6, 838, tileSheetIndex),
        new Tile(TileLayer.Back, 54, 6, 838, tileSheetIndex),
        new Tile(TileLayer.Back, 51, 7, 2200, tileSheetIndex),
        new Tile(TileLayer.Back, 52, 7, 2201, tileSheetIndex),
        new Tile(TileLayer.Back, 53, 7, 2202, tileSheetIndex),
        new Tile(TileLayer.Back, 54, 7, 2203, tileSheetIndex),
        new Tile(TileLayer.Back, 51, 8, 2225, tileSheetIndex),
        new Tile(TileLayer.Back, 52, 8, 2226, tileSheetIndex),
        new Tile(TileLayer.Back, 53, 8, 2227, tileSheetIndex),
        new Tile(TileLayer.Back, 54, 8, 2228, tileSheetIndex),
        new Tile(TileLayer.Back, 51, 9, 2250, tileSheetIndex),
        new Tile(TileLayer.Back, 52, 9, 2251, tileSheetIndex),
        new Tile(TileLayer.Back, 53, 9, 2252, tileSheetIndex),
        new Tile(TileLayer.Back, 54, 9, 2253, tileSheetIndex),
        new Tile(TileLayer.Buildings, 51, 4, 2668, tileSheetIndex),
        new Tile(TileLayer.Buildings, 52, 4, 2669, tileSheetIndex),
        new Tile(TileLayer.Buildings, 53, 4, 2670, tileSheetIndex),
        new Tile(TileLayer.Buildings, 54, 4, 2671, tileSheetIndex),
        new Tile(TileLayer.Buildings, 51, 5, 2693, tileSheetIndex),
        new Tile(TileLayer.Buildings, 52, 5, 2694, tileSheetIndex),
        new Tile(TileLayer.Buildings, 53, 5, 2695, tileSheetIndex),
        new Tile(TileLayer.Buildings, 54, 5, 2696, tileSheetIndex),
        new Tile(TileLayer.Buildings, 51, 6, 2718, tileSheetIndex),
        new Tile(TileLayer.Buildings, 52, 6, 2719, tileSheetIndex),
        new Tile(TileLayer.Buildings, 53, 6, 2720, tileSheetIndex),
        new Tile(TileLayer.Buildings, 54, 6, 2721, tileSheetIndex),
        new Tile(TileLayer.Buildings, 52, 7, 2201, tileSheetIndex),
        new Tile(TileLayer.Buildings, 53, 7, 2202, tileSheetIndex),
        new Tile(TileLayer.Buildings, 54, 7, -1),
        new Tile(TileLayer.AlwaysFront, 51, 3, 2643, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 52, 3, 2644, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 53, 3, 2645, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 54, 3, 2646, tileSheetIndex),
        new Tile(TileLayer.Front, 52, 4, -1),
        new Tile(TileLayer.Front, 50, 5, -1),
        new Tile(TileLayer.Front, 51, 5, -1),
        new Tile(TileLayer.Front, 52, 5, -1),
        new Tile(TileLayer.Front, 53, 5, -1),
        new Tile(TileLayer.Front, 54, 5, -1),
        new Tile(TileLayer.Buildings, 50, 6, -1)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> YardGardenEditsAndBinClutter(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Back, 57, 8, -1),
        new Tile(TileLayer.Back, 57, 11, -1),
        new Tile(TileLayer.Back, 56, 4, 200, tileSheetIndex),
        new Tile(TileLayer.Back, 57, 4, 179, tileSheetIndex),
        new Tile(TileLayer.Back, 58, 4, 180, tileSheetIndex),
        new Tile(TileLayer.Back, 59, 4, 179, tileSheetIndex),
        new Tile(TileLayer.Back, 60, 4, 203, tileSheetIndex),
        new Tile(TileLayer.Back, 56, 5, 225, tileSheetIndex),
        new Tile(TileLayer.Back, 57, 5, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 58, 5, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 59, 5, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 60, 5, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 56, 6, 225, tileSheetIndex),
        new Tile(TileLayer.Back, 57, 6, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 58, 6, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 59, 6, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 60, 6, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 56, 7, 225, tileSheetIndex),
        new Tile(TileLayer.Back, 57, 7, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 58, 7, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 59, 7, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 60, 7, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 56, 8, 225, tileSheetIndex),
        new Tile(TileLayer.Back, 57, 8, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 58, 8, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 59, 8, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 60, 8, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 56, 9, 250, tileSheetIndex),
        new Tile(TileLayer.Back, 57, 9, 251, tileSheetIndex),
        new Tile(TileLayer.Back, 58, 9, 251, tileSheetIndex),
        new Tile(TileLayer.Back, 59, 9, 251, tileSheetIndex),
        new Tile(TileLayer.Back, 60, 9, 253, tileSheetIndex),
        new Tile(TileLayer.Back, 55, 4, 203, tileSheetIndex),
        new Tile(TileLayer.Back, 55, 5, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 55, 6, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 55, 7, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 55, 8, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 55, 9, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 55, 10, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 55, 11, 177, tileSheetIndex),
        new Tile(TileLayer.Back, 56, 11, 201, tileSheetIndex),
        new Tile(TileLayer.Back, 57, 11, 201, tileSheetIndex),
        new Tile(TileLayer.Back, 58, 11, 203, tileSheetIndex),
        new Tile(TileLayer.Back, 58, 12, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 58, 13, 228, tileSheetIndex),
        new Tile(TileLayer.Back, 56, 10, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 56, 12, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 57, 12, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 57, 13, 227, tileSheetIndex),
        new Tile(TileLayer.Back, 61, 5, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 62, 5, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 63, 5, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 64, 5, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 65, 5, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 61, 6, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 62, 6, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 63, 6, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 64, 6, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 65, 6, 175, tileSheetIndex),
        new Tile(TileLayer.Front, 55, 3, 2469, tileSheetIndex),
        new Tile(TileLayer.Front, 56, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 57, 3, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 58, 3, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 59, 3, 2469, tileSheetIndex),
        new Tile(TileLayer.Front, 60, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 61, 3, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 62, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 63, 3, 2469, tileSheetIndex),
        new Tile(TileLayer.Front, 64, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 65, 3, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 66, 3, 2469, tileSheetIndex),
        new Tile(TileLayer.Front, 67, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 68, 3, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 69, 3, 2469, tileSheetIndex),
        new Tile(TileLayer.Front, 70, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 71, 3, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 72, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 73, 3, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 74, 3, 2469, tileSheetIndex),
        new Tile(TileLayer.Front, 75, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 76, 3, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 77, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 78, 3, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 79, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 51, 3, 2468, tileSheetIndex),
        new Tile(TileLayer.Front, 52, 3, 2469, tileSheetIndex),
        new Tile(TileLayer.Front, 53, 3, 2470, tileSheetIndex),
        new Tile(TileLayer.Front, 54, 3, 2471, tileSheetIndex),
        new Tile(TileLayer.Front, 55, 3, 2472, tileSheetIndex),
        new Tile(TileLayer.Back, 51, 4, 2493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 52, 4, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 53, 4, 2495, tileSheetIndex),
        new Tile(TileLayer.Back, 54, 4, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 55, 4, 2497, tileSheetIndex),
        new Tile(TileLayer.Buildings, 56, 4, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 57, 4, 2495, tileSheetIndex),
        new Tile(TileLayer.Buildings, 58, 4, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 59, 4, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 60, 4, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 61, 4, 2495, tileSheetIndex),
        new Tile(TileLayer.Buildings, 62, 4, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 63, 4, 2497, tileSheetIndex),
        new Tile(TileLayer.Buildings, 64, 4, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 65, 4, 2497, tileSheetIndex),
        new Tile(TileLayer.Buildings, 66, 4, 2493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 67, 4, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 68, 4, 2495, tileSheetIndex),
        new Tile(TileLayer.Buildings, 69, 4, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 70, 4, 2493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 71, 4, 2493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 72, 4, 2494, tileSheetIndex),
        new Tile(TileLayer.Buildings, 73, 4, 2495, tileSheetIndex),
        new Tile(TileLayer.Buildings, 74, 4, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 75, 4, 2497, tileSheetIndex),
        new Tile(TileLayer.Buildings, 76, 4, 2126, tileSheetIndex),
        new Tile(TileLayer.Buildings, 77, 4, 2496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 78, 4, 2493, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 4, 1496, tileSheetIndex),
        new Tile(TileLayer.Buildings, 75, 5, 2741, tileSheetIndex),
        new Tile(TileLayer.Buildings, 76, 5, 2003, tileSheetIndex),
        new Tile(TileLayer.Buildings, 77, 5, 2062, tileSheetIndex),
        new Tile(TileLayer.Buildings, 78, 5, 1520, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 5, 1521, tileSheetIndex),
        new Tile(TileLayer.Buildings, 77, 6, 2087, tileSheetIndex),
        new Tile(TileLayer.Buildings, 78, 6, 2088, tileSheetIndex),
        new Tile(TileLayer.Buildings, 77, 7, 2112, tileSheetIndex),
        new Tile(TileLayer.Buildings, 78, 7, 2113, tileSheetIndex),
        new Tile(TileLayer.Buildings, 76, 6, 21, tileSheetIndex),
        new Tile(TileLayer.Front, 75, 4, 2716, tileSheetIndex),
        new Tile(TileLayer.Front, 76, 4, 2036, tileSheetIndex),
        new Tile(TileLayer.Front, 77, 4, 2037, tileSheetIndex),
        new Tile(TileLayer.Front, 78, 4, 1495, tileSheetIndex),
        new Tile(TileLayer.Back, 79, 4, 2496, tileSheetIndex),
        new Tile(TileLayer.Front, 76, 5, 2061, tileSheetIndex),
        new Tile(TileLayer.Front, 78, 5, 2063, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 76, 3, 2101, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 78, 4, 2038, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> PatioEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>
      {
        new Tile(TileLayer.Back, 71, 7, -1),
        new Tile(TileLayer.Back, 71, 11, -1),
        new Tile(TileLayer.Back, 66, 5, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 67, 5, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 68, 5, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 69, 5, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 70, 5, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 71, 5, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 72, 5, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 73, 5, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 66, 6, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 67, 6, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 68, 6, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 69, 6, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 70, 6, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 71, 6, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 72, 6, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 73, 6, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 66, 7, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 67, 7, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 68, 7, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 69, 7, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 70, 7, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 71, 7, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 72, 7, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 73, 7, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 66, 8, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 67, 8, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 68, 8, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 69, 8, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 70, 8, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 71, 8, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 72, 8, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 73, 8, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 66, 9, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 67, 9, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 68, 9, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 69, 9, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 70, 9, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 71, 9, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 72, 9, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 73, 9, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 66, 10, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 67, 10, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 68, 10, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 69, 10, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 70, 10, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 71, 10, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 72, 10, 2738, tileSheetIndex),
        new Tile(TileLayer.Back, 73, 10, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 68, 11, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 69, 11, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 70, 11, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 71, 11, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 72, 11, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 73, 11, 2715, tileSheetIndex),
        new Tile(TileLayer.Back, 65, 11, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 66, 11, 2715, tileSheetIndex),
        new Tile(TileLayer.Buildings, 67, 11, 2715, tileSheetIndex),
        new Tile(TileLayer.Front, 55, 8, -1),
        new Tile(TileLayer.Front, 56, 8, -1),
        new Tile(TileLayer.Front, 57, 8, -1),
        new Tile(TileLayer.Front, 58, 8, -1),
        new Tile(TileLayer.Front, 59, 8, -1),
        new Tile(TileLayer.Front, 60, 8, -1),
        new Tile(TileLayer.Front, 61, 8, -1),
        new Tile(TileLayer.Front, 62, 8, -1),
        new Tile(TileLayer.Front, 63, 8, -1),
        new Tile(TileLayer.Front, 64, 8, -1),
        new Tile(TileLayer.Front, 65, 8, -1),
        new Tile(TileLayer.Front, 66, 8, -1),
        new Tile(TileLayer.Front, 67, 8, -1),
        new Tile(TileLayer.Front, 68, 8, -1),
        new Tile(TileLayer.Front, 69, 8, -1),
        new Tile(TileLayer.Front, 70, 8, -1),
        new Tile(TileLayer.Front, 71, 8, -1),
        new Tile(TileLayer.Front, 72, 8, -1),
        new Tile(TileLayer.Front, 73, 8, -1),
        new Tile(TileLayer.Front, 74, 8, -1),
        new Tile(TileLayer.Front, 75, 8, -1),
        new Tile(TileLayer.Front, 76, 8, -1),
        new Tile(TileLayer.Front, 77, 8, -1),
        new Tile(TileLayer.Front, 78, 8, -1),
        new Tile(TileLayer.Front, 79, 8, -1),
        new Tile(TileLayer.Buildings, 55, 9, -1),
        new Tile(TileLayer.Buildings, 56, 9, -1),
        new Tile(TileLayer.Buildings, 57, 9, -1),
        new Tile(TileLayer.Buildings, 58, 9, -1),
        new Tile(TileLayer.Buildings, 59, 9, -1),
        new Tile(TileLayer.Buildings, 60, 9, -1),
        new Tile(TileLayer.Buildings, 61, 9, -1),
        new Tile(TileLayer.Buildings, 62, 9, -1),
        new Tile(TileLayer.Buildings, 63, 9, -1),
        new Tile(TileLayer.Buildings, 64, 9, -1),
        new Tile(TileLayer.Buildings, 65, 9, -1),
        new Tile(TileLayer.Buildings, 66, 9, -1),
        new Tile(TileLayer.Buildings, 67, 9, -1),
        new Tile(TileLayer.Buildings, 68, 9, -1),
        new Tile(TileLayer.Buildings, 69, 9, -1),
        new Tile(TileLayer.Buildings, 70, 9, -1),
        new Tile(TileLayer.Buildings, 71, 9, -1),
        new Tile(TileLayer.Buildings, 72, 9, -1),
        new Tile(TileLayer.Buildings, 73, 9, -1),
        new Tile(TileLayer.Buildings, 74, 9, -1),
        new Tile(TileLayer.Buildings, 75, 9, -1),
        new Tile(TileLayer.Buildings, 76, 9, -1),
        new Tile(TileLayer.Buildings, 77, 9, -1),
        new Tile(TileLayer.Buildings, 78, 5, -1),
        new Tile(TileLayer.Buildings, 78, 6, -1),
        new Tile(TileLayer.Buildings, 78, 7, -1),
        new Tile(TileLayer.Buildings, 78, 8, -1),
        new Tile(TileLayer.Buildings, 78, 9, -1),
        new Tile(TileLayer.Buildings, 78, 10, -1),
        new Tile(TileLayer.Buildings, 55, 8, -1),
        new Tile(TileLayer.Buildings, 56, 8, -1),
        new Tile(TileLayer.Buildings, 57, 8, -1),
        new Tile(TileLayer.Buildings, 58, 8, -1),
        new Tile(TileLayer.Buildings, 59, 8, -1),
        new Tile(TileLayer.Buildings, 60, 8, -1),
        new Tile(TileLayer.Buildings, 61, 8, -1),
        new Tile(TileLayer.Buildings, 62, 8, -1),
        new Tile(TileLayer.Buildings, 63, 8, -1),
        new Tile(TileLayer.Buildings, 64, 8, -1),
        new Tile(TileLayer.Buildings, 65, 8, -1),
        new Tile(TileLayer.Buildings, 66, 8, -1),
        new Tile(TileLayer.Buildings, 67, 8, -1),
        new Tile(TileLayer.Buildings, 68, 8, -1),
        new Tile(TileLayer.Buildings, 69, 8, -1),
        new Tile(TileLayer.Buildings, 70, 8, -1),
        new Tile(TileLayer.Buildings, 71, 8, -1),
        new Tile(TileLayer.Buildings, 72, 8, -1),
        new Tile(TileLayer.Buildings, 73, 8, -1),
        new Tile(TileLayer.Buildings, 74, 8, -1),
        new Tile(TileLayer.Buildings, 75, 8, -1),
        new Tile(TileLayer.Buildings, 76, 8, -1),
        new Tile(TileLayer.Buildings, 77, 8, -1),
        new Tile(TileLayer.Buildings, 55, 7, -1),
        new Tile(TileLayer.Buildings, 56, 7, -1),
        new Tile(TileLayer.Buildings, 57, 7, -1),
        new Tile(TileLayer.Buildings, 58, 7, -1),
        new Tile(TileLayer.Buildings, 59, 7, -1),
        new Tile(TileLayer.Buildings, 60, 7, -1),
        new Tile(TileLayer.Buildings, 61, 7, -1),
        new Tile(TileLayer.Buildings, 62, 7, -1),
        new Tile(TileLayer.Buildings, 63, 7, -1),
        new Tile(TileLayer.Buildings, 64, 7, -1),
        new Tile(TileLayer.Buildings, 65, 7, -1),
        new Tile(TileLayer.Buildings, 66, 7, -1),
        new Tile(TileLayer.Buildings, 67, 7, -1),
        new Tile(TileLayer.Buildings, 68, 7, -1),
        new Tile(TileLayer.Buildings, 69, 7, -1),
        new Tile(TileLayer.Buildings, 70, 7, -1),
        new Tile(TileLayer.Buildings, 71, 7, -1),
        new Tile(TileLayer.Buildings, 72, 7, -1),
        new Tile(TileLayer.Buildings, 73, 7, -1),
        new Tile(TileLayer.Buildings, 75, 7, -1),
        new Tile(TileLayer.Buildings, 76, 7, -1),
        new Tile(TileLayer.Buildings, 77, 7, -1),
        new Tile(TileLayer.Buildings, 55, 6, -1),
        new Tile(TileLayer.Buildings, 56, 6, -1),
        new Tile(TileLayer.Buildings, 57, 6, -1),
        new Tile(TileLayer.Buildings, 58, 6, -1),
        new Tile(TileLayer.Buildings, 59, 6, -1),
        new Tile(TileLayer.Buildings, 60, 6, -1),
        new Tile(TileLayer.Buildings, 61, 6, -1),
        new Tile(TileLayer.Buildings, 62, 6, -1),
        new Tile(TileLayer.Buildings, 63, 6, -1),
        new Tile(TileLayer.Buildings, 64, 6, -1),
        new Tile(TileLayer.Buildings, 65, 6, -1),
        new Tile(TileLayer.Buildings, 66, 6, -1),
        new Tile(TileLayer.Buildings, 67, 6, -1),
        new Tile(TileLayer.Buildings, 68, 6, -1),
        new Tile(TileLayer.Buildings, 69, 6, -1),
        new Tile(TileLayer.Buildings, 70, 6, -1),
        new Tile(TileLayer.Buildings, 71, 6, -1),
        new Tile(TileLayer.Buildings, 72, 6, -1),
        new Tile(TileLayer.Buildings, 73, 6, -1),
        new Tile(TileLayer.Buildings, 74, 6, -1),
        new Tile(TileLayer.Buildings, 75, 6, -1),
        new Tile(TileLayer.Buildings, 77, 6, -1),
        new Tile(TileLayer.Buildings, 55, 5, -1),
        new Tile(TileLayer.Buildings, 56, 5, -1),
        new Tile(TileLayer.Buildings, 57, 5, -1),
        new Tile(TileLayer.Buildings, 58, 5, -1),
        new Tile(TileLayer.Buildings, 59, 5, -1),
        new Tile(TileLayer.Buildings, 60, 5, -1),
        new Tile(TileLayer.Buildings, 61, 5, -1),
        new Tile(TileLayer.Buildings, 62, 5, -1),
        new Tile(TileLayer.Buildings, 63, 5, -1),
        new Tile(TileLayer.Buildings, 64, 5, -1),
        new Tile(TileLayer.Buildings, 65, 5, -1),
        new Tile(TileLayer.Buildings, 66, 5, -1),
        new Tile(TileLayer.Buildings, 67, 5, -1),
        new Tile(TileLayer.Buildings, 68, 5, -1),
        new Tile(TileLayer.Buildings, 69, 5, -1),
        new Tile(TileLayer.Buildings, 70, 5, -1),
        new Tile(TileLayer.Buildings, 71, 5, -1),
        new Tile(TileLayer.Buildings, 72, 5, -1),
        new Tile(TileLayer.Buildings, 73, 5, -1),
        new Tile(TileLayer.Buildings, 74, 5, -1),
        new Tile(TileLayer.Buildings, 77, 5, -1),
        new Tile(TileLayer.Front, 55, 3, -1),
        new Tile(TileLayer.Front, 56, 3, -1),
        new Tile(TileLayer.Front, 57, 3, -1),
        new Tile(TileLayer.Front, 55, 4, -1),
        new Tile(TileLayer.Front, 56, 4, -1),
        new Tile(TileLayer.Front, 57, 4, -1),
        new Tile(TileLayer.Front, 55, 5, -1),
        new Tile(TileLayer.Front, 56, 5, -1),
        new Tile(TileLayer.Front, 57, 5, -1),
        new Tile(TileLayer.Front, 55, 6, -1),
        new Tile(TileLayer.Front, 56, 6, -1),
        new Tile(TileLayer.Front, 57, 6, -1),
        new Tile(TileLayer.Front, 56, 7, -1),
        new Tile(TileLayer.Front, 54, 5, -1),
        new Tile(TileLayer.Buildings, 55, 5, -1),
        new Tile(TileLayer.Front, 68, 3, -1),
        new Tile(TileLayer.Front, 69, 3, -1),
        new Tile(TileLayer.Front, 70, 3, -1),
        new Tile(TileLayer.Front, 68, 4, -1),
        new Tile(TileLayer.Front, 69, 4, -1),
        new Tile(TileLayer.Front, 70, 4, -1),
        new Tile(TileLayer.Front, 68, 5, -1),
        new Tile(TileLayer.Front, 69, 5, -1),
        new Tile(TileLayer.Front, 70, 5, -1),
        new Tile(TileLayer.Front, 68, 6, -1),
        new Tile(TileLayer.Front, 69, 6, -1),
        new Tile(TileLayer.Front, 70, 6, -1),
        new Tile(TileLayer.Front, 69, 7, -1),
        new Tile(TileLayer.Front, 73, 2, -1),
        new Tile(TileLayer.Front, 74, 2, -1),
        new Tile(TileLayer.Front, 75, 2, -1),
        new Tile(TileLayer.Front, 73, 3, -1),
        new Tile(TileLayer.Front, 74, 3, -1),
        new Tile(TileLayer.Front, 75, 3, -1),
        new Tile(TileLayer.Front, 73, 4, -1),
        new Tile(TileLayer.Front, 74, 4, -1),
        new Tile(TileLayer.Front, 75, 4, -1),
        new Tile(TileLayer.Front, 73, 5, -1),
        new Tile(TileLayer.Front, 74, 5, -1),
        new Tile(TileLayer.Front, 75, 5, -1),
        new Tile(TileLayer.Front, 74, 6, -1),
        new Tile(TileLayer.Buildings, 74, 7, -1),
        new Tile(TileLayer.Front, 78, 4, -1),
        new Tile(TileLayer.Front, 79, 4, -1),
        new Tile(TileLayer.Front, 78, 5, -1),
        new Tile(TileLayer.Front, 79, 5, -1),
        new Tile(TileLayer.Front, 78, 6, -1),
        new Tile(TileLayer.Front, 79, 6, -1),
        new Tile(TileLayer.Front, 78, 7, -1),
        new Tile(TileLayer.Front, 79, 7, -1),
        new Tile(TileLayer.Front, 57, 0, -1),
        new Tile(TileLayer.Front, 58, 0, -1),
        new Tile(TileLayer.Front, 59, 0, -1),
        new Tile(TileLayer.Front, 60, 0, -1),
        new Tile(TileLayer.Front, 61, 0, -1),
        new Tile(TileLayer.Front, 57, 1, -1),
        new Tile(TileLayer.Front, 58, 1, -1),
        new Tile(TileLayer.Front, 59, 1, -1),
        new Tile(TileLayer.Front, 60, 1, -1),
        new Tile(TileLayer.Front, 61, 1, -1),
        new Tile(TileLayer.Front, 62, 1, -1),
        new Tile(TileLayer.Front, 57, 2, -1),
        new Tile(TileLayer.Front, 58, 2, -1),
        new Tile(TileLayer.Front, 59, 2, -1),
        new Tile(TileLayer.Front, 60, 2, -1),
        new Tile(TileLayer.Front, 61, 2, -1),
        new Tile(TileLayer.Front, 62, 2, -1),
        new Tile(TileLayer.Front, 59, 3, -1),
        new Tile(TileLayer.Front, 60, 3, -1),
        new Tile(TileLayer.Front, 59, 4, -1),
        new Tile(TileLayer.Front, 60, 4, -1),
        new Tile(TileLayer.Buildings, 58, 0, 77, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 62, 0, 55, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 62, 1, 80, tileSheetIndex),
        new Tile(TileLayer.Back, 65, 5, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 74, 5, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 74, 6, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 74, 7, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 74, 8, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 74, 9, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 74, 10, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 75, 9, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 75, 10, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 76, 10, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 77, 10, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 76, 11, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 77, 11, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 56, 8, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 56, 9, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 57, 9, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 58, 9, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 59, 9, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 60, 9, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 61, 9, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 62, 9, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 63, 9, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 64, 9, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 65, 9, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 59, 4, 351, tileSheetIndex),
        new Tile(TileLayer.Back, 60, 4, 351, tileSheetIndex),
        new Tile(TileLayer.Front, 58, 1, 13, tileSheetIndex),
        new Tile(TileLayer.Front, 59, 1, 14, tileSheetIndex),
        new Tile(TileLayer.Front, 60, 1, 15, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 68, 4, 2069),
        new Tile(TileLayer.AlwaysFront, 69, 4, 2070, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 70, 4, 2071, tileSheetIndex),
        new Tile(TileLayer.Front, 68, 5, 2094, tileSheetIndex),
        new Tile(TileLayer.Front, 69, 5, 2095, tileSheetIndex),
        new Tile(TileLayer.Front, 70, 5, 2096, tileSheetIndex),
        new Tile(TileLayer.Front, 71, 5, 2097, tileSheetIndex),
        new Tile(TileLayer.Buildings, 68, 6, 2119, tileSheetIndex),
        new Tile(TileLayer.Buildings, 69, 6, 2120, tileSheetIndex),
        new Tile(TileLayer.Buildings, 70, 6, 2121, tileSheetIndex),
        new Tile(TileLayer.Buildings, 71, 6, 2122, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 69, 7, 2145),
        new Tile(TileLayer.AlwaysFront, 70, 7, 2146),
        new Tile(TileLayer.AlwaysFront, 71, 7, 2147),
        new Tile(TileLayer.Buildings, 69, 8, 2170, tileSheetIndex),
        new Tile(TileLayer.Buildings, 70, 8, 2171, tileSheetIndex),
        new Tile(TileLayer.Buildings, 71, 8, 2172, tileSheetIndex),
        new Tile(TileLayer.Buildings, 69, 9, 2195, tileSheetIndex),
        new Tile(TileLayer.Buildings, 70, 9, 2196, tileSheetIndex),
        new Tile(TileLayer.Buildings, 71, 9, 2197, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private List<Tile> FarmStandEdits(GameLocation gl)
        {
            int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
            List<Tile> tileArray = new List<Tile>()
      {
        new Tile(TileLayer.Back, 71, 12, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 72, 12, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 70, 13, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 71, 13, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 72, 13, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 73, 13, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 70, 14, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 73, 14, 175, tileSheetIndex),
        new Tile(TileLayer.Back, 73, 14, 2232, tileSheetIndex),
        new Tile(TileLayer.Back, 74, 14, 2232, tileSheetIndex),
        new Tile(TileLayer.Back, 75, 14, 2232, tileSheetIndex),
        new Tile(TileLayer.Back, 76, 14, 2232, tileSheetIndex),
        new Tile(TileLayer.Paths, 77, 11, -1),
        new Tile(TileLayer.Paths, 74, 10, -1),
        new Tile(TileLayer.Paths, 75, 10, -1),
        new Tile(TileLayer.Front, 79, 10, -1),
        new Tile(TileLayer.Front, 79, 13, -1),
        new Tile(TileLayer.Front, 79, 12, 2138, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 76, 9, 1982, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 77, 9, 1983, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 78, 9, 1984, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 79, 9, 1985, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 77, 10, 2008, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 78, 10, 2009, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 79, 10, 2010, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 79, 11, 2035, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 73, 10, 2004, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 74, 10, 2005, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 75, 10, 2006, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 76, 10, 2007, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 73, 11, 2029, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 74, 11, 2030, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 75, 11, 2031, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 76, 11, 2032, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 73, 12, 2054, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 74, 12, 2055, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 75, 12, 2056, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 76, 12, 2057, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 73, 13, 2079, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 74, 13, 2080, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 75, 13, 2081, tileSheetIndex),
        new Tile(TileLayer.AlwaysFront, 76, 13, 2082, tileSheetIndex),
        new Tile(TileLayer.Front, 72, 14, 2103, tileSheetIndex),
        new Tile(TileLayer.Front, 73, 14, 2104, tileSheetIndex),
        new Tile(TileLayer.Front, 74, 14, 2105, tileSheetIndex),
        new Tile(TileLayer.Front, 75, 14, 2106, tileSheetIndex),
        new Tile(TileLayer.Front, 76, 14, 2107, tileSheetIndex),
        new Tile(TileLayer.Front, 77, 14, 2108, tileSheetIndex),
        new Tile(TileLayer.Front, 78, 14, 2109, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 11, -1),
        new Tile(TileLayer.Buildings, 79, 6, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 7, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 8, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 9, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 10, 21, tileSheetIndex),
        new Tile(TileLayer.Buildings, 78, 12, 2059, tileSheetIndex),
        new Tile(TileLayer.Buildings, 78, 11, 2034, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 12, 2060, tileSheetIndex),
        new Tile(TileLayer.Buildings, 77, 13, 2083, tileSheetIndex),
        new Tile(TileLayer.Buildings, 77, 12, 2058, tileSheetIndex),
        new Tile(TileLayer.Buildings, 77, 11, 2033, tileSheetIndex),
        new Tile(TileLayer.Buildings, 78, 13, 2084, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 13, 2163, tileSheetIndex),
        new Tile(TileLayer.Buildings, 78, 14, 2109, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 14, 2188, tileSheetIndex),
        new Tile(TileLayer.Buildings, 72, 15, 2128, tileSheetIndex),
        new Tile(TileLayer.Buildings, 73, 15, 2129, tileSheetIndex),
        new Tile(TileLayer.Buildings, 74, 15, 2130, tileSheetIndex),
        new Tile(TileLayer.Buildings, 75, 15, 2131, tileSheetIndex),
        new Tile(TileLayer.Buildings, 76, 15, 2132, tileSheetIndex),
        new Tile(TileLayer.Buildings, 77, 15, 2133, tileSheetIndex),
        new Tile(TileLayer.Buildings, 78, 15, 2134, tileSheetIndex),
        new Tile(TileLayer.Buildings, 79, 15, 2213, tileSheetIndex),
        new Tile(TileLayer.Buildings, 72, 16, 2153, tileSheetIndex),
        new Tile(TileLayer.Buildings, 73, 16, 2154, tileSheetIndex)
      };
            return this.InitializeTileArray(gl, tileArray);
        }

        private void PatchMap(GameLocation gl, List<Tile> tileArray)
        {
            try
            {
                foreach (Tile tile in tileArray)
                {
                    if (tile.TileIndex < 0)
                    {
                        gl.removeTile(tile.X, tile.Y, tile.LayerName);
                        gl.waterTiles[tile.X, tile.Y] = false;

                        foreach (LargeTerrainFeature feature in gl.largeTerrainFeatures)
                        {
                            if (feature.tilePosition.X == tile.X && feature.tilePosition.Y == tile.Y)
                            {
                                gl.largeTerrainFeatures.Remove(feature);
                                break;
                            }
                        }
                    }
                    else if (gl.map.GetLayer(tile.LayerName).Tiles[tile.X, tile.Y] == null || gl.map.GetLayer(tile.LayerName).Tiles[tile.X, tile.Y].TileSheet.Id != tile.TileSheet)
                    {
                        int tileSheetIndex = Tile.GetTileSheetIndex("untitled tile sheet", gl.map.TileSheets);
                        gl.map.GetLayer(tile.LayerName).Tiles[tile.X, tile.Y] = new StaticTile(gl.map.GetLayer(tile.LayerName), gl.map.TileSheets[tileSheetIndex], 0, tile.TileIndex);
                    }
                    else
                        gl.setMapTileIndex(tile.X, tile.Y, tile.TileIndex, gl.map.GetLayer(tile.LayerName).Id);
                }
            }
            finally
            {
                tileArray.Clear();
            }
        }

        private List<Tile> InitializeTileArray(GameLocation gl, List<Tile> tileArray)
        {
            foreach (Tile tile in tileArray)
            {
                if (tile.TileSheetIndex < 0 || tile.TileSheetIndex >= gl.map.TileSheets.Count)
                    tile.TileSheetIndex = Tile.GetTileSheetIndex(tile.TileSheet, gl.map.TileSheets);
                if (string.IsNullOrEmpty(tile.TileSheet))
                    tile.TileSheet = Tile.GetTileSheetName(tile.TileSheetIndex, gl.map.TileSheets);
            }
            return tileArray;
        }

        public Texture2D PatchTexture(Texture2D targetTexture, string overridingTexturePath, Dictionary<int, int> spriteOverrides, int gridWidth, int gridHeight)
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
