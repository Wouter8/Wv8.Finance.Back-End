using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalFinance.Data.Migrations
{
    public partial class FixTableNaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyBalanceEntity_Accounts_AccountId",
                table: "DailyBalanceEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DailyBalanceEntity",
                table: "DailyBalanceEntity");

            migrationBuilder.RenameTable(
                name: "DailyBalanceEntity",
                newName: "DailyBalances");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DailyBalances",
                table: "DailyBalances",
                columns: new[] { "AccountId", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_DailyBalances_Accounts_AccountId",
                table: "DailyBalances",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyBalances_Accounts_AccountId",
                table: "DailyBalances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DailyBalances",
                table: "DailyBalances");

            migrationBuilder.RenameTable(
                name: "DailyBalances",
                newName: "DailyBalanceEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DailyBalanceEntity",
                table: "DailyBalanceEntity",
                columns: new[] { "AccountId", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_DailyBalanceEntity_Accounts_AccountId",
                table: "DailyBalanceEntity",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
