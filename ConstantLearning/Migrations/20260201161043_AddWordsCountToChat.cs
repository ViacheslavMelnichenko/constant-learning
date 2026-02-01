using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstantLearning.Migrations
{
    /// <inheritdoc />
    public partial class AddWordsCountToChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewWordsCount",
                table: "ChatRegistrations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RepetitionWordsCount",
                table: "ChatRegistrations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewWordsCount",
                table: "ChatRegistrations");

            migrationBuilder.DropColumn(
                name: "RepetitionWordsCount",
                table: "ChatRegistrations");
        }
    }
}
