namespace PersonalFinance.Common.Exceptions
{
    using Wv8.Core.Exceptions;

    /// <summary>
    /// An exception that can be thrown if a object is obsolete.
    /// </summary>
    public class IsObsoleteException : CustomException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsObsoleteException"/> class.
        /// </summary>
        public IsObsoleteException()
            : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IsObsoleteException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public IsObsoleteException(string message)
            : base(message) { }
    }
}