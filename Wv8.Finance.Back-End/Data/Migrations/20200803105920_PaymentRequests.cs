﻿// <auto-generated />

using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalFinance.Data.Migrations
{
    public partial class PaymentRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Transaction type 'External' (1) changed to 'Income' (2) and 'Expense' (1)
            // Transaction type 'Internal' (2) changed to (3).
            
            migrationBuilder.Sql(@"UPDATE [dbo].[Transactions] SET [Type] = 3 WHERE [Type] = 2;");
            migrationBuilder.Sql(@"UPDATE [dbo].[Transactions] SET [Type] = 2 WHERE [Type] = 1 AND [Amount] > 0;");
            
            migrationBuilder.Sql(@"UPDATE [dbo].[RecurringTransactions] SET [Type] = 3 WHERE [Type] = 2;");
            migrationBuilder.Sql(@"UPDATE [dbo].[RecurringTransactions] SET [Type] = 2 WHERE [Type] = 1 AND [Amount] > 0;");
            
            migrationBuilder.CreateTable(
                name: "PaymentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    PaidCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRequests_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_TransactionId",
                table: "PaymentRequests",
                column: "TransactionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentRequests");
        }
    }
}