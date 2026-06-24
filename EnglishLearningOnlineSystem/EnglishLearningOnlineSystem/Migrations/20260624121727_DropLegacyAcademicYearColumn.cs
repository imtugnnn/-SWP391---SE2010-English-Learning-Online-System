using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnglishLearningOnlineSystem.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyAcademicYearColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns
                    WHERE Name = N'AcademicYear'
                      AND Object_ID = Object_ID(N'Classes')
                )
                BEGIN
                    ALTER TABLE Classes DROP COLUMN AcademicYear;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcademicYear",
                table: "Classes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
