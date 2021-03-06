﻿// <auto-generated />

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalFinance.Data.Migrations
{
    public partial class SplitwiseTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SplitwiseTransactionId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SplitwiseTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    PersonalAmount = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Imported = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SplitwiseTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SplitwiseTransactionId",
                table: "Transactions",
                column: "SplitwiseTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_SplitwiseTransactions_SplitwiseTransactionId",
                table: "Transactions",
                column: "SplitwiseTransactionId",
                principalTable: "SplitwiseTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_SplitwiseTransactions_SplitwiseTransactionId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "SplitwiseTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SplitwiseTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SplitwiseTransactionId",
                table: "Transactions");
        }
    }
}
