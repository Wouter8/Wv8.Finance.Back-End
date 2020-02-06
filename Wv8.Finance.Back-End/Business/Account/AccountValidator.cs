namespace PersonalFinance.Business.Account
{
    /// <summary>
    /// The validator for all fields related to accounts.
    /// </summary>
    public class AccountValidator : BaseValidator
    {
        private const int MinDescriptionLength = 3;
        private const int MaxDescriptionLength = 32;

        /// <summary>
        /// Validates and normalizes the description of an account.
        /// </summary>
        /// <param name="description">The input.</param>
        /// <returns>The normalized input.</returns>
        public string Description(string description)
        {
            this.NotEmpty(description, nameof(description));

            description = description.Trim();

            this.InRange(description, MinDescriptionLength, MaxDescriptionLength, nameof(description));

            return description;
        }
    }
}