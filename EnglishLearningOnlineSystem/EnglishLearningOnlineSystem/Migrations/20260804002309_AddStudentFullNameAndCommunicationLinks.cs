using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearningOnlineSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentFullNameAndCommunicationLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "AssignmentId",
                table: "TeacherFeedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "TeacherFeedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "StudentProfiles",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            // Business process: giữ lại định danh học sinh cũ khi bổ sung trường FullName.
            migrationBuilder.Sql(@"
                UPDATE profile
                SET profile.FullName = CASE
                    WHEN NULLIF(LTRIM(RTRIM(profile.Nickname)), '') IS NOT NULL THEN profile.Nickname
                    ELSE [user].Username
                END
                FROM StudentProfiles AS profile
                INNER JOIN Users AS [user] ON [user].UserId = profile.UserId;");

            migrationBuilder.AddColumn<int>(
                name: "AssignmentId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FeedbackId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherFeedbacks_AssignmentId",
                table: "TeacherFeedbacks",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherFeedbacks_ClassId",
                table: "TeacherFeedbacks",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AssignmentId",
                table: "Notifications",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_FeedbackId",
                table: "Notifications",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreateAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreateAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_TeacherFeedbacks_FeedbackId",
                table: "Notifications",
                column: "FeedbackId",
                principalTable: "TeacherFeedbacks",
                principalColumn: "FeedbackId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_WeeklyAssignments_AssignmentId",
                table: "Notifications",
                column: "AssignmentId",
                principalTable: "WeeklyAssignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherFeedbacks_Classes_ClassId",
                table: "TeacherFeedbacks",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherFeedbacks_WeeklyAssignments_AssignmentId",
                table: "TeacherFeedbacks",
                column: "AssignmentId",
                principalTable: "WeeklyAssignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_TeacherFeedbacks_FeedbackId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_WeeklyAssignments_AssignmentId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherFeedbacks_Classes_ClassId",
                table: "TeacherFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherFeedbacks_WeeklyAssignments_AssignmentId",
                table: "TeacherFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_TeacherFeedbacks_AssignmentId",
                table: "TeacherFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_TeacherFeedbacks_ClassId",
                table: "TeacherFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_AssignmentId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_FeedbackId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead_CreateAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "TeacherFeedbacks");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "TeacherFeedbacks");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "FeedbackId",
                table: "Notifications");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }
    }
}
