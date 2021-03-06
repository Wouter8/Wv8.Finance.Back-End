﻿namespace PersonalFinance.Business.Category
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
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
            return this.Context.Categories.GetEntity(id).AsCategory();
        }

        /// <inheritdoc />
        public List<Category> GetCategories(bool includeObsolete, bool group)
        {
            return this.Context.Categories
                .IncludeAll()
                .WhereIf(!includeObsolete, c => !c.IsObsolete)
                .WhereIf(group, c => !c.ParentCategoryId.HasValue)
                .OrderBy(c => c.Description)
                .Select(c => c.AsCategory(includeObsolete))
                .ToList();
        }

        /// <inheritdoc />
        public List<Category> GetCategoriesByFilter(bool includeObsolete, bool group)
        {
            return this.Context.Categories
                .IncludeAll()
                .WhereIf(!includeObsolete, c => !c.IsObsolete)
                .WhereIf(group, c => !c.ParentCategoryId.HasValue)
                .OrderBy(c => c.Description)
                .Select(c => c.AsCategory(includeObsolete))
                .ToList();
        }

        /// <inheritdoc />
        public Category UpdateCategory(
            int id,
            string description,
            Maybe<decimal> expectedMonthlyAmount,
            Maybe<int> parentCategoryId,
            string iconPack,
            string iconName,
            string iconColor)
        {
            description = this.validator.Description(description);
            this.validator.Icon(iconPack, iconName, iconColor);

            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Categories.GetEntity(id);

                CategoryEntity parentCategory = null;
                if (parentCategoryId.IsSome)
                {
                    parentCategory = this.Context.Categories.GetEntity(parentCategoryId.Value);

                    if (parentCategory.IsObsolete)
                        throw new ValidationException($"Parent category \"{parentCategory.Description}\" is obsolete.");

                    if (parentCategory.Description == description)
                        throw new ValidationException($"The same description as parent \"{parentCategory.Description}\" is not allowed.");

                    if (parentCategory.Children.Any(c => c.Id != id && c.Description == description && !c.IsObsolete))
                        throw new ValidationException($"An active category with description \"{description}\" already exists under \"{parentCategory.Description}\".");

                    if (parentCategory.ExpectedMonthlyAmount.HasValue && expectedMonthlyAmount.IsSome)
                    {
                        var expectedParent = Math.Abs(parentCategory.ExpectedMonthlyAmount.Value);
                        if (expectedParent < Math.Abs(expectedMonthlyAmount.Value))
                        {
                            throw new ValidationException(
                                $"Expected monthly amount can not exceed expected monthly amount of \"{parentCategory.Description}\" ({expectedParent}).");
                        }

                        var totalExpectedChildren =
                            parentCategory.Children.Sum(c => Math.Abs(c.ExpectedMonthlyAmount.GetValueOrDefault(0))) -
                            Math.Abs(entity.ExpectedMonthlyAmount.GetValueOrDefault(0)) + Math.Abs(expectedMonthlyAmount.Value);
                        if (totalExpectedChildren > expectedParent)
                        {
                            throw new ValidationException(
                                $"Expected monthly amount of all child categories ({totalExpectedChildren}) will exceed the expected monthly amount of \"{parentCategory.Description}\" ({expectedParent}).");
                        }
                    }
                }
                else
                {
                    if (this.Context.Categories.Any(c => c.Id != id && !c.ParentCategoryId.HasValue && c.Description == description && !c.IsObsolete))
                        throw new ValidationException($"An active category with description \"{description}\" already exists.");

                    if (entity.Children.Any() && expectedMonthlyAmount.IsSome)
                    {
                        var totalExpectedChildren =
                            entity.Children.Sum(c => Math.Abs(c.ExpectedMonthlyAmount.GetValueOrDefault(0)));
                        if (totalExpectedChildren > Math.Abs(expectedMonthlyAmount.Value))
                        {
                            throw new ValidationException(
                                $"Expected monthly amount of the child categories ({totalExpectedChildren}) of \"{entity.Description}\" will exceed the expected monthly amount.");
                        }
                    }
                }

                entity.Description = description;
                entity.ExpectedMonthlyAmount = expectedMonthlyAmount.ToNullable();
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
        public Category CreateCategory(
            string description,
            Maybe<decimal> expectedMonthlyAmount,
            Maybe<int> parentCategoryId,
            string iconPack,
            string iconName,
            string iconColor)
        {
            description = this.validator.Description(description);
            this.validator.Icon(iconPack, iconName, iconColor);

            return this.ConcurrentInvoke(() =>
            {
                CategoryEntity parentCategory = null;
                if (parentCategoryId.IsSome)
                {
                    parentCategory = this.Context.Categories.GetEntity(parentCategoryId.Value);

                    if (parentCategory.IsObsolete)
                        throw new ValidationException($"Parent category \"{parentCategory.Description}\" is obsolete.");

                    if (parentCategory.Description == description)
                        throw new ValidationException($"The same description as parent \"{parentCategory.Description}\" is not allowed.");

                    if (parentCategory.Children.Any(c => c.Description == description && !c.IsObsolete))
                        throw new ValidationException($"An active category with description \"{description}\" already exists under \"{parentCategory.Description}\".");

                    if (parentCategory.ExpectedMonthlyAmount.HasValue && expectedMonthlyAmount.IsSome)
                    {
                        var expectedParent = Math.Abs(parentCategory.ExpectedMonthlyAmount.Value);
                        if (expectedParent < Math.Abs(expectedMonthlyAmount.Value))
                        {
                            throw new ValidationException(
                                $"Expected monthly amount can not exceed expected monthly amount of \"{parentCategory.Description}\" ({expectedParent}).");
                        }

                        var totalExpectedChildren =
                            parentCategory.Children.Sum(c => Math.Abs(c.ExpectedMonthlyAmount.GetValueOrDefault(0))) +
                            Math.Abs(expectedMonthlyAmount.Value);
                        if (totalExpectedChildren > expectedParent)
                        {
                            throw new ValidationException(
                                $"Expected monthly amount of all child categories ({totalExpectedChildren}) will exceed the expected monthly amount of \"{parentCategory.Description}\" ({expectedParent}).");
                        }
                    }
                }
                else
                {
                    if (this.Context.Categories.Any(c => !c.ParentCategoryId.HasValue && c.Description == description && !c.IsObsolete))
                        throw new ValidationException($"An active category with description \"{description}\" already exists.");
                }

                var entity = new CategoryEntity
                {
                    Description = description,
                    ExpectedMonthlyAmount = expectedMonthlyAmount.ToNullable(),
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
                var entity = this.Context.Categories.GetEntity(id);

                if (entity.ParentCategoryId.HasValue && entity.ParentCategory.IsObsolete)
                    throw new ValidationException($"Parent category \"{entity.ParentCategory.Description}\" is obsolete.");

                if (obsolete)
                {
                    // Set all child categories to obsolete as well.
                    this.SetChildrenObsolete(entity);
                }
                else
                {
                    // Validate that no other active category exists with the same description.
                    if (entity.ParentCategoryId.HasValue && entity.ParentCategory.Description == entity.Description)
                    {
                        throw new ValidationException(
                            $"The same description as parent \"{entity.ParentCategory.Description}\" is not allowed.");
                    }

                    if (this.Context.Categories.Any(c =>
                        c.Id != entity.Id &&
                        c.ParentCategoryId == entity.ParentCategoryId &&
                        c.Description == entity.Description &&
                        !c.IsObsolete))
                    {
                        throw new ValidationException(
                            entity.ParentCategoryId.HasValue
                            ? $"An active category with description \"{entity.Description}\" already exists under \"{entity.ParentCategory.Description}\"."
                            : $"An active category with description \"{entity.Description}\" already exists.");
                    }
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