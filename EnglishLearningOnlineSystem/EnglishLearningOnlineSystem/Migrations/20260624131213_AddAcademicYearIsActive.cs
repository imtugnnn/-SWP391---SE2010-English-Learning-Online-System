using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearningOnlineSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicYearIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AcademicYears",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM AcademicYears WHERE IsActive = 1)
                BEGIN
                    UPDATE AcademicYears
                    SET IsActive = 0
                    WHERE AcademicYearId <> (
                        SELECT TOP (1) AcademicYearId
                        FROM AcademicYears
                        ORDER BY StartDate DESC, AcademicYearId DESC
                    );
                END
                ELSE IF EXISTS (SELECT 1 FROM AcademicYears)
                BEGIN
                    ;WITH target AS (
                        SELECT TOP (1) AcademicYearId
                        FROM AcademicYears
                        ORDER BY StartDate DESC, AcademicYearId DESC
                    )
                    UPDATE AcademicYears
                    SET IsActive = 1
                    WHERE AcademicYearId IN (SELECT AcademicYearId FROM target);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AcademicYears");
        }
    }
}
