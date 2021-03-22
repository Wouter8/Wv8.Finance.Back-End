namespace Business.UnitTest.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Wv8.Core;
    using Xunit;

    /// <summary>
    /// A class containing assertion methods.
    /// </summary>
    public static class Wv8Assert
    {
        /// <summary>
        /// Asserts that an exception is thrown with a specific message.
        /// </summary>
        /// <param name="testCode">The code that should throw the exception.</param>
        /// <param name="exceptionMessage">The expected message of the exception.</param>
        /// <typeparam name="TException">The type of the expected message.</typeparam>
        public static void Throws<TException>(Func<object> testCode, string exceptionMessage)
            where TException : Exception
        {
            var exception = Xunit.Assert.Throws<TException>(testCode);
            Xunit.Assert.Contains(exceptionMessage, exception.Message);
        }

        /// <summary>
        /// Asserts that an exception is thrown with a specific message.
        /// </summary>
        /// <param name="testCode">The code that should throw the exception.</param>
        /// <param name="exceptionMessage">The expected message of the exception.</param>
        /// <typeparam name="TException">The type of the expected message.</typeparam>
        public static void Throws<TException>(Action testCode, string exceptionMessage)
            where TException : Exception
        {
            var exception = Xunit.Assert.Throws<TException>(testCode);
            Xunit.Assert.Contains(exceptionMessage, exception.Message);
        }

        /// <summary>
        /// Asserts that an exception is thrown with a specific message.
        /// </summary>
        /// <param name="testCode">The code that should throw the exception.</param>
        /// <param name="exceptionMessage">The expected message of the exception.</param>
        /// <typeparam name="TException">The type of the expected message.</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ThrowsAsync<TException>(Func<Task> testCode, string exceptionMessage)
            where TException : Exception
        {
            var exception = await Xunit.Assert.ThrowsAsync<TException>(testCode);
            Xunit.Assert.Contains(exceptionMessage, exception.Message);
        }

        /// <summary>
        /// Asserts that a <see cref="Maybe"/> is <c>Some</c>.
        /// </summary>
        /// <param name="maybe">The value.</param>
        /// <typeparam name="T">The type of the Maybe.</typeparam>
        public static void IsSome<T>(Maybe<T> maybe)
        {
            Assert.True(maybe.IsSome);
        }

        /// <summary>
        /// Asserts that a <see cref="Maybe"/> is <c>None</c>.
        /// </summary>
        /// <param name="maybe">The value.</param>
        /// <typeparam name="T">The type of the Maybe.</typeparam>
        public static void IsNone<T>(Maybe<T> maybe)
        {
            Assert.True(maybe.IsNone);
        }
    }
}