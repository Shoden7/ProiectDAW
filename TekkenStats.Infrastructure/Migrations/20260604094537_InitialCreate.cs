using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TekkenStats.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IngestionStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastBattleAt = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngestionStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PolarisId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TekkenUserId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrentRank = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DanRank = table.Column<int>(type: "INTEGER", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    Losses = table.Column<int>(type: "INTEGER", nullable: false),
                    MainCharacter = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    LinkedPolarisId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BattleId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Player1Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Player2Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Player1CharacterId = table.Column<string>(type: "TEXT", nullable: false),
                    Player2CharacterId = table.Column<string>(type: "TEXT", nullable: false),
                    Player1CharacterName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Player2CharacterName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Player1RankDan = table.Column<int>(type: "INTEGER", nullable: false),
                    Player2RankDan = table.Column<int>(type: "INTEGER", nullable: false),
                    Winner = table.Column<int>(type: "INTEGER", nullable: false),
                    Rounds = table.Column<int>(type: "INTEGER", nullable: false),
                    FoughtAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Region = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Players_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Players_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerCharacterStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    Losses = table.Column<int>(type: "INTEGER", nullable: false),
                    Rank = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DanRank = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCharacterStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerCharacterStats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookmarks_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookmarks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_PlayerId",
                table: "Bookmarks",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_UserId_PlayerId",
                table: "Bookmarks",
                columns: new[] { "UserId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_BattleId",
                table: "Matches",
                column: "BattleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_FoughtAt",
                table: "Matches",
                column: "FoughtAt");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_Player1Id",
                table: "Matches",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_Player2Id",
                table: "Matches",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCharacterStats_PlayerId_CharacterId",
                table: "PlayerCharacterStats",
                columns: new[] { "PlayerId", "CharacterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_DanRank",
                table: "Players",
                column: "DanRank");

            migrationBuilder.CreateIndex(
                name: "IX_Players_PlayerName",
                table: "Players",
                column: "PlayerName");

            migrationBuilder.CreateIndex(
                name: "IX_Players_PolarisId",
                table: "Players",
                column: "PolarisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_TekkenUserId",
                table: "Players",
                column: "TekkenUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookmarks");

            migrationBuilder.DropTable(
                name: "IngestionStates");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "PlayerCharacterStats");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
