using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearningOnlineSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystemNotificationsCreatorToUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Creator",
                table: "SystemNotifications");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "SystemNotifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SystemNotifications_UserId",
                table: "SystemNotifications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SystemNotifications_Users_UserId",
                table: "SystemNotifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SystemNotifications_Users_UserId",
                table: "SystemNotifications");

            migrationBuilder.DropIndex(
                name: "IX_SystemNotifications_UserId",
                table: "SystemNotifications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SystemNotifications");

            migrationBuilder.AddColumn<string>(
                name: "Creator",
                table: "SystemNotifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
