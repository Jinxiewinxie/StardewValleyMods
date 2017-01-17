// Decompiled with JetBrains decompiler
// Type: WonderfulFarmLife.WonderfulFarmLife
// Assembly: WonderfulFarmLife, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 811ABA56-734C-41D0-915D-FE94C07965A0
// Assembly location: C:\Program Files (x86)\GalaxyClient\Games\Stardew Valley\Mods\WonderfulFarmLife\WonderfulFarmLife.dll

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.ObjectModel;
using xTile.Tiles;

namespace WonderfulFarmLife
{
  public class WonderfulFarmLife : Mod
  {
    private static string modPath = "";
    public static bool PetBowlFilled = false;
    public static bool farmSheetPatched = false;
    public static int tickCount = 0;

    public static WonderfulFarmLife.WonderfulFarmLife.PatchConfig ModConfig { get; private set; }

    public WonderfulFarmLife()
    {
      base.\u002Ector();
    }

    public virtual void Entry(params object[] objects)
    {
      WonderfulFarmLife.WonderfulFarmLife.modPath = this.get_PathOnDisk();
      WonderfulFarmLife.WonderfulFarmLife.ModConfig = (WonderfulFarmLife.WonderfulFarmLife.PatchConfig) ConfigExtensions.InitializeConfig<WonderfulFarmLife.WonderfulFarmLife.PatchConfig>((M0) new WonderfulFarmLife.WonderfulFarmLife.PatchConfig(), this.get_BaseConfigPath());
      GameEvents.add_UpdateTick(new EventHandler(this.Event_UpdateTick));
      LocationEvents.add_CurrentLocationChanged(new EventHandler<EventArgsCurrentLocationChanged>(this.Event_CurrentLocationChanged));
      TimeEvents.add_DayOfMonthChanged(new EventHandler<EventArgsIntChanged>(WonderfulFarmLife.WonderfulFarmLife.Event_DayOfMonthChanged));
      ControlEvents.add_MouseChanged(new EventHandler<EventArgsMouseStateChanged>(WonderfulFarmLife.WonderfulFarmLife.Event_MouseChanged));
      ControlEvents.add_ControllerButtonPressed(new EventHandler<EventArgsControllerButtonPressed>(WonderfulFarmLife.WonderfulFarmLife.Event_ControllerButtonPressed));
    }

    private void Event_UpdateTick(object sender, EventArgs e)
    {
      if (Game1.hasLoadedGame == 0 || Game1.currentLocation != Game1.getLocationFromName("Farm"))
        return;
      TileSheet tileSheet = ((Map) Game1.getLocationFromName("Farm").map).get_TileSheets()[Tile.getTileSheetIndex("untitled tile sheet", ((Map) Game1.getLocationFromName("Farm").map).get_TileSheets())];
      tileSheet.set_SheetSize(new Size((int) tileSheet.get_SheetSize().Width, tileSheet.get_SheetSize().Height + 44));
      List<ResourceClump> resourceClumps = (List<ResourceClump>) Game1.getFarm().resourceClumps;
      using (List<ResourceClump>.Enumerator enumerator = ((IEnumerable) Game1.getFarm().resourceClumps).OfType<ResourceClump>().ToList<ResourceClump>().GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          ResourceClump current = enumerator.Current;
          if (current.occupiesTile(71, 13) || current.occupiesTile(72, 13) || current.occupiesTile(71, 14) || current.occupiesTile(72, 14))
            resourceClumps.Remove(current);
        }
      }
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.ShowFarmStand)
      {
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.FarmStandEdits((GameLocation) Game1.getFarm()));
        ((GameLocation) Game1.getFarm()).setTileProperty(74, 15, "Buildings", "Action", "NewShippingBin");
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2058@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2083@Passable", new PropertyValue(true));
      }
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.EditPath)
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.PathEdits((GameLocation) Game1.getFarm()));
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.RemovePathAlltogether)
      {
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.PathEdits((GameLocation) Game1.getFarm()));
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.RemovePathEdits((GameLocation) Game1.getFarm()));
      }
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.RemoveShippingBin)
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.RemoveShippingBinEdits((GameLocation) Game1.getFarm()));
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.ShowPatio)
      {
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.PatioEdits((GameLocation) Game1.getFarm()));
        ((GameLocation) Game1.getFarm()).setTileProperty(68, 6, "Buildings", "Action", "kitchen");
        ((GameLocation) Game1.getFarm()).setTileProperty(69, 6, "Buildings", "Action", "kitchen");
      }
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.ShowBinClutter)
      {
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.YardGardenEditsAndBinClutter((GameLocation) Game1.getFarm()));
        ((GameLocation) Game1.getFarm()).setTileProperty(75, 4, "Buildings", "Action", "Jukebox");
        ((GameLocation) Game1.getFarm()).setTileProperty(75, 5, "Buildings", "Action", "Jukebox");
      }
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.AddDogHouse)
      {
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.DogHouseEdits((GameLocation) Game1.getFarm()));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2718@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2719@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2720@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2721@Passable", new PropertyValue(true));
      }
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.AddGreenHouseArch)
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.GreenHouseArchEdits((GameLocation) Game1.getFarm()));
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.ShowPicnicBlanket)
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.PicnicBlanketEdits((GameLocation) Game1.getFarm()));
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.ShowPicnicTable)
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.PicnicAreaTableEdits((GameLocation) Game1.getFarm()));
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.ShowTreeSwing)
      {
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.TreeSwingEdits((GameLocation) Game1.getFarm()));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2944@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2969@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2941@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2942@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2966@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2967@Passable", new PropertyValue(true));
      }
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.AddStoneBridge)
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.StoneBridgeEdits((GameLocation) Game1.getFarm()));
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.AddTelescopeArea)
      {
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.TelescopeEdits((GameLocation) Game1.getFarm()));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2619@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2620@Passable", new PropertyValue(true));
        ((IDictionary<string, PropertyValue>) ((Component) ((Map) ((GameLocation) Game1.getFarm()).map).GetTileSheet("untitled tile sheet")).get_Properties()).Add("@TileIndex@2621@Passable", new PropertyValue(true));
        ((GameLocation) Game1.getFarm()).setTileProperty(30, 2, "Buildings", "Action", "TelescopeMessage");
      }
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.ShowMemorialArea)
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.MemorialArea((GameLocation) Game1.getFarm()));
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.ShowMemorialAreaArch)
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.MemorialAreaArch((GameLocation) Game1.getFarm()));
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.UsingTSE)
        WonderfulFarmLife.WonderfulFarmLife.PatchMap((GameLocation) Game1.getFarm(), WonderfulFarmLife.WonderfulFarmLife.TegoFixes((GameLocation) Game1.getFarm()));
      GameEvents.remove_UpdateTick(new EventHandler(this.Event_UpdateTick));
    }

    private void Event_SecondUpdateTick(object sender, EventArgs e)
    {
      TileSheet tileSheet = ((Map) Game1.getLocationFromName("Farm").map).get_TileSheets()[Tile.getTileSheetIndex("untitled tile sheet", ((Map) Game1.getLocationFromName("Farm").map).get_TileSheets())];
      Dictionary<TileSheet, Texture2D> dictionary = (Dictionary<TileSheet, Texture2D>) typeof (XnaDisplayDevice).GetField("m_tileSheetTextures", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object) (Game1.mapDisplayDevice as XnaDisplayDevice));
      Texture2D targetTexture = dictionary[((Map) ((GameLocation) Game1.getFarm()).map).get_TileSheets()[Tile.getTileSheetIndex("untitled tile sheet", ((Map) ((GameLocation) Game1.getFarm()).map).get_TileSheets())]];
      int num = 1100;
      Dictionary<int, int> spriteOverrides = new Dictionary<int, int>();
      for (int key = 0; key < num; ++key)
        spriteOverrides.Add(key, 1975 + key);
      if (targetTexture != null)
        dictionary[((Map) ((GameLocation) Game1.getFarm()).map).get_TileSheets()[Tile.getTileSheetIndex("untitled tile sheet", ((Map) ((GameLocation) Game1.getFarm()).map).get_TileSheets())]] = WonderfulFarmLife.WonderfulFarmLife.PatchTexture(targetTexture, (string) Game1.currentSeason + "_wonderful.png", spriteOverrides, 16, 16);
      WonderfulFarmLife.WonderfulFarmLife.farmSheetPatched = true;
      GameEvents.remove_SecondUpdateTick(new EventHandler(this.Event_SecondUpdateTick));
    }

    private void Event_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
    {
      if (e.get_NewLocation() != Game1.getFarm())
        return;
      if (WonderfulFarmLife.WonderfulFarmLife.ModConfig.RemoveShippingBin)
        typeof (Farm).GetField("shippingBinLid", BindingFlags.Instance | BindingFlags.NonPublic).SetValue((object) Game1.getFarm(), (object) null);
      Dictionary<TileSheet, Texture2D> dictionary = (Dictionary<TileSheet, Texture2D>) typeof (XnaDisplayDevice).GetField("m_tileSheetTextures", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object) (Game1.mapDisplayDevice as XnaDisplayDevice));
      Texture2D targetTexture = dictionary[((Map) ((GameLocation) Game1.getFarm()).map).get_TileSheets()[Tile.getTileSheetIndex("untitled tile sheet", ((Map) ((GameLocation) Game1.getFarm()).map).get_TileSheets())]];
      int num = 1100;
      Dictionary<int, int> spriteOverrides = new Dictionary<int, int>();
      for (int key = 0; key < num; ++key)
        spriteOverrides.Add(key, 1975 + key);
      if (targetTexture != null)
        dictionary[((Map) ((GameLocation) Game1.getFarm()).map).get_TileSheets()[Tile.getTileSheetIndex("untitled tile sheet", ((Map) ((GameLocation) Game1.getFarm()).map).get_TileSheets())]] = WonderfulFarmLife.WonderfulFarmLife.PatchTexture(targetTexture, (string) Game1.currentSeason + "_wonderful.png", spriteOverrides, 16, 16);
      if (!WonderfulFarmLife.WonderfulFarmLife.farmSheetPatched)
        GameEvents.add_SecondUpdateTick(new EventHandler(this.Event_SecondUpdateTick));
    }

    private static void Event_DayOfMonthChanged(object sender, EventArgs e)
    {
      if (!WonderfulFarmLife.WonderfulFarmLife.PetBowlFilled)
        return;
      List<Pet> pets = WonderfulFarmLife.WonderfulFarmLife.findPets();
      if (pets == null)
        return;
      using (List<Pet>.Enumerator enumerator = pets.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          Pet current = enumerator.Current;
          current.friendshipTowardFarmer = (__Null) Math.Min(1000, current.friendshipTowardFarmer + 6);
        }
      }
      ((GameLocation) Game1.getFarm()).setMapTileIndex(52, 7, 2201, "Buildings");
      ((GameLocation) Game1.getFarm()).setMapTileIndex(53, 7, 2202, "Buildings");
      WonderfulFarmLife.WonderfulFarmLife.PetBowlFilled = false;
    }

    private static void Event_MouseChanged(object sender, EventArgsMouseStateChanged e)
    {
      if (Game1.hasLoadedGame == 0)
        return;
      if (e.get_NewState().RightButton == ButtonState.Pressed && e.get_PriorState().RightButton != ButtonState.Pressed)
        WonderfulFarmLife.WonderfulFarmLife.CheckForAction();
      if (e.get_NewState().LeftButton != ButtonState.Pressed || e.get_PriorState().LeftButton == ButtonState.Pressed)
        return;
      WonderfulFarmLife.WonderfulFarmLife.ChangeTileOnClick();
      WonderfulFarmLife.WonderfulFarmLife.CheckForAction();
    }

    private static void Event_ControllerButtonPressed(object sender, EventArgsControllerButtonPressed e)
    {
      if (Game1.hasLoadedGame == 0 || e.get_ButtonPressed() != Buttons.A)
        return;
      WonderfulFarmLife.WonderfulFarmLife.CheckForAction();
    }

    private static void ChangeTileOnClick()
    {
      if (!(((Farmer) Game1.player).get_CurrentTool() is WateringCan) || (((Farmer) Game1.player).get_CurrentTool() as WateringCan).get_WaterLeft() <= 0)
        return;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      Vector2 vector2 = new Vector2((float) (Game1.getOldMouseX() + ((Rectangle) @Game1.viewport).get_X()), (float) (Game1.getOldMouseY() + ((Rectangle) @Game1.viewport).get_Y())) / (float) Game1.tileSize;
      if (!Utility.tileWithinRadiusOfPlayer((int) vector2.X, (int) vector2.Y, 1, (Farmer) Game1.player))
        vector2 = ((Character) Game1.player).GetGrabTile();
      if (((GameLocation) Game1.getFarm()).getTileIndexAt((int) vector2.X, (int) vector2.Y, "Buildings") == 2201 || ((GameLocation) Game1.getFarm()).getTileIndexAt((int) vector2.X, (int) vector2.Y, "Buildings") == 2202)
      {
        ((GameLocation) Game1.getFarm()).setMapTileIndex(52, 7, 2204, "Buildings");
        ((GameLocation) Game1.getFarm()).setMapTileIndex(53, 7, 2205, "Buildings");
        WonderfulFarmLife.WonderfulFarmLife.PetBowlFilled = true;
      }
    }

    private static void CheckForAction()
    {
      if (((Farmer) Game1.player).get_UsingTool() || Game1.pickingTool != null || Game1.menuUp != null || (Game1.eventUp != null && ((Event) ((GameLocation) Game1.currentLocation).currentEvent).playerControlSequence == null || (Game1.nameSelectUp != null || Game1.numberOfSelectedItems != -1)) || Game1.fadeToBlack != null || Game1.activeClickableMenu != null)
        return;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      Vector2 vector2 = new Vector2((float) (Game1.getOldMouseX() + ((Rectangle) @Game1.viewport).get_X()), (float) (Game1.getOldMouseY() + ((Rectangle) @Game1.viewport).get_Y())) / (float) Game1.tileSize;
      if (!Utility.tileWithinRadiusOfPlayer((int) vector2.X, (int) vector2.Y, 1, (Farmer) Game1.player))
        vector2 = ((Character) Game1.player).GetGrabTile();
      // ISSUE: explicit reference operation
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      Tile tile = ((Map) ((GameLocation) Game1.currentLocation).map).GetLayer("Buildings").PickTile(new Location((int) vector2.X * Game1.tileSize, (int) vector2.Y * Game1.tileSize), (Size) (^(Rectangle&) @Game1.viewport).Size);
      PropertyValue propertyValue = (PropertyValue) null;
      if (tile != null)
        ((IDictionary<string, PropertyValue>) ((Component) tile).get_Properties()).TryGetValue("Action", out propertyValue);
      if (propertyValue != null)
      {
        if (PropertyValue.op_Implicit(propertyValue) == "NewShippingBin")
        {
          // ISSUE: method pointer
          // ISSUE: method pointer
          ItemGrabMenu itemGrabMenu = new ItemGrabMenu((List<Item>) null, true, false, new InventoryMenu.highlightThisItem((object) null, __methodptr(highlightShippableObjects)), new ItemGrabMenu.behaviorOnItemSelect((object) null, __methodptr(shipItem)), "", (ItemGrabMenu.behaviorOnItemSelect) null, true, true, false, true, false, 0);
          ((IClickableMenu) itemGrabMenu).initializeUpperRightCloseButton();
          itemGrabMenu.setBackgroundTransparency(false);
          itemGrabMenu.setDestroyItemOnClick(true);
          itemGrabMenu.initializeShippingBin();
          Game1.activeClickableMenu = (__Null) itemGrabMenu;
          Game1.playSound("shwip");
          if (((Character) Game1.player).facingDirection == 1)
            ((Character) Game1.player).Halt();
          ((Farmer) Game1.player).showCarrying();
        }
        if (PropertyValue.op_Implicit(propertyValue) == "TelescopeMessage")
        {
          Random random = new Random();
          List<string> stringList = new List<string>()
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

    private static void shipItem(Item i, Farmer who)
    {
      if (i == null)
        return;
      ((List<Item>) Game1.getFarm().shippingBin).Add(i);
      if (i is Object)
        DelayedAction.playSoundAfterDelay("Ship", 0);
      Game1.getFarm().lastItemShipped = (__Null) i;
      who.removeItemFromInventory(i);
      if (((Farmer) Game1.player).get_ActiveObject() == null)
      {
        ((Farmer) Game1.player).showNotCarrying();
        ((Character) Game1.player).Halt();
      }
    }

    private static List<Pet> findPets()
    {
      if (!((Farmer) Game1.player).hasPet())
        return (List<Pet>) null;
      List<Pet> petList = new List<Pet>();
      using (List<NPC>.Enumerator enumerator = ((List<NPC>) ((GameLocation) Game1.getFarm()).characters).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          NPC current = enumerator.Current;
          if (current is Pet)
            petList.Add(current as Pet);
        }
      }
      using (List<NPC>.Enumerator enumerator = ((List<NPC>) ((GameLocation) Utility.getHomeOfFarmer((Farmer) Game1.player)).characters).GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          NPC current = enumerator.Current;
          if (current is Pet)
            petList.Add(current as Pet);
        }
      }
      if (petList.Count < 1)
        return (List<Pet>) null;
      return petList;
    }

    private static List<Tile> PathEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 70, 15, 2232, tileSheetIndex, ""),
        new Tile(0, 71, 15, 2232, tileSheetIndex, ""),
        new Tile(0, 72, 15, 2232, tileSheetIndex, ""),
        new Tile(0, 73, 15, 2232, tileSheetIndex, ""),
        new Tile(0, 69, 16, 200, tileSheetIndex, ""),
        new Tile(0, 70, 16, 179, tileSheetIndex, ""),
        new Tile(0, 71, 16, 205, tileSheetIndex, ""),
        new Tile(0, 72, 16, 179, tileSheetIndex, ""),
        new Tile(0, 73, 16, 205, tileSheetIndex, ""),
        new Tile(0, 72, 17, 227, tileSheetIndex, ""),
        new Tile(0, 73, 17, 227, tileSheetIndex, ""),
        new Tile(0, 74, 17, 227, tileSheetIndex, ""),
        new Tile(0, 75, 17, 624, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> GreenHouseArchEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(4, 25, 10, 2626, tileSheetIndex, ""),
        new Tile(4, 26, 10, 2627, tileSheetIndex, ""),
        new Tile(4, 27, 10, 2628, tileSheetIndex, ""),
        new Tile(4, 28, 10, 2629, tileSheetIndex, ""),
        new Tile(4, 29, 10, 2630, tileSheetIndex, ""),
        new Tile(4, 30, 10, 2631, tileSheetIndex, ""),
        new Tile(4, 31, 10, 2632, tileSheetIndex, ""),
        new Tile(4, 32, 10, 2633, tileSheetIndex, ""),
        new Tile(4, 33, 10, 2635, tileSheetIndex, ""),
        new Tile(4, 24, 11, 2650, tileSheetIndex, ""),
        new Tile(4, 25, 11, 2651, tileSheetIndex, ""),
        new Tile(4, 26, 11, 2652, tileSheetIndex, ""),
        new Tile(4, 27, 11, 2653, tileSheetIndex, ""),
        new Tile(4, 28, 11, 2654, tileSheetIndex, ""),
        new Tile(4, 29, 11, 2655, tileSheetIndex, ""),
        new Tile(4, 30, 11, 2656, tileSheetIndex, ""),
        new Tile(4, 31, 11, 2657, tileSheetIndex, ""),
        new Tile(4, 32, 11, 2658, tileSheetIndex, ""),
        new Tile(4, 33, 11, 2659, tileSheetIndex, ""),
        new Tile(4, 24, 12, 2675, tileSheetIndex, ""),
        new Tile(4, 25, 12, 2676, tileSheetIndex, ""),
        new Tile(4, 26, 12, 2677, tileSheetIndex, ""),
        new Tile(4, 27, 12, 2678, tileSheetIndex, ""),
        new Tile(4, 28, 12, 2679, tileSheetIndex, ""),
        new Tile(4, 29, 12, 2680, tileSheetIndex, ""),
        new Tile(4, 30, 12, 2681, tileSheetIndex, ""),
        new Tile(4, 31, 12, 2682, tileSheetIndex, ""),
        new Tile(4, 32, 12, 2683, tileSheetIndex, ""),
        new Tile(4, 33, 12, 2684, tileSheetIndex, ""),
        new Tile(4, 24, 13, 2700, tileSheetIndex, ""),
        new Tile(4, 25, 13, 2701, tileSheetIndex, ""),
        new Tile(4, 26, 13, 2702, tileSheetIndex, ""),
        new Tile(4, 27, 13, 2703, tileSheetIndex, ""),
        new Tile(4, 28, 13, 2704, tileSheetIndex, ""),
        new Tile(4, 29, 13, 2705, tileSheetIndex, ""),
        new Tile(4, 30, 13, 2706, tileSheetIndex, ""),
        new Tile(4, 25, 14, 2726, tileSheetIndex, ""),
        new Tile(4, 26, 14, 2727, tileSheetIndex, ""),
        new Tile(4, 27, 14, 2728, tileSheetIndex, ""),
        new Tile(4, 28, 14, 2729, tileSheetIndex, ""),
        new Tile(4, 29, 14, 2730, tileSheetIndex, ""),
        new Tile(4, 30, 14, 2731, tileSheetIndex, ""),
        new Tile(3, 25, 15, 2751, tileSheetIndex, ""),
        new Tile(3, 26, 15, 2752, tileSheetIndex, ""),
        new Tile(3, 27, 15, 2753, tileSheetIndex, ""),
        new Tile(3, 28, 15, 2754, tileSheetIndex, ""),
        new Tile(3, 29, 15, 2755, tileSheetIndex, ""),
        new Tile(3, 30, 15, 2756, tileSheetIndex, ""),
        new Tile(1, 25, 16, 2776, tileSheetIndex, ""),
        new Tile(1, 26, 16, 2777, tileSheetIndex, ""),
        new Tile(1, 30, 16, 2781, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> TegoFixes(GameLocation gl)
    {
      Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(3, 58, 5, -1, -1, ""),
        new Tile(3, 59, 5, -1, -1, ""),
        new Tile(3, 60, 5, -1, -1, ""),
        new Tile(3, 61, 5, -1, -1, ""),
        new Tile(3, 62, 5, -1, -1, ""),
        new Tile(3, 63, 5, -1, -1, ""),
        new Tile(3, 64, 5, -1, -1, ""),
        new Tile(3, 65, 5, -1, -1, ""),
        new Tile(3, 66, 5, -1, -1, ""),
        new Tile(3, 67, 5, -1, -1, ""),
        new Tile(3, 72, 5, -1, -1, ""),
        new Tile(3, 77, 5, -1, -1, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> RemoveShippingBinEdits(GameLocation gl)
    {
      Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(3, 71, 13, -1, -1, ""),
        new Tile(3, 72, 13, -1, -1, ""),
        new Tile(1, 71, 14, -1, -1, ""),
        new Tile(1, 72, 14, -1, -1, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> RemovePathEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 75, 17, 227, tileSheetIndex, ""),
        new Tile(0, 76, 17, 227, tileSheetIndex, ""),
        new Tile(0, 77, 17, 227, tileSheetIndex, ""),
        new Tile(0, 78, 17, 227, tileSheetIndex, ""),
        new Tile(0, 79, 17, 227, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> TelescopeEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 16, 4, 312, tileSheetIndex, ""),
        new Tile(0, 17, 4, 313, tileSheetIndex, ""),
        new Tile(0, 18, 4, 314, tileSheetIndex, ""),
        new Tile(0, 16, 5, 337, tileSheetIndex, ""),
        new Tile(0, 17, 5, 338, tileSheetIndex, ""),
        new Tile(0, 18, 5, 339, tileSheetIndex, ""),
        new Tile(0, 16, 6, 598, tileSheetIndex, ""),
        new Tile(0, 17, 6, 599, tileSheetIndex, ""),
        new Tile(0, 18, 6, 587, tileSheetIndex, ""),
        new Tile(0, 16, 7, 587, tileSheetIndex, ""),
        new Tile(0, 17, 7, 587, tileSheetIndex, ""),
        new Tile(0, 18, 7, 226, tileSheetIndex, ""),
        new Tile(0, 16, 8, 587, tileSheetIndex, ""),
        new Tile(0, 17, 8, 587, tileSheetIndex, ""),
        new Tile(0, 18, 7, 587, tileSheetIndex, ""),
        new Tile(0, 15, 4, 312, tileSheetIndex, ""),
        new Tile(0, 15, 5, 337, tileSheetIndex, ""),
        new Tile(0, 15, 8, 565, tileSheetIndex, ""),
        new Tile(1, 15, 3, 444, tileSheetIndex, ""),
        new Tile(1, 16, 3, -1, -1, ""),
        new Tile(1, 17, 3, -1, -1, ""),
        new Tile(1, 18, 3, -1, -1, ""),
        new Tile(1, 15, 4, 469, tileSheetIndex, ""),
        new Tile(1, 16, 4, -1, -1, ""),
        new Tile(1, 17, 4, -1, -1, ""),
        new Tile(1, 18, 4, 416, tileSheetIndex, ""),
        new Tile(1, 15, 5, 494, tileSheetIndex, ""),
        new Tile(1, 16, 5, -1, -1, ""),
        new Tile(1, 17, 5, -1, -1, ""),
        new Tile(1, 18, 5, 441, tileSheetIndex, ""),
        new Tile(1, 15, 6, 519, tileSheetIndex, ""),
        new Tile(1, 16, 6, -1, -1, ""),
        new Tile(1, 17, 6, -1, -1, ""),
        new Tile(1, 18, 6, 466, tileSheetIndex, ""),
        new Tile(1, 15, 7, 540, tileSheetIndex, ""),
        new Tile(1, 16, 7, -1, -1, ""),
        new Tile(1, 17, 7, -1, -1, ""),
        new Tile(1, 18, 7, 491, tileSheetIndex, ""),
        new Tile(1, 15, 8, -1, -1, ""),
        new Tile(1, 18, 8, 516, tileSheetIndex, ""),
        new Tile(1, 18, 9, 541, tileSheetIndex, ""),
        new Tile(0, 32, 4, 467, tileSheetIndex, ""),
        new Tile(0, 33, 4, 468, tileSheetIndex, ""),
        new Tile(0, 32, 5, 492, tileSheetIndex, ""),
        new Tile(0, 33, 5, 493, tileSheetIndex, ""),
        new Tile(0, 32, 6, 346, tileSheetIndex, ""),
        new Tile(0, 32, 7, 695, tileSheetIndex, ""),
        new Tile(0, 18, 2, 277, tileSheetIndex, ""),
        new Tile(0, 19, 2, 277, tileSheetIndex, ""),
        new Tile(0, 20, 2, 278, tileSheetIndex, ""),
        new Tile(0, 21, 2, 377, tileSheetIndex, ""),
        new Tile(0, 21, 1, 352, tileSheetIndex, ""),
        new Tile(0, 21, 0, 352, tileSheetIndex, ""),
        new Tile(0, 19, 4, 402, tileSheetIndex, ""),
        new Tile(0, 19, 5, 402, tileSheetIndex, ""),
        new Tile(0, 20, 3, 200, tileSheetIndex, ""),
        new Tile(0, 21, 3, 201, tileSheetIndex, ""),
        new Tile(0, 22, 3, 201, tileSheetIndex, ""),
        new Tile(0, 23, 3, 201, tileSheetIndex, ""),
        new Tile(0, 24, 3, 203, tileSheetIndex, ""),
        new Tile(0, 20, 4, 225, tileSheetIndex, ""),
        new Tile(0, 21, 4, 226, tileSheetIndex, ""),
        new Tile(0, 22, 4, 227, tileSheetIndex, ""),
        new Tile(0, 23, 4, 488, tileSheetIndex, ""),
        new Tile(0, 24, 4, 228, tileSheetIndex, ""),
        new Tile(0, 20, 5, 250, tileSheetIndex, ""),
        new Tile(0, 21, 5, 251, tileSheetIndex, ""),
        new Tile(0, 22, 5, 251, tileSheetIndex, ""),
        new Tile(0, 23, 5, 251, tileSheetIndex, ""),
        new Tile(0, 24, 5, 253, tileSheetIndex, ""),
        new Tile(0, 22, 0, 175, tileSheetIndex, ""),
        new Tile(0, 23, 0, 175, tileSheetIndex, ""),
        new Tile(0, 24, 0, 175, tileSheetIndex, ""),
        new Tile(0, 25, 0, 151, tileSheetIndex, ""),
        new Tile(0, 26, 0, 150, tileSheetIndex, ""),
        new Tile(0, 22, 1, 175, tileSheetIndex, ""),
        new Tile(0, 23, 1, 175, tileSheetIndex, ""),
        new Tile(0, 24, 1, 175, tileSheetIndex, ""),
        new Tile(0, 25, 1, 175, tileSheetIndex, ""),
        new Tile(0, 26, 1, 175, tileSheetIndex, ""),
        new Tile(0, 22, 2, 175, tileSheetIndex, ""),
        new Tile(0, 23, 2, 175, tileSheetIndex, ""),
        new Tile(0, 24, 2, 151, tileSheetIndex, ""),
        new Tile(0, 25, 2, 175, tileSheetIndex, ""),
        new Tile(0, 26, 2, 175, tileSheetIndex, ""),
        new Tile(0, 27, 1, 200, tileSheetIndex, ""),
        new Tile(0, 28, 1, 201, tileSheetIndex, ""),
        new Tile(0, 29, 1, 201, tileSheetIndex, ""),
        new Tile(0, 30, 1, 201, tileSheetIndex, ""),
        new Tile(0, 31, 1, 201, tileSheetIndex, ""),
        new Tile(0, 32, 1, 203, tileSheetIndex, ""),
        new Tile(0, 27, 2, 225, tileSheetIndex, ""),
        new Tile(0, 28, 2, 1125, tileSheetIndex, ""),
        new Tile(0, 29, 2, 1126, tileSheetIndex, ""),
        new Tile(0, 30, 2, 1127, tileSheetIndex, ""),
        new Tile(0, 31, 2, 1128, tileSheetIndex, ""),
        new Tile(0, 32, 2, 228, tileSheetIndex, ""),
        new Tile(0, 27, 3, 225, tileSheetIndex, ""),
        new Tile(0, 28, 3, 1150, tileSheetIndex, ""),
        new Tile(0, 29, 3, 1151, tileSheetIndex, ""),
        new Tile(0, 30, 3, 1152, tileSheetIndex, ""),
        new Tile(0, 31, 3, 1153, tileSheetIndex, ""),
        new Tile(0, 32, 3, 228, tileSheetIndex, ""),
        new Tile(0, 27, 4, 225, tileSheetIndex, ""),
        new Tile(0, 28, 4, 1175, tileSheetIndex, ""),
        new Tile(0, 29, 4, 1176, tileSheetIndex, ""),
        new Tile(0, 30, 4, 1177, tileSheetIndex, ""),
        new Tile(0, 31, 4, 1178, tileSheetIndex, ""),
        new Tile(0, 27, 5, 250, tileSheetIndex, ""),
        new Tile(0, 28, 5, 251, tileSheetIndex, ""),
        new Tile(0, 29, 5, 251, tileSheetIndex, ""),
        new Tile(0, 30, 5, 251, tileSheetIndex, ""),
        new Tile(0, 31, 5, 230, tileSheetIndex, ""),
        new Tile(0, 27, 0, 175, tileSheetIndex, ""),
        new Tile(0, 28, 0, 175, tileSheetIndex, ""),
        new Tile(0, 29, 0, 175, tileSheetIndex, ""),
        new Tile(0, 30, 0, 175, tileSheetIndex, ""),
        new Tile(0, 31, 0, 175, tileSheetIndex, ""),
        new Tile(0, 32, 0, 175, tileSheetIndex, ""),
        new Tile(0, 33, 0, 175, tileSheetIndex, ""),
        new Tile(0, 34, 0, 175, tileSheetIndex, ""),
        new Tile(0, 25, 4, 175, tileSheetIndex, ""),
        new Tile(0, 26, 4, 175, tileSheetIndex, ""),
        new Tile(0, 25, 5, 175, tileSheetIndex, ""),
        new Tile(0, 26, 5, 175, tileSheetIndex, ""),
        new Tile(1, 32, 4, 419, tileSheetIndex, ""),
        new Tile(1, 32, 5, 444, tileSheetIndex, ""),
        new Tile(1, 19, 6, 467, tileSheetIndex, ""),
        new Tile(1, 20, 6, 467, tileSheetIndex, ""),
        new Tile(1, 21, 6, 468, tileSheetIndex, ""),
        new Tile(1, 22, 6, 467, tileSheetIndex, ""),
        new Tile(1, 23, 6, 468, tileSheetIndex, ""),
        new Tile(1, 24, 6, 467, tileSheetIndex, ""),
        new Tile(1, 25, 6, 468, tileSheetIndex, ""),
        new Tile(1, 26, 6, 446, tileSheetIndex, ""),
        new Tile(1, 27, 6, 468, tileSheetIndex, ""),
        new Tile(1, 28, 6, 467, tileSheetIndex, ""),
        new Tile(1, 29, 6, 468, tileSheetIndex, ""),
        new Tile(1, 30, 6, 467, tileSheetIndex, ""),
        new Tile(1, 31, 6, 468, tileSheetIndex, ""),
        new Tile(1, 32, 6, 469, tileSheetIndex, ""),
        new Tile(1, 19, 7, 492, tileSheetIndex, ""),
        new Tile(1, 20, 7, 493, tileSheetIndex, ""),
        new Tile(1, 21, 7, 493, tileSheetIndex, ""),
        new Tile(1, 22, 7, 492, tileSheetIndex, ""),
        new Tile(1, 23, 7, 493, tileSheetIndex, ""),
        new Tile(1, 24, 7, 492, tileSheetIndex, ""),
        new Tile(1, 25, 7, 493, tileSheetIndex, ""),
        new Tile(1, 26, 7, 492, tileSheetIndex, ""),
        new Tile(1, 27, 7, 493, tileSheetIndex, ""),
        new Tile(1, 28, 7, 492, tileSheetIndex, ""),
        new Tile(1, 29, 7, 493, tileSheetIndex, ""),
        new Tile(1, 30, 7, 492, tileSheetIndex, ""),
        new Tile(1, 31, 7, 493, tileSheetIndex, ""),
        new Tile(1, 32, 7, 494, tileSheetIndex, ""),
        new Tile(1, 19, 8, 517, tileSheetIndex, ""),
        new Tile(1, 20, 8, 518, tileSheetIndex, ""),
        new Tile(1, 21, 8, 518, tileSheetIndex, ""),
        new Tile(1, 22, 8, 517, tileSheetIndex, ""),
        new Tile(1, 23, 8, 518, tileSheetIndex, ""),
        new Tile(1, 24, 8, 517, tileSheetIndex, ""),
        new Tile(1, 25, 8, 518, tileSheetIndex, ""),
        new Tile(1, 26, 8, 517, tileSheetIndex, ""),
        new Tile(1, 27, 8, 518, tileSheetIndex, ""),
        new Tile(1, 28, 8, 448, tileSheetIndex, ""),
        new Tile(1, 29, 8, 518, tileSheetIndex, ""),
        new Tile(1, 30, 8, 517, tileSheetIndex, ""),
        new Tile(1, 31, 8, 518, tileSheetIndex, ""),
        new Tile(1, 32, 8, 519, tileSheetIndex, ""),
        new Tile(1, 19, 9, 542, tileSheetIndex, ""),
        new Tile(1, 20, 9, 543, tileSheetIndex, ""),
        new Tile(1, 21, 9, 542, tileSheetIndex, ""),
        new Tile(1, 22, 9, 543, tileSheetIndex, ""),
        new Tile(1, 23, 9, 542, tileSheetIndex, ""),
        new Tile(1, 24, 9, 543, tileSheetIndex, ""),
        new Tile(1, 25, 9, 542, tileSheetIndex, ""),
        new Tile(1, 26, 9, 543, tileSheetIndex, ""),
        new Tile(1, 27, 9, 542, tileSheetIndex, ""),
        new Tile(1, 28, 9, 543, tileSheetIndex, ""),
        new Tile(1, 29, 9, 542, tileSheetIndex, ""),
        new Tile(1, 30, 9, 542, tileSheetIndex, ""),
        new Tile(1, 31, 9, 543, tileSheetIndex, ""),
        new Tile(1, 32, 9, 544, tileSheetIndex, ""),
        new Tile(1, 17, 0, -1, -1, ""),
        new Tile(1, 18, 0, -1, -1, ""),
        new Tile(1, 19, 0, -1, -1, ""),
        new Tile(1, 17, 1, -1, -1, ""),
        new Tile(1, 18, 1, -1, -1, ""),
        new Tile(1, 19, 1, -1, -1, ""),
        new Tile(1, 18, 2, -1, -1, ""),
        new Tile(1, 20, 0, -1, -1, ""),
        new Tile(1, 21, 0, -1, -1, ""),
        new Tile(1, 20, 1, -1, -1, ""),
        new Tile(1, 21, 1, -1, -1, ""),
        new Tile(1, 20, 2, -1, -1, ""),
        new Tile(1, 21, 2, -1, -1, ""),
        new Tile(1, 17, 2, -1, -1, ""),
        new Tile(1, 19, 2, -1, -1, ""),
        new Tile(1, 19, 3, -1, -1, ""),
        new Tile(1, 22, 0, -1, -1, ""),
        new Tile(1, 22, 1, -1, -1, ""),
        new Tile(1, 22, 2, -1, -1, ""),
        new Tile(1, 23, 1, -1, -1, ""),
        new Tile(1, 23, 2, -1, -1, ""),
        new Tile(1, 25, 1, -1, -1, ""),
        new Tile(1, 25, 2, -1, -1, ""),
        new Tile(1, 26, 1, -1, -1, ""),
        new Tile(1, 30, 3, -1, -1, ""),
        new Tile(1, 32, 2, -1, -1, ""),
        new Tile(1, 32, 3, -1, -1, ""),
        new Tile(1, 33, 0, -1, -1, ""),
        new Tile(1, 33, 1, -1, -1, ""),
        new Tile(1, 19, 4, -1, -1, ""),
        new Tile(1, 25, 4, -1, -1, ""),
        new Tile(1, 26, 4, -1, -1, ""),
        new Tile(1, 27, 4, -1, -1, ""),
        new Tile(1, 28, 4, -1, -1, ""),
        new Tile(1, 29, 4, -1, -1, ""),
        new Tile(1, 30, 4, -1, -1, ""),
        new Tile(1, 31, 4, -1, -1, ""),
        new Tile(1, 19, 5, -1, -1, ""),
        new Tile(1, 20, 5, -1, -1, ""),
        new Tile(1, 21, 5, -1, -1, ""),
        new Tile(1, 22, 5, -1, -1, ""),
        new Tile(1, 23, 5, -1, -1, ""),
        new Tile(1, 24, 5, -1, -1, ""),
        new Tile(1, 25, 5, -1, -1, ""),
        new Tile(1, 26, 5, -1, -1, ""),
        new Tile(1, 27, 5, -1, -1, ""),
        new Tile(1, 28, 5, -1, -1, ""),
        new Tile(1, 29, 5, -1, -1, ""),
        new Tile(1, 30, 5, -1, -1, ""),
        new Tile(1, 31, 5, -1, -1, ""),
        new Tile(1, 23, 0, -1, -1, ""),
        new Tile(1, 24, 0, -1, -1, ""),
        new Tile(1, 25, 0, -1, -1, ""),
        new Tile(1, 24, 1, -1, -1, ""),
        new Tile(1, 24, 2, -1, -1, ""),
        new Tile(1, 27, 0, -1, -1, ""),
        new Tile(1, 28, 0, -1, -1, ""),
        new Tile(3, 29, 0, -1, -1, ""),
        new Tile(1, 28, 1, -1, -1, ""),
        new Tile(1, 28, 2, -1, -1, ""),
        new Tile(1, 29, 0, -1, -1, ""),
        new Tile(1, 30, 0, -1, -1, ""),
        new Tile(1, 29, 1, -1, -1, ""),
        new Tile(1, 30, 1, -1, -1, ""),
        new Tile(3, 30, 0, -1, -1, ""),
        new Tile(3, 30, 1, -1, -1, ""),
        new Tile(1, 31, 0, -1, -1, ""),
        new Tile(1, 32, 0, -1, -1, ""),
        new Tile(1, 31, 1, -1, -1, ""),
        new Tile(1, 32, 1, -1, -1, ""),
        new Tile(1, 31, 2, -1, -1, ""),
        new Tile(1, 31, 3, -1, -1, ""),
        new Tile(1, 21, 3, 2594, tileSheetIndex, ""),
        new Tile(1, 22, 3, 2595, tileSheetIndex, ""),
        new Tile(1, 23, 3, 2596, tileSheetIndex, ""),
        new Tile(1, 20, 4, 2618, tileSheetIndex, ""),
        new Tile(1, 21, 4, 2619, tileSheetIndex, ""),
        new Tile(1, 22, 4, 2620, tileSheetIndex, ""),
        new Tile(1, 23, 4, 2621, tileSheetIndex, ""),
        new Tile(1, 24, 4, 2622, tileSheetIndex, ""),
        new Tile(3, 20, 0, 2518, tileSheetIndex, ""),
        new Tile(3, 21, 0, 2519, tileSheetIndex, ""),
        new Tile(3, 22, 0, 2520, tileSheetIndex, ""),
        new Tile(3, 23, 0, 2521, tileSheetIndex, ""),
        new Tile(3, 24, 0, 2522, tileSheetIndex, ""),
        new Tile(3, 20, 1, 2543, tileSheetIndex, ""),
        new Tile(3, 21, 1, 2544, tileSheetIndex, ""),
        new Tile(3, 22, 1, 2545, tileSheetIndex, ""),
        new Tile(3, 23, 1, 2546, tileSheetIndex, ""),
        new Tile(3, 24, 1, 2547, tileSheetIndex, ""),
        new Tile(3, 20, 2, 2568, tileSheetIndex, ""),
        new Tile(3, 21, 2, 2569, tileSheetIndex, ""),
        new Tile(3, 22, 2, 2570, tileSheetIndex, ""),
        new Tile(3, 23, 2, 2571, tileSheetIndex, ""),
        new Tile(3, 24, 2, 2572, tileSheetIndex, ""),
        new Tile(3, 20, 3, 2593, tileSheetIndex, ""),
        new Tile(3, 24, 3, 2597, tileSheetIndex, ""),
        new Tile(3, 30, 1, 2266, tileSheetIndex, ""),
        new Tile(1, 30, 2, 2291, tileSheetIndex, ""),
        new Tile(1, 30, 5, 117, tileSheetIndex, ""),
        new Tile(1, 31, 5, 113, tileSheetIndex, ""),
        new Tile(3, 31, 4, 88, tileSheetIndex, ""),
        new Tile(3, 32, 4, 89, tileSheetIndex, ""),
        new Tile(3, 32, 5, 114, tileSheetIndex, ""),
        new Tile(1, 33, 3, 21, tileSheetIndex, ""),
        new Tile(1, 17, 0, 307, tileSheetIndex, ""),
        new Tile(1, 18, 0, 257, tileSheetIndex, ""),
        new Tile(1, 19, 0, 21, tileSheetIndex, ""),
        new Tile(1, 20, 0, 126, tileSheetIndex, ""),
        new Tile(1, 21, 0, 21, tileSheetIndex, ""),
        new Tile(1, 22, 0, 21, tileSheetIndex, ""),
        new Tile(1, 23, 0, 21, tileSheetIndex, ""),
        new Tile(1, 24, 0, 307, tileSheetIndex, ""),
        new Tile(1, 25, 0, 21, tileSheetIndex, ""),
        new Tile(1, 27, 0, 21, tileSheetIndex, ""),
        new Tile(1, 28, 0, 21, tileSheetIndex, ""),
        new Tile(1, 29, 0, 63, tileSheetIndex, ""),
        new Tile(1, 30, 0, 64, tileSheetIndex, ""),
        new Tile(1, 31, 0, 65, tileSheetIndex, ""),
        new Tile(1, 32, 0, 257, tileSheetIndex, ""),
        new Tile(1, 33, 1, 21, tileSheetIndex, ""),
        new Tile(1, 33, 2, 21, tileSheetIndex, ""),
        new Tile(1, 12, 2, 21, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> MemorialArea(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 3, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 4, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 5, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 6, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 7, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 9, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 10, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 11, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 12, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 13, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 14, 7, 2283, tileSheetIndex, ""),
        new Tile(0, 3, 8, 2300, tileSheetIndex, ""),
        new Tile(0, 4, 8, 2301, tileSheetIndex, ""),
        new Tile(0, 5, 8, 2302, tileSheetIndex, ""),
        new Tile(0, 6, 8, 2283, tileSheetIndex, ""),
        new Tile(0, 10, 8, 2400, tileSheetIndex, ""),
        new Tile(0, 11, 8, 2401, tileSheetIndex, ""),
        new Tile(0, 12, 8, 2401, tileSheetIndex, ""),
        new Tile(0, 13, 8, 2401, tileSheetIndex, ""),
        new Tile(0, 14, 8, 2280, tileSheetIndex, ""),
        new Tile(0, 15, 8, 838, tileSheetIndex, ""),
        new Tile(0, 3, 9, 2331, tileSheetIndex, ""),
        new Tile(0, 4, 9, 153, tileSheetIndex, ""),
        new Tile(0, 5, 9, 2327, tileSheetIndex, ""),
        new Tile(0, 6, 9, 2283, tileSheetIndex, ""),
        new Tile(0, 7, 9, 2283, tileSheetIndex, ""),
        new Tile(0, 8, 9, 2283, tileSheetIndex, ""),
        new Tile(0, 9, 9, 2283, tileSheetIndex, ""),
        new Tile(0, 10, 9, 2303, tileSheetIndex, ""),
        new Tile(0, 11, 9, 226, tileSheetIndex, ""),
        new Tile(0, 12, 9, 227, tileSheetIndex, ""),
        new Tile(0, 13, 9, 227, tileSheetIndex, ""),
        new Tile(0, 14, 9, 2305, tileSheetIndex, ""),
        new Tile(0, 15, 9, 812, tileSheetIndex, ""),
        new Tile(0, 19, 9, 2283, tileSheetIndex, ""),
        new Tile(0, 20, 9, 2283, tileSheetIndex, ""),
        new Tile(0, 21, 9, 2283, tileSheetIndex, ""),
        new Tile(0, 22, 9, 2283, tileSheetIndex, ""),
        new Tile(0, 3, 10, 2350, tileSheetIndex, ""),
        new Tile(0, 4, 10, 2351, tileSheetIndex, ""),
        new Tile(0, 5, 10, 2352, tileSheetIndex, ""),
        new Tile(0, 6, 10, 2283, tileSheetIndex, ""),
        new Tile(0, 7, 10, 2283, tileSheetIndex, ""),
        new Tile(0, 8, 10, 2283, tileSheetIndex, ""),
        new Tile(0, 9, 10, 2283, tileSheetIndex, ""),
        new Tile(0, 10, 10, 2328, tileSheetIndex, ""),
        new Tile(0, 11, 10, 226, tileSheetIndex, ""),
        new Tile(0, 12, 10, 227, tileSheetIndex, ""),
        new Tile(0, 13, 10, 227, tileSheetIndex, ""),
        new Tile(0, 14, 10, 2330, tileSheetIndex, ""),
        new Tile(0, 19, 10, 2300, tileSheetIndex, ""),
        new Tile(0, 20, 10, 2301, tileSheetIndex, ""),
        new Tile(0, 21, 10, 2301, tileSheetIndex, ""),
        new Tile(0, 22, 10, 2302, tileSheetIndex, ""),
        new Tile(0, 3, 11, 2300, tileSheetIndex, ""),
        new Tile(0, 4, 11, 2301, tileSheetIndex, ""),
        new Tile(0, 5, 11, 2302, tileSheetIndex, ""),
        new Tile(0, 6, 11, 2283, tileSheetIndex, ""),
        new Tile(0, 7, 11, 2283, tileSheetIndex, ""),
        new Tile(0, 8, 11, 2283, tileSheetIndex, ""),
        new Tile(0, 9, 11, 2283, -1, ""),
        new Tile(0, 10, 11, 2353, -1, ""),
        new Tile(0, 11, 11, 226, tileSheetIndex, ""),
        new Tile(0, 12, 11, 227, tileSheetIndex, ""),
        new Tile(0, 13, 11, 227, tileSheetIndex, ""),
        new Tile(0, 14, 11, 2355, tileSheetIndex, ""),
        new Tile(0, 19, 11, 2303, tileSheetIndex, ""),
        new Tile(0, 20, 11, 226, tileSheetIndex, ""),
        new Tile(0, 21, 11, 227, tileSheetIndex, ""),
        new Tile(0, 22, 11, 2305, tileSheetIndex, ""),
        new Tile(0, 3, 12, 2475, tileSheetIndex, ""),
        new Tile(0, 4, 12, 227, tileSheetIndex, ""),
        new Tile(0, 5, 12, 2327, tileSheetIndex, ""),
        new Tile(0, 6, 12, 2283, tileSheetIndex, ""),
        new Tile(0, 7, 12, 2283, tileSheetIndex, ""),
        new Tile(0, 8, 12, 2283, tileSheetIndex, ""),
        new Tile(0, 9, 12, 2283, -1, ""),
        new Tile(0, 10, 12, 2378, -1, ""),
        new Tile(0, 11, 12, 2379, -1, ""),
        new Tile(0, 12, 12, 2379, -1, ""),
        new Tile(0, 13, 12, 2379, -1, ""),
        new Tile(0, 14, 12, 2380, -1, ""),
        new Tile(0, 19, 12, 2328, -1, ""),
        new Tile(0, 20, 12, 226, -1, ""),
        new Tile(0, 21, 12, 227, -1, ""),
        new Tile(0, 22, 12, 2330, -1, ""),
        new Tile(0, 3, 13, 2350, tileSheetIndex, ""),
        new Tile(0, 4, 13, 2351, tileSheetIndex, ""),
        new Tile(0, 5, 13, 2352, tileSheetIndex, ""),
        new Tile(0, 6, 13, 2283, tileSheetIndex, ""),
        new Tile(0, 7, 13, 2283, tileSheetIndex, ""),
        new Tile(0, 8, 13, 2283, tileSheetIndex, ""),
        new Tile(0, 9, 13, 2283, tileSheetIndex, ""),
        new Tile(0, 10, 13, 2278, tileSheetIndex, ""),
        new Tile(0, 11, 13, 2301, tileSheetIndex, ""),
        new Tile(0, 12, 13, 2301, tileSheetIndex, ""),
        new Tile(0, 13, 13, 2301, tileSheetIndex, ""),
        new Tile(0, 14, 13, 2302, tileSheetIndex, ""),
        new Tile(0, 19, 13, 2328, -1, ""),
        new Tile(0, 20, 13, 226, -1, ""),
        new Tile(0, 21, 13, 227, -1, ""),
        new Tile(0, 22, 13, 2330, -1, ""),
        new Tile(0, 3, 14, 2300, tileSheetIndex, ""),
        new Tile(0, 4, 14, 2301, tileSheetIndex, ""),
        new Tile(0, 5, 14, 2302, tileSheetIndex, ""),
        new Tile(0, 6, 14, 2283, tileSheetIndex, ""),
        new Tile(0, 7, 14, 2283, tileSheetIndex, ""),
        new Tile(0, 8, 14, 2283, tileSheetIndex, ""),
        new Tile(0, 9, 14, 2283, -1, ""),
        new Tile(0, 10, 14, 2353, -1, ""),
        new Tile(0, 11, 14, 226, tileSheetIndex, ""),
        new Tile(0, 12, 14, 227, tileSheetIndex, ""),
        new Tile(0, 13, 14, 227, tileSheetIndex, ""),
        new Tile(0, 14, 14, 2355, tileSheetIndex, ""),
        new Tile(0, 19, 14, 2328, -1, ""),
        new Tile(0, 20, 14, 226, -1, ""),
        new Tile(0, 21, 14, 227, -1, ""),
        new Tile(0, 22, 14, 2330, -1, ""),
        new Tile(0, 3, 15, 2475, tileSheetIndex, ""),
        new Tile(0, 4, 15, 227, tileSheetIndex, ""),
        new Tile(0, 5, 15, 2327, tileSheetIndex, ""),
        new Tile(0, 6, 15, 2283, tileSheetIndex, ""),
        new Tile(0, 7, 15, 2283, tileSheetIndex, ""),
        new Tile(0, 8, 15, 2283, tileSheetIndex, ""),
        new Tile(0, 9, 15, 2283, -1, ""),
        new Tile(0, 10, 15, 2353, -1, ""),
        new Tile(0, 11, 15, 226, tileSheetIndex, ""),
        new Tile(0, 12, 15, 227, tileSheetIndex, ""),
        new Tile(0, 13, 15, 227, tileSheetIndex, ""),
        new Tile(0, 14, 15, 2355, tileSheetIndex, ""),
        new Tile(0, 19, 15, 2328, -1, ""),
        new Tile(0, 20, 15, 226, -1, ""),
        new Tile(0, 21, 15, 227, -1, ""),
        new Tile(0, 22, 15, 2330, -1, ""),
        new Tile(0, 3, 16, 2350, tileSheetIndex, ""),
        new Tile(0, 4, 16, 2351, tileSheetIndex, ""),
        new Tile(0, 5, 16, 2352, tileSheetIndex, ""),
        new Tile(0, 6, 16, 2283, tileSheetIndex, ""),
        new Tile(0, 7, 16, 2283, tileSheetIndex, ""),
        new Tile(0, 8, 16, 2283, tileSheetIndex, ""),
        new Tile(0, 9, 16, 2283, tileSheetIndex, ""),
        new Tile(0, 10, 16, 2378, -1, ""),
        new Tile(0, 11, 16, 2379, -1, ""),
        new Tile(0, 12, 16, 2379, -1, ""),
        new Tile(0, 13, 16, 2379, -1, ""),
        new Tile(0, 14, 16, 2380, -1, ""),
        new Tile(0, 19, 16, 2475, -1, ""),
        new Tile(0, 20, 16, 226, -1, ""),
        new Tile(0, 21, 16, 227, -1, ""),
        new Tile(0, 22, 16, 2330, -1, ""),
        new Tile(0, 3, 17, 2300, tileSheetIndex, ""),
        new Tile(0, 4, 17, 2301, tileSheetIndex, ""),
        new Tile(0, 5, 17, 2302, tileSheetIndex, ""),
        new Tile(0, 6, 17, 2300, tileSheetIndex, ""),
        new Tile(0, 7, 17, 2301, tileSheetIndex, ""),
        new Tile(0, 8, 17, 2302, tileSheetIndex, ""),
        new Tile(0, 9, 17, 2300, tileSheetIndex, ""),
        new Tile(0, 10, 17, 2301, tileSheetIndex, ""),
        new Tile(0, 11, 17, 2302, tileSheetIndex, ""),
        new Tile(0, 12, 17, 2300, tileSheetIndex, ""),
        new Tile(0, 13, 17, 2301, tileSheetIndex, ""),
        new Tile(0, 14, 17, 2302, tileSheetIndex, ""),
        new Tile(0, 19, 17, 2350, -1, ""),
        new Tile(0, 20, 17, 2351, -1, ""),
        new Tile(0, 21, 17, 2351, -1, ""),
        new Tile(0, 22, 17, 2352, -1, ""),
        new Tile(0, 3, 18, 2325, tileSheetIndex, ""),
        new Tile(0, 4, 18, 227, tileSheetIndex, ""),
        new Tile(0, 5, 18, 2327, tileSheetIndex, ""),
        new Tile(0, 6, 18, 2325, tileSheetIndex, ""),
        new Tile(0, 7, 18, 227, tileSheetIndex, ""),
        new Tile(0, 8, 18, 2327, tileSheetIndex, ""),
        new Tile(0, 9, 18, 2325, tileSheetIndex, ""),
        new Tile(0, 10, 18, 227, tileSheetIndex, ""),
        new Tile(0, 11, 18, 2327, tileSheetIndex, ""),
        new Tile(0, 12, 18, 2325, tileSheetIndex, ""),
        new Tile(0, 13, 18, 227, tileSheetIndex, ""),
        new Tile(0, 14, 18, 2327, tileSheetIndex, ""),
        new Tile(0, 19, 18, 2283, -1, ""),
        new Tile(0, 20, 18, 2283, -1, ""),
        new Tile(0, 21, 18, 2283, -1, ""),
        new Tile(0, 22, 18, 2283, -1, ""),
        new Tile(0, 3, 19, 2350, tileSheetIndex, ""),
        new Tile(0, 4, 19, 2351, tileSheetIndex, ""),
        new Tile(0, 5, 19, 2352, tileSheetIndex, ""),
        new Tile(0, 6, 19, 2350, tileSheetIndex, ""),
        new Tile(0, 7, 19, 2351, tileSheetIndex, ""),
        new Tile(0, 8, 19, 2352, tileSheetIndex, ""),
        new Tile(0, 9, 19, 2350, tileSheetIndex, ""),
        new Tile(0, 10, 19, 2351, tileSheetIndex, ""),
        new Tile(0, 11, 19, 2352, tileSheetIndex, ""),
        new Tile(0, 12, 19, 2350, tileSheetIndex, ""),
        new Tile(0, 13, 19, 2351, tileSheetIndex, ""),
        new Tile(0, 14, 19, 2352, tileSheetIndex, ""),
        new Tile(0, 19, 19, 2283, -1, ""),
        new Tile(0, 20, 19, 2283, -1, ""),
        new Tile(0, 21, 19, 2283, -1, ""),
        new Tile(0, 22, 19, 2283, -1, ""),
        new Tile(0, 3, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 4, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 5, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 6, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 7, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 8, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 9, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 10, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 11, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 12, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 13, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 14, 20, 2715, tileSheetIndex, ""),
        new Tile(0, 15, 20, 838, tileSheetIndex, ""),
        new Tile(0, 18, 20, 838, tileSheetIndex, ""),
        new Tile(0, 19, 20, 2715, -1, ""),
        new Tile(0, 20, 20, 2715, -1, ""),
        new Tile(0, 21, 20, 2715, -1, ""),
        new Tile(0, 22, 20, 2715, -1, ""),
        new Tile(0, 23, 20, 2715, -1, ""),
        new Tile(1, 3, 19, 2494, tileSheetIndex, ""),
        new Tile(1, 4, 19, 2495, tileSheetIndex, ""),
        new Tile(1, 5, 19, 2496, tileSheetIndex, ""),
        new Tile(1, 6, 19, 2494, tileSheetIndex, ""),
        new Tile(1, 7, 19, 2495, tileSheetIndex, ""),
        new Tile(1, 8, 19, 2496, tileSheetIndex, ""),
        new Tile(1, 9, 19, 2494, tileSheetIndex, ""),
        new Tile(1, 10, 19, 2495, tileSheetIndex, ""),
        new Tile(1, 11, 19, 2496, tileSheetIndex, ""),
        new Tile(1, 12, 19, 2494, tileSheetIndex, ""),
        new Tile(1, 13, 19, 2495, tileSheetIndex, ""),
        new Tile(1, 14, 19, 2496, tileSheetIndex, ""),
        new Tile(1, 15, 19, 2496, tileSheetIndex, ""),
        new Tile(1, 18, 19, 2494, tileSheetIndex, ""),
        new Tile(1, 19, 19, 2495, tileSheetIndex, ""),
        new Tile(1, 20, 19, 2496, tileSheetIndex, ""),
        new Tile(1, 21, 19, 2494, tileSheetIndex, ""),
        new Tile(1, 22, 19, 2495, tileSheetIndex, ""),
        new Tile(1, 23, 19, 2496, tileSheetIndex, ""),
        new Tile(3, 3, 18, 2419, tileSheetIndex, ""),
        new Tile(3, 4, 18, 2420, tileSheetIndex, ""),
        new Tile(3, 5, 18, 2421, tileSheetIndex, ""),
        new Tile(3, 6, 18, 2419, tileSheetIndex, ""),
        new Tile(3, 7, 18, 2420, tileSheetIndex, ""),
        new Tile(3, 8, 18, 2421, tileSheetIndex, ""),
        new Tile(3, 9, 18, 2419, tileSheetIndex, ""),
        new Tile(3, 10, 18, 2420, tileSheetIndex, ""),
        new Tile(3, 11, 18, 2421, tileSheetIndex, ""),
        new Tile(3, 12, 18, 2419, tileSheetIndex, ""),
        new Tile(3, 13, 18, 2420, tileSheetIndex, ""),
        new Tile(3, 14, 18, 2421, tileSheetIndex, ""),
        new Tile(3, 15, 18, 2421, tileSheetIndex, ""),
        new Tile(3, 18, 18, 2419, tileSheetIndex, ""),
        new Tile(3, 19, 18, 2420, tileSheetIndex, ""),
        new Tile(3, 20, 18, 2421, tileSheetIndex, ""),
        new Tile(3, 21, 18, 2419, tileSheetIndex, ""),
        new Tile(3, 22, 18, 2420, tileSheetIndex, ""),
        new Tile(3, 23, 18, 2421, tileSheetIndex, ""),
        new Tile(3, 15, 7, 2419, tileSheetIndex, ""),
        new Tile(1, 15, 8, 2444, tileSheetIndex, ""),
        new Tile(1, 15, 9, 2469, tileSheetIndex, ""),
        new Tile(1, 15, 10, 2419, tileSheetIndex, ""),
        new Tile(1, 15, 11, 2444, tileSheetIndex, ""),
        new Tile(1, 15, 11, 2469, tileSheetIndex, ""),
        new Tile(1, 15, 12, 2494, tileSheetIndex, ""),
        new Tile(0, 15, 7, 812, tileSheetIndex, ""),
        new Tile(0, 15, 8, 812, tileSheetIndex, ""),
        new Tile(0, 15, 9, 812, tileSheetIndex, ""),
        new Tile(0, 15, 10, 812, tileSheetIndex, ""),
        new Tile(0, 15, 11, 812, tileSheetIndex, ""),
        new Tile(0, 15, 11, 812, tileSheetIndex, ""),
        new Tile(0, 15, 12, 812, tileSheetIndex, ""),
        new Tile(0, 15, 13, 812, tileSheetIndex, ""),
        new Tile(0, 15, 14, 812, tileSheetIndex, ""),
        new Tile(0, 15, 15, 812, tileSheetIndex, ""),
        new Tile(0, 15, 16, 812, tileSheetIndex, ""),
        new Tile(0, 15, 17, 812, tileSheetIndex, ""),
        new Tile(0, 15, 18, 812, tileSheetIndex, ""),
        new Tile(0, 15, 19, 812, tileSheetIndex, ""),
        new Tile(0, 18, 9, 812, tileSheetIndex, ""),
        new Tile(0, 18, 10, 812, tileSheetIndex, ""),
        new Tile(0, 18, 11, 812, tileSheetIndex, ""),
        new Tile(0, 18, 11, 812, tileSheetIndex, ""),
        new Tile(0, 18, 12, 812, tileSheetIndex, ""),
        new Tile(0, 18, 13, 812, tileSheetIndex, ""),
        new Tile(0, 18, 14, 812, tileSheetIndex, ""),
        new Tile(0, 18, 15, 812, tileSheetIndex, ""),
        new Tile(0, 18, 16, 812, tileSheetIndex, ""),
        new Tile(0, 18, 17, 812, tileSheetIndex, ""),
        new Tile(0, 18, 18, 812, tileSheetIndex, ""),
        new Tile(0, 18, 19, 812, tileSheetIndex, ""),
        new Tile(0, 23, 9, 812, tileSheetIndex, ""),
        new Tile(0, 23, 10, 812, tileSheetIndex, ""),
        new Tile(0, 23, 11, 812, tileSheetIndex, ""),
        new Tile(0, 23, 11, 812, tileSheetIndex, ""),
        new Tile(0, 23, 12, 812, tileSheetIndex, ""),
        new Tile(0, 23, 13, 812, tileSheetIndex, ""),
        new Tile(0, 23, 14, 812, tileSheetIndex, ""),
        new Tile(0, 23, 15, 812, tileSheetIndex, ""),
        new Tile(0, 23, 16, 812, tileSheetIndex, ""),
        new Tile(0, 23, 17, 812, tileSheetIndex, ""),
        new Tile(0, 23, 18, 812, tileSheetIndex, ""),
        new Tile(0, 23, 19, 812, tileSheetIndex, ""),
        new Tile(3, 18, 9, 2421, tileSheetIndex, ""),
        new Tile(1, 18, 10, 2446, tileSheetIndex, ""),
        new Tile(1, 18, 11, 2471, tileSheetIndex, ""),
        new Tile(1, 18, 12, 2471, tileSheetIndex, ""),
        new Tile(1, 18, 13, 2496, tileSheetIndex, ""),
        new Tile(3, 23, 9, 2421, tileSheetIndex, ""),
        new Tile(1, 23, 10, 2446, tileSheetIndex, ""),
        new Tile(1, 23, 11, 2471, tileSheetIndex, ""),
        new Tile(1, 23, 12, 2421, tileSheetIndex, ""),
        new Tile(1, 23, 13, 2446, tileSheetIndex, ""),
        new Tile(1, 23, 14, 2471, tileSheetIndex, ""),
        new Tile(3, 23, 15, 2421, tileSheetIndex, ""),
        new Tile(1, 23, 16, 2446, tileSheetIndex, ""),
        new Tile(1, 23, 17, 2471, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> MemorialAreaArch(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(4, 15, 16, 2388, tileSheetIndex, ""),
        new Tile(4, 16, 16, 2389, tileSheetIndex, ""),
        new Tile(4, 17, 16, 2390, tileSheetIndex, ""),
        new Tile(4, 18, 16, 2391, tileSheetIndex, ""),
        new Tile(4, 15, 17, 2413, tileSheetIndex, ""),
        new Tile(4, 16, 17, 2414, tileSheetIndex, ""),
        new Tile(4, 17, 17, 2415, tileSheetIndex, ""),
        new Tile(4, 18, 17, 2416, tileSheetIndex, ""),
        new Tile(4, 15, 18, 2438, tileSheetIndex, ""),
        new Tile(4, 16, 18, 2439, tileSheetIndex, ""),
        new Tile(4, 17, 18, 2440, tileSheetIndex, ""),
        new Tile(4, 18, 18, 2441, tileSheetIndex, ""),
        new Tile(3, 15, 19, 2463, tileSheetIndex, ""),
        new Tile(3, 18, 19, 2466, tileSheetIndex, ""),
        new Tile(1, 15, 20, 2488, tileSheetIndex, ""),
        new Tile(1, 18, 20, 2491, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> PicnicBlanketEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 33, 45, 2992, tileSheetIndex, ""),
        new Tile(0, 34, 45, 2993, tileSheetIndex, ""),
        new Tile(0, 35, 45, 2994, tileSheetIndex, ""),
        new Tile(0, 36, 45, 2995, tileSheetIndex, ""),
        new Tile(0, 37, 45, 2996, tileSheetIndex, ""),
        new Tile(0, 38, 45, 2997, tileSheetIndex, ""),
        new Tile(0, 33, 46, 3017, tileSheetIndex, ""),
        new Tile(0, 34, 46, 3018, tileSheetIndex, ""),
        new Tile(0, 35, 46, 3019, tileSheetIndex, ""),
        new Tile(0, 36, 46, 3020, tileSheetIndex, ""),
        new Tile(0, 37, 46, 3021, tileSheetIndex, ""),
        new Tile(0, 38, 46, 3022, tileSheetIndex, ""),
        new Tile(0, 33, 47, 3042, tileSheetIndex, ""),
        new Tile(0, 34, 47, 3043, tileSheetIndex, ""),
        new Tile(0, 35, 47, 3044, tileSheetIndex, ""),
        new Tile(0, 36, 47, 3045, tileSheetIndex, ""),
        new Tile(0, 37, 47, 3046, tileSheetIndex, ""),
        new Tile(0, 38, 47, 3047, tileSheetIndex, ""),
        new Tile(0, 33, 48, 3067, tileSheetIndex, ""),
        new Tile(0, 34, 48, 3068, tileSheetIndex, ""),
        new Tile(0, 35, 48, 3069, tileSheetIndex, ""),
        new Tile(0, 36, 48, 3070, tileSheetIndex, ""),
        new Tile(0, 37, 48, 3071, tileSheetIndex, ""),
        new Tile(0, 38, 48, 3072, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> StoneBridgeEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 40, 48, -1, -1, ""),
        new Tile(0, 41, 48, -1, -1, ""),
        new Tile(0, 40, 49, -1, -1, ""),
        new Tile(0, 41, 49, -1, -1, ""),
        new Tile(1, 39, 49, -1, -1, ""),
        new Tile(1, 40, 49, -1, -1, ""),
        new Tile(1, 41, 49, -1, -1, ""),
        new Tile(1, 42, 49, -1, -1, ""),
        new Tile(0, 40, 50, -1, -1, ""),
        new Tile(0, 41, 50, -1, -1, ""),
        new Tile(1, 39, 50, -1, -1, ""),
        new Tile(1, 40, 50, -1, -1, ""),
        new Tile(1, 41, 50, -1, -1, ""),
        new Tile(1, 42, 50, -1, -1, ""),
        new Tile(0, 40, 51, -1, -1, ""),
        new Tile(0, 41, 51, -1, -1, ""),
        new Tile(0, 40, 52, -1, -1, ""),
        new Tile(0, 41, 52, -1, -1, ""),
        new Tile(0, 40, 53, -1, -1, ""),
        new Tile(0, 41, 53, -1, -1, ""),
        new Tile(0, 40, 54, -1, -1, ""),
        new Tile(0, 41, 54, -1, -1, ""),
        new Tile(0, 40, 55, -1, -1, ""),
        new Tile(0, 41, 55, -1, -1, ""),
        new Tile(0, 40, 56, -1, -1, ""),
        new Tile(0, 41, 56, -1, -1, ""),
        new Tile(0, 40, 57, -1, -1, ""),
        new Tile(0, 41, 57, -1, -1, ""),
        new Tile(1, 41, 57, -1, -1, ""),
        new Tile(1, 42, 57, -1, -1, ""),
        new Tile(0, 40, 58, -1, -1, ""),
        new Tile(0, 41, 58, -1, -1, ""),
        new Tile(1, 39, 58, -1, -1, ""),
        new Tile(1, 40, 58, -1, -1, ""),
        new Tile(1, 41, 58, -1, -1, ""),
        new Tile(0, 39, 59, -1, -1, ""),
        new Tile(0, 40, 59, -1, -1, ""),
        new Tile(0, 41, 59, -1, -1, ""),
        new Tile(0, 42, 59, -1, -1, ""),
        new Tile(1, 39, 48, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 48, 2714, tileSheetIndex, ""),
        new Tile(0, 41, 48, 2714, tileSheetIndex, ""),
        new Tile(1, 42, 48, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 49, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 49, 2738, tileSheetIndex, ""),
        new Tile(0, 41, 49, 2738, tileSheetIndex, ""),
        new Tile(1, 42, 49, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 50, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 50, 2738, tileSheetIndex, ""),
        new Tile(0, 41, 50, 2738, tileSheetIndex, ""),
        new Tile(1, 42, 50, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 51, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 51, 2738, tileSheetIndex, ""),
        new Tile(0, 41, 51, 2738, tileSheetIndex, ""),
        new Tile(1, 42, 51, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 52, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 52, 2738, tileSheetIndex, ""),
        new Tile(0, 41, 52, 2738, tileSheetIndex, ""),
        new Tile(1, 42, 52, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 53, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 53, 2738, tileSheetIndex, ""),
        new Tile(0, 41, 53, 2738, tileSheetIndex, ""),
        new Tile(1, 42, 53, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 54, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 54, 2738, tileSheetIndex, ""),
        new Tile(0, 41, 54, 2738, tileSheetIndex, ""),
        new Tile(1, 42, 54, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 55, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 55, 2738, tileSheetIndex, ""),
        new Tile(0, 41, 55, 2738, tileSheetIndex, ""),
        new Tile(1, 42, 55, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 56, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 56, 2738, tileSheetIndex, ""),
        new Tile(0, 41, 56, 2738, tileSheetIndex, ""),
        new Tile(1, 42, 56, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 57, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 57, 2738, tileSheetIndex, ""),
        new Tile(0, 41, 57, 2738, tileSheetIndex, ""),
        new Tile(1, 42, 57, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 58, 2715, tileSheetIndex, ""),
        new Tile(0, 40, 58, 2738, tileSheetIndex, ""),
        new Tile(0, 41, 58, 2738, tileSheetIndex, ""),
        new Tile(1, 42, 58, 2715, tileSheetIndex, ""),
        new Tile(1, 39, 59, 2740, tileSheetIndex, ""),
        new Tile(0, 40, 59, 2713, tileSheetIndex, ""),
        new Tile(0, 41, 59, 2713, tileSheetIndex, ""),
        new Tile(1, 42, 59, 2740, tileSheetIndex, ""),
        new Tile(4, 38, 48, 2999, tileSheetIndex, ""),
        new Tile(4, 38, 49, 3024, tileSheetIndex, ""),
        new Tile(4, 38, 50, 3024, tileSheetIndex, ""),
        new Tile(4, 38, 51, 3024, tileSheetIndex, ""),
        new Tile(4, 38, 52, 3024, tileSheetIndex, ""),
        new Tile(4, 38, 53, 3024, tileSheetIndex, ""),
        new Tile(4, 38, 54, 3024, tileSheetIndex, ""),
        new Tile(4, 38, 55, 3024, tileSheetIndex, ""),
        new Tile(4, 38, 56, 3024, tileSheetIndex, ""),
        new Tile(4, 38, 57, 3024, tileSheetIndex, ""),
        new Tile(4, 38, 58, 3024, tileSheetIndex, ""),
        new Tile(4, 38, 59, 3049, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> PicnicAreaTableEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(1, 36, 44, 2970, tileSheetIndex, ""),
        new Tile(1, 37, 44, 2971, tileSheetIndex, ""),
        new Tile(3, 37, 42, 2921, tileSheetIndex, ""),
        new Tile(3, 36, 43, 2945, tileSheetIndex, ""),
        new Tile(3, 37, 43, 2946, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> TreeSwingEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 32, 42, 433, tileSheetIndex, ""),
        new Tile(0, 33, 42, 433, tileSheetIndex, ""),
        new Tile(1, 32, 43, 2941, tileSheetIndex, ""),
        new Tile(1, 33, 43, 2942, tileSheetIndex, ""),
        new Tile(1, 34, 43, 2943, tileSheetIndex, ""),
        new Tile(1, 35, 43, 2944, tileSheetIndex, ""),
        new Tile(1, 32, 42, 2916, tileSheetIndex, ""),
        new Tile(1, 33, 42, 2917, tileSheetIndex, ""),
        new Tile(1, 32, 44, 2966, tileSheetIndex, ""),
        new Tile(1, 33, 44, 2967, tileSheetIndex, ""),
        new Tile(1, 34, 44, 2968, tileSheetIndex, ""),
        new Tile(1, 35, 44, 2969, tileSheetIndex, ""),
        new Tile(3, 34, 42, 2918, tileSheetIndex, ""),
        new Tile(3, 35, 42, 2919, tileSheetIndex, ""),
        new Tile(4, 32, 38, 2816, tileSheetIndex, ""),
        new Tile(4, 33, 38, 2817, tileSheetIndex, ""),
        new Tile(4, 34, 38, 2818, tileSheetIndex, ""),
        new Tile(4, 35, 38, 2819, tileSheetIndex, ""),
        new Tile(4, 36, 38, 2820, tileSheetIndex, ""),
        new Tile(4, 32, 39, 2841, tileSheetIndex, ""),
        new Tile(4, 33, 39, 2842, tileSheetIndex, ""),
        new Tile(4, 34, 39, 2843, tileSheetIndex, ""),
        new Tile(4, 35, 39, 2844, tileSheetIndex, ""),
        new Tile(4, 36, 39, 2845, tileSheetIndex, ""),
        new Tile(4, 32, 40, 2866, tileSheetIndex, ""),
        new Tile(4, 33, 40, 2867, tileSheetIndex, ""),
        new Tile(4, 34, 40, 2868, tileSheetIndex, ""),
        new Tile(4, 35, 40, 2869, tileSheetIndex, ""),
        new Tile(4, 36, 40, 2870, tileSheetIndex, ""),
        new Tile(4, 32, 41, 2891, tileSheetIndex, ""),
        new Tile(4, 33, 41, 2892, tileSheetIndex, ""),
        new Tile(4, 34, 41, 2893, tileSheetIndex, ""),
        new Tile(4, 35, 41, 2894, tileSheetIndex, ""),
        new Tile(4, 36, 41, 2895, tileSheetIndex, ""),
        new Tile(4, 34, 42, 2918, tileSheetIndex, ""),
        new Tile(4, 35, 42, 2919, tileSheetIndex, ""),
        new Tile(4, 36, 42, 2920, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> DogHouseEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 51, 5, 838, tileSheetIndex, ""),
        new Tile(0, 52, 5, 1203, tileSheetIndex, ""),
        new Tile(0, 53, 5, 1203, tileSheetIndex, ""),
        new Tile(0, 54, 5, 838, tileSheetIndex, ""),
        new Tile(0, 51, 6, 838, tileSheetIndex, ""),
        new Tile(0, 52, 6, 838, tileSheetIndex, ""),
        new Tile(0, 53, 6, 838, tileSheetIndex, ""),
        new Tile(0, 54, 6, 838, tileSheetIndex, ""),
        new Tile(0, 51, 7, 2200, tileSheetIndex, ""),
        new Tile(0, 52, 7, 2201, tileSheetIndex, ""),
        new Tile(0, 53, 7, 2202, tileSheetIndex, ""),
        new Tile(0, 54, 7, 2203, tileSheetIndex, ""),
        new Tile(0, 51, 8, 2225, tileSheetIndex, ""),
        new Tile(0, 52, 8, 2226, tileSheetIndex, ""),
        new Tile(0, 53, 8, 2227, tileSheetIndex, ""),
        new Tile(0, 54, 8, 2228, tileSheetIndex, ""),
        new Tile(0, 51, 9, 2250, tileSheetIndex, ""),
        new Tile(0, 52, 9, 2251, tileSheetIndex, ""),
        new Tile(0, 53, 9, 2252, tileSheetIndex, ""),
        new Tile(0, 54, 9, 2253, tileSheetIndex, ""),
        new Tile(1, 51, 4, 2668, tileSheetIndex, ""),
        new Tile(1, 52, 4, 2669, tileSheetIndex, ""),
        new Tile(1, 53, 4, 2670, tileSheetIndex, ""),
        new Tile(1, 54, 4, 2671, tileSheetIndex, ""),
        new Tile(1, 51, 5, 2693, tileSheetIndex, ""),
        new Tile(1, 52, 5, 2694, tileSheetIndex, ""),
        new Tile(1, 53, 5, 2695, tileSheetIndex, ""),
        new Tile(1, 54, 5, 2696, tileSheetIndex, ""),
        new Tile(1, 51, 6, 2718, tileSheetIndex, ""),
        new Tile(1, 52, 6, 2719, tileSheetIndex, ""),
        new Tile(1, 53, 6, 2720, tileSheetIndex, ""),
        new Tile(1, 54, 6, 2721, tileSheetIndex, ""),
        new Tile(1, 52, 7, 2201, tileSheetIndex, ""),
        new Tile(1, 53, 7, 2202, tileSheetIndex, ""),
        new Tile(1, 54, 7, -1, -1, ""),
        new Tile(4, 51, 3, 2643, tileSheetIndex, ""),
        new Tile(4, 52, 3, 2644, tileSheetIndex, ""),
        new Tile(4, 53, 3, 2645, tileSheetIndex, ""),
        new Tile(4, 54, 3, 2646, tileSheetIndex, ""),
        new Tile(3, 52, 4, -1, -1, ""),
        new Tile(3, 50, 5, -1, -1, ""),
        new Tile(3, 51, 5, -1, -1, ""),
        new Tile(3, 52, 5, -1, -1, ""),
        new Tile(3, 53, 5, -1, -1, ""),
        new Tile(3, 54, 5, -1, -1, ""),
        new Tile(1, 50, 6, -1, -1, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> YardGardenEditsAndBinClutter(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 57, 8, -1, -1, ""),
        new Tile(0, 57, 11, -1, -1, ""),
        new Tile(0, 56, 4, 200, tileSheetIndex, ""),
        new Tile(0, 57, 4, 179, tileSheetIndex, ""),
        new Tile(0, 58, 4, 180, tileSheetIndex, ""),
        new Tile(0, 59, 4, 179, tileSheetIndex, ""),
        new Tile(0, 60, 4, 203, tileSheetIndex, ""),
        new Tile(0, 56, 5, 225, tileSheetIndex, ""),
        new Tile(0, 57, 5, 227, tileSheetIndex, ""),
        new Tile(0, 58, 5, 227, tileSheetIndex, ""),
        new Tile(0, 59, 5, 227, tileSheetIndex, ""),
        new Tile(0, 60, 5, 228, tileSheetIndex, ""),
        new Tile(0, 56, 6, 225, tileSheetIndex, ""),
        new Tile(0, 57, 6, 227, tileSheetIndex, ""),
        new Tile(0, 58, 6, 227, tileSheetIndex, ""),
        new Tile(0, 59, 6, 227, tileSheetIndex, ""),
        new Tile(0, 60, 6, 228, tileSheetIndex, ""),
        new Tile(0, 56, 7, 225, tileSheetIndex, ""),
        new Tile(0, 57, 7, 227, tileSheetIndex, ""),
        new Tile(0, 58, 7, 227, tileSheetIndex, ""),
        new Tile(0, 59, 7, 227, tileSheetIndex, ""),
        new Tile(0, 60, 7, 228, tileSheetIndex, ""),
        new Tile(0, 56, 8, 225, tileSheetIndex, ""),
        new Tile(0, 57, 8, 227, tileSheetIndex, ""),
        new Tile(0, 58, 8, 227, tileSheetIndex, ""),
        new Tile(0, 59, 8, 227, tileSheetIndex, ""),
        new Tile(0, 60, 8, 228, tileSheetIndex, ""),
        new Tile(0, 56, 9, 250, tileSheetIndex, ""),
        new Tile(0, 57, 9, 251, tileSheetIndex, ""),
        new Tile(0, 58, 9, 251, tileSheetIndex, ""),
        new Tile(0, 59, 9, 251, tileSheetIndex, ""),
        new Tile(0, 60, 9, 253, tileSheetIndex, ""),
        new Tile(0, 55, 4, 203, tileSheetIndex, ""),
        new Tile(0, 55, 5, 228, tileSheetIndex, ""),
        new Tile(0, 55, 6, 228, tileSheetIndex, ""),
        new Tile(0, 55, 7, 228, tileSheetIndex, ""),
        new Tile(0, 55, 8, 228, tileSheetIndex, ""),
        new Tile(0, 55, 9, 228, tileSheetIndex, ""),
        new Tile(0, 55, 10, 228, tileSheetIndex, ""),
        new Tile(0, 55, 11, 177, tileSheetIndex, ""),
        new Tile(0, 56, 11, 201, tileSheetIndex, ""),
        new Tile(0, 57, 11, 201, tileSheetIndex, ""),
        new Tile(0, 58, 11, 203, tileSheetIndex, ""),
        new Tile(0, 58, 12, 228, tileSheetIndex, ""),
        new Tile(0, 58, 13, 228, tileSheetIndex, ""),
        new Tile(0, 56, 10, 175, tileSheetIndex, ""),
        new Tile(0, 56, 12, 227, tileSheetIndex, ""),
        new Tile(0, 57, 12, 227, tileSheetIndex, ""),
        new Tile(0, 57, 13, 227, tileSheetIndex, ""),
        new Tile(0, 61, 5, 175, tileSheetIndex, ""),
        new Tile(0, 62, 5, 175, tileSheetIndex, ""),
        new Tile(0, 63, 5, 175, tileSheetIndex, ""),
        new Tile(0, 64, 5, 175, tileSheetIndex, ""),
        new Tile(0, 65, 5, 175, tileSheetIndex, ""),
        new Tile(0, 61, 6, 175, tileSheetIndex, ""),
        new Tile(0, 62, 6, 175, tileSheetIndex, ""),
        new Tile(0, 63, 6, 175, tileSheetIndex, ""),
        new Tile(0, 64, 6, 175, tileSheetIndex, ""),
        new Tile(0, 65, 6, 175, tileSheetIndex, ""),
        new Tile(3, 55, 3, 2469, tileSheetIndex, ""),
        new Tile(3, 56, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 57, 3, 2471, tileSheetIndex, ""),
        new Tile(3, 58, 3, 2471, tileSheetIndex, ""),
        new Tile(3, 59, 3, 2469, tileSheetIndex, ""),
        new Tile(3, 60, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 61, 3, 2471, tileSheetIndex, ""),
        new Tile(3, 62, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 63, 3, 2469, tileSheetIndex, ""),
        new Tile(3, 64, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 65, 3, 2471, tileSheetIndex, ""),
        new Tile(3, 66, 3, 2469, tileSheetIndex, ""),
        new Tile(3, 67, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 68, 3, 2471, tileSheetIndex, ""),
        new Tile(3, 69, 3, 2469, tileSheetIndex, ""),
        new Tile(3, 70, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 71, 3, 2471, tileSheetIndex, ""),
        new Tile(3, 72, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 73, 3, 2471, tileSheetIndex, ""),
        new Tile(3, 74, 3, 2469, tileSheetIndex, ""),
        new Tile(3, 75, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 76, 3, 2471, tileSheetIndex, ""),
        new Tile(3, 77, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 78, 3, 2471, tileSheetIndex, ""),
        new Tile(3, 79, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 51, 3, 2468, tileSheetIndex, ""),
        new Tile(3, 52, 3, 2469, tileSheetIndex, ""),
        new Tile(3, 53, 3, 2470, tileSheetIndex, ""),
        new Tile(3, 54, 3, 2471, tileSheetIndex, ""),
        new Tile(3, 55, 3, 2472, tileSheetIndex, ""),
        new Tile(0, 51, 4, 2493, tileSheetIndex, ""),
        new Tile(1, 52, 4, 2494, tileSheetIndex, ""),
        new Tile(1, 53, 4, 2495, tileSheetIndex, ""),
        new Tile(0, 54, 4, 2496, tileSheetIndex, ""),
        new Tile(1, 55, 4, 2497, tileSheetIndex, ""),
        new Tile(1, 56, 4, 2494, tileSheetIndex, ""),
        new Tile(1, 57, 4, 2495, tileSheetIndex, ""),
        new Tile(1, 58, 4, 2496, tileSheetIndex, ""),
        new Tile(1, 59, 4, 2494, tileSheetIndex, ""),
        new Tile(1, 60, 4, 2494, tileSheetIndex, ""),
        new Tile(1, 61, 4, 2495, tileSheetIndex, ""),
        new Tile(1, 62, 4, 2496, tileSheetIndex, ""),
        new Tile(1, 63, 4, 2497, tileSheetIndex, ""),
        new Tile(1, 64, 4, 2496, tileSheetIndex, ""),
        new Tile(1, 65, 4, 2497, tileSheetIndex, ""),
        new Tile(1, 66, 4, 2493, tileSheetIndex, ""),
        new Tile(1, 67, 4, 2494, tileSheetIndex, ""),
        new Tile(1, 68, 4, 2495, tileSheetIndex, ""),
        new Tile(1, 69, 4, 2496, tileSheetIndex, ""),
        new Tile(1, 70, 4, 2493, tileSheetIndex, ""),
        new Tile(1, 71, 4, 2493, tileSheetIndex, ""),
        new Tile(1, 72, 4, 2494, tileSheetIndex, ""),
        new Tile(1, 73, 4, 2495, tileSheetIndex, ""),
        new Tile(1, 74, 4, 2496, tileSheetIndex, ""),
        new Tile(1, 75, 4, 2497, tileSheetIndex, ""),
        new Tile(1, 76, 4, 2126, tileSheetIndex, ""),
        new Tile(1, 77, 4, 2496, tileSheetIndex, ""),
        new Tile(1, 78, 4, 2493, tileSheetIndex, ""),
        new Tile(1, 79, 4, 1496, tileSheetIndex, ""),
        new Tile(1, 75, 5, 2741, tileSheetIndex, ""),
        new Tile(1, 76, 5, 2003, tileSheetIndex, ""),
        new Tile(1, 77, 5, 2062, tileSheetIndex, ""),
        new Tile(1, 78, 5, 1520, tileSheetIndex, ""),
        new Tile(1, 79, 5, 1521, tileSheetIndex, ""),
        new Tile(1, 77, 6, 2087, tileSheetIndex, ""),
        new Tile(1, 78, 6, 2088, tileSheetIndex, ""),
        new Tile(1, 77, 7, 2112, tileSheetIndex, ""),
        new Tile(1, 78, 7, 2113, tileSheetIndex, ""),
        new Tile(1, 76, 6, 21, tileSheetIndex, ""),
        new Tile(3, 75, 4, 2716, tileSheetIndex, ""),
        new Tile(3, 76, 4, 2036, tileSheetIndex, ""),
        new Tile(3, 77, 4, 2037, tileSheetIndex, ""),
        new Tile(3, 78, 4, 1495, tileSheetIndex, ""),
        new Tile(0, 79, 4, 2496, tileSheetIndex, ""),
        new Tile(3, 76, 5, 2061, tileSheetIndex, ""),
        new Tile(3, 78, 5, 2063, tileSheetIndex, ""),
        new Tile(4, 76, 3, 2101, tileSheetIndex, ""),
        new Tile(4, 78, 4, 2038, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> PatioEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 71, 7, -1, -1, ""),
        new Tile(0, 71, 11, -1, -1, ""),
        new Tile(0, 66, 5, 2715, tileSheetIndex, ""),
        new Tile(0, 67, 5, 2715, tileSheetIndex, ""),
        new Tile(0, 68, 5, 2715, tileSheetIndex, ""),
        new Tile(0, 69, 5, 2715, tileSheetIndex, ""),
        new Tile(0, 70, 5, 2715, tileSheetIndex, ""),
        new Tile(0, 71, 5, 2715, tileSheetIndex, ""),
        new Tile(0, 72, 5, 2715, tileSheetIndex, ""),
        new Tile(0, 73, 5, 2715, tileSheetIndex, ""),
        new Tile(0, 66, 6, 2715, tileSheetIndex, ""),
        new Tile(0, 67, 6, 2738, tileSheetIndex, ""),
        new Tile(0, 68, 6, 2738, tileSheetIndex, ""),
        new Tile(0, 69, 6, 2738, tileSheetIndex, ""),
        new Tile(0, 70, 6, 2738, tileSheetIndex, ""),
        new Tile(0, 71, 6, 2738, tileSheetIndex, ""),
        new Tile(0, 72, 6, 2738, tileSheetIndex, ""),
        new Tile(0, 73, 6, 2715, tileSheetIndex, ""),
        new Tile(0, 66, 7, 2715, tileSheetIndex, ""),
        new Tile(0, 67, 7, 2738, tileSheetIndex, ""),
        new Tile(0, 68, 7, 2738, tileSheetIndex, ""),
        new Tile(0, 69, 7, 2738, tileSheetIndex, ""),
        new Tile(0, 70, 7, 2738, tileSheetIndex, ""),
        new Tile(0, 71, 7, 2738, tileSheetIndex, ""),
        new Tile(0, 72, 7, 2738, tileSheetIndex, ""),
        new Tile(0, 73, 7, 2715, tileSheetIndex, ""),
        new Tile(0, 66, 8, 2715, tileSheetIndex, ""),
        new Tile(0, 67, 8, 2738, tileSheetIndex, ""),
        new Tile(0, 68, 8, 2738, tileSheetIndex, ""),
        new Tile(0, 69, 8, 2738, tileSheetIndex, ""),
        new Tile(0, 70, 8, 2738, tileSheetIndex, ""),
        new Tile(0, 71, 8, 2738, tileSheetIndex, ""),
        new Tile(0, 72, 8, 2738, tileSheetIndex, ""),
        new Tile(0, 73, 8, 2715, tileSheetIndex, ""),
        new Tile(0, 66, 9, 2715, tileSheetIndex, ""),
        new Tile(0, 67, 9, 2738, tileSheetIndex, ""),
        new Tile(0, 68, 9, 2738, tileSheetIndex, ""),
        new Tile(0, 69, 9, 2738, tileSheetIndex, ""),
        new Tile(0, 70, 9, 2738, tileSheetIndex, ""),
        new Tile(0, 71, 9, 2738, tileSheetIndex, ""),
        new Tile(0, 72, 9, 2738, tileSheetIndex, ""),
        new Tile(0, 73, 9, 2715, tileSheetIndex, ""),
        new Tile(0, 66, 10, 2715, tileSheetIndex, ""),
        new Tile(0, 67, 10, 2738, tileSheetIndex, ""),
        new Tile(0, 68, 10, 2738, tileSheetIndex, ""),
        new Tile(0, 69, 10, 2738, tileSheetIndex, ""),
        new Tile(0, 70, 10, 2738, tileSheetIndex, ""),
        new Tile(0, 71, 10, 2738, tileSheetIndex, ""),
        new Tile(0, 72, 10, 2738, tileSheetIndex, ""),
        new Tile(0, 73, 10, 2715, tileSheetIndex, ""),
        new Tile(0, 68, 11, 2715, tileSheetIndex, ""),
        new Tile(0, 69, 11, 2715, tileSheetIndex, ""),
        new Tile(0, 70, 11, 2715, tileSheetIndex, ""),
        new Tile(0, 71, 11, 2715, tileSheetIndex, ""),
        new Tile(0, 72, 11, 2715, tileSheetIndex, ""),
        new Tile(0, 73, 11, 2715, tileSheetIndex, ""),
        new Tile(0, 65, 11, 2715, tileSheetIndex, ""),
        new Tile(1, 66, 11, 2715, tileSheetIndex, ""),
        new Tile(1, 67, 11, 2715, tileSheetIndex, ""),
        new Tile(3, 55, 8, -1, -1, ""),
        new Tile(3, 56, 8, -1, -1, ""),
        new Tile(3, 57, 8, -1, -1, ""),
        new Tile(3, 58, 8, -1, -1, ""),
        new Tile(3, 59, 8, -1, -1, ""),
        new Tile(3, 60, 8, -1, -1, ""),
        new Tile(3, 61, 8, -1, -1, ""),
        new Tile(3, 62, 8, -1, -1, ""),
        new Tile(3, 63, 8, -1, -1, ""),
        new Tile(3, 64, 8, -1, -1, ""),
        new Tile(3, 65, 8, -1, -1, ""),
        new Tile(3, 66, 8, -1, -1, ""),
        new Tile(3, 67, 8, -1, -1, ""),
        new Tile(3, 68, 8, -1, -1, ""),
        new Tile(3, 69, 8, -1, -1, ""),
        new Tile(3, 70, 8, -1, -1, ""),
        new Tile(3, 71, 8, -1, -1, ""),
        new Tile(3, 72, 8, -1, -1, ""),
        new Tile(3, 73, 8, -1, -1, ""),
        new Tile(3, 74, 8, -1, -1, ""),
        new Tile(3, 75, 8, -1, -1, ""),
        new Tile(3, 76, 8, -1, -1, ""),
        new Tile(3, 77, 8, -1, -1, ""),
        new Tile(3, 78, 8, -1, -1, ""),
        new Tile(3, 79, 8, -1, -1, ""),
        new Tile(1, 55, 9, -1, -1, ""),
        new Tile(1, 56, 9, -1, -1, ""),
        new Tile(1, 57, 9, -1, -1, ""),
        new Tile(1, 58, 9, -1, -1, ""),
        new Tile(1, 59, 9, -1, -1, ""),
        new Tile(1, 60, 9, -1, -1, ""),
        new Tile(1, 61, 9, -1, -1, ""),
        new Tile(1, 62, 9, -1, -1, ""),
        new Tile(1, 63, 9, -1, -1, ""),
        new Tile(1, 64, 9, -1, -1, ""),
        new Tile(1, 65, 9, -1, -1, ""),
        new Tile(1, 66, 9, -1, -1, ""),
        new Tile(1, 67, 9, -1, -1, ""),
        new Tile(1, 68, 9, -1, -1, ""),
        new Tile(1, 69, 9, -1, -1, ""),
        new Tile(1, 70, 9, -1, -1, ""),
        new Tile(1, 71, 9, -1, -1, ""),
        new Tile(1, 72, 9, -1, -1, ""),
        new Tile(1, 73, 9, -1, -1, ""),
        new Tile(1, 74, 9, -1, -1, ""),
        new Tile(1, 75, 9, -1, -1, ""),
        new Tile(1, 76, 9, -1, -1, ""),
        new Tile(1, 77, 9, -1, -1, ""),
        new Tile(1, 78, 5, -1, -1, ""),
        new Tile(1, 78, 6, -1, -1, ""),
        new Tile(1, 78, 7, -1, -1, ""),
        new Tile(1, 78, 8, -1, -1, ""),
        new Tile(1, 78, 9, -1, -1, ""),
        new Tile(1, 78, 10, -1, -1, ""),
        new Tile(1, 55, 8, -1, -1, ""),
        new Tile(1, 56, 8, -1, -1, ""),
        new Tile(1, 57, 8, -1, -1, ""),
        new Tile(1, 58, 8, -1, -1, ""),
        new Tile(1, 59, 8, -1, -1, ""),
        new Tile(1, 60, 8, -1, -1, ""),
        new Tile(1, 61, 8, -1, -1, ""),
        new Tile(1, 62, 8, -1, -1, ""),
        new Tile(1, 63, 8, -1, -1, ""),
        new Tile(1, 64, 8, -1, -1, ""),
        new Tile(1, 65, 8, -1, -1, ""),
        new Tile(1, 66, 8, -1, -1, ""),
        new Tile(1, 67, 8, -1, -1, ""),
        new Tile(1, 68, 8, -1, -1, ""),
        new Tile(1, 69, 8, -1, -1, ""),
        new Tile(1, 70, 8, -1, -1, ""),
        new Tile(1, 71, 8, -1, -1, ""),
        new Tile(1, 72, 8, -1, -1, ""),
        new Tile(1, 73, 8, -1, -1, ""),
        new Tile(1, 74, 8, -1, -1, ""),
        new Tile(1, 75, 8, -1, -1, ""),
        new Tile(1, 76, 8, -1, -1, ""),
        new Tile(1, 77, 8, -1, -1, ""),
        new Tile(1, 55, 7, -1, -1, ""),
        new Tile(1, 56, 7, -1, -1, ""),
        new Tile(1, 57, 7, -1, -1, ""),
        new Tile(1, 58, 7, -1, -1, ""),
        new Tile(1, 59, 7, -1, -1, ""),
        new Tile(1, 60, 7, -1, -1, ""),
        new Tile(1, 61, 7, -1, -1, ""),
        new Tile(1, 62, 7, -1, -1, ""),
        new Tile(1, 63, 7, -1, -1, ""),
        new Tile(1, 64, 7, -1, -1, ""),
        new Tile(1, 65, 7, -1, -1, ""),
        new Tile(1, 66, 7, -1, -1, ""),
        new Tile(1, 67, 7, -1, -1, ""),
        new Tile(1, 68, 7, -1, -1, ""),
        new Tile(1, 69, 7, -1, -1, ""),
        new Tile(1, 70, 7, -1, -1, ""),
        new Tile(1, 71, 7, -1, -1, ""),
        new Tile(1, 72, 7, -1, -1, ""),
        new Tile(1, 73, 7, -1, -1, ""),
        new Tile(1, 75, 7, -1, -1, ""),
        new Tile(1, 76, 7, -1, -1, ""),
        new Tile(1, 77, 7, -1, -1, ""),
        new Tile(1, 55, 6, -1, -1, ""),
        new Tile(1, 56, 6, -1, -1, ""),
        new Tile(1, 57, 6, -1, -1, ""),
        new Tile(1, 58, 6, -1, -1, ""),
        new Tile(1, 59, 6, -1, -1, ""),
        new Tile(1, 60, 6, -1, -1, ""),
        new Tile(1, 61, 6, -1, -1, ""),
        new Tile(1, 62, 6, -1, -1, ""),
        new Tile(1, 63, 6, -1, -1, ""),
        new Tile(1, 64, 6, -1, -1, ""),
        new Tile(1, 65, 6, -1, -1, ""),
        new Tile(1, 66, 6, -1, -1, ""),
        new Tile(1, 67, 6, -1, -1, ""),
        new Tile(1, 68, 6, -1, -1, ""),
        new Tile(1, 69, 6, -1, -1, ""),
        new Tile(1, 70, 6, -1, -1, ""),
        new Tile(1, 71, 6, -1, -1, ""),
        new Tile(1, 72, 6, -1, -1, ""),
        new Tile(1, 73, 6, -1, -1, ""),
        new Tile(1, 74, 6, -1, -1, ""),
        new Tile(1, 75, 6, -1, -1, ""),
        new Tile(1, 77, 6, -1, -1, ""),
        new Tile(1, 55, 5, -1, -1, ""),
        new Tile(1, 56, 5, -1, -1, ""),
        new Tile(1, 57, 5, -1, -1, ""),
        new Tile(1, 58, 5, -1, -1, ""),
        new Tile(1, 59, 5, -1, -1, ""),
        new Tile(1, 60, 5, -1, -1, ""),
        new Tile(1, 61, 5, -1, -1, ""),
        new Tile(1, 62, 5, -1, -1, ""),
        new Tile(1, 63, 5, -1, -1, ""),
        new Tile(1, 64, 5, -1, -1, ""),
        new Tile(1, 65, 5, -1, -1, ""),
        new Tile(1, 66, 5, -1, -1, ""),
        new Tile(1, 67, 5, -1, -1, ""),
        new Tile(1, 68, 5, -1, -1, ""),
        new Tile(1, 69, 5, -1, -1, ""),
        new Tile(1, 70, 5, -1, -1, ""),
        new Tile(1, 71, 5, -1, -1, ""),
        new Tile(1, 72, 5, -1, -1, ""),
        new Tile(1, 73, 5, -1, -1, ""),
        new Tile(1, 74, 5, -1, -1, ""),
        new Tile(1, 77, 5, -1, -1, ""),
        new Tile(3, 55, 3, -1, -1, ""),
        new Tile(3, 56, 3, -1, -1, ""),
        new Tile(3, 57, 3, -1, -1, ""),
        new Tile(3, 55, 4, -1, -1, ""),
        new Tile(3, 56, 4, -1, -1, ""),
        new Tile(3, 57, 4, -1, -1, ""),
        new Tile(3, 55, 5, -1, -1, ""),
        new Tile(3, 56, 5, -1, -1, ""),
        new Tile(3, 57, 5, -1, -1, ""),
        new Tile(3, 55, 6, -1, -1, ""),
        new Tile(3, 56, 6, -1, -1, ""),
        new Tile(3, 57, 6, -1, -1, ""),
        new Tile(3, 56, 7, -1, -1, ""),
        new Tile(3, 54, 5, -1, -1, ""),
        new Tile(1, 55, 5, -1, -1, ""),
        new Tile(3, 68, 3, -1, -1, ""),
        new Tile(3, 69, 3, -1, -1, ""),
        new Tile(3, 70, 3, -1, -1, ""),
        new Tile(3, 68, 4, -1, -1, ""),
        new Tile(3, 69, 4, -1, -1, ""),
        new Tile(3, 70, 4, -1, -1, ""),
        new Tile(3, 68, 5, -1, -1, ""),
        new Tile(3, 69, 5, -1, -1, ""),
        new Tile(3, 70, 5, -1, -1, ""),
        new Tile(3, 68, 6, -1, -1, ""),
        new Tile(3, 69, 6, -1, -1, ""),
        new Tile(3, 70, 6, -1, -1, ""),
        new Tile(3, 69, 7, -1, -1, ""),
        new Tile(3, 73, 2, -1, -1, ""),
        new Tile(3, 74, 2, -1, -1, ""),
        new Tile(3, 75, 2, -1, -1, ""),
        new Tile(3, 73, 3, -1, -1, ""),
        new Tile(3, 74, 3, -1, -1, ""),
        new Tile(3, 75, 3, -1, -1, ""),
        new Tile(3, 73, 4, -1, -1, ""),
        new Tile(3, 74, 4, -1, -1, ""),
        new Tile(3, 75, 4, -1, -1, ""),
        new Tile(3, 73, 5, -1, -1, ""),
        new Tile(3, 74, 5, -1, -1, ""),
        new Tile(3, 75, 5, -1, -1, ""),
        new Tile(3, 74, 6, -1, -1, ""),
        new Tile(1, 74, 7, -1, -1, ""),
        new Tile(3, 78, 4, -1, -1, ""),
        new Tile(3, 79, 4, -1, -1, ""),
        new Tile(3, 78, 5, -1, -1, ""),
        new Tile(3, 79, 5, -1, -1, ""),
        new Tile(3, 78, 6, -1, -1, ""),
        new Tile(3, 79, 6, -1, -1, ""),
        new Tile(3, 78, 7, -1, -1, ""),
        new Tile(3, 79, 7, -1, -1, ""),
        new Tile(3, 57, 0, -1, -1, ""),
        new Tile(3, 58, 0, -1, -1, ""),
        new Tile(3, 59, 0, -1, -1, ""),
        new Tile(3, 60, 0, -1, -1, ""),
        new Tile(3, 61, 0, -1, -1, ""),
        new Tile(3, 57, 1, -1, -1, ""),
        new Tile(3, 58, 1, -1, -1, ""),
        new Tile(3, 59, 1, -1, -1, ""),
        new Tile(3, 60, 1, -1, -1, ""),
        new Tile(3, 61, 1, -1, -1, ""),
        new Tile(3, 62, 1, -1, -1, ""),
        new Tile(3, 57, 2, -1, -1, ""),
        new Tile(3, 58, 2, -1, -1, ""),
        new Tile(3, 59, 2, -1, -1, ""),
        new Tile(3, 60, 2, -1, -1, ""),
        new Tile(3, 61, 2, -1, -1, ""),
        new Tile(3, 62, 2, -1, -1, ""),
        new Tile(3, 59, 3, -1, -1, ""),
        new Tile(3, 60, 3, -1, -1, ""),
        new Tile(3, 59, 4, -1, -1, ""),
        new Tile(3, 60, 4, -1, -1, ""),
        new Tile(1, 58, 0, 77, tileSheetIndex, ""),
        new Tile(4, 62, 0, 55, tileSheetIndex, ""),
        new Tile(4, 62, 1, 80, tileSheetIndex, ""),
        new Tile(0, 65, 5, 351, tileSheetIndex, ""),
        new Tile(0, 74, 5, 351, tileSheetIndex, ""),
        new Tile(0, 74, 6, 351, tileSheetIndex, ""),
        new Tile(0, 74, 7, 351, tileSheetIndex, ""),
        new Tile(0, 74, 8, 351, tileSheetIndex, ""),
        new Tile(0, 74, 9, 351, tileSheetIndex, ""),
        new Tile(0, 74, 10, 351, tileSheetIndex, ""),
        new Tile(0, 75, 9, 351, tileSheetIndex, ""),
        new Tile(0, 75, 10, 351, tileSheetIndex, ""),
        new Tile(0, 76, 10, 351, tileSheetIndex, ""),
        new Tile(0, 77, 10, 351, tileSheetIndex, ""),
        new Tile(0, 76, 11, 351, tileSheetIndex, ""),
        new Tile(0, 77, 11, 351, tileSheetIndex, ""),
        new Tile(0, 56, 8, 175, tileSheetIndex, ""),
        new Tile(0, 56, 9, 175, tileSheetIndex, ""),
        new Tile(0, 57, 9, 175, tileSheetIndex, ""),
        new Tile(0, 58, 9, 175, tileSheetIndex, ""),
        new Tile(0, 59, 9, 175, tileSheetIndex, ""),
        new Tile(0, 60, 9, 175, tileSheetIndex, ""),
        new Tile(0, 61, 9, 175, tileSheetIndex, ""),
        new Tile(0, 62, 9, 175, tileSheetIndex, ""),
        new Tile(0, 63, 9, 175, tileSheetIndex, ""),
        new Tile(0, 64, 9, 175, tileSheetIndex, ""),
        new Tile(0, 65, 9, 175, tileSheetIndex, ""),
        new Tile(0, 59, 4, 351, tileSheetIndex, ""),
        new Tile(0, 60, 4, 351, tileSheetIndex, ""),
        new Tile(3, 58, 1, 13, tileSheetIndex, ""),
        new Tile(3, 59, 1, 14, tileSheetIndex, ""),
        new Tile(3, 60, 1, 15, tileSheetIndex, ""),
        new Tile(4, 68, 4, 2069, -1, ""),
        new Tile(4, 69, 4, 2070, tileSheetIndex, ""),
        new Tile(4, 70, 4, 2071, tileSheetIndex, ""),
        new Tile(3, 68, 5, 2094, tileSheetIndex, ""),
        new Tile(3, 69, 5, 2095, tileSheetIndex, ""),
        new Tile(3, 70, 5, 2096, tileSheetIndex, ""),
        new Tile(3, 71, 5, 2097, tileSheetIndex, ""),
        new Tile(1, 68, 6, 2119, tileSheetIndex, ""),
        new Tile(1, 69, 6, 2120, tileSheetIndex, ""),
        new Tile(1, 70, 6, 2121, tileSheetIndex, ""),
        new Tile(1, 71, 6, 2122, tileSheetIndex, ""),
        new Tile(4, 69, 7, 2145, -1, ""),
        new Tile(4, 70, 7, 2146, -1, ""),
        new Tile(4, 71, 7, 2147, -1, ""),
        new Tile(1, 69, 8, 2170, tileSheetIndex, ""),
        new Tile(1, 70, 8, 2171, tileSheetIndex, ""),
        new Tile(1, 71, 8, 2172, tileSheetIndex, ""),
        new Tile(1, 69, 9, 2195, tileSheetIndex, ""),
        new Tile(1, 70, 9, 2196, tileSheetIndex, ""),
        new Tile(1, 71, 9, 2197, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static List<Tile> FarmStandEdits(GameLocation gl)
    {
      int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
      List<Tile> tileArray = new List<Tile>()
      {
        new Tile(0, 71, 12, 175, tileSheetIndex, ""),
        new Tile(0, 72, 12, 175, tileSheetIndex, ""),
        new Tile(0, 70, 13, 175, tileSheetIndex, ""),
        new Tile(0, 71, 13, 175, tileSheetIndex, ""),
        new Tile(0, 72, 13, 175, tileSheetIndex, ""),
        new Tile(0, 73, 13, 175, tileSheetIndex, ""),
        new Tile(0, 70, 14, 175, tileSheetIndex, ""),
        new Tile(0, 73, 14, 175, tileSheetIndex, ""),
        new Tile(0, 73, 14, 2232, tileSheetIndex, ""),
        new Tile(0, 74, 14, 2232, tileSheetIndex, ""),
        new Tile(0, 75, 14, 2232, tileSheetIndex, ""),
        new Tile(0, 76, 14, 2232, tileSheetIndex, ""),
        new Tile("Paths", 77, 11, -1, -1, ""),
        new Tile("Paths", 74, 10, -1, -1, ""),
        new Tile("Paths", 75, 10, -1, -1, ""),
        new Tile("Front", 79, 10, -1, -1, ""),
        new Tile("Front", 79, 13, -1, -1, ""),
        new Tile("Front", 79, 12, 2138, tileSheetIndex, ""),
        new Tile(4, 76, 9, 1982, tileSheetIndex, ""),
        new Tile(4, 77, 9, 1983, tileSheetIndex, ""),
        new Tile(4, 78, 9, 1984, tileSheetIndex, ""),
        new Tile(4, 79, 9, 1985, tileSheetIndex, ""),
        new Tile(4, 77, 10, 2008, tileSheetIndex, ""),
        new Tile(4, 78, 10, 2009, tileSheetIndex, ""),
        new Tile(4, 79, 10, 2010, tileSheetIndex, ""),
        new Tile(4, 79, 11, 2035, tileSheetIndex, ""),
        new Tile(4, 73, 10, 2004, tileSheetIndex, ""),
        new Tile(4, 74, 10, 2005, tileSheetIndex, ""),
        new Tile(4, 75, 10, 2006, tileSheetIndex, ""),
        new Tile(4, 76, 10, 2007, tileSheetIndex, ""),
        new Tile(4, 73, 11, 2029, tileSheetIndex, ""),
        new Tile(4, 74, 11, 2030, tileSheetIndex, ""),
        new Tile(4, 75, 11, 2031, tileSheetIndex, ""),
        new Tile(4, 76, 11, 2032, tileSheetIndex, ""),
        new Tile(4, 73, 12, 2054, tileSheetIndex, ""),
        new Tile(4, 74, 12, 2055, tileSheetIndex, ""),
        new Tile(4, 75, 12, 2056, tileSheetIndex, ""),
        new Tile(4, 76, 12, 2057, tileSheetIndex, ""),
        new Tile(4, 73, 13, 2079, tileSheetIndex, ""),
        new Tile(4, 74, 13, 2080, tileSheetIndex, ""),
        new Tile(4, 75, 13, 2081, tileSheetIndex, ""),
        new Tile(4, 76, 13, 2082, tileSheetIndex, ""),
        new Tile(3, 72, 14, 2103, tileSheetIndex, ""),
        new Tile(3, 73, 14, 2104, tileSheetIndex, ""),
        new Tile(3, 74, 14, 2105, tileSheetIndex, ""),
        new Tile(3, 75, 14, 2106, tileSheetIndex, ""),
        new Tile(3, 76, 14, 2107, tileSheetIndex, ""),
        new Tile(3, 77, 14, 2108, tileSheetIndex, ""),
        new Tile(3, 78, 14, 2109, tileSheetIndex, ""),
        new Tile(1, 79, 11, -1, -1, ""),
        new Tile(1, 79, 6, 21, tileSheetIndex, ""),
        new Tile(1, 79, 7, 21, tileSheetIndex, ""),
        new Tile(1, 79, 8, 21, tileSheetIndex, ""),
        new Tile(1, 79, 9, 21, tileSheetIndex, ""),
        new Tile(1, 79, 10, 21, tileSheetIndex, ""),
        new Tile(1, 78, 12, 2059, tileSheetIndex, ""),
        new Tile(1, 78, 11, 2034, tileSheetIndex, ""),
        new Tile(1, 79, 12, 2060, tileSheetIndex, ""),
        new Tile(1, 77, 13, 2083, tileSheetIndex, ""),
        new Tile(1, 77, 12, 2058, tileSheetIndex, ""),
        new Tile(1, 77, 11, 2033, tileSheetIndex, ""),
        new Tile(1, 78, 13, 2084, tileSheetIndex, ""),
        new Tile(1, 79, 13, 2163, tileSheetIndex, ""),
        new Tile(1, 78, 14, 2109, tileSheetIndex, ""),
        new Tile(1, 79, 14, 2188, tileSheetIndex, ""),
        new Tile(1, 72, 15, 2128, tileSheetIndex, ""),
        new Tile(1, 73, 15, 2129, tileSheetIndex, ""),
        new Tile(1, 74, 15, 2130, tileSheetIndex, ""),
        new Tile(1, 75, 15, 2131, tileSheetIndex, ""),
        new Tile(1, 76, 15, 2132, tileSheetIndex, ""),
        new Tile(1, 77, 15, 2133, tileSheetIndex, ""),
        new Tile(1, 78, 15, 2134, tileSheetIndex, ""),
        new Tile(1, 79, 15, 2213, tileSheetIndex, ""),
        new Tile(1, 72, 16, 2153, tileSheetIndex, ""),
        new Tile(1, 73, 16, 2154, tileSheetIndex, "")
      };
      return WonderfulFarmLife.WonderfulFarmLife.InitializeTileArray(gl, tileArray);
    }

    private static void PatchMap(GameLocation gl, List<Tile> tileArray)
    {
      try
      {
        foreach (Tile tile in tileArray)
        {
          if (tile.tileIndex < 0)
          {
            gl.removeTile(tile.x, tile.y, tile.layer);
            ((bool[,]) gl.waterTiles)[tile.x, tile.y] = false;
            using (List<LargeTerrainFeature>.Enumerator enumerator = ((List<LargeTerrainFeature>) gl.largeTerrainFeatures).GetEnumerator())
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
                if ((double) (^(Vector2&) @current.tilePosition).X == (double) tile.x && (double) (^(Vector2&) @current.tilePosition).Y == (double) tile.y)
                {
                  ((List<LargeTerrainFeature>) gl.largeTerrainFeatures).Remove(current);
                  break;
                }
              }
            }
          }
          else if (((Map) gl.map).GetLayer(tile.layer).get_Tiles().get_Item(tile.x, tile.y) == null || ((Component) ((Map) gl.map).GetLayer(tile.layer).get_Tiles().get_Item(tile.x, tile.y).get_TileSheet()).get_Id() != tile.tileSheet)
          {
            int tileSheetIndex = Tile.getTileSheetIndex("untitled tile sheet", ((Map) gl.map).get_TileSheets());
            ((Map) gl.map).GetLayer(tile.layer).get_Tiles().set_Item(tile.x, tile.y, (Tile) new StaticTile(((Map) gl.map).GetLayer(tile.layer), ((Map) gl.map).get_TileSheets()[tileSheetIndex], (BlendMode) 0, tile.tileIndex));
          }
          else
            gl.setMapTileIndex(tile.x, tile.y, tile.tileIndex, ((Component) ((Map) gl.map).GetLayer(tile.layer)).get_Id());
        }
      }
      finally
      {
        tileArray.Clear();
      }
    }

    private static List<Tile> InitializeTileArray(GameLocation gl, List<Tile> tileArray)
    {
      foreach (Tile tile in tileArray)
      {
        if (tile.layerIndex < 0 || tile.layerIndex >= ((Map) gl.map).get_Layers().Count)
          tile.layerIndex = Tile.getLayerIndex(tile.layer, ((Map) gl.map).get_Layers());
        if (tile.tileSheetIndex < 0 || tile.tileSheetIndex >= ((Map) gl.map).get_TileSheets().Count)
          tile.tileSheetIndex = Tile.getTileSheetIndex(tile.tileSheet, ((Map) gl.map).get_TileSheets());
        if (string.IsNullOrEmpty(tile.layer))
          tile.layer = Tile.getLayerName(tile.layerIndex, ((Map) gl.map).get_Layers());
        if (string.IsNullOrEmpty(tile.tileSheet))
          tile.tileSheet = Tile.getTileSheetName(tile.tileSheetIndex, ((Map) gl.map).get_TileSheets());
      }
      return tileArray;
    }

    public static Texture2D PatchTexture(Texture2D targetTexture, string overridingTexturePath, Dictionary<int, int> spriteOverrides, int gridWidth, int gridHeight)
    {
      int bottom = WonderfulFarmLife.WonderfulFarmLife.GetSourceRect(spriteOverrides.Values.Max(), targetTexture, gridWidth, gridHeight).Bottom;
      if (bottom > targetTexture.Height)
      {
        Color[] data1 = new Color[targetTexture.Width * targetTexture.Height];
        targetTexture.GetData<Color>(data1);
        Color[] data2 = new Color[targetTexture.Width * bottom];
        Array.Copy((Array) data1, (Array) data2, data1.Length);
        targetTexture = new Texture2D(((GraphicsDeviceManager) Game1.graphics).GraphicsDevice, targetTexture.Width, bottom);
        targetTexture.SetData<Color>(data2);
      }
      using (FileStream fileStream = new FileStream(Path.Combine(WonderfulFarmLife.WonderfulFarmLife.modPath, "overrides\\" + overridingTexturePath), FileMode.Open))
      {
        Texture2D texture = Texture2D.FromStream(((GraphicsDeviceManager) Game1.graphics).GraphicsDevice, (Stream) fileStream);
        foreach (KeyValuePair<int, int> spriteOverride in spriteOverrides)
        {
          Color[] data = new Color[gridWidth * gridHeight];
          texture.GetData<Color>(0, new Rectangle?(WonderfulFarmLife.WonderfulFarmLife.GetSourceRect(spriteOverride.Key, texture, gridWidth, gridHeight)), data, 0, data.Length);
          targetTexture.SetData<Color>(0, new Rectangle?(WonderfulFarmLife.WonderfulFarmLife.GetSourceRect(spriteOverride.Value, targetTexture, gridWidth, gridHeight)), data, 0, data.Length);
        }
      }
      return targetTexture;
    }

    private static Rectangle GetSourceRect(int index, Texture2D texture, int gridWidth, int gridHeight)
    {
      return new Rectangle(index % (texture.Width / gridWidth) * gridWidth, index / (texture.Width / gridWidth) * gridHeight, gridWidth, gridHeight);
    }
  }
}
