namespace PersonalFinance.Data.Models
{
    /// <summary>
    /// An entity representing an icon.
    /// </summary>
    public class IconEntity
    {
        /// <summary>
        /// The identifier of this icon.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The icon pack this icon belongs to.
        /// </summary>
        public string IconPack { get; set; }

        /// <summary>
        /// The name of the icon.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The hexadecimal color value of the background.
        /// </summary>
        public string Color { get; set; }
    }
}