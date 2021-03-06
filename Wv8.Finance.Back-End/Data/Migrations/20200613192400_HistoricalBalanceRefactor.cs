﻿// <auto-generated />

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PersonalFinance.Data.Migrations
{
    public partial class HistoricalBalanceRefactor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyBalanceEntity",
                columns: table => new
                {
                    AccountId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyBalanceEntity", x => new { x.AccountId, x.Date });
                    table.ForeignKey(
                        name: "FK_DailyBalanceEntity_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyBalanceEntity");
        }
    }
}
