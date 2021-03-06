﻿namespace PersonalFinance.Business
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using Wv8.Core;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// Base class for specific managers providing base functionality.
    /// </summary>
    public abstract class BaseManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseManager"/> class.
        /// Note that the base manager can not be instantiated on its own.
        /// </summary>
        /// <param name="context">The database context.</param>
        protected BaseManager(Context context)
        {
            this.Context = context;
        }

        /// <summary>
        /// The database context to be used to talk with the database.
        /// </summary>
        protected Context Context { get; set; }

        /// <summary>
        /// Invokes the given action with a number of retry attempts relative to the given exception type
        /// <see cref="DbUpdateConcurrencyException"/> which may be raised by the action. This method will either
        /// succeed within this many attempts, throw an exception of type <see cref="DbUpdateConcurrencyException"/>
        /// when all attempts are used or throw another exception at any time, depending on the provided action.
        /// </summary>
        /// <param name="action">The action that should be invoked with 10 retries.</param>
        protected void ConcurrentInvoke(Action action)
        {
            ExceptionHelpers.InvokeWithRetries<DbUpdateConcurrencyException>(action);
        }

        /// <summary>
        /// Invokes the given function with a number of retry attempts relative to the given exception type
        /// <see cref="DbUpdateConcurrencyException"/> which may be raised by the function. This method will either
        /// succeed within this many attempts, throw an exception of type <see cref="DbUpdateConcurrencyException"/>
        /// when all attempts are used or throw another exception at any time, depending on the provided function.
        /// </summary>
        /// <typeparam name="T">The type of the result of the function.</typeparam>
        /// <param name="func">The function that should be invoked with 10 retries.</param>
        /// <returns>The result of <paramref name="func"/>.</returns>
        protected T ConcurrentInvoke<T>(Func<T> func)
        {
            return ExceptionHelpers.InvokeWithRetries<DbUpdateConcurrencyException, T>(func);
        }

        /// <summary>
        /// Validates the input to determine the transaction type and return the appropriate one.
        /// A transaction with a category is an external transaction,
        /// while a transaction with a receiving account is an internal account.
        /// </summary>
        /// <param name="categoryId">The category id input.</param>
        /// <param name="receivingAccountId">The receiving account id input.</param>
        /// <param name="amount">The amount input.</param>
        /// <returns>The transaction type that corresponds to the input.</returns>
        protected TransactionType GetTransactionType(Maybe<int> categoryId, Maybe<int> receivingAccountId, decimal amount)
        {
            if (categoryId.IsSome && receivingAccountId.IsSome)
                throw new ValidationException("A transaction can not specify a category and receiving account.");
            if (categoryId.IsNone && receivingAccountId.IsNone)
                throw new ValidationException("A transaction must specify either a category or receiving account.");

            var type = categoryId.IsSome
                ? amount < 0
                    ? TransactionType.Expense
                    : TransactionType.Income
                : TransactionType.Transfer;

            if (type == TransactionType.Transfer && amount <= 0)
                throw new ValidationException($"The amount has to be greater than 0.");

            return type;
        }
    }
}