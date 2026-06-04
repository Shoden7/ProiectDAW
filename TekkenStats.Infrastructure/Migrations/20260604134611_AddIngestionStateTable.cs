using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TekkenStats.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIngestionStateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastBattleAt",
                table: "IngestionStates",
                newName: "LastBattleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastBattleId",
                table: "IngestionStates",
                newName: "LastBattleAt");
        }
    }
}
