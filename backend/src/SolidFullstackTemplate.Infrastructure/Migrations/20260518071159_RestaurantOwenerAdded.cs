using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolidFullstackTemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestaurantOwenerAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Restaurants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Restaurants"
                SET "OwnerId" = (
                    select "AspNetUserRoles"."UserId"
                        from "AspNetUsers"
                            inner join "AspNetUserRoles" on "AspNetUsers"."Id" = "AspNetUserRoles"."UserId"
                            inner join "AspNetRoles" on "AspNetRoles"."Id" = "AspNetUserRoles"."RoleId"
                        where "AspNetRoles"."Name" = 'Admin'
                )
            """);

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_OwnerId",
                table: "Restaurants",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_AspNetUsers_OwnerId",
                table: "Restaurants",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_AspNetUsers_OwnerId",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_OwnerId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Restaurants");
        }
    }
}
