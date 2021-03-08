namespace PersonalFinance.Data.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <summary>
    /// A migration that moves overlapping properties of transactions and recurring transactions to table
    /// BaseTransactions.
    /// </summary>
    public partial class BaseTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create new table.
            migrationBuilder.CreateTable(
                name: "BaseTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    ReceivingAccountId = table.Column<int>(type: "int", nullable: true),
                    NeedsConfirmation = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaseTransactions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BaseTransactions_Accounts_ReceivingAccountId",
                        column: x => x.ReceivingAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaseTransactions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Add temporary column to table to know what old (recurring) transaction id was
            migrationBuilder.AddColumn<int>(
                name: "PreviousTransactionId",
                table: "BaseTransactions");

            // Copy data to BaseTransactions
            migrationBuilder.Sql(@"
                INSERT INTO [dbo].[BaseTransactions] 
                    (Description, Type, Amount, CategoryId, AccountId, ReceivingAccountId, NeedsConfirmation, PreviousTransactionId)
                (SELECT Description, Type, Amount, CategoryId, AccountId, ReceivingAccountId, NeedsConfirmation, Id
                 FROM [dbo].[RecurringTransactions])");
            migrationBuilder.Sql(@"
                INSERT INTO [dbo].[BaseTransactions] 
                    (Description, Type, Amount, CategoryId, AccountId, ReceivingAccountId, NeedsConfirmation, PreviousTransactionId)
                (SELECT Description, Type, Amount, CategoryId, AccountId, ReceivingAccountId, NeedsConfirmation, Id
                 FROM [dbo].[Transactions])");

            // Remove foreign keys such that the primary keys can be changed
            migrationBuilder.DropForeignKey(
                name: "FK_SplitDetails_Transactions_TransactionId",
                table: "SplitDetails");
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_Transactions_TransactionId",
                table: "PaymentRequests");
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_RecurringTransactions_RecurringTransactionId",
                table: "Transactions");

            // Update transaction ids to new values
            migrationBuilder.Sql(@"
                UPDATE sd
                    SET sd.TransactionId = bt.Id 
                FROM [dbo].[SplitDetails] sd, [dbo].[BaseTransactions] bt
                WHERE bt.PreviousTransactionId = sd.TransactionId");
            migrationBuilder.Sql(@"
                UPDATE pr
                    SET pr.TransactionId = bt.Id 
                FROM [dbo].[PaymentRequests] pr 
                    INNER JOIN [dbo].[BaseTransactions] bt
                        ON bt.PreviousTransactionId = pr.TransactionId");
            migrationBuilder.Sql(@"
                UPDATE t 
                    SET t.RecurringTransactionId = bt.Id 
                FROM [dbo].[Transactions] t 
                    INNER JOIN [dbo].[BaseTransactions] bt
                        ON bt.PreviousTransactionId = t.RecurringTransactionId");

            // Drop the moved columns.
            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTransactions_Accounts_AccountId",
                table: "RecurringTransactions");
            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTransactions_Accounts_ReceivingAccountId",
                table: "RecurringTransactions");
            migrationBuilder.DropForeignKey(
                name: "FK_RecurringTransactions_Categories_CategoryId",
                table: "RecurringTransactions");
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_AccountId",
                table: "Transactions");
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Accounts_ReceivingAccountId",
                table: "Transactions");
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions");
            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountId",
                table: "Transactions");
            migrationBuilder.DropIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions");
            migrationBuilder.DropIndex(
                name: "IX_Transactions_ReceivingAccountId",
                table: "Transactions");
            migrationBuilder.DropIndex(
                name: "IX_RecurringTransactions_AccountId",
                table: "RecurringTransactions");
            migrationBuilder.DropIndex(
                name: "IX_RecurringTransactions_CategoryId",
                table: "RecurringTransactions");
            migrationBuilder.DropIndex(
                name: "IX_RecurringTransactions_ReceivingAccountId",
                table: "RecurringTransactions");
            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Transactions");
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Transactions");
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Transactions");
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transactions");
            migrationBuilder.DropColumn(
                name: "NeedsConfirmation",
                table: "Transactions");
            migrationBuilder.DropColumn(
                name: "ReceivingAccountId",
                table: "Transactions");
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Transactions");
            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "RecurringTransactions");
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "RecurringTransactions");
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "RecurringTransactions");
            migrationBuilder.DropColumn(
                name: "Description",
                table: "RecurringTransactions");
            migrationBuilder.DropColumn(
                name: "NeedsConfirmation",
                table: "RecurringTransactions");
            migrationBuilder.DropColumn(
                name: "ReceivingAccountId",
                table: "RecurringTransactions");
            migrationBuilder.DropColumn(
                name: "Type",
                table: "RecurringTransactions");

            // Add the new id columns
            migrationBuilder.AddColumn<int>(
                name: "Id2",
                table: "RecurringTransactions");
            migrationBuilder.AddColumn<int>(
                name: "Id2",
                table: "Transactions");

            migrationBuilder.Sql(@"
                UPDATE rt 
                    SET rt.Id2 = bt.Id 
                FROM [dbo].[RecurringTransactions] rt 
                    INNER JOIN [dbo].[BaseTransactions] bt
                        ON bt.PreviousTransactionId = rt.Id");
            migrationBuilder.Sql(@"
                UPDATE t
                    SET t.Id2 = bt.Id 
                FROM [dbo].[Transactions] t 
                    INNER JOIN [dbo].[BaseTransactions] bt
                        ON bt.PreviousTransactionId = t.Id");

            // Drop the temporary column
            migrationBuilder.DropColumn(
                name: "PreviousTransactionId",
                table: "BaseTransactions");

            // Drop the old id columns that are no longer IDENTITY columns
            migrationBuilder.DropPrimaryKey(
                name: "PK_RecurringTransactions",
                table: "RecurringTransactions");
            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");
            migrationBuilder.DropColumn(
                name: "Id",
                table: "RecurringTransactions");
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Transactions");

            // Rename the new id columns
            migrationBuilder.RenameColumn(
                name: "Id2",
                table: "RecurringTransactions",
                newName: "Id");
            migrationBuilder.RenameColumn(
                name: "Id2",
                table: "Transactions",
                newName: "Id");

            // Mark id columns as primary keys
            migrationBuilder.AddPrimaryKey(
                name: "PK_RecurringTransactions",
                table: "RecurringTransactions",
                column: "Id");
            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            // Add foreign keys to new id columns
            migrationBuilder.AddForeignKey(
                name: "FK_RecurringTransactions_BaseTransactions_Id",
                table: "RecurringTransactions",
                column: "Id",
                principalTable: "BaseTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_BaseTransactions_Id",
                table: "Transactions",
                column: "Id",
                principalTable: "BaseTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Add indices and foreign keys to BaseTransactions table
            migrationBuilder.CreateIndex(
                name: "IX_BaseTransactions_AccountId",
                table: "BaseTransactions",
                column: "AccountId");
            migrationBuilder.CreateIndex(
                name: "IX_BaseTransactions_CategoryId",
                table: "BaseTransactions",
                column: "CategoryId");
            migrationBuilder.CreateIndex(
                name: "IX_BaseTransactions_ReceivingAccountId",
                table: "BaseTransactions",
                column: "ReceivingAccountId");
            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_BaseTransactions_TransactionId",
                table: "PaymentRequests",
                column: "TransactionId",
                principalTable: "BaseTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_SplitDetails_BaseTransactions_TransactionId",
                table: "SplitDetails",
                column: "TransactionId",
                principalTable: "BaseTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_RecurringTransactions_RecurringTransactionId",
                table: "Transactions",
                column: "RecurringTransactionId",
                principalTable: "RecurringTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
