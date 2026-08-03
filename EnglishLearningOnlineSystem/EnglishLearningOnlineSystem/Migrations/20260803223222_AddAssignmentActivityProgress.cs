using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearningOnlineSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentActivityProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "WeeklyAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "WeeklyAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "WeeklyAssignments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "WeeklyAssignmentId",
                table: "StudentGameProgresses",
                type: "int",
                nullable: true);

            // Backfill vòng đời cho dữ liệu cũ: IsVisible trước đây là nguồn trạng thái duy nhất.
            migrationBuilder.Sql("""
                UPDATE [WeeklyAssignments]
                SET [Status] = CASE WHEN [IsVisible] = 1 THEN 1 ELSE 0 END,
                    [CreatedAt] = SYSUTCDATETIME(),
                    [UpdatedAt] = SYSUTCDATETIME();
                """);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyAssignmentId",
                table: "FlashcardSessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssignmentActivityProgresses",
                columns: table => new
                {
                    ActivityProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentActivityProgresses", x => x.ActivityProgressId);
                    table.ForeignKey(
                        name: "FK_AssignmentActivityProgresses_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignmentActivityProgresses_WeeklyAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "WeeklyAssignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentProgresses",
                columns: table => new
                {
                    AssignmentProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedActivityCount = table.Column<int>(type: "int", nullable: false),
                    RequiredActivityCount = table.Column<int>(type: "int", nullable: false),
                    BestQuizScore = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompletedLate = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentProgresses", x => x.AssignmentProgressId);
                    table.ForeignKey(
                        name: "FK_AssignmentProgresses_StudentProfiles_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentProfiles",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignmentProgresses_WeeklyAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "WeeklyAssignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGameProgresses_WeeklyAssignmentId",
                table: "StudentGameProgresses",
                column: "WeeklyAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardSessions_WeeklyAssignmentId",
                table: "FlashcardSessions",
                column: "WeeklyAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentActivityProgresses_AssignmentId_StudentId_ActivityType_ActivityId",
                table: "AssignmentActivityProgresses",
                columns: new[] { "AssignmentId", "StudentId", "ActivityType", "ActivityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentActivityProgresses_StudentId",
                table: "AssignmentActivityProgresses",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentProgresses_AssignmentId_StudentId",
                table: "AssignmentProgresses",
                columns: new[] { "AssignmentId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentProgresses_StudentId",
                table: "AssignmentProgresses",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlashcardSessions_WeeklyAssignments_WeeklyAssignmentId",
                table: "FlashcardSessions",
                column: "WeeklyAssignmentId",
                principalTable: "WeeklyAssignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGameProgresses_WeeklyAssignments_WeeklyAssignmentId",
                table: "StudentGameProgresses",
                column: "WeeklyAssignmentId",
                principalTable: "WeeklyAssignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlashcardSessions_WeeklyAssignments_WeeklyAssignmentId",
                table: "FlashcardSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGameProgresses_WeeklyAssignments_WeeklyAssignmentId",
                table: "StudentGameProgresses");

            migrationBuilder.DropTable(
                name: "AssignmentActivityProgresses");

            migrationBuilder.DropTable(
                name: "AssignmentProgresses");

            migrationBuilder.DropIndex(
                name: "IX_StudentGameProgresses_WeeklyAssignmentId",
                table: "StudentGameProgresses");

            migrationBuilder.DropIndex(
                name: "IX_FlashcardSessions_WeeklyAssignmentId",
                table: "FlashcardSessions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "WeeklyAssignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WeeklyAssignments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WeeklyAssignments");

            migrationBuilder.DropColumn(
                name: "WeeklyAssignmentId",
                table: "StudentGameProgresses");

            migrationBuilder.DropColumn(
                name: "WeeklyAssignmentId",
                table: "FlashcardSessions");
        }
    }
}
