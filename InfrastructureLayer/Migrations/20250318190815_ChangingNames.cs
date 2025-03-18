using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class ChangingNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileImageUrl",
                table: "AspNetUsers",
                newName: "ProfileImage");

            migrationBuilder.RenameColumn(
                name: "BackgroundImageUrl",
                table: "AspNetUsers",
                newName: "BackgroundImage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileImage",
                table: "AspNetUsers",
                newName: "ProfileImageUrl");

            migrationBuilder.RenameColumn(
                name: "BackgroundImage",
                table: "AspNetUsers",
                newName: "BackgroundImageUrl");
        }
    }
}
