namespace PersonalFinance.Business.Category
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;
    using Wv8.Core.EntityFramework;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// The manager for functionality related to categories.
    /// </summary>
    public class CategoryManager : BaseManager, ICategoryManager
    {
        private readonly CategoryValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public CategoryManager(Context context)
            : base(context)
        {
            this.validator = new CategoryValidator();
        }

        /// <inheritdoc />
        public Category GetCategory(int id)
        {
            return this.Context.Categories
                .Include(c => c.Icon)
                .Include(c => c.ParentCategory)
                .SingleOrNone(c => c.Id == id)
                .ValueOrThrow(() => new DoesNotExistException($"Category with identifier {id} does not exist."))
                .AsCategory();
        }

        /// <inheritdoc />
        public List<Category> GetCategories(bool includeObsolete)
        {
            return this.Context.Categories
                .Include(c => c.Icon)
                .Include(c => c.ParentCategory)
                .WhereIf(!includeObsolete, c => !c.IsObsolete)
                .OrderBy(c => c.Description)
                .Select(c => c.AsCategory())
                .ToList();
        }

        /// <inheritdoc />
        public List<Category> GetCategoriesByFilter(bool includeObsolete, CategoryType type)
        {
            return this.Context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Icon)
                .WhereIf(!includeObsolete, c => !c.IsObsolete)
                .Where(c => c.Type == type)
                .OrderBy(c => c.Description)
                .Select(c => c.AsCategory())
                .ToList();
        }

        /// <inheritdoc />
        public Category UpdateCategory(int id, string description, CategoryType type, Maybe<int> parentCategoryId, string iconPack, string iconName, string iconColor)
        {
            description = this.validator.Description(description);
            this.validator.Icon(iconPack, iconName, iconColor);

            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Categories
                    .Include(c => c.Icon)
                    .Include(c => c.ParentCategory)
                    .SingleOrNone(c => c.Id == id)
                    .ValueOrThrow(() => new DoesNotExistException($"Category with identifier {id} does not exist."));

                if (this.Context.Categories.Any(c => c.Id != id && c.Description == description && !c.IsObsolete))
                    throw new ValidationException($"An active category with description \"{description}\" already exists.");

                CategoryEntity parentCategory = null;
                if (parentCategoryId.IsSome)
                {
                    parentCategory = this.Context.Categories
                        .SingleOrNone(c => c.Id == parentCategoryId.Value)
                        .ValueOrThrow(() => new DoesNotExistException($"Parent category with identifier {parentCategoryId.Value} does not exist."));

                    if (parentCategory.IsObsolete)
                        throw new ValidationException($"Parent category \"{parentCategory.Description}\" is obsolete.");

                    if (parentCategory.Type != type)
                        throw new ValidationException($"Parent category has different category type.");
                }

                entity.Description = description;
                entity.Type = type;
                entity.ParentCategoryId = parentCategoryId.ToNullable();
                entity.ParentCategory = parentCategory;

                entity.Icon.Name = iconName;
                entity.Icon.Pack = iconPack;
                entity.Icon.Color = iconColor;

                this.Context.SaveChanges();

                return entity.AsCategory();
            });
        }

        /// <inheritdoc />
        public Category CreateCategory(string description, CategoryType type, Maybe<int> parentCategoryId, string iconPack, string iconName, string iconColor)
        {
            description = this.validator.Description(description);
            this.validator.Icon(iconPack, iconName, iconColor);

            return this.ConcurrentInvoke(() =>
            {
                if (this.Context.Categories.Any(c => c.Description == description && !c.IsObsolete))
                    throw new ValidationException($"An active category with description \"{description}\" already exists.");

                CategoryEntity parentCategory = null;
                if (parentCategoryId.IsSome)
                {
                    parentCategory = this.Context.Categories
                        .SingleOrNone(c => c.Id == parentCategoryId.Value)
                        .ValueOrThrow(() => new DoesNotExistException($"Parent category with identifier {parentCategoryId.Value} does not exist."));

                    if (parentCategory.IsObsolete)
                        throw new ValidationException($"Parent category \"{parentCategory.Description}\" is obsolete.");

                    if (parentCategory.Type != type)
                        throw new ValidationException($"Parent category has different category type.");
                }

                var entity = new CategoryEntity
                {
                    Description = description,
                    Type = type,
                    ParentCategoryId = parentCategoryId.ToNullable(),
                    ParentCategory = parentCategory,
                    IsObsolete = false,
                    Icon = new IconEntity
                    {
                        Pack = iconPack,
                        Name = iconName,
                        Color = iconColor,
                    },
                };

                this.Context.Categories.Add(entity);
                this.Context.SaveChanges();

                return entity.AsCategory();
            });
        }

        /// <inheritdoc />
        public void SetCategoryObsolete(int id, bool obsolete)
        {
            this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Categories
                    .SingleOrNone(c => c.Id == id)
                    .ValueOrThrow(() => new DoesNotExistException($"Category with identifier {id} does not exist."));

                if (obsolete)
                {
                    // Set all child categories to obsolete as well.
                    this.SetChildrenObsolete(entity);
                }
                else
                {
                    // Validate that no other active category exists with the same description.
                    if (this.Context.Categories.Any(c => c.Description == entity.Description && !c.IsObsolete && c.Id != entity.Id))
                        throw new ValidationException($"An active category with description \"{entity.Description}\" already exists. Change the description of that category first.");
                }

                entity.IsObsolete = obsolete;

                this.Context.SaveChanges();
            });
        }

        /// <summary>
        /// Recursively set all child categories to obsolete.
        /// </summary>
        /// <param name="entity">The parent entity.</param>
        private void SetChildrenObsolete(CategoryEntity entity)
        {
            var children = this.Context.Categories
                .Where(c => c.ParentCategoryId == entity.Id)
                .ToList();

            foreach (var child in children)
            {
                child.IsObsolete = true;
                this.SetChildrenObsolete(child);
            }
        }
    }
}