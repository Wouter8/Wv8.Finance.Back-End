namespace Business.UnitTest.Helpers
{
    using System;

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
    }
}