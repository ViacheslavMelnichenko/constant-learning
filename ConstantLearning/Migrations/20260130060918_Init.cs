using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConstantLearning.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    ChatTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RepetitionTime = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    NewWordsTime = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRegistrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TargetWord = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceMeaning = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PhoneticTranscription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FrequencyRank = table.Column<int>(type: "integer", nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LearnedWords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    WordId = table.Column<int>(type: "integer", nullable: false),
                    LearnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastRepeatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RepetitionCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnedWords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnedWords_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatRegistrations_ChatId",
                table: "ChatRegistrations",
                column: "ChatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnedWords_ChatId_WordId",
                table: "LearnedWords",
                columns: new[] { "ChatId", "WordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearnedWords_WordId",
                table: "LearnedWords",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_Words_FrequencyRank",
                table: "Words",
                column: "FrequencyRank");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatRegistrations");

            migrationBuilder.DropTable(
                name: "LearnedWords");

            migrationBuilder.DropTable(
                name: "Words");
        }
    }
}
