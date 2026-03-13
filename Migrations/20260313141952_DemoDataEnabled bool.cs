using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nowplaying_webapp.Migrations
{
    /// <inheritdoc />
    public partial class DemoDataEnabledbool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DemoDataEnabled",
                table: "Configs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DemoDataEnabled",
                table: "Configs");
        }
    }
}
