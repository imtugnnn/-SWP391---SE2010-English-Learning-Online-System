using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearningOnlineSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddClassSpecificAssignmentContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "WeeklyAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeMiniGame",
                table: "WeeklyAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeQuiz",
                table: "WeeklyAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeVocabulary",
                table: "WeeklyAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "WeeklyAssignmentMiniGames",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyAssignmentMiniGames", x => new { x.AssignmentId, x.GameId });
                    table.ForeignKey(
                        name: "FK_WeeklyAssignmentMiniGames_MiniGames_GameId",
                        column: x => x.GameId,
                        principalTable: "MiniGames",
                        principalColumn: "GameId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklyAssignmentMiniGames_WeeklyAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "WeeklyAssignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyAssignmentQuizzes",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    QuizId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyAssignmentQuizzes", x => new { x.AssignmentId, x.QuizId });
                    table.ForeignKey(
                        name: "FK_WeeklyAssignmentQuizzes_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "QuizId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklyAssignmentQuizzes_WeeklyAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "WeeklyAssignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyAssignmentVocabularies",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyAssignmentVocabularies", x => new { x.AssignmentId, x.VocabularyId });
                    table.ForeignKey(
                        name: "FK_WeeklyAssignmentVocabularies_Vocabularies_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "Vocabularies",
                        principalColumn: "VocabularyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WeeklyAssignmentVocabularies_WeeklyAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "WeeklyAssignments",
                        principalColumn: "AssignmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAssignments_ClassId_LessonId_WeekStartDate",
                table: "WeeklyAssignments",
                columns: new[] { "ClassId", "LessonId", "WeekStartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAssignmentMiniGames_GameId",
                table: "WeeklyAssignmentMiniGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAssignmentQuizzes_QuizId",
                table: "WeeklyAssignmentQuizzes",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyAssignmentVocabularies_VocabularyId",
                table: "WeeklyAssignmentVocabularies",
                column: "VocabularyId");

            migrationBuilder.AddForeignKey(
                name: "FK_WeeklyAssignments_Classes_ClassId",
                table: "WeeklyAssignments",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeeklyAssignments_Classes_ClassId",
                table: "WeeklyAssignments");

            migrationBuilder.DropTable(
                name: "WeeklyAssignmentMiniGames");

            migrationBuilder.DropTable(
                name: "WeeklyAssignmentQuizzes");

            migrationBuilder.DropTable(
                name: "WeeklyAssignmentVocabularies");

            migrationBuilder.DropIndex(
                name: "IX_WeeklyAssignments_ClassId_LessonId_WeekStartDate",
                table: "WeeklyAssignments");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "WeeklyAssignments");

            migrationBuilder.DropColumn(
                name: "IncludeMiniGame",
                table: "WeeklyAssignments");

            migrationBuilder.DropColumn(
                name: "IncludeQuiz",
                table: "WeeklyAssignments");

            migrationBuilder.DropColumn(
                name: "IncludeVocabulary",
                table: "WeeklyAssignments");
        }
    }
}
