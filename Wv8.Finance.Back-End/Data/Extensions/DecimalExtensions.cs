namespace PersonalFinance.Data.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// A class containing extension methods for model building. Adding the missing HasPrecision methods.
    /// </summary>
    public static class DecimalExtensions
    {
        /// <summary>
        /// Generates the column type for a decimal property.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="precision">The precision of the property.</param>
        /// <param name="scale">The scale of the property.</param>
        /// <returns>The updated builder.</returns>
        public static PropertyBuilder<decimal?> HasPrecision(this PropertyBuilder<decimal?> builder, int precision, int scale)
        {
            return builder.HasColumnType($"decimal({precision},{scale})");
        }

        /// <summary>
        /// Generates the column type for a decimal property.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="precision">The precision of the property.</param>
        /// <param name="scale">The scale of the property.</param>
        /// <returns>The updated builder.</returns>
        public static PropertyBuilder<decimal> HasPrecision(this PropertyBuilder<decimal> builder, int precision, int scale)
        {
            return builder.HasColumnType($"decimal({precision},{scale})");
        }
    }
}