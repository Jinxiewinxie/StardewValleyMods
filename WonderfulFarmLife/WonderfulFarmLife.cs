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
                this.PatchMap(farm, this.FarmStandEdits());
                farm.setTileProperty(74, 15, "Buildings", "Action", "NewShippingBin");
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2058@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2083@Passable", new PropertyValue(true));
            }
            if (this.Config.EditPath)
                this.PatchMap(farm, this.PathEdits());
            if (this.Config.RemovePathAlltogether)
            {
                this.PatchMap(farm, this.PathEdits());
                this.PatchMap(farm, this.RemovePathEdits());
            }
            if (this.Config.RemoveShippingBin)
                this.PatchMap(farm, this.RemoveShippingBinEdits());
            if (this.Config.ShowPatio)
            {
                this.PatchMap(farm, this.PatioEdits());
                farm.setTileProperty(68, 6, "Buildings", "Action", "kitchen");
                farm.setTileProperty(69, 6, "Buildings", "Action", "kitchen");
            }
            if (this.Config.ShowBinClutter)
            {
                this.PatchMap(farm, this.YardGardenEditsAndBinClutter());
                farm.setTileProperty(75, 4, "Buildings", "Action", "Jukebox");
                farm.setTileProperty(75, 5, "Buildings", "Action", "Jukebox");
            }
            if (this.Config.AddDogHouse)
            {
                this.PatchMap(farm, this.DogHouseEdits());
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2718@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2719@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2720@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2721@Passable", new PropertyValue(true));
            }
            if (this.Config.AddGreenHouseArch)
                this.PatchMap(farm, this.GreenHouseArchEdits());
            if (this.Config.ShowPicnicBlanket)
                this.PatchMap(farm, this.PicnicBlanketEdits());
            if (this.Config.ShowPicnicTable)
                this.PatchMap(farm, this.PicnicAreaTableEdits());
            if (this.Config.ShowTreeSwing)
            {
                this.PatchMap(farm, this.TreeSwingEdits());
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2944@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2969@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2941@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2942@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2966@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2967@Passable", new PropertyValue(true));
            }
            if (this.Config.AddStoneBridge)
                this.PatchMap(farm, this.StoneBridgeEdits());
            if (this.Config.AddTelescopeArea)
            {
                this.PatchMap(farm, this.TelescopeEdits());
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2619@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2620@Passable", new PropertyValue(true));
                farm.map.GetTileSheet("untitled tile sheet").Properties.Add("@TileIndex@2621@Passable", new PropertyValue(true));
                farm.setTileProperty(30, 2, "Buildings", "Action", "TelescopeMessage");
            }
            if (this.Config.ShowMemorialArea)
                this.PatchMap(farm, this.MemorialArea());
            if (this.Config.ShowMemorialAreaArch)
                this.PatchMap(farm, this.MemorialAreaArch());
            if (this.Config.UsingTSE)
                this.PatchMap(farm, this.TegoFixes());
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

        private Tile[] PathEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 70, 15, 2232, tilesheet),
                new Tile(TileLayer.Back, 71, 15, 2232, tilesheet),
                new Tile(TileLayer.Back, 72, 15, 2232, tilesheet),
                new Tile(TileLayer.Back, 73, 15, 2232, tilesheet),
                new Tile(TileLayer.Back, 69, 16, 200, tilesheet),
                new Tile(TileLayer.Back, 70, 16, 179, tilesheet),
                new Tile(TileLayer.Back, 71, 16, 205, tilesheet),
                new Tile(TileLayer.Back, 72, 16, 179, tilesheet),
                new Tile(TileLayer.Back, 73, 16, 205, tilesheet),
                new Tile(TileLayer.Back, 72, 17, 227, tilesheet),
                new Tile(TileLayer.Back, 73, 17, 227, tilesheet),
                new Tile(TileLayer.Back, 74, 17, 227, tilesheet),
                new Tile(TileLayer.Back, 75, 17, 624, tilesheet)
            };
        }

        private Tile[] GreenHouseArchEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.AlwaysFront, 25, 10, 2626, tilesheet),
                new Tile(TileLayer.AlwaysFront, 26, 10, 2627, tilesheet),
                new Tile(TileLayer.AlwaysFront, 27, 10, 2628, tilesheet),
                new Tile(TileLayer.AlwaysFront, 28, 10, 2629, tilesheet),
                new Tile(TileLayer.AlwaysFront, 29, 10, 2630, tilesheet),
                new Tile(TileLayer.AlwaysFront, 30, 10, 2631, tilesheet),
                new Tile(TileLayer.AlwaysFront, 31, 10, 2632, tilesheet),
                new Tile(TileLayer.AlwaysFront, 32, 10, 2633, tilesheet),
                new Tile(TileLayer.AlwaysFront, 33, 10, 2635, tilesheet),
                new Tile(TileLayer.AlwaysFront, 24, 11, 2650, tilesheet),
                new Tile(TileLayer.AlwaysFront, 25, 11, 2651, tilesheet),
                new Tile(TileLayer.AlwaysFront, 26, 11, 2652, tilesheet),
                new Tile(TileLayer.AlwaysFront, 27, 11, 2653, tilesheet),
                new Tile(TileLayer.AlwaysFront, 28, 11, 2654, tilesheet),
                new Tile(TileLayer.AlwaysFront, 29, 11, 2655, tilesheet),
                new Tile(TileLayer.AlwaysFront, 30, 11, 2656, tilesheet),
                new Tile(TileLayer.AlwaysFront, 31, 11, 2657, tilesheet),
                new Tile(TileLayer.AlwaysFront, 32, 11, 2658, tilesheet),
                new Tile(TileLayer.AlwaysFront, 33, 11, 2659, tilesheet),
                new Tile(TileLayer.AlwaysFront, 24, 12, 2675, tilesheet),
                new Tile(TileLayer.AlwaysFront, 25, 12, 2676, tilesheet),
                new Tile(TileLayer.AlwaysFront, 26, 12, 2677, tilesheet),
                new Tile(TileLayer.AlwaysFront, 27, 12, 2678, tilesheet),
                new Tile(TileLayer.AlwaysFront, 28, 12, 2679, tilesheet),
                new Tile(TileLayer.AlwaysFront, 29, 12, 2680, tilesheet),
                new Tile(TileLayer.AlwaysFront, 30, 12, 2681, tilesheet),
                new Tile(TileLayer.AlwaysFront, 31, 12, 2682, tilesheet),
                new Tile(TileLayer.AlwaysFront, 32, 12, 2683, tilesheet),
                new Tile(TileLayer.AlwaysFront, 33, 12, 2684, tilesheet),
                new Tile(TileLayer.AlwaysFront, 24, 13, 2700, tilesheet),
                new Tile(TileLayer.AlwaysFront, 25, 13, 2701, tilesheet),
                new Tile(TileLayer.AlwaysFront, 26, 13, 2702, tilesheet),
                new Tile(TileLayer.AlwaysFront, 27, 13, 2703, tilesheet),
                new Tile(TileLayer.AlwaysFront, 28, 13, 2704, tilesheet),
                new Tile(TileLayer.AlwaysFront, 29, 13, 2705, tilesheet),
                new Tile(TileLayer.AlwaysFront, 30, 13, 2706, tilesheet),
                new Tile(TileLayer.AlwaysFront, 25, 14, 2726, tilesheet),
                new Tile(TileLayer.AlwaysFront, 26, 14, 2727, tilesheet),
                new Tile(TileLayer.AlwaysFront, 27, 14, 2728, tilesheet),
                new Tile(TileLayer.AlwaysFront, 28, 14, 2729, tilesheet),
                new Tile(TileLayer.AlwaysFront, 29, 14, 2730, tilesheet),
                new Tile(TileLayer.AlwaysFront, 30, 14, 2731, tilesheet),
                new Tile(TileLayer.Front, 25, 15, 2751, tilesheet),
                new Tile(TileLayer.Front, 26, 15, 2752, tilesheet),
                new Tile(TileLayer.Front, 27, 15, 2753, tilesheet),
                new Tile(TileLayer.Front, 28, 15, 2754, tilesheet),
                new Tile(TileLayer.Front, 29, 15, 2755, tilesheet),
                new Tile(TileLayer.Front, 30, 15, 2756, tilesheet),
                new Tile(TileLayer.Buildings, 25, 16, 2776, tilesheet),
                new Tile(TileLayer.Buildings, 26, 16, 2777, tilesheet),
                new Tile(TileLayer.Buildings, 30, 16, 2781, tilesheet)
            };
        }

        private Tile[] TegoFixes()
        {
            return new[]
            {
                new Tile(TileLayer.Front, 58, 5),
                new Tile(TileLayer.Front, 59, 5),
                new Tile(TileLayer.Front, 60, 5),
                new Tile(TileLayer.Front, 61, 5),
                new Tile(TileLayer.Front, 62, 5),
                new Tile(TileLayer.Front, 63, 5),
                new Tile(TileLayer.Front, 64, 5),
                new Tile(TileLayer.Front, 65, 5),
                new Tile(TileLayer.Front, 66, 5),
                new Tile(TileLayer.Front, 67, 5),
                new Tile(TileLayer.Front, 72, 5),
                new Tile(TileLayer.Front, 77, 5)
            };
        }

        private Tile[] RemoveShippingBinEdits()
        {
            return new[]
            {
                new Tile(TileLayer.Front, 71, 13),
                new Tile(TileLayer.Front, 72, 13),
                new Tile(TileLayer.Buildings, 71, 14),
                new Tile(TileLayer.Buildings, 72, 14)
            };
        }

        private Tile[] RemovePathEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 75, 17, 227, tilesheet),
                new Tile(TileLayer.Back, 76, 17, 227, tilesheet),
                new Tile(TileLayer.Back, 77, 17, 227, tilesheet),
                new Tile(TileLayer.Back, 78, 17, 227, tilesheet),
                new Tile(TileLayer.Back, 79, 17, 227, tilesheet)
            };
        }

        private Tile[] TelescopeEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 16, 4, 312, tilesheet),
                new Tile(TileLayer.Back, 17, 4, 313, tilesheet),
                new Tile(TileLayer.Back, 18, 4, 314, tilesheet),
                new Tile(TileLayer.Back, 16, 5, 337, tilesheet),
                new Tile(TileLayer.Back, 17, 5, 338, tilesheet),
                new Tile(TileLayer.Back, 18, 5, 339, tilesheet),
                new Tile(TileLayer.Back, 16, 6, 598, tilesheet),
                new Tile(TileLayer.Back, 17, 6, 599, tilesheet),
                new Tile(TileLayer.Back, 18, 6, 587, tilesheet),
                new Tile(TileLayer.Back, 16, 7, 587, tilesheet),
                new Tile(TileLayer.Back, 17, 7, 587, tilesheet),
                new Tile(TileLayer.Back, 18, 7, 226, tilesheet),
                new Tile(TileLayer.Back, 16, 8, 587, tilesheet),
                new Tile(TileLayer.Back, 17, 8, 587, tilesheet),
                new Tile(TileLayer.Back, 18, 7, 587, tilesheet),
                new Tile(TileLayer.Back, 15, 4, 312, tilesheet),
                new Tile(TileLayer.Back, 15, 5, 337, tilesheet),
                new Tile(TileLayer.Back, 15, 8, 565, tilesheet),
                new Tile(TileLayer.Buildings, 15, 3, 444, tilesheet),
                new Tile(TileLayer.Buildings, 16, 3),
                new Tile(TileLayer.Buildings, 17, 3),
                new Tile(TileLayer.Buildings, 18, 3),
                new Tile(TileLayer.Buildings, 15, 4, 469, tilesheet),
                new Tile(TileLayer.Buildings, 16, 4),
                new Tile(TileLayer.Buildings, 17, 4),
                new Tile(TileLayer.Buildings, 18, 4, 416, tilesheet),
                new Tile(TileLayer.Buildings, 15, 5, 494, tilesheet),
                new Tile(TileLayer.Buildings, 16, 5),
                new Tile(TileLayer.Buildings, 17, 5),
                new Tile(TileLayer.Buildings, 18, 5, 441, tilesheet),
                new Tile(TileLayer.Buildings, 15, 6, 519, tilesheet),
                new Tile(TileLayer.Buildings, 16, 6),
                new Tile(TileLayer.Buildings, 17, 6),
                new Tile(TileLayer.Buildings, 18, 6, 466, tilesheet),
                new Tile(TileLayer.Buildings, 15, 7, 540, tilesheet),
                new Tile(TileLayer.Buildings, 16, 7),
                new Tile(TileLayer.Buildings, 17, 7),
                new Tile(TileLayer.Buildings, 18, 7, 491, tilesheet),
                new Tile(TileLayer.Buildings, 15, 8),
                new Tile(TileLayer.Buildings, 18, 8, 516, tilesheet),
                new Tile(TileLayer.Buildings, 18, 9, 541, tilesheet),
                new Tile(TileLayer.Back, 32, 4, 467, tilesheet),
                new Tile(TileLayer.Back, 33, 4, 468, tilesheet),
                new Tile(TileLayer.Back, 32, 5, 492, tilesheet),
                new Tile(TileLayer.Back, 33, 5, 493, tilesheet),
                new Tile(TileLayer.Back, 32, 6, 346, tilesheet),
                new Tile(TileLayer.Back, 32, 7, 695, tilesheet),
                new Tile(TileLayer.Back, 18, 2, 277, tilesheet),
                new Tile(TileLayer.Back, 19, 2, 277, tilesheet),
                new Tile(TileLayer.Back, 20, 2, 278, tilesheet),
                new Tile(TileLayer.Back, 21, 2, 377, tilesheet),
                new Tile(TileLayer.Back, 21, 1, 352, tilesheet),
                new Tile(TileLayer.Back, 21, 0, 352, tilesheet),
                new Tile(TileLayer.Back, 19, 4, 402, tilesheet),
                new Tile(TileLayer.Back, 19, 5, 402, tilesheet),
                new Tile(TileLayer.Back, 20, 3, 200, tilesheet),
                new Tile(TileLayer.Back, 21, 3, 201, tilesheet),
                new Tile(TileLayer.Back, 22, 3, 201, tilesheet),
                new Tile(TileLayer.Back, 23, 3, 201, tilesheet),
                new Tile(TileLayer.Back, 24, 3, 203, tilesheet),
                new Tile(TileLayer.Back, 20, 4, 225, tilesheet),
                new Tile(TileLayer.Back, 21, 4, 226, tilesheet),
                new Tile(TileLayer.Back, 22, 4, 227, tilesheet),
                new Tile(TileLayer.Back, 23, 4, 488, tilesheet),
                new Tile(TileLayer.Back, 24, 4, 228, tilesheet),
                new Tile(TileLayer.Back, 20, 5, 250, tilesheet),
                new Tile(TileLayer.Back, 21, 5, 251, tilesheet),
                new Tile(TileLayer.Back, 22, 5, 251, tilesheet),
                new Tile(TileLayer.Back, 23, 5, 251, tilesheet),
                new Tile(TileLayer.Back, 24, 5, 253, tilesheet),
                new Tile(TileLayer.Back, 22, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 23, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 24, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 25, 0, 151, tilesheet),
                new Tile(TileLayer.Back, 26, 0, 150, tilesheet),
                new Tile(TileLayer.Back, 22, 1, 175, tilesheet),
                new Tile(TileLayer.Back, 23, 1, 175, tilesheet),
                new Tile(TileLayer.Back, 24, 1, 175, tilesheet),
                new Tile(TileLayer.Back, 25, 1, 175, tilesheet),
                new Tile(TileLayer.Back, 26, 1, 175, tilesheet),
                new Tile(TileLayer.Back, 22, 2, 175, tilesheet),
                new Tile(TileLayer.Back, 23, 2, 175, tilesheet),
                new Tile(TileLayer.Back, 24, 2, 151, tilesheet),
                new Tile(TileLayer.Back, 25, 2, 175, tilesheet),
                new Tile(TileLayer.Back, 26, 2, 175, tilesheet),
                new Tile(TileLayer.Back, 27, 1, 200, tilesheet),
                new Tile(TileLayer.Back, 28, 1, 201, tilesheet),
                new Tile(TileLayer.Back, 29, 1, 201, tilesheet),
                new Tile(TileLayer.Back, 30, 1, 201, tilesheet),
                new Tile(TileLayer.Back, 31, 1, 201, tilesheet),
                new Tile(TileLayer.Back, 32, 1, 203, tilesheet),
                new Tile(TileLayer.Back, 27, 2, 225, tilesheet),
                new Tile(TileLayer.Back, 28, 2, 1125, tilesheet),
                new Tile(TileLayer.Back, 29, 2, 1126, tilesheet),
                new Tile(TileLayer.Back, 30, 2, 1127, tilesheet),
                new Tile(TileLayer.Back, 31, 2, 1128, tilesheet),
                new Tile(TileLayer.Back, 32, 2, 228, tilesheet),
                new Tile(TileLayer.Back, 27, 3, 225, tilesheet),
                new Tile(TileLayer.Back, 28, 3, 1150, tilesheet),
                new Tile(TileLayer.Back, 29, 3, 1151, tilesheet),
                new Tile(TileLayer.Back, 30, 3, 1152, tilesheet),
                new Tile(TileLayer.Back, 31, 3, 1153, tilesheet),
                new Tile(TileLayer.Back, 32, 3, 228, tilesheet),
                new Tile(TileLayer.Back, 27, 4, 225, tilesheet),
                new Tile(TileLayer.Back, 28, 4, 1175, tilesheet),
                new Tile(TileLayer.Back, 29, 4, 1176, tilesheet),
                new Tile(TileLayer.Back, 30, 4, 1177, tilesheet),
                new Tile(TileLayer.Back, 31, 4, 1178, tilesheet),
                new Tile(TileLayer.Back, 27, 5, 250, tilesheet),
                new Tile(TileLayer.Back, 28, 5, 251, tilesheet),
                new Tile(TileLayer.Back, 29, 5, 251, tilesheet),
                new Tile(TileLayer.Back, 30, 5, 251, tilesheet),
                new Tile(TileLayer.Back, 31, 5, 230, tilesheet),
                new Tile(TileLayer.Back, 27, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 28, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 29, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 30, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 31, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 32, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 33, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 34, 0, 175, tilesheet),
                new Tile(TileLayer.Back, 25, 4, 175, tilesheet),
                new Tile(TileLayer.Back, 26, 4, 175, tilesheet),
                new Tile(TileLayer.Back, 25, 5, 175, tilesheet),
                new Tile(TileLayer.Back, 26, 5, 175, tilesheet),
                new Tile(TileLayer.Buildings, 32, 4, 419, tilesheet),
                new Tile(TileLayer.Buildings, 32, 5, 444, tilesheet),
                new Tile(TileLayer.Buildings, 19, 6, 467, tilesheet),
                new Tile(TileLayer.Buildings, 20, 6, 467, tilesheet),
                new Tile(TileLayer.Buildings, 21, 6, 468, tilesheet),
                new Tile(TileLayer.Buildings, 22, 6, 467, tilesheet),
                new Tile(TileLayer.Buildings, 23, 6, 468, tilesheet),
                new Tile(TileLayer.Buildings, 24, 6, 467, tilesheet),
                new Tile(TileLayer.Buildings, 25, 6, 468, tilesheet),
                new Tile(TileLayer.Buildings, 26, 6, 446, tilesheet),
                new Tile(TileLayer.Buildings, 27, 6, 468, tilesheet),
                new Tile(TileLayer.Buildings, 28, 6, 467, tilesheet),
                new Tile(TileLayer.Buildings, 29, 6, 468, tilesheet),
                new Tile(TileLayer.Buildings, 30, 6, 467, tilesheet),
                new Tile(TileLayer.Buildings, 31, 6, 468, tilesheet),
                new Tile(TileLayer.Buildings, 32, 6, 469, tilesheet),
                new Tile(TileLayer.Buildings, 19, 7, 492, tilesheet),
                new Tile(TileLayer.Buildings, 20, 7, 493, tilesheet),
                new Tile(TileLayer.Buildings, 21, 7, 493, tilesheet),
                new Tile(TileLayer.Buildings, 22, 7, 492, tilesheet),
                new Tile(TileLayer.Buildings, 23, 7, 493, tilesheet),
                new Tile(TileLayer.Buildings, 24, 7, 492, tilesheet),
                new Tile(TileLayer.Buildings, 25, 7, 493, tilesheet),
                new Tile(TileLayer.Buildings, 26, 7, 492, tilesheet),
                new Tile(TileLayer.Buildings, 27, 7, 493, tilesheet),
                new Tile(TileLayer.Buildings, 28, 7, 492, tilesheet),
                new Tile(TileLayer.Buildings, 29, 7, 493, tilesheet),
                new Tile(TileLayer.Buildings, 30, 7, 492, tilesheet),
                new Tile(TileLayer.Buildings, 31, 7, 493, tilesheet),
                new Tile(TileLayer.Buildings, 32, 7, 494, tilesheet),
                new Tile(TileLayer.Buildings, 19, 8, 517, tilesheet),
                new Tile(TileLayer.Buildings, 20, 8, 518, tilesheet),
                new Tile(TileLayer.Buildings, 21, 8, 518, tilesheet),
                new Tile(TileLayer.Buildings, 22, 8, 517, tilesheet),
                new Tile(TileLayer.Buildings, 23, 8, 518, tilesheet),
                new Tile(TileLayer.Buildings, 24, 8, 517, tilesheet),
                new Tile(TileLayer.Buildings, 25, 8, 518, tilesheet),
                new Tile(TileLayer.Buildings, 26, 8, 517, tilesheet),
                new Tile(TileLayer.Buildings, 27, 8, 518, tilesheet),
                new Tile(TileLayer.Buildings, 28, 8, 448, tilesheet),
                new Tile(TileLayer.Buildings, 29, 8, 518, tilesheet),
                new Tile(TileLayer.Buildings, 30, 8, 517, tilesheet),
                new Tile(TileLayer.Buildings, 31, 8, 518, tilesheet),
                new Tile(TileLayer.Buildings, 32, 8, 519, tilesheet),
                new Tile(TileLayer.Buildings, 19, 9, 542, tilesheet),
                new Tile(TileLayer.Buildings, 20, 9, 543, tilesheet),
                new Tile(TileLayer.Buildings, 21, 9, 542, tilesheet),
                new Tile(TileLayer.Buildings, 22, 9, 543, tilesheet),
                new Tile(TileLayer.Buildings, 23, 9, 542, tilesheet),
                new Tile(TileLayer.Buildings, 24, 9, 543, tilesheet),
                new Tile(TileLayer.Buildings, 25, 9, 542, tilesheet),
                new Tile(TileLayer.Buildings, 26, 9, 543, tilesheet),
                new Tile(TileLayer.Buildings, 27, 9, 542, tilesheet),
                new Tile(TileLayer.Buildings, 28, 9, 543, tilesheet),
                new Tile(TileLayer.Buildings, 29, 9, 542, tilesheet),
                new Tile(TileLayer.Buildings, 30, 9, 542, tilesheet),
                new Tile(TileLayer.Buildings, 31, 9, 543, tilesheet),
                new Tile(TileLayer.Buildings, 32, 9, 544, tilesheet),
                new Tile(TileLayer.Buildings, 17, 0),
                new Tile(TileLayer.Buildings, 18, 0),
                new Tile(TileLayer.Buildings, 19, 0),
                new Tile(TileLayer.Buildings, 17, 1),
                new Tile(TileLayer.Buildings, 18, 1),
                new Tile(TileLayer.Buildings, 19, 1),
                new Tile(TileLayer.Buildings, 18, 2),
                new Tile(TileLayer.Buildings, 20, 0),
                new Tile(TileLayer.Buildings, 21, 0),
                new Tile(TileLayer.Buildings, 20, 1),
                new Tile(TileLayer.Buildings, 21, 1),
                new Tile(TileLayer.Buildings, 20, 2),
                new Tile(TileLayer.Buildings, 21, 2),
                new Tile(TileLayer.Buildings, 17, 2),
                new Tile(TileLayer.Buildings, 19, 2),
                new Tile(TileLayer.Buildings, 19, 3),
                new Tile(TileLayer.Buildings, 22, 0),
                new Tile(TileLayer.Buildings, 22, 1),
                new Tile(TileLayer.Buildings, 22, 2),
                new Tile(TileLayer.Buildings, 23, 1),
                new Tile(TileLayer.Buildings, 23, 2),
                new Tile(TileLayer.Buildings, 25, 1),
                new Tile(TileLayer.Buildings, 25, 2),
                new Tile(TileLayer.Buildings, 26, 1),
                new Tile(TileLayer.Buildings, 30, 3),
                new Tile(TileLayer.Buildings, 32, 2),
                new Tile(TileLayer.Buildings, 32, 3),
                new Tile(TileLayer.Buildings, 33, 0),
                new Tile(TileLayer.Buildings, 33, 1),
                new Tile(TileLayer.Buildings, 19, 4),
                new Tile(TileLayer.Buildings, 25, 4),
                new Tile(TileLayer.Buildings, 26, 4),
                new Tile(TileLayer.Buildings, 27, 4),
                new Tile(TileLayer.Buildings, 28, 4),
                new Tile(TileLayer.Buildings, 29, 4),
                new Tile(TileLayer.Buildings, 30, 4),
                new Tile(TileLayer.Buildings, 31, 4),
                new Tile(TileLayer.Buildings, 19, 5),
                new Tile(TileLayer.Buildings, 20, 5),
                new Tile(TileLayer.Buildings, 21, 5),
                new Tile(TileLayer.Buildings, 22, 5),
                new Tile(TileLayer.Buildings, 23, 5),
                new Tile(TileLayer.Buildings, 24, 5),
                new Tile(TileLayer.Buildings, 25, 5),
                new Tile(TileLayer.Buildings, 26, 5),
                new Tile(TileLayer.Buildings, 27, 5),
                new Tile(TileLayer.Buildings, 28, 5),
                new Tile(TileLayer.Buildings, 29, 5),
                new Tile(TileLayer.Buildings, 30, 5),
                new Tile(TileLayer.Buildings, 31, 5),
                new Tile(TileLayer.Buildings, 23, 0),
                new Tile(TileLayer.Buildings, 24, 0),
                new Tile(TileLayer.Buildings, 25, 0),
                new Tile(TileLayer.Buildings, 24, 1),
                new Tile(TileLayer.Buildings, 24, 2),
                new Tile(TileLayer.Buildings, 27, 0),
                new Tile(TileLayer.Buildings, 28, 0),
                new Tile(TileLayer.Front, 29, 0),
                new Tile(TileLayer.Buildings, 28, 1),
                new Tile(TileLayer.Buildings, 28, 2),
                new Tile(TileLayer.Buildings, 29, 0),
                new Tile(TileLayer.Buildings, 30, 0),
                new Tile(TileLayer.Buildings, 29, 1),
                new Tile(TileLayer.Buildings, 30, 1),
                new Tile(TileLayer.Front, 30, 0),
                new Tile(TileLayer.Front, 30, 1),
                new Tile(TileLayer.Buildings, 31, 0),
                new Tile(TileLayer.Buildings, 32, 0),
                new Tile(TileLayer.Buildings, 31, 1),
                new Tile(TileLayer.Buildings, 32, 1),
                new Tile(TileLayer.Buildings, 31, 2),
                new Tile(TileLayer.Buildings, 31, 3),
                new Tile(TileLayer.Buildings, 21, 3, 2594, tilesheet),
                new Tile(TileLayer.Buildings, 22, 3, 2595, tilesheet),
                new Tile(TileLayer.Buildings, 23, 3, 2596, tilesheet),
                new Tile(TileLayer.Buildings, 20, 4, 2618, tilesheet),
                new Tile(TileLayer.Buildings, 21, 4, 2619, tilesheet),
                new Tile(TileLayer.Buildings, 22, 4, 2620, tilesheet),
                new Tile(TileLayer.Buildings, 23, 4, 2621, tilesheet),
                new Tile(TileLayer.Buildings, 24, 4, 2622, tilesheet),
                new Tile(TileLayer.Front, 20, 0, 2518, tilesheet),
                new Tile(TileLayer.Front, 21, 0, 2519, tilesheet),
                new Tile(TileLayer.Front, 22, 0, 2520, tilesheet),
                new Tile(TileLayer.Front, 23, 0, 2521, tilesheet),
                new Tile(TileLayer.Front, 24, 0, 2522, tilesheet),
                new Tile(TileLayer.Front, 20, 1, 2543, tilesheet),
                new Tile(TileLayer.Front, 21, 1, 2544, tilesheet),
                new Tile(TileLayer.Front, 22, 1, 2545, tilesheet),
                new Tile(TileLayer.Front, 23, 1, 2546, tilesheet),
                new Tile(TileLayer.Front, 24, 1, 2547, tilesheet),
                new Tile(TileLayer.Front, 20, 2, 2568, tilesheet),
                new Tile(TileLayer.Front, 21, 2, 2569, tilesheet),
                new Tile(TileLayer.Front, 22, 2, 2570, tilesheet),
                new Tile(TileLayer.Front, 23, 2, 2571, tilesheet),
                new Tile(TileLayer.Front, 24, 2, 2572, tilesheet),
                new Tile(TileLayer.Front, 20, 3, 2593, tilesheet),
                new Tile(TileLayer.Front, 24, 3, 2597, tilesheet),
                new Tile(TileLayer.Front, 30, 1, 2266, tilesheet),
                new Tile(TileLayer.Buildings, 30, 2, 2291, tilesheet),
                new Tile(TileLayer.Buildings, 30, 5, 117, tilesheet),
                new Tile(TileLayer.Buildings, 31, 5, 113, tilesheet),
                new Tile(TileLayer.Front, 31, 4, 88, tilesheet),
                new Tile(TileLayer.Front, 32, 4, 89, tilesheet),
                new Tile(TileLayer.Front, 32, 5, 114, tilesheet),
                new Tile(TileLayer.Buildings, 33, 3, 21, tilesheet),
                new Tile(TileLayer.Buildings, 17, 0, 307, tilesheet),
                new Tile(TileLayer.Buildings, 18, 0, 257, tilesheet),
                new Tile(TileLayer.Buildings, 19, 0, 21, tilesheet),
                new Tile(TileLayer.Buildings, 20, 0, 126, tilesheet),
                new Tile(TileLayer.Buildings, 21, 0, 21, tilesheet),
                new Tile(TileLayer.Buildings, 22, 0, 21, tilesheet),
                new Tile(TileLayer.Buildings, 23, 0, 21, tilesheet),
                new Tile(TileLayer.Buildings, 24, 0, 307, tilesheet),
                new Tile(TileLayer.Buildings, 25, 0, 21, tilesheet),
                new Tile(TileLayer.Buildings, 27, 0, 21, tilesheet),
                new Tile(TileLayer.Buildings, 28, 0, 21, tilesheet),
                new Tile(TileLayer.Buildings, 29, 0, 63, tilesheet),
                new Tile(TileLayer.Buildings, 30, 0, 64, tilesheet),
                new Tile(TileLayer.Buildings, 31, 0, 65, tilesheet),
                new Tile(TileLayer.Buildings, 32, 0, 257, tilesheet),
                new Tile(TileLayer.Buildings, 33, 1, 21, tilesheet),
                new Tile(TileLayer.Buildings, 33, 2, 21, tilesheet),
                new Tile(TileLayer.Buildings, 12, 2, 21, tilesheet)
            };
        }

        private Tile[] MemorialArea()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 3, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 4, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 5, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 6, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 7, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 9, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 10, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 11, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 12, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 13, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 14, 7, 2283, tilesheet),
                new Tile(TileLayer.Back, 3, 8, 2300, tilesheet),
                new Tile(TileLayer.Back, 4, 8, 2301, tilesheet),
                new Tile(TileLayer.Back, 5, 8, 2302, tilesheet),
                new Tile(TileLayer.Back, 6, 8, 2283, tilesheet),
                new Tile(TileLayer.Back, 10, 8, 2400, tilesheet),
                new Tile(TileLayer.Back, 11, 8, 2401, tilesheet),
                new Tile(TileLayer.Back, 12, 8, 2401, tilesheet),
                new Tile(TileLayer.Back, 13, 8, 2401, tilesheet),
                new Tile(TileLayer.Back, 14, 8, 2280, tilesheet),
                new Tile(TileLayer.Back, 15, 8, 838, tilesheet),
                new Tile(TileLayer.Back, 3, 9, 2331, tilesheet),
                new Tile(TileLayer.Back, 4, 9, 153, tilesheet),
                new Tile(TileLayer.Back, 5, 9, 2327, tilesheet),
                new Tile(TileLayer.Back, 6, 9, 2283, tilesheet),
                new Tile(TileLayer.Back, 7, 9, 2283, tilesheet),
                new Tile(TileLayer.Back, 8, 9, 2283, tilesheet),
                new Tile(TileLayer.Back, 9, 9, 2283, tilesheet),
                new Tile(TileLayer.Back, 10, 9, 2303, tilesheet),
                new Tile(TileLayer.Back, 11, 9, 226, tilesheet),
                new Tile(TileLayer.Back, 12, 9, 227, tilesheet),
                new Tile(TileLayer.Back, 13, 9, 227, tilesheet),
                new Tile(TileLayer.Back, 14, 9, 2305, tilesheet),
                new Tile(TileLayer.Back, 15, 9, 812, tilesheet),
                new Tile(TileLayer.Back, 19, 9, 2283, tilesheet),
                new Tile(TileLayer.Back, 20, 9, 2283, tilesheet),
                new Tile(TileLayer.Back, 21, 9, 2283, tilesheet),
                new Tile(TileLayer.Back, 22, 9, 2283, tilesheet),
                new Tile(TileLayer.Back, 3, 10, 2350, tilesheet),
                new Tile(TileLayer.Back, 4, 10, 2351, tilesheet),
                new Tile(TileLayer.Back, 5, 10, 2352, tilesheet),
                new Tile(TileLayer.Back, 6, 10, 2283, tilesheet),
                new Tile(TileLayer.Back, 7, 10, 2283, tilesheet),
                new Tile(TileLayer.Back, 8, 10, 2283, tilesheet),
                new Tile(TileLayer.Back, 9, 10, 2283, tilesheet),
                new Tile(TileLayer.Back, 10, 10, 2328, tilesheet),
                new Tile(TileLayer.Back, 11, 10, 226, tilesheet),
                new Tile(TileLayer.Back, 12, 10, 227, tilesheet),
                new Tile(TileLayer.Back, 13, 10, 227, tilesheet),
                new Tile(TileLayer.Back, 14, 10, 2330, tilesheet),
                new Tile(TileLayer.Back, 19, 10, 2300, tilesheet),
                new Tile(TileLayer.Back, 20, 10, 2301, tilesheet),
                new Tile(TileLayer.Back, 21, 10, 2301, tilesheet),
                new Tile(TileLayer.Back, 22, 10, 2302, tilesheet),
                new Tile(TileLayer.Back, 3, 11, 2300, tilesheet),
                new Tile(TileLayer.Back, 4, 11, 2301, tilesheet),
                new Tile(TileLayer.Back, 5, 11, 2302, tilesheet),
                new Tile(TileLayer.Back, 6, 11, 2283, tilesheet),
                new Tile(TileLayer.Back, 7, 11, 2283, tilesheet),
                new Tile(TileLayer.Back, 8, 11, 2283, tilesheet),
                new Tile(TileLayer.Back, 9, 11, 2283, tilesheet),
                new Tile(TileLayer.Back, 10, 11, 2353, tilesheet),
                new Tile(TileLayer.Back, 11, 11, 226, tilesheet),
                new Tile(TileLayer.Back, 12, 11, 227, tilesheet),
                new Tile(TileLayer.Back, 13, 11, 227, tilesheet),
                new Tile(TileLayer.Back, 14, 11, 2355, tilesheet),
                new Tile(TileLayer.Back, 19, 11, 2303, tilesheet),
                new Tile(TileLayer.Back, 20, 11, 226, tilesheet),
                new Tile(TileLayer.Back, 21, 11, 227, tilesheet),
                new Tile(TileLayer.Back, 22, 11, 2305, tilesheet),
                new Tile(TileLayer.Back, 3, 12, 2475, tilesheet),
                new Tile(TileLayer.Back, 4, 12, 227, tilesheet),
                new Tile(TileLayer.Back, 5, 12, 2327, tilesheet),
                new Tile(TileLayer.Back, 6, 12, 2283, tilesheet),
                new Tile(TileLayer.Back, 7, 12, 2283, tilesheet),
                new Tile(TileLayer.Back, 8, 12, 2283, tilesheet),
                new Tile(TileLayer.Back, 9, 12, 2283, tilesheet),
                new Tile(TileLayer.Back, 10, 12, 2378, tilesheet),
                new Tile(TileLayer.Back, 11, 12, 2379, tilesheet),
                new Tile(TileLayer.Back, 12, 12, 2379, tilesheet),
                new Tile(TileLayer.Back, 13, 12, 2379, tilesheet),
                new Tile(TileLayer.Back, 14, 12, 2380, tilesheet),
                new Tile(TileLayer.Back, 19, 12, 2328, tilesheet),
                new Tile(TileLayer.Back, 20, 12, 226, tilesheet),
                new Tile(TileLayer.Back, 21, 12, 227, tilesheet),
                new Tile(TileLayer.Back, 22, 12, 2330, tilesheet),
                new Tile(TileLayer.Back, 3, 13, 2350, tilesheet),
                new Tile(TileLayer.Back, 4, 13, 2351, tilesheet),
                new Tile(TileLayer.Back, 5, 13, 2352, tilesheet),
                new Tile(TileLayer.Back, 6, 13, 2283, tilesheet),
                new Tile(TileLayer.Back, 7, 13, 2283, tilesheet),
                new Tile(TileLayer.Back, 8, 13, 2283, tilesheet),
                new Tile(TileLayer.Back, 9, 13, 2283, tilesheet),
                new Tile(TileLayer.Back, 10, 13, 2278, tilesheet),
                new Tile(TileLayer.Back, 11, 13, 2301, tilesheet),
                new Tile(TileLayer.Back, 12, 13, 2301, tilesheet),
                new Tile(TileLayer.Back, 13, 13, 2301, tilesheet),
                new Tile(TileLayer.Back, 14, 13, 2302, tilesheet),
                new Tile(TileLayer.Back, 19, 13, 2328, tilesheet),
                new Tile(TileLayer.Back, 20, 13, 226, tilesheet),
                new Tile(TileLayer.Back, 21, 13, 227, tilesheet),
                new Tile(TileLayer.Back, 22, 13, 2330, tilesheet),
                new Tile(TileLayer.Back, 3, 14, 2300, tilesheet),
                new Tile(TileLayer.Back, 4, 14, 2301, tilesheet),
                new Tile(TileLayer.Back, 5, 14, 2302, tilesheet),
                new Tile(TileLayer.Back, 6, 14, 2283, tilesheet),
                new Tile(TileLayer.Back, 7, 14, 2283, tilesheet),
                new Tile(TileLayer.Back, 8, 14, 2283, tilesheet),
                new Tile(TileLayer.Back, 9, 14, 2283, tilesheet),
                new Tile(TileLayer.Back, 10, 14, 2353, tilesheet),
                new Tile(TileLayer.Back, 11, 14, 226, tilesheet),
                new Tile(TileLayer.Back, 12, 14, 227, tilesheet),
                new Tile(TileLayer.Back, 13, 14, 227, tilesheet),
                new Tile(TileLayer.Back, 14, 14, 2355, tilesheet),
                new Tile(TileLayer.Back, 19, 14, 2328, tilesheet),
                new Tile(TileLayer.Back, 20, 14, 226, tilesheet),
                new Tile(TileLayer.Back, 21, 14, 227, tilesheet),
                new Tile(TileLayer.Back, 22, 14, 2330, tilesheet),
                new Tile(TileLayer.Back, 3, 15, 2475, tilesheet),
                new Tile(TileLayer.Back, 4, 15, 227, tilesheet),
                new Tile(TileLayer.Back, 5, 15, 2327, tilesheet),
                new Tile(TileLayer.Back, 6, 15, 2283, tilesheet),
                new Tile(TileLayer.Back, 7, 15, 2283, tilesheet),
                new Tile(TileLayer.Back, 8, 15, 2283, tilesheet),
                new Tile(TileLayer.Back, 9, 15, 2283, tilesheet),
                new Tile(TileLayer.Back, 10, 15, 2353, tilesheet),
                new Tile(TileLayer.Back, 11, 15, 226, tilesheet),
                new Tile(TileLayer.Back, 12, 15, 227, tilesheet),
                new Tile(TileLayer.Back, 13, 15, 227, tilesheet),
                new Tile(TileLayer.Back, 14, 15, 2355, tilesheet),
                new Tile(TileLayer.Back, 19, 15, 2328, tilesheet),
                new Tile(TileLayer.Back, 20, 15, 226, tilesheet),
                new Tile(TileLayer.Back, 21, 15, 227, tilesheet),
                new Tile(TileLayer.Back, 22, 15, 2330, tilesheet),
                new Tile(TileLayer.Back, 3, 16, 2350, tilesheet),
                new Tile(TileLayer.Back, 4, 16, 2351, tilesheet),
                new Tile(TileLayer.Back, 5, 16, 2352, tilesheet),
                new Tile(TileLayer.Back, 6, 16, 2283, tilesheet),
                new Tile(TileLayer.Back, 7, 16, 2283, tilesheet),
                new Tile(TileLayer.Back, 8, 16, 2283, tilesheet),
                new Tile(TileLayer.Back, 9, 16, 2283, tilesheet),
                new Tile(TileLayer.Back, 10, 16, 2378, tilesheet),
                new Tile(TileLayer.Back, 11, 16, 2379, tilesheet),
                new Tile(TileLayer.Back, 12, 16, 2379, tilesheet),
                new Tile(TileLayer.Back, 13, 16, 2379, tilesheet),
                new Tile(TileLayer.Back, 14, 16, 2380, tilesheet),
                new Tile(TileLayer.Back, 19, 16, 2475, tilesheet),
                new Tile(TileLayer.Back, 20, 16, 226, tilesheet),
                new Tile(TileLayer.Back, 21, 16, 227, tilesheet),
                new Tile(TileLayer.Back, 22, 16, 2330, tilesheet),
                new Tile(TileLayer.Back, 3, 17, 2300, tilesheet),
                new Tile(TileLayer.Back, 4, 17, 2301, tilesheet),
                new Tile(TileLayer.Back, 5, 17, 2302, tilesheet),
                new Tile(TileLayer.Back, 6, 17, 2300, tilesheet),
                new Tile(TileLayer.Back, 7, 17, 2301, tilesheet),
                new Tile(TileLayer.Back, 8, 17, 2302, tilesheet),
                new Tile(TileLayer.Back, 9, 17, 2300, tilesheet),
                new Tile(TileLayer.Back, 10, 17, 2301, tilesheet),
                new Tile(TileLayer.Back, 11, 17, 2302, tilesheet),
                new Tile(TileLayer.Back, 12, 17, 2300, tilesheet),
                new Tile(TileLayer.Back, 13, 17, 2301, tilesheet),
                new Tile(TileLayer.Back, 14, 17, 2302, tilesheet),
                new Tile(TileLayer.Back, 19, 17, 2350, tilesheet),
                new Tile(TileLayer.Back, 20, 17, 2351, tilesheet),
                new Tile(TileLayer.Back, 21, 17, 2351, tilesheet),
                new Tile(TileLayer.Back, 22, 17, 2352, tilesheet),
                new Tile(TileLayer.Back, 3, 18, 2325, tilesheet),
                new Tile(TileLayer.Back, 4, 18, 227, tilesheet),
                new Tile(TileLayer.Back, 5, 18, 2327, tilesheet),
                new Tile(TileLayer.Back, 6, 18, 2325, tilesheet),
                new Tile(TileLayer.Back, 7, 18, 227, tilesheet),
                new Tile(TileLayer.Back, 8, 18, 2327, tilesheet),
                new Tile(TileLayer.Back, 9, 18, 2325, tilesheet),
                new Tile(TileLayer.Back, 10, 18, 227, tilesheet),
                new Tile(TileLayer.Back, 11, 18, 2327, tilesheet),
                new Tile(TileLayer.Back, 12, 18, 2325, tilesheet),
                new Tile(TileLayer.Back, 13, 18, 227, tilesheet),
                new Tile(TileLayer.Back, 14, 18, 2327, tilesheet),
                new Tile(TileLayer.Back, 19, 18, 2283, tilesheet),
                new Tile(TileLayer.Back, 20, 18, 2283, tilesheet),
                new Tile(TileLayer.Back, 21, 18, 2283, tilesheet),
                new Tile(TileLayer.Back, 22, 18, 2283, tilesheet),
                new Tile(TileLayer.Back, 3, 19, 2350, tilesheet),
                new Tile(TileLayer.Back, 4, 19, 2351, tilesheet),
                new Tile(TileLayer.Back, 5, 19, 2352, tilesheet),
                new Tile(TileLayer.Back, 6, 19, 2350, tilesheet),
                new Tile(TileLayer.Back, 7, 19, 2351, tilesheet),
                new Tile(TileLayer.Back, 8, 19, 2352, tilesheet),
                new Tile(TileLayer.Back, 9, 19, 2350, tilesheet),
                new Tile(TileLayer.Back, 10, 19, 2351, tilesheet),
                new Tile(TileLayer.Back, 11, 19, 2352, tilesheet),
                new Tile(TileLayer.Back, 12, 19, 2350, tilesheet),
                new Tile(TileLayer.Back, 13, 19, 2351, tilesheet),
                new Tile(TileLayer.Back, 14, 19, 2352, tilesheet),
                new Tile(TileLayer.Back, 19, 19, 2283, tilesheet),
                new Tile(TileLayer.Back, 20, 19, 2283, tilesheet),
                new Tile(TileLayer.Back, 21, 19, 2283, tilesheet),
                new Tile(TileLayer.Back, 22, 19, 2283, tilesheet),
                new Tile(TileLayer.Back, 3, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 4, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 5, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 6, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 7, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 8, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 9, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 10, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 11, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 12, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 13, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 14, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 15, 20, 838, tilesheet),
                new Tile(TileLayer.Back, 18, 20, 838, tilesheet),
                new Tile(TileLayer.Back, 19, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 20, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 21, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 22, 20, 2715, tilesheet),
                new Tile(TileLayer.Back, 23, 20, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 3, 19, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 4, 19, 2495, tilesheet),
                new Tile(TileLayer.Buildings, 5, 19, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 6, 19, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 7, 19, 2495, tilesheet),
                new Tile(TileLayer.Buildings, 8, 19, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 9, 19, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 10, 19, 2495, tilesheet),
                new Tile(TileLayer.Buildings, 11, 19, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 12, 19, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 13, 19, 2495, tilesheet),
                new Tile(TileLayer.Buildings, 14, 19, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 15, 19, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 18, 19, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 19, 19, 2495, tilesheet),
                new Tile(TileLayer.Buildings, 20, 19, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 21, 19, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 22, 19, 2495, tilesheet),
                new Tile(TileLayer.Buildings, 23, 19, 2496, tilesheet),
                new Tile(TileLayer.Front, 3, 18, 2419, tilesheet),
                new Tile(TileLayer.Front, 4, 18, 2420, tilesheet),
                new Tile(TileLayer.Front, 5, 18, 2421, tilesheet),
                new Tile(TileLayer.Front, 6, 18, 2419, tilesheet),
                new Tile(TileLayer.Front, 7, 18, 2420, tilesheet),
                new Tile(TileLayer.Front, 8, 18, 2421, tilesheet),
                new Tile(TileLayer.Front, 9, 18, 2419, tilesheet),
                new Tile(TileLayer.Front, 10, 18, 2420, tilesheet),
                new Tile(TileLayer.Front, 11, 18, 2421, tilesheet),
                new Tile(TileLayer.Front, 12, 18, 2419, tilesheet),
                new Tile(TileLayer.Front, 13, 18, 2420, tilesheet),
                new Tile(TileLayer.Front, 14, 18, 2421, tilesheet),
                new Tile(TileLayer.Front, 15, 18, 2421, tilesheet),
                new Tile(TileLayer.Front, 18, 18, 2419, tilesheet),
                new Tile(TileLayer.Front, 19, 18, 2420, tilesheet),
                new Tile(TileLayer.Front, 20, 18, 2421, tilesheet),
                new Tile(TileLayer.Front, 21, 18, 2419, tilesheet),
                new Tile(TileLayer.Front, 22, 18, 2420, tilesheet),
                new Tile(TileLayer.Front, 23, 18, 2421, tilesheet),
                new Tile(TileLayer.Front, 15, 7, 2419, tilesheet),
                new Tile(TileLayer.Buildings, 15, 8, 2444, tilesheet),
                new Tile(TileLayer.Buildings, 15, 9, 2469, tilesheet),
                new Tile(TileLayer.Buildings, 15, 10, 2419, tilesheet),
                new Tile(TileLayer.Buildings, 15, 11, 2444, tilesheet),
                new Tile(TileLayer.Buildings, 15, 11, 2469, tilesheet),
                new Tile(TileLayer.Buildings, 15, 12, 2494, tilesheet),
                new Tile(TileLayer.Back, 15, 7, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 8, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 9, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 10, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 11, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 11, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 12, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 13, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 14, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 15, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 16, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 17, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 18, 812, tilesheet),
                new Tile(TileLayer.Back, 15, 19, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 9, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 10, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 11, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 11, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 12, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 13, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 14, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 15, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 16, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 17, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 18, 812, tilesheet),
                new Tile(TileLayer.Back, 18, 19, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 9, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 10, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 11, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 11, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 12, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 13, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 14, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 15, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 16, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 17, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 18, 812, tilesheet),
                new Tile(TileLayer.Back, 23, 19, 812, tilesheet),
                new Tile(TileLayer.Front, 18, 9, 2421, tilesheet),
                new Tile(TileLayer.Buildings, 18, 10, 2446, tilesheet),
                new Tile(TileLayer.Buildings, 18, 11, 2471, tilesheet),
                new Tile(TileLayer.Buildings, 18, 12, 2471, tilesheet),
                new Tile(TileLayer.Buildings, 18, 13, 2496, tilesheet),
                new Tile(TileLayer.Front, 23, 9, 2421, tilesheet),
                new Tile(TileLayer.Buildings, 23, 10, 2446, tilesheet),
                new Tile(TileLayer.Buildings, 23, 11, 2471, tilesheet),
                new Tile(TileLayer.Buildings, 23, 12, 2421, tilesheet),
                new Tile(TileLayer.Buildings, 23, 13, 2446, tilesheet),
                new Tile(TileLayer.Buildings, 23, 14, 2471, tilesheet),
                new Tile(TileLayer.Front, 23, 15, 2421, tilesheet),
                new Tile(TileLayer.Buildings, 23, 16, 2446, tilesheet),
                new Tile(TileLayer.Buildings, 23, 17, 2471, tilesheet)
            };
        }

        private Tile[] MemorialAreaArch()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.AlwaysFront, 15, 16, 2388, tilesheet),
                new Tile(TileLayer.AlwaysFront, 16, 16, 2389, tilesheet),
                new Tile(TileLayer.AlwaysFront, 17, 16, 2390, tilesheet),
                new Tile(TileLayer.AlwaysFront, 18, 16, 2391, tilesheet),
                new Tile(TileLayer.AlwaysFront, 15, 17, 2413, tilesheet),
                new Tile(TileLayer.AlwaysFront, 16, 17, 2414, tilesheet),
                new Tile(TileLayer.AlwaysFront, 17, 17, 2415, tilesheet),
                new Tile(TileLayer.AlwaysFront, 18, 17, 2416, tilesheet),
                new Tile(TileLayer.AlwaysFront, 15, 18, 2438, tilesheet),
                new Tile(TileLayer.AlwaysFront, 16, 18, 2439, tilesheet),
                new Tile(TileLayer.AlwaysFront, 17, 18, 2440, tilesheet),
                new Tile(TileLayer.AlwaysFront, 18, 18, 2441, tilesheet),
                new Tile(TileLayer.Front, 15, 19, 2463, tilesheet),
                new Tile(TileLayer.Front, 18, 19, 2466, tilesheet),
                new Tile(TileLayer.Buildings, 15, 20, 2488, tilesheet),
                new Tile(TileLayer.Buildings, 18, 20, 2491, tilesheet)
            };
        }

        private Tile[] PicnicBlanketEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 33, 45, 2992, tilesheet),
                new Tile(TileLayer.Back, 34, 45, 2993, tilesheet),
                new Tile(TileLayer.Back, 35, 45, 2994, tilesheet),
                new Tile(TileLayer.Back, 36, 45, 2995, tilesheet),
                new Tile(TileLayer.Back, 37, 45, 2996, tilesheet),
                new Tile(TileLayer.Back, 38, 45, 2997, tilesheet),
                new Tile(TileLayer.Back, 33, 46, 3017, tilesheet),
                new Tile(TileLayer.Back, 34, 46, 3018, tilesheet),
                new Tile(TileLayer.Back, 35, 46, 3019, tilesheet),
                new Tile(TileLayer.Back, 36, 46, 3020, tilesheet),
                new Tile(TileLayer.Back, 37, 46, 3021, tilesheet),
                new Tile(TileLayer.Back, 38, 46, 3022, tilesheet),
                new Tile(TileLayer.Back, 33, 47, 3042, tilesheet),
                new Tile(TileLayer.Back, 34, 47, 3043, tilesheet),
                new Tile(TileLayer.Back, 35, 47, 3044, tilesheet),
                new Tile(TileLayer.Back, 36, 47, 3045, tilesheet),
                new Tile(TileLayer.Back, 37, 47, 3046, tilesheet),
                new Tile(TileLayer.Back, 38, 47, 3047, tilesheet),
                new Tile(TileLayer.Back, 33, 48, 3067, tilesheet),
                new Tile(TileLayer.Back, 34, 48, 3068, tilesheet),
                new Tile(TileLayer.Back, 35, 48, 3069, tilesheet),
                new Tile(TileLayer.Back, 36, 48, 3070, tilesheet),
                new Tile(TileLayer.Back, 37, 48, 3071, tilesheet),
                new Tile(TileLayer.Back, 38, 48, 3072, tilesheet)
            };
        }

        private Tile[] StoneBridgeEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 40, 48),
                new Tile(TileLayer.Back, 41, 48),
                new Tile(TileLayer.Back, 40, 49),
                new Tile(TileLayer.Back, 41, 49),
                new Tile(TileLayer.Buildings, 39, 49),
                new Tile(TileLayer.Buildings, 40, 49),
                new Tile(TileLayer.Buildings, 41, 49),
                new Tile(TileLayer.Buildings, 42, 49),
                new Tile(TileLayer.Back, 40, 50),
                new Tile(TileLayer.Back, 41, 50),
                new Tile(TileLayer.Buildings, 39, 50),
                new Tile(TileLayer.Buildings, 40, 50),
                new Tile(TileLayer.Buildings, 41, 50),
                new Tile(TileLayer.Buildings, 42, 50),
                new Tile(TileLayer.Back, 40, 51),
                new Tile(TileLayer.Back, 41, 51),
                new Tile(TileLayer.Back, 40, 52),
                new Tile(TileLayer.Back, 41, 52),
                new Tile(TileLayer.Back, 40, 53),
                new Tile(TileLayer.Back, 41, 53),
                new Tile(TileLayer.Back, 40, 54),
                new Tile(TileLayer.Back, 41, 54),
                new Tile(TileLayer.Back, 40, 55),
                new Tile(TileLayer.Back, 41, 55),
                new Tile(TileLayer.Back, 40, 56),
                new Tile(TileLayer.Back, 41, 56),
                new Tile(TileLayer.Back, 40, 57),
                new Tile(TileLayer.Back, 41, 57),
                new Tile(TileLayer.Buildings, 41, 57),
                new Tile(TileLayer.Buildings, 42, 57),
                new Tile(TileLayer.Back, 40, 58),
                new Tile(TileLayer.Back, 41, 58),
                new Tile(TileLayer.Buildings, 39, 58),
                new Tile(TileLayer.Buildings, 40, 58),
                new Tile(TileLayer.Buildings, 41, 58),
                new Tile(TileLayer.Back, 39, 59),
                new Tile(TileLayer.Back, 40, 59),
                new Tile(TileLayer.Back, 41, 59),
                new Tile(TileLayer.Back, 42, 59),
                new Tile(TileLayer.Buildings, 39, 48, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 48, 2714, tilesheet),
                new Tile(TileLayer.Back, 41, 48, 2714, tilesheet),
                new Tile(TileLayer.Buildings, 42, 48, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 49, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 49, 2738, tilesheet),
                new Tile(TileLayer.Back, 41, 49, 2738, tilesheet),
                new Tile(TileLayer.Buildings, 42, 49, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 50, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 50, 2738, tilesheet),
                new Tile(TileLayer.Back, 41, 50, 2738, tilesheet),
                new Tile(TileLayer.Buildings, 42, 50, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 51, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 51, 2738, tilesheet),
                new Tile(TileLayer.Back, 41, 51, 2738, tilesheet),
                new Tile(TileLayer.Buildings, 42, 51, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 52, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 52, 2738, tilesheet),
                new Tile(TileLayer.Back, 41, 52, 2738, tilesheet),
                new Tile(TileLayer.Buildings, 42, 52, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 53, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 53, 2738, tilesheet),
                new Tile(TileLayer.Back, 41, 53, 2738, tilesheet),
                new Tile(TileLayer.Buildings, 42, 53, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 54, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 54, 2738, tilesheet),
                new Tile(TileLayer.Back, 41, 54, 2738, tilesheet),
                new Tile(TileLayer.Buildings, 42, 54, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 55, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 55, 2738, tilesheet),
                new Tile(TileLayer.Back, 41, 55, 2738, tilesheet),
                new Tile(TileLayer.Buildings, 42, 55, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 56, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 56, 2738, tilesheet),
                new Tile(TileLayer.Back, 41, 56, 2738, tilesheet),
                new Tile(TileLayer.Buildings, 42, 56, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 57, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 57, 2738, tilesheet),
                new Tile(TileLayer.Back, 41, 57, 2738, tilesheet),
                new Tile(TileLayer.Buildings, 42, 57, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 58, 2715, tilesheet),
                new Tile(TileLayer.Back, 40, 58, 2738, tilesheet),
                new Tile(TileLayer.Back, 41, 58, 2738, tilesheet),
                new Tile(TileLayer.Buildings, 42, 58, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 39, 59, 2740, tilesheet),
                new Tile(TileLayer.Back, 40, 59, 2713, tilesheet),
                new Tile(TileLayer.Back, 41, 59, 2713, tilesheet),
                new Tile(TileLayer.Buildings, 42, 59, 2740, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 48, 2999, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 49, 3024, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 50, 3024, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 51, 3024, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 52, 3024, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 53, 3024, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 54, 3024, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 55, 3024, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 56, 3024, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 57, 3024, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 58, 3024, tilesheet),
                new Tile(TileLayer.AlwaysFront, 38, 59, 3049, tilesheet)
            };
        }

        private Tile[] PicnicAreaTableEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Buildings, 36, 44, 2970, tilesheet),
                new Tile(TileLayer.Buildings, 37, 44, 2971, tilesheet),
                new Tile(TileLayer.Front, 37, 42, 2921, tilesheet),
                new Tile(TileLayer.Front, 36, 43, 2945, tilesheet),
                new Tile(TileLayer.Front, 37, 43, 2946, tilesheet)
            };
        }

        private Tile[] TreeSwingEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 32, 42, 433, tilesheet),
                new Tile(TileLayer.Back, 33, 42, 433, tilesheet),
                new Tile(TileLayer.Buildings, 32, 43, 2941, tilesheet),
                new Tile(TileLayer.Buildings, 33, 43, 2942, tilesheet),
                new Tile(TileLayer.Buildings, 34, 43, 2943, tilesheet),
                new Tile(TileLayer.Buildings, 35, 43, 2944, tilesheet),
                new Tile(TileLayer.Buildings, 32, 42, 2916, tilesheet),
                new Tile(TileLayer.Buildings, 33, 42, 2917, tilesheet),
                new Tile(TileLayer.Buildings, 32, 44, 2966, tilesheet),
                new Tile(TileLayer.Buildings, 33, 44, 2967, tilesheet),
                new Tile(TileLayer.Buildings, 34, 44, 2968, tilesheet),
                new Tile(TileLayer.Buildings, 35, 44, 2969, tilesheet),
                new Tile(TileLayer.Front, 34, 42, 2918, tilesheet),
                new Tile(TileLayer.Front, 35, 42, 2919, tilesheet),
                new Tile(TileLayer.AlwaysFront, 32, 38, 2816, tilesheet),
                new Tile(TileLayer.AlwaysFront, 33, 38, 2817, tilesheet),
                new Tile(TileLayer.AlwaysFront, 34, 38, 2818, tilesheet),
                new Tile(TileLayer.AlwaysFront, 35, 38, 2819, tilesheet),
                new Tile(TileLayer.AlwaysFront, 36, 38, 2820, tilesheet),
                new Tile(TileLayer.AlwaysFront, 32, 39, 2841, tilesheet),
                new Tile(TileLayer.AlwaysFront, 33, 39, 2842, tilesheet),
                new Tile(TileLayer.AlwaysFront, 34, 39, 2843, tilesheet),
                new Tile(TileLayer.AlwaysFront, 35, 39, 2844, tilesheet),
                new Tile(TileLayer.AlwaysFront, 36, 39, 2845, tilesheet),
                new Tile(TileLayer.AlwaysFront, 32, 40, 2866, tilesheet),
                new Tile(TileLayer.AlwaysFront, 33, 40, 2867, tilesheet),
                new Tile(TileLayer.AlwaysFront, 34, 40, 2868, tilesheet),
                new Tile(TileLayer.AlwaysFront, 35, 40, 2869, tilesheet),
                new Tile(TileLayer.AlwaysFront, 36, 40, 2870, tilesheet),
                new Tile(TileLayer.AlwaysFront, 32, 41, 2891, tilesheet),
                new Tile(TileLayer.AlwaysFront, 33, 41, 2892, tilesheet),
                new Tile(TileLayer.AlwaysFront, 34, 41, 2893, tilesheet),
                new Tile(TileLayer.AlwaysFront, 35, 41, 2894, tilesheet),
                new Tile(TileLayer.AlwaysFront, 36, 41, 2895, tilesheet),
                new Tile(TileLayer.AlwaysFront, 34, 42, 2918, tilesheet),
                new Tile(TileLayer.AlwaysFront, 35, 42, 2919, tilesheet),
                new Tile(TileLayer.AlwaysFront, 36, 42, 2920, tilesheet)
            };
        }

        private Tile[] DogHouseEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 51, 5, 838, tilesheet),
                new Tile(TileLayer.Back, 52, 5, 1203, tilesheet),
                new Tile(TileLayer.Back, 53, 5, 1203, tilesheet),
                new Tile(TileLayer.Back, 54, 5, 838, tilesheet),
                new Tile(TileLayer.Back, 51, 6, 838, tilesheet),
                new Tile(TileLayer.Back, 52, 6, 838, tilesheet),
                new Tile(TileLayer.Back, 53, 6, 838, tilesheet),
                new Tile(TileLayer.Back, 54, 6, 838, tilesheet),
                new Tile(TileLayer.Back, 51, 7, 2200, tilesheet),
                new Tile(TileLayer.Back, 52, 7, 2201, tilesheet),
                new Tile(TileLayer.Back, 53, 7, 2202, tilesheet),
                new Tile(TileLayer.Back, 54, 7, 2203, tilesheet),
                new Tile(TileLayer.Back, 51, 8, 2225, tilesheet),
                new Tile(TileLayer.Back, 52, 8, 2226, tilesheet),
                new Tile(TileLayer.Back, 53, 8, 2227, tilesheet),
                new Tile(TileLayer.Back, 54, 8, 2228, tilesheet),
                new Tile(TileLayer.Back, 51, 9, 2250, tilesheet),
                new Tile(TileLayer.Back, 52, 9, 2251, tilesheet),
                new Tile(TileLayer.Back, 53, 9, 2252, tilesheet),
                new Tile(TileLayer.Back, 54, 9, 2253, tilesheet),
                new Tile(TileLayer.Buildings, 51, 4, 2668, tilesheet),
                new Tile(TileLayer.Buildings, 52, 4, 2669, tilesheet),
                new Tile(TileLayer.Buildings, 53, 4, 2670, tilesheet),
                new Tile(TileLayer.Buildings, 54, 4, 2671, tilesheet),
                new Tile(TileLayer.Buildings, 51, 5, 2693, tilesheet),
                new Tile(TileLayer.Buildings, 52, 5, 2694, tilesheet),
                new Tile(TileLayer.Buildings, 53, 5, 2695, tilesheet),
                new Tile(TileLayer.Buildings, 54, 5, 2696, tilesheet),
                new Tile(TileLayer.Buildings, 51, 6, 2718, tilesheet),
                new Tile(TileLayer.Buildings, 52, 6, 2719, tilesheet),
                new Tile(TileLayer.Buildings, 53, 6, 2720, tilesheet),
                new Tile(TileLayer.Buildings, 54, 6, 2721, tilesheet),
                new Tile(TileLayer.Buildings, 52, 7, 2201, tilesheet),
                new Tile(TileLayer.Buildings, 53, 7, 2202, tilesheet),
                new Tile(TileLayer.Buildings, 54, 7),
                new Tile(TileLayer.AlwaysFront, 51, 3, 2643, tilesheet),
                new Tile(TileLayer.AlwaysFront, 52, 3, 2644, tilesheet),
                new Tile(TileLayer.AlwaysFront, 53, 3, 2645, tilesheet),
                new Tile(TileLayer.AlwaysFront, 54, 3, 2646, tilesheet),
                new Tile(TileLayer.Front, 52, 4),
                new Tile(TileLayer.Front, 50, 5),
                new Tile(TileLayer.Front, 51, 5),
                new Tile(TileLayer.Front, 52, 5),
                new Tile(TileLayer.Front, 53, 5),
                new Tile(TileLayer.Front, 54, 5),
                new Tile(TileLayer.Buildings, 50, 6)
            };
        }

        private Tile[] YardGardenEditsAndBinClutter()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 57, 8),
                new Tile(TileLayer.Back, 57, 11),
                new Tile(TileLayer.Back, 56, 4, 200, tilesheet),
                new Tile(TileLayer.Back, 57, 4, 179, tilesheet),
                new Tile(TileLayer.Back, 58, 4, 180, tilesheet),
                new Tile(TileLayer.Back, 59, 4, 179, tilesheet),
                new Tile(TileLayer.Back, 60, 4, 203, tilesheet),
                new Tile(TileLayer.Back, 56, 5, 225, tilesheet),
                new Tile(TileLayer.Back, 57, 5, 227, tilesheet),
                new Tile(TileLayer.Back, 58, 5, 227, tilesheet),
                new Tile(TileLayer.Back, 59, 5, 227, tilesheet),
                new Tile(TileLayer.Back, 60, 5, 228, tilesheet),
                new Tile(TileLayer.Back, 56, 6, 225, tilesheet),
                new Tile(TileLayer.Back, 57, 6, 227, tilesheet),
                new Tile(TileLayer.Back, 58, 6, 227, tilesheet),
                new Tile(TileLayer.Back, 59, 6, 227, tilesheet),
                new Tile(TileLayer.Back, 60, 6, 228, tilesheet),
                new Tile(TileLayer.Back, 56, 7, 225, tilesheet),
                new Tile(TileLayer.Back, 57, 7, 227, tilesheet),
                new Tile(TileLayer.Back, 58, 7, 227, tilesheet),
                new Tile(TileLayer.Back, 59, 7, 227, tilesheet),
                new Tile(TileLayer.Back, 60, 7, 228, tilesheet),
                new Tile(TileLayer.Back, 56, 8, 225, tilesheet),
                new Tile(TileLayer.Back, 57, 8, 227, tilesheet),
                new Tile(TileLayer.Back, 58, 8, 227, tilesheet),
                new Tile(TileLayer.Back, 59, 8, 227, tilesheet),
                new Tile(TileLayer.Back, 60, 8, 228, tilesheet),
                new Tile(TileLayer.Back, 56, 9, 250, tilesheet),
                new Tile(TileLayer.Back, 57, 9, 251, tilesheet),
                new Tile(TileLayer.Back, 58, 9, 251, tilesheet),
                new Tile(TileLayer.Back, 59, 9, 251, tilesheet),
                new Tile(TileLayer.Back, 60, 9, 253, tilesheet),
                new Tile(TileLayer.Back, 55, 4, 203, tilesheet),
                new Tile(TileLayer.Back, 55, 5, 228, tilesheet),
                new Tile(TileLayer.Back, 55, 6, 228, tilesheet),
                new Tile(TileLayer.Back, 55, 7, 228, tilesheet),
                new Tile(TileLayer.Back, 55, 8, 228, tilesheet),
                new Tile(TileLayer.Back, 55, 9, 228, tilesheet),
                new Tile(TileLayer.Back, 55, 10, 228, tilesheet),
                new Tile(TileLayer.Back, 55, 11, 177, tilesheet),
                new Tile(TileLayer.Back, 56, 11, 201, tilesheet),
                new Tile(TileLayer.Back, 57, 11, 201, tilesheet),
                new Tile(TileLayer.Back, 58, 11, 203, tilesheet),
                new Tile(TileLayer.Back, 58, 12, 228, tilesheet),
                new Tile(TileLayer.Back, 58, 13, 228, tilesheet),
                new Tile(TileLayer.Back, 56, 10, 175, tilesheet),
                new Tile(TileLayer.Back, 56, 12, 227, tilesheet),
                new Tile(TileLayer.Back, 57, 12, 227, tilesheet),
                new Tile(TileLayer.Back, 57, 13, 227, tilesheet),
                new Tile(TileLayer.Back, 61, 5, 175, tilesheet),
                new Tile(TileLayer.Back, 62, 5, 175, tilesheet),
                new Tile(TileLayer.Back, 63, 5, 175, tilesheet),
                new Tile(TileLayer.Back, 64, 5, 175, tilesheet),
                new Tile(TileLayer.Back, 65, 5, 175, tilesheet),
                new Tile(TileLayer.Back, 61, 6, 175, tilesheet),
                new Tile(TileLayer.Back, 62, 6, 175, tilesheet),
                new Tile(TileLayer.Back, 63, 6, 175, tilesheet),
                new Tile(TileLayer.Back, 64, 6, 175, tilesheet),
                new Tile(TileLayer.Back, 65, 6, 175, tilesheet),
                new Tile(TileLayer.Front, 55, 3, 2469, tilesheet),
                new Tile(TileLayer.Front, 56, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 57, 3, 2471, tilesheet),
                new Tile(TileLayer.Front, 58, 3, 2471, tilesheet),
                new Tile(TileLayer.Front, 59, 3, 2469, tilesheet),
                new Tile(TileLayer.Front, 60, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 61, 3, 2471, tilesheet),
                new Tile(TileLayer.Front, 62, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 63, 3, 2469, tilesheet),
                new Tile(TileLayer.Front, 64, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 65, 3, 2471, tilesheet),
                new Tile(TileLayer.Front, 66, 3, 2469, tilesheet),
                new Tile(TileLayer.Front, 67, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 68, 3, 2471, tilesheet),
                new Tile(TileLayer.Front, 69, 3, 2469, tilesheet),
                new Tile(TileLayer.Front, 70, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 71, 3, 2471, tilesheet),
                new Tile(TileLayer.Front, 72, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 73, 3, 2471, tilesheet),
                new Tile(TileLayer.Front, 74, 3, 2469, tilesheet),
                new Tile(TileLayer.Front, 75, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 76, 3, 2471, tilesheet),
                new Tile(TileLayer.Front, 77, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 78, 3, 2471, tilesheet),
                new Tile(TileLayer.Front, 79, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 51, 3, 2468, tilesheet),
                new Tile(TileLayer.Front, 52, 3, 2469, tilesheet),
                new Tile(TileLayer.Front, 53, 3, 2470, tilesheet),
                new Tile(TileLayer.Front, 54, 3, 2471, tilesheet),
                new Tile(TileLayer.Front, 55, 3, 2472, tilesheet),
                new Tile(TileLayer.Back, 51, 4, 2493, tilesheet),
                new Tile(TileLayer.Buildings, 52, 4, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 53, 4, 2495, tilesheet),
                new Tile(TileLayer.Back, 54, 4, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 55, 4, 2497, tilesheet),
                new Tile(TileLayer.Buildings, 56, 4, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 57, 4, 2495, tilesheet),
                new Tile(TileLayer.Buildings, 58, 4, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 59, 4, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 60, 4, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 61, 4, 2495, tilesheet),
                new Tile(TileLayer.Buildings, 62, 4, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 63, 4, 2497, tilesheet),
                new Tile(TileLayer.Buildings, 64, 4, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 65, 4, 2497, tilesheet),
                new Tile(TileLayer.Buildings, 66, 4, 2493, tilesheet),
                new Tile(TileLayer.Buildings, 67, 4, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 68, 4, 2495, tilesheet),
                new Tile(TileLayer.Buildings, 69, 4, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 70, 4, 2493, tilesheet),
                new Tile(TileLayer.Buildings, 71, 4, 2493, tilesheet),
                new Tile(TileLayer.Buildings, 72, 4, 2494, tilesheet),
                new Tile(TileLayer.Buildings, 73, 4, 2495, tilesheet),
                new Tile(TileLayer.Buildings, 74, 4, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 75, 4, 2497, tilesheet),
                new Tile(TileLayer.Buildings, 76, 4, 2126, tilesheet),
                new Tile(TileLayer.Buildings, 77, 4, 2496, tilesheet),
                new Tile(TileLayer.Buildings, 78, 4, 2493, tilesheet),
                new Tile(TileLayer.Buildings, 79, 4, 1496, tilesheet),
                new Tile(TileLayer.Buildings, 75, 5, 2741, tilesheet),
                new Tile(TileLayer.Buildings, 76, 5, 2003, tilesheet),
                new Tile(TileLayer.Buildings, 77, 5, 2062, tilesheet),
                new Tile(TileLayer.Buildings, 78, 5, 1520, tilesheet),
                new Tile(TileLayer.Buildings, 79, 5, 1521, tilesheet),
                new Tile(TileLayer.Buildings, 77, 6, 2087, tilesheet),
                new Tile(TileLayer.Buildings, 78, 6, 2088, tilesheet),
                new Tile(TileLayer.Buildings, 77, 7, 2112, tilesheet),
                new Tile(TileLayer.Buildings, 78, 7, 2113, tilesheet),
                new Tile(TileLayer.Buildings, 76, 6, 21, tilesheet),
                new Tile(TileLayer.Front, 75, 4, 2716, tilesheet),
                new Tile(TileLayer.Front, 76, 4, 2036, tilesheet),
                new Tile(TileLayer.Front, 77, 4, 2037, tilesheet),
                new Tile(TileLayer.Front, 78, 4, 1495, tilesheet),
                new Tile(TileLayer.Back, 79, 4, 2496, tilesheet),
                new Tile(TileLayer.Front, 76, 5, 2061, tilesheet),
                new Tile(TileLayer.Front, 78, 5, 2063, tilesheet),
                new Tile(TileLayer.AlwaysFront, 76, 3, 2101, tilesheet),
                new Tile(TileLayer.AlwaysFront, 78, 4, 2038, tilesheet)
            };
        }

        private Tile[] PatioEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 71, 7),
                new Tile(TileLayer.Back, 71, 11),
                new Tile(TileLayer.Back, 66, 5, 2715, tilesheet),
                new Tile(TileLayer.Back, 67, 5, 2715, tilesheet),
                new Tile(TileLayer.Back, 68, 5, 2715, tilesheet),
                new Tile(TileLayer.Back, 69, 5, 2715, tilesheet),
                new Tile(TileLayer.Back, 70, 5, 2715, tilesheet),
                new Tile(TileLayer.Back, 71, 5, 2715, tilesheet),
                new Tile(TileLayer.Back, 72, 5, 2715, tilesheet),
                new Tile(TileLayer.Back, 73, 5, 2715, tilesheet),
                new Tile(TileLayer.Back, 66, 6, 2715, tilesheet),
                new Tile(TileLayer.Back, 67, 6, 2738, tilesheet),
                new Tile(TileLayer.Back, 68, 6, 2738, tilesheet),
                new Tile(TileLayer.Back, 69, 6, 2738, tilesheet),
                new Tile(TileLayer.Back, 70, 6, 2738, tilesheet),
                new Tile(TileLayer.Back, 71, 6, 2738, tilesheet),
                new Tile(TileLayer.Back, 72, 6, 2738, tilesheet),
                new Tile(TileLayer.Back, 73, 6, 2715, tilesheet),
                new Tile(TileLayer.Back, 66, 7, 2715, tilesheet),
                new Tile(TileLayer.Back, 67, 7, 2738, tilesheet),
                new Tile(TileLayer.Back, 68, 7, 2738, tilesheet),
                new Tile(TileLayer.Back, 69, 7, 2738, tilesheet),
                new Tile(TileLayer.Back, 70, 7, 2738, tilesheet),
                new Tile(TileLayer.Back, 71, 7, 2738, tilesheet),
                new Tile(TileLayer.Back, 72, 7, 2738, tilesheet),
                new Tile(TileLayer.Back, 73, 7, 2715, tilesheet),
                new Tile(TileLayer.Back, 66, 8, 2715, tilesheet),
                new Tile(TileLayer.Back, 67, 8, 2738, tilesheet),
                new Tile(TileLayer.Back, 68, 8, 2738, tilesheet),
                new Tile(TileLayer.Back, 69, 8, 2738, tilesheet),
                new Tile(TileLayer.Back, 70, 8, 2738, tilesheet),
                new Tile(TileLayer.Back, 71, 8, 2738, tilesheet),
                new Tile(TileLayer.Back, 72, 8, 2738, tilesheet),
                new Tile(TileLayer.Back, 73, 8, 2715, tilesheet),
                new Tile(TileLayer.Back, 66, 9, 2715, tilesheet),
                new Tile(TileLayer.Back, 67, 9, 2738, tilesheet),
                new Tile(TileLayer.Back, 68, 9, 2738, tilesheet),
                new Tile(TileLayer.Back, 69, 9, 2738, tilesheet),
                new Tile(TileLayer.Back, 70, 9, 2738, tilesheet),
                new Tile(TileLayer.Back, 71, 9, 2738, tilesheet),
                new Tile(TileLayer.Back, 72, 9, 2738, tilesheet),
                new Tile(TileLayer.Back, 73, 9, 2715, tilesheet),
                new Tile(TileLayer.Back, 66, 10, 2715, tilesheet),
                new Tile(TileLayer.Back, 67, 10, 2738, tilesheet),
                new Tile(TileLayer.Back, 68, 10, 2738, tilesheet),
                new Tile(TileLayer.Back, 69, 10, 2738, tilesheet),
                new Tile(TileLayer.Back, 70, 10, 2738, tilesheet),
                new Tile(TileLayer.Back, 71, 10, 2738, tilesheet),
                new Tile(TileLayer.Back, 72, 10, 2738, tilesheet),
                new Tile(TileLayer.Back, 73, 10, 2715, tilesheet),
                new Tile(TileLayer.Back, 68, 11, 2715, tilesheet),
                new Tile(TileLayer.Back, 69, 11, 2715, tilesheet),
                new Tile(TileLayer.Back, 70, 11, 2715, tilesheet),
                new Tile(TileLayer.Back, 71, 11, 2715, tilesheet),
                new Tile(TileLayer.Back, 72, 11, 2715, tilesheet),
                new Tile(TileLayer.Back, 73, 11, 2715, tilesheet),
                new Tile(TileLayer.Back, 65, 11, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 66, 11, 2715, tilesheet),
                new Tile(TileLayer.Buildings, 67, 11, 2715, tilesheet),
                new Tile(TileLayer.Front, 55, 8),
                new Tile(TileLayer.Front, 56, 8),
                new Tile(TileLayer.Front, 57, 8),
                new Tile(TileLayer.Front, 58, 8),
                new Tile(TileLayer.Front, 59, 8),
                new Tile(TileLayer.Front, 60, 8),
                new Tile(TileLayer.Front, 61, 8),
                new Tile(TileLayer.Front, 62, 8),
                new Tile(TileLayer.Front, 63, 8),
                new Tile(TileLayer.Front, 64, 8),
                new Tile(TileLayer.Front, 65, 8),
                new Tile(TileLayer.Front, 66, 8),
                new Tile(TileLayer.Front, 67, 8),
                new Tile(TileLayer.Front, 68, 8),
                new Tile(TileLayer.Front, 69, 8),
                new Tile(TileLayer.Front, 70, 8),
                new Tile(TileLayer.Front, 71, 8),
                new Tile(TileLayer.Front, 72, 8),
                new Tile(TileLayer.Front, 73, 8),
                new Tile(TileLayer.Front, 74, 8),
                new Tile(TileLayer.Front, 75, 8),
                new Tile(TileLayer.Front, 76, 8),
                new Tile(TileLayer.Front, 77, 8),
                new Tile(TileLayer.Front, 78, 8),
                new Tile(TileLayer.Front, 79, 8),
                new Tile(TileLayer.Buildings, 55, 9),
                new Tile(TileLayer.Buildings, 56, 9),
                new Tile(TileLayer.Buildings, 57, 9),
                new Tile(TileLayer.Buildings, 58, 9),
                new Tile(TileLayer.Buildings, 59, 9),
                new Tile(TileLayer.Buildings, 60, 9),
                new Tile(TileLayer.Buildings, 61, 9),
                new Tile(TileLayer.Buildings, 62, 9),
                new Tile(TileLayer.Buildings, 63, 9),
                new Tile(TileLayer.Buildings, 64, 9),
                new Tile(TileLayer.Buildings, 65, 9),
                new Tile(TileLayer.Buildings, 66, 9),
                new Tile(TileLayer.Buildings, 67, 9),
                new Tile(TileLayer.Buildings, 68, 9),
                new Tile(TileLayer.Buildings, 69, 9),
                new Tile(TileLayer.Buildings, 70, 9),
                new Tile(TileLayer.Buildings, 71, 9),
                new Tile(TileLayer.Buildings, 72, 9),
                new Tile(TileLayer.Buildings, 73, 9),
                new Tile(TileLayer.Buildings, 74, 9),
                new Tile(TileLayer.Buildings, 75, 9),
                new Tile(TileLayer.Buildings, 76, 9),
                new Tile(TileLayer.Buildings, 77, 9),
                new Tile(TileLayer.Buildings, 78, 5),
                new Tile(TileLayer.Buildings, 78, 6),
                new Tile(TileLayer.Buildings, 78, 7),
                new Tile(TileLayer.Buildings, 78, 8),
                new Tile(TileLayer.Buildings, 78, 9),
                new Tile(TileLayer.Buildings, 78, 10),
                new Tile(TileLayer.Buildings, 55, 8),
                new Tile(TileLayer.Buildings, 56, 8),
                new Tile(TileLayer.Buildings, 57, 8),
                new Tile(TileLayer.Buildings, 58, 8),
                new Tile(TileLayer.Buildings, 59, 8),
                new Tile(TileLayer.Buildings, 60, 8),
                new Tile(TileLayer.Buildings, 61, 8),
                new Tile(TileLayer.Buildings, 62, 8),
                new Tile(TileLayer.Buildings, 63, 8),
                new Tile(TileLayer.Buildings, 64, 8),
                new Tile(TileLayer.Buildings, 65, 8),
                new Tile(TileLayer.Buildings, 66, 8),
                new Tile(TileLayer.Buildings, 67, 8),
                new Tile(TileLayer.Buildings, 68, 8),
                new Tile(TileLayer.Buildings, 69, 8),
                new Tile(TileLayer.Buildings, 70, 8),
                new Tile(TileLayer.Buildings, 71, 8),
                new Tile(TileLayer.Buildings, 72, 8),
                new Tile(TileLayer.Buildings, 73, 8),
                new Tile(TileLayer.Buildings, 74, 8),
                new Tile(TileLayer.Buildings, 75, 8),
                new Tile(TileLayer.Buildings, 76, 8),
                new Tile(TileLayer.Buildings, 77, 8),
                new Tile(TileLayer.Buildings, 55, 7),
                new Tile(TileLayer.Buildings, 56, 7),
                new Tile(TileLayer.Buildings, 57, 7),
                new Tile(TileLayer.Buildings, 58, 7),
                new Tile(TileLayer.Buildings, 59, 7),
                new Tile(TileLayer.Buildings, 60, 7),
                new Tile(TileLayer.Buildings, 61, 7),
                new Tile(TileLayer.Buildings, 62, 7),
                new Tile(TileLayer.Buildings, 63, 7),
                new Tile(TileLayer.Buildings, 64, 7),
                new Tile(TileLayer.Buildings, 65, 7),
                new Tile(TileLayer.Buildings, 66, 7),
                new Tile(TileLayer.Buildings, 67, 7),
                new Tile(TileLayer.Buildings, 68, 7),
                new Tile(TileLayer.Buildings, 69, 7),
                new Tile(TileLayer.Buildings, 70, 7),
                new Tile(TileLayer.Buildings, 71, 7),
                new Tile(TileLayer.Buildings, 72, 7),
                new Tile(TileLayer.Buildings, 73, 7),
                new Tile(TileLayer.Buildings, 75, 7),
                new Tile(TileLayer.Buildings, 76, 7),
                new Tile(TileLayer.Buildings, 77, 7),
                new Tile(TileLayer.Buildings, 55, 6),
                new Tile(TileLayer.Buildings, 56, 6),
                new Tile(TileLayer.Buildings, 57, 6),
                new Tile(TileLayer.Buildings, 58, 6),
                new Tile(TileLayer.Buildings, 59, 6),
                new Tile(TileLayer.Buildings, 60, 6),
                new Tile(TileLayer.Buildings, 61, 6),
                new Tile(TileLayer.Buildings, 62, 6),
                new Tile(TileLayer.Buildings, 63, 6),
                new Tile(TileLayer.Buildings, 64, 6),
                new Tile(TileLayer.Buildings, 65, 6),
                new Tile(TileLayer.Buildings, 66, 6),
                new Tile(TileLayer.Buildings, 67, 6),
                new Tile(TileLayer.Buildings, 68, 6),
                new Tile(TileLayer.Buildings, 69, 6),
                new Tile(TileLayer.Buildings, 70, 6),
                new Tile(TileLayer.Buildings, 71, 6),
                new Tile(TileLayer.Buildings, 72, 6),
                new Tile(TileLayer.Buildings, 73, 6),
                new Tile(TileLayer.Buildings, 74, 6),
                new Tile(TileLayer.Buildings, 75, 6),
                new Tile(TileLayer.Buildings, 77, 6),
                new Tile(TileLayer.Buildings, 55, 5),
                new Tile(TileLayer.Buildings, 56, 5),
                new Tile(TileLayer.Buildings, 57, 5),
                new Tile(TileLayer.Buildings, 58, 5),
                new Tile(TileLayer.Buildings, 59, 5),
                new Tile(TileLayer.Buildings, 60, 5),
                new Tile(TileLayer.Buildings, 61, 5),
                new Tile(TileLayer.Buildings, 62, 5),
                new Tile(TileLayer.Buildings, 63, 5),
                new Tile(TileLayer.Buildings, 64, 5),
                new Tile(TileLayer.Buildings, 65, 5),
                new Tile(TileLayer.Buildings, 66, 5),
                new Tile(TileLayer.Buildings, 67, 5),
                new Tile(TileLayer.Buildings, 68, 5),
                new Tile(TileLayer.Buildings, 69, 5),
                new Tile(TileLayer.Buildings, 70, 5),
                new Tile(TileLayer.Buildings, 71, 5),
                new Tile(TileLayer.Buildings, 72, 5),
                new Tile(TileLayer.Buildings, 73, 5),
                new Tile(TileLayer.Buildings, 74, 5),
                new Tile(TileLayer.Buildings, 77, 5),
                new Tile(TileLayer.Front, 55, 3),
                new Tile(TileLayer.Front, 56, 3),
                new Tile(TileLayer.Front, 57, 3),
                new Tile(TileLayer.Front, 55, 4),
                new Tile(TileLayer.Front, 56, 4),
                new Tile(TileLayer.Front, 57, 4),
                new Tile(TileLayer.Front, 55, 5),
                new Tile(TileLayer.Front, 56, 5),
                new Tile(TileLayer.Front, 57, 5),
                new Tile(TileLayer.Front, 55, 6),
                new Tile(TileLayer.Front, 56, 6),
                new Tile(TileLayer.Front, 57, 6),
                new Tile(TileLayer.Front, 56, 7),
                new Tile(TileLayer.Front, 54, 5),
                new Tile(TileLayer.Buildings, 55, 5),
                new Tile(TileLayer.Front, 68, 3),
                new Tile(TileLayer.Front, 69, 3),
                new Tile(TileLayer.Front, 70, 3),
                new Tile(TileLayer.Front, 68, 4),
                new Tile(TileLayer.Front, 69, 4),
                new Tile(TileLayer.Front, 70, 4),
                new Tile(TileLayer.Front, 68, 5),
                new Tile(TileLayer.Front, 69, 5),
                new Tile(TileLayer.Front, 70, 5),
                new Tile(TileLayer.Front, 68, 6),
                new Tile(TileLayer.Front, 69, 6),
                new Tile(TileLayer.Front, 70, 6),
                new Tile(TileLayer.Front, 69, 7),
                new Tile(TileLayer.Front, 73, 2),
                new Tile(TileLayer.Front, 74, 2),
                new Tile(TileLayer.Front, 75, 2),
                new Tile(TileLayer.Front, 73, 3),
                new Tile(TileLayer.Front, 74, 3),
                new Tile(TileLayer.Front, 75, 3),
                new Tile(TileLayer.Front, 73, 4),
                new Tile(TileLayer.Front, 74, 4),
                new Tile(TileLayer.Front, 75, 4),
                new Tile(TileLayer.Front, 73, 5),
                new Tile(TileLayer.Front, 74, 5),
                new Tile(TileLayer.Front, 75, 5),
                new Tile(TileLayer.Front, 74, 6),
                new Tile(TileLayer.Buildings, 74, 7),
                new Tile(TileLayer.Front, 78, 4),
                new Tile(TileLayer.Front, 79, 4),
                new Tile(TileLayer.Front, 78, 5),
                new Tile(TileLayer.Front, 79, 5),
                new Tile(TileLayer.Front, 78, 6),
                new Tile(TileLayer.Front, 79, 6),
                new Tile(TileLayer.Front, 78, 7),
                new Tile(TileLayer.Front, 79, 7),
                new Tile(TileLayer.Front, 57, 0),
                new Tile(TileLayer.Front, 58, 0),
                new Tile(TileLayer.Front, 59, 0),
                new Tile(TileLayer.Front, 60, 0),
                new Tile(TileLayer.Front, 61, 0),
                new Tile(TileLayer.Front, 57, 1),
                new Tile(TileLayer.Front, 58, 1),
                new Tile(TileLayer.Front, 59, 1),
                new Tile(TileLayer.Front, 60, 1),
                new Tile(TileLayer.Front, 61, 1),
                new Tile(TileLayer.Front, 62, 1),
                new Tile(TileLayer.Front, 57, 2),
                new Tile(TileLayer.Front, 58, 2),
                new Tile(TileLayer.Front, 59, 2),
                new Tile(TileLayer.Front, 60, 2),
                new Tile(TileLayer.Front, 61, 2),
                new Tile(TileLayer.Front, 62, 2),
                new Tile(TileLayer.Front, 59, 3),
                new Tile(TileLayer.Front, 60, 3),
                new Tile(TileLayer.Front, 59, 4),
                new Tile(TileLayer.Front, 60, 4),
                new Tile(TileLayer.Buildings, 58, 0, 77, tilesheet),
                new Tile(TileLayer.AlwaysFront, 62, 0, 55, tilesheet),
                new Tile(TileLayer.AlwaysFront, 62, 1, 80, tilesheet),
                new Tile(TileLayer.Back, 65, 5, 351, tilesheet),
                new Tile(TileLayer.Back, 74, 5, 351, tilesheet),
                new Tile(TileLayer.Back, 74, 6, 351, tilesheet),
                new Tile(TileLayer.Back, 74, 7, 351, tilesheet),
                new Tile(TileLayer.Back, 74, 8, 351, tilesheet),
                new Tile(TileLayer.Back, 74, 9, 351, tilesheet),
                new Tile(TileLayer.Back, 74, 10, 351, tilesheet),
                new Tile(TileLayer.Back, 75, 9, 351, tilesheet),
                new Tile(TileLayer.Back, 75, 10, 351, tilesheet),
                new Tile(TileLayer.Back, 76, 10, 351, tilesheet),
                new Tile(TileLayer.Back, 77, 10, 351, tilesheet),
                new Tile(TileLayer.Back, 76, 11, 351, tilesheet),
                new Tile(TileLayer.Back, 77, 11, 351, tilesheet),
                new Tile(TileLayer.Back, 56, 8, 175, tilesheet),
                new Tile(TileLayer.Back, 56, 9, 175, tilesheet),
                new Tile(TileLayer.Back, 57, 9, 175, tilesheet),
                new Tile(TileLayer.Back, 58, 9, 175, tilesheet),
                new Tile(TileLayer.Back, 59, 9, 175, tilesheet),
                new Tile(TileLayer.Back, 60, 9, 175, tilesheet),
                new Tile(TileLayer.Back, 61, 9, 175, tilesheet),
                new Tile(TileLayer.Back, 62, 9, 175, tilesheet),
                new Tile(TileLayer.Back, 63, 9, 175, tilesheet),
                new Tile(TileLayer.Back, 64, 9, 175, tilesheet),
                new Tile(TileLayer.Back, 65, 9, 175, tilesheet),
                new Tile(TileLayer.Back, 59, 4, 351, tilesheet),
                new Tile(TileLayer.Back, 60, 4, 351, tilesheet),
                new Tile(TileLayer.Front, 58, 1, 13, tilesheet),
                new Tile(TileLayer.Front, 59, 1, 14, tilesheet),
                new Tile(TileLayer.Front, 60, 1, 15, tilesheet),
                new Tile(TileLayer.AlwaysFront, 68, 4, 2069, tilesheet),
                new Tile(TileLayer.AlwaysFront, 69, 4, 2070, tilesheet),
                new Tile(TileLayer.AlwaysFront, 70, 4, 2071, tilesheet),
                new Tile(TileLayer.Front, 68, 5, 2094, tilesheet),
                new Tile(TileLayer.Front, 69, 5, 2095, tilesheet),
                new Tile(TileLayer.Front, 70, 5, 2096, tilesheet),
                new Tile(TileLayer.Front, 71, 5, 2097, tilesheet),
                new Tile(TileLayer.Buildings, 68, 6, 2119, tilesheet),
                new Tile(TileLayer.Buildings, 69, 6, 2120, tilesheet),
                new Tile(TileLayer.Buildings, 70, 6, 2121, tilesheet),
                new Tile(TileLayer.Buildings, 71, 6, 2122, tilesheet),
                new Tile(TileLayer.AlwaysFront, 69, 7, 2145, tilesheet),
                new Tile(TileLayer.AlwaysFront, 70, 7, 2146, tilesheet),
                new Tile(TileLayer.AlwaysFront, 71, 7, 2147, tilesheet),
                new Tile(TileLayer.Buildings, 69, 8, 2170, tilesheet),
                new Tile(TileLayer.Buildings, 70, 8, 2171, tilesheet),
                new Tile(TileLayer.Buildings, 71, 8, 2172, tilesheet),
                new Tile(TileLayer.Buildings, 69, 9, 2195, tilesheet),
                new Tile(TileLayer.Buildings, 70, 9, 2196, tilesheet),
                new Tile(TileLayer.Buildings, 71, 9, 2197, tilesheet)
            };
        }

        private Tile[] FarmStandEdits()
        {
            const string tilesheet = FarmTilesheet.Outdoors;
            return new[]
            {
                new Tile(TileLayer.Back, 71, 12, 175, tilesheet),
                new Tile(TileLayer.Back, 72, 12, 175, tilesheet),
                new Tile(TileLayer.Back, 70, 13, 175, tilesheet),
                new Tile(TileLayer.Back, 71, 13, 175, tilesheet),
                new Tile(TileLayer.Back, 72, 13, 175, tilesheet),
                new Tile(TileLayer.Back, 73, 13, 175, tilesheet),
                new Tile(TileLayer.Back, 70, 14, 175, tilesheet),
                new Tile(TileLayer.Back, 73, 14, 175, tilesheet),
                new Tile(TileLayer.Back, 73, 14, 2232, tilesheet),
                new Tile(TileLayer.Back, 74, 14, 2232, tilesheet),
                new Tile(TileLayer.Back, 75, 14, 2232, tilesheet),
                new Tile(TileLayer.Back, 76, 14, 2232, tilesheet),
                new Tile(TileLayer.Paths, 77, 11),
                new Tile(TileLayer.Paths, 74, 10),
                new Tile(TileLayer.Paths, 75, 10),
                new Tile(TileLayer.Front, 79, 10),
                new Tile(TileLayer.Front, 79, 13),
                new Tile(TileLayer.Front, 79, 12, 2138, tilesheet),
                new Tile(TileLayer.AlwaysFront, 76, 9, 1982, tilesheet),
                new Tile(TileLayer.AlwaysFront, 77, 9, 1983, tilesheet),
                new Tile(TileLayer.AlwaysFront, 78, 9, 1984, tilesheet),
                new Tile(TileLayer.AlwaysFront, 79, 9, 1985, tilesheet),
                new Tile(TileLayer.AlwaysFront, 77, 10, 2008, tilesheet),
                new Tile(TileLayer.AlwaysFront, 78, 10, 2009, tilesheet),
                new Tile(TileLayer.AlwaysFront, 79, 10, 2010, tilesheet),
                new Tile(TileLayer.AlwaysFront, 79, 11, 2035, tilesheet),
                new Tile(TileLayer.AlwaysFront, 73, 10, 2004, tilesheet),
                new Tile(TileLayer.AlwaysFront, 74, 10, 2005, tilesheet),
                new Tile(TileLayer.AlwaysFront, 75, 10, 2006, tilesheet),
                new Tile(TileLayer.AlwaysFront, 76, 10, 2007, tilesheet),
                new Tile(TileLayer.AlwaysFront, 73, 11, 2029, tilesheet),
                new Tile(TileLayer.AlwaysFront, 74, 11, 2030, tilesheet),
                new Tile(TileLayer.AlwaysFront, 75, 11, 2031, tilesheet),
                new Tile(TileLayer.AlwaysFront, 76, 11, 2032, tilesheet),
                new Tile(TileLayer.AlwaysFront, 73, 12, 2054, tilesheet),
                new Tile(TileLayer.AlwaysFront, 74, 12, 2055, tilesheet),
                new Tile(TileLayer.AlwaysFront, 75, 12, 2056, tilesheet),
                new Tile(TileLayer.AlwaysFront, 76, 12, 2057, tilesheet),
                new Tile(TileLayer.AlwaysFront, 73, 13, 2079, tilesheet),
                new Tile(TileLayer.AlwaysFront, 74, 13, 2080, tilesheet),
                new Tile(TileLayer.AlwaysFront, 75, 13, 2081, tilesheet),
                new Tile(TileLayer.AlwaysFront, 76, 13, 2082, tilesheet),
                new Tile(TileLayer.Front, 72, 14, 2103, tilesheet),
                new Tile(TileLayer.Front, 73, 14, 2104, tilesheet),
                new Tile(TileLayer.Front, 74, 14, 2105, tilesheet),
                new Tile(TileLayer.Front, 75, 14, 2106, tilesheet),
                new Tile(TileLayer.Front, 76, 14, 2107, tilesheet),
                new Tile(TileLayer.Front, 77, 14, 2108, tilesheet),
                new Tile(TileLayer.Front, 78, 14, 2109, tilesheet),
                new Tile(TileLayer.Buildings, 79, 11),
                new Tile(TileLayer.Buildings, 79, 6, 21, tilesheet),
                new Tile(TileLayer.Buildings, 79, 7, 21, tilesheet),
                new Tile(TileLayer.Buildings, 79, 8, 21, tilesheet),
                new Tile(TileLayer.Buildings, 79, 9, 21, tilesheet),
                new Tile(TileLayer.Buildings, 79, 10, 21, tilesheet),
                new Tile(TileLayer.Buildings, 78, 12, 2059, tilesheet),
                new Tile(TileLayer.Buildings, 78, 11, 2034, tilesheet),
                new Tile(TileLayer.Buildings, 79, 12, 2060, tilesheet),
                new Tile(TileLayer.Buildings, 77, 13, 2083, tilesheet),
                new Tile(TileLayer.Buildings, 77, 12, 2058, tilesheet),
                new Tile(TileLayer.Buildings, 77, 11, 2033, tilesheet),
                new Tile(TileLayer.Buildings, 78, 13, 2084, tilesheet),
                new Tile(TileLayer.Buildings, 79, 13, 2163, tilesheet),
                new Tile(TileLayer.Buildings, 78, 14, 2109, tilesheet),
                new Tile(TileLayer.Buildings, 79, 14, 2188, tilesheet),
                new Tile(TileLayer.Buildings, 72, 15, 2128, tilesheet),
                new Tile(TileLayer.Buildings, 73, 15, 2129, tilesheet),
                new Tile(TileLayer.Buildings, 74, 15, 2130, tilesheet),
                new Tile(TileLayer.Buildings, 75, 15, 2131, tilesheet),
                new Tile(TileLayer.Buildings, 76, 15, 2132, tilesheet),
                new Tile(TileLayer.Buildings, 77, 15, 2133, tilesheet),
                new Tile(TileLayer.Buildings, 78, 15, 2134, tilesheet),
                new Tile(TileLayer.Buildings, 79, 15, 2213, tilesheet),
                new Tile(TileLayer.Buildings, 72, 16, 2153, tilesheet),
                new Tile(TileLayer.Buildings, 73, 16, 2154, tilesheet)
            };
        }

        private void PatchMap(GameLocation location, Tile[] tiles)
        {
            foreach (Tile tile in tiles)
            {
                if (tile.TileIndex < 0)
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
                else if (location.map.GetLayer(tile.LayerName).Tiles[tile.X, tile.Y] == null || location.map.GetLayer(tile.LayerName).Tiles[tile.X, tile.Y].TileSheet.Id != tile.Tilesheet)
                {
                    int tileSheetIndex = Tile.GetTileSheetIndex(FarmTilesheet.Outdoors, location.map.TileSheets);
                    location.map.GetLayer(tile.LayerName).Tiles[tile.X, tile.Y] = new StaticTile(location.map.GetLayer(tile.LayerName), location.map.TileSheets[tileSheetIndex], 0, tile.TileIndex);
                }
                else
                    location.setMapTileIndex(tile.X, tile.Y, tile.TileIndex, location.map.GetLayer(tile.LayerName).Id);
            }
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
