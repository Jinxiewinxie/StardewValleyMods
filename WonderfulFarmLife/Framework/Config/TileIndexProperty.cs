namespace WonderfulFarmLife.Framework.Config
{
    /// <summary>Defines a property to set on a tilesheet.</summary>
    internal class TileIndexProperty
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The tilesheet to edit.</summary>
        public string Tilesheet { get; set; }

        /// <summary>The property key.</summary>
        public string Key { get; set; }

        /// <summary>The property value.</summary>
        public string Value { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="tilesheet">The tilesheet ID.</param>
        /// <param name="id">The tile ID.</param>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public TileIndexProperty(string tilesheet, int id, string key, string value)
        {
            this.Tilesheet = tilesheet;
            this.Key = $"@TileIndex@{id}@{key}";
            this.Value = value;
        }
    }
}
