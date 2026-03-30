using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nowplaying_webapp.Migrations
{
    /// <inheritdoc />
    public partial class PlexUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlexUserName",
                table: "Configs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlexUserName",
                table: "Configs");
        }
    }
}
