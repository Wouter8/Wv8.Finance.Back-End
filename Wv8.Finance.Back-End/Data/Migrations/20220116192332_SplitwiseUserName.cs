namespace PersonalFinance.Data.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <summary>
    /// A migration which adds the names of Splitwise users to the Splitwise splits.
    /// </summary>
    public partial class SplitwiseUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SplitwiseUserName",
                table: "SplitDetails",
                type: "nvarchar(max)",
                nullable: true); // First make it nullable since the existing rows will have no value.

            // Then set all the names.
            migrationBuilder.Sql(@"UPDATE dbo.SplitDetails
SET SplitwiseUserName = 
    CASE
        WHEN SplitwiseUserId = 37284415 THEN 'Wouter'
        WHEN SplitwiseUserId = 38673381 THEN 'Brent'
        WHEN SplitwiseUserId = 38636920 THEN 'Stef'
        WHEN SplitwiseUserId = 38637278 THEN 'Stefan'
        WHEN SplitwiseUserId = 38650805 THEN 'Britt'
        WHEN SplitwiseUserId = 38651320 THEN 'Emma Van De Wijdeven'
        WHEN SplitwiseUserId = 45681221 THEN 'Luke'
        ELSE 'Onbekend'
    END");

            // Lastly make the column not nullable.
            migrationBuilder.AlterColumn<string>(name: "SplitwiseUserName", table: "SplitDetails", nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SplitwiseUserName",
                table: "SplitDetails");
        }
    }
}
