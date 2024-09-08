using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FUFC.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightClassToBout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WeightClass",
                table: "Bouts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeightClass",
                table: "Bouts");
        }
    }
}
