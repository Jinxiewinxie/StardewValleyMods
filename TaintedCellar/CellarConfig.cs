using StardewModdingAPI;

namespace TaintedCellar
{
    public class CellarConfig : Config
    {
        public bool onlyUnlockAfterFinalHouseUpgrade { get; set; }

        public bool flipCellarEntrance { get; set; }

        public int xPositionOffset { get; set; }

        public int yPositionOffset { get; set; }

        public CellarConfig()
        {
            base.\u002Ector();
        }

        public virtual T GenerateDefaultConfig<T>() where T : Config
        {
            this.onlyUnlockAfterFinalHouseUpgrade = false;
            this.flipCellarEntrance = false;
            this.xPositionOffset = 0;
            this.yPositionOffset = 0;
            return this as T;
        }
    }
}