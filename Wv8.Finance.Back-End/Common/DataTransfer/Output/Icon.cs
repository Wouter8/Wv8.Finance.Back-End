namespace PersonalFinance.Common.DataTransfer.Output
{
    /// <summary>
    /// Data transfer object for an icon.
    /// </summary>
    public class Icon
    {
        /// <summary>
        /// The identifier of this icon.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The icon pack this icon belongs to.
        /// </summary>
        public string Pack { get; set; }

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