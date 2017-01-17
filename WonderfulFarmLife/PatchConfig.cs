using StardewModdingAPI;

namespace WonderfulFarmLife
{
    public class PatchConfig : Config
    {
        public bool EditPath { get; set; }

        public bool RemovePathAlltogether { get; set; }

        public bool ShowFarmStand { get; set; }

        public bool RemoveShippingBin { get; set; }

        public bool ShowPatio { get; set; }

        public bool ShowBinClutter { get; set; }

        public bool AddDogHouse { get; set; }

        public bool AddGreenHouseArch { get; set; }

        public bool AddTelescopeArea { get; set; }

        public bool ShowTreeSwing { get; set; }

        public bool ShowPicnicTable { get; set; }

        public bool ShowPicnicBlanket { get; set; }

        public bool AddStoneBridge { get; set; }

        public bool ShowMemorialArea { get; set; }

        public bool ShowMemorialAreaArch { get; set; }

        public bool UsingTSE { get; set; }

        public override T GenerateDefaultConfig<T>()
        {
            this.EditPath = true;
            this.RemovePathAlltogether = false;
            this.ShowFarmStand = true;
            this.RemoveShippingBin = true;
            this.ShowPatio = true;
            this.ShowBinClutter = true;
            this.AddDogHouse = true;
            this.AddGreenHouseArch = true;
            this.AddTelescopeArea = true;
            this.ShowTreeSwing = true;
            this.ShowPicnicTable = true;
            this.ShowPicnicBlanket = true;
            this.AddStoneBridge = true;
            this.ShowMemorialArea = true;
            this.ShowMemorialAreaArch = true;
            this.UsingTSE = true;
            return this as T;
        }
    }
}