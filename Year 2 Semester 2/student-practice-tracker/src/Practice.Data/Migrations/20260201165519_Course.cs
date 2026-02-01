using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Data.Migrations
{
    /// <inheritdoc />
    public partial class Course : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisciplineId",
                table: "InternshipTopics",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "Courses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Courses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InternshipTopics_DisciplineId",
                table: "InternshipTopics",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SupervisorId",
                table: "Courses",
                column: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Supervisors_SupervisorId",
                table: "Courses",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipTopics_Disciplines_DisciplineId",
                table: "InternshipTopics",
                column: "DisciplineId",
                principalTable: "Disciplines",
                principalColumn: "DisciplineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Supervisors_SupervisorId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipTopics_Disciplines_DisciplineId",
                table: "InternshipTopics");

            migrationBuilder.DropIndex(
                name: "IX_InternshipTopics_DisciplineId",
                table: "InternshipTopics");

            migrationBuilder.DropIndex(
                name: "IX_Courses_SupervisorId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "DisciplineId",
                table: "InternshipTopics");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Courses");
        }
    }
}
