using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class addedfollowers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowerUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FollowerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FollowedUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FollowedId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowUsers_AspNetUsers_FollowedId",
                        column: x => x.FollowedId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FollowUsers_AspNetUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowUsers_FollowedId",
                table: "FollowUsers",
                column: "FollowedId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUsers_FollowerId",
                table: "FollowUsers",
                column: "FollowerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowUsers");
        }
    }
}
