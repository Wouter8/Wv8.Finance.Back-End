namespace PersonalFinance.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <summary>
    /// A class containing a migration that removes duplicate split details.
    /// </summary>
    public partial class FixDuplicateSplitDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the unique index as there will be duplicates by setting the correct values.
            migrationBuilder.DropIndex("IX_SplitDetails_TransactionId_SplitwiseUserId", "SplitDetails");

            // Set the transaction identifier of splits created for splitwise transactions.
            migrationBuilder.Sql(@"UPDATE sd
SET sd.[TransactionId] = t.[Id]
FROM [dbo].[SplitDetails] sd INNER JOIN [dbo].[Transactions] t ON t.[SplitwiseTransactionId] = sd.[SplitwiseTransactionId]
WHERE sd.[SplitwiseTransactionId] IS NOT NULL AND [TransactionId] IS NULL;
");

            // Remove duplicate entries.
            migrationBuilder.Sql(@"DELETE FROM [dbo].[SplitDetails]
WHERE SplitwiseTransactionId IS NULL AND (SELECT t.SplitwiseTransactionId FROM [dbo].[Transactions] t WHERE t.Id = TransactionId) IS NOT NULL;");

            // Re-add the index.
            migrationBuilder.CreateIndex(
                name: "IX_SplitDetails_TransactionId_SplitwiseUserId",
                table: "SplitDetails",
                columns: new[] { "TransactionId", "SplitwiseUserId" },
                unique: true,
                filter: "[TransactionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not implemented.
        }
    }
}
