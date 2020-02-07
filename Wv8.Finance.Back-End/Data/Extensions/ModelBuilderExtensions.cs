namespace PersonalFinance.Data.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// A class containing extension methods for the model builder of the database context.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Adds the required properties to the fields of all entities.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static void BuildEntities(this ModelBuilder builder)
        {
            builder
                .BuildIconEntity()
                .BuildAccountEntity()
                .BuildCategoryEntity();
        }

        /// <summary>
        /// Adds the required properties to the fields of the icon entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The updated builder.</returns>
        private static ModelBuilder BuildIconEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<IconEntity>();

            entity.Property(e => e.Pack).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Color).IsRequired();

            return builder;
        }

        /// <summary>
        /// Adds the required properties to the fields of the account entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The updated builder.</returns>
        private static ModelBuilder BuildAccountEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<AccountEntity>();

            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.CurrentBalance).HasPrecision(12, 2);

            return builder;
        }

        /// <summary>
        /// Adds the required properties to the fields of the category entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The updated builder.</returns>
        private static ModelBuilder BuildCategoryEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<CategoryEntity>();

            entity.Property(e => e.Description).IsRequired();

            return builder;
        }
    }
}