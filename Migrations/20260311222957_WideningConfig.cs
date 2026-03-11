using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace nowplaying_webapp.Migrations
{
    /// <inheritdoc />
    public partial class WideningConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Configs",
                newName: "PlexUrl");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Configs",
                newName: "PlexApi");

            migrationBuilder.AddColumn<string>(
                name: "JellyfinApi",
                table: "Configs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "JellyfinEnabled",
                table: "Configs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JellyfinUrl",
                table: "Configs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "PlexEnabled",
                table: "Configs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JellyfinApi",
                table: "Configs");

            migrationBuilder.DropColumn(
                name: "JellyfinEnabled",
                table: "Configs");

            migrationBuilder.DropColumn(
                name: "JellyfinUrl",
                table: "Configs");

            migrationBuilder.DropColumn(
                name: "PlexEnabled",
                table: "Configs");

            migrationBuilder.RenameColumn(
                name: "PlexUrl",
                table: "Configs",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "PlexApi",
                table: "Configs",
                newName: "Name");
        }
    }
}
