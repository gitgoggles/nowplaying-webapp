using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nowplaying_webapp.Migrations
{
    /// <inheritdoc />
    public partial class JellyfinUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JellyfinUserName",
                table: "Configs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JellyfinUserName",
                table: "Configs");
        }
    }
}
