using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Data.Migrations
{
    /// <inheritdoc />
    public partial class Rules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Disciplines_DisciplineId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipAssignments_InternshipTopics_TopicId",
                table: "InternshipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipAssignments_Students_StudentId",
                table: "InternshipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipTopics_Organizations_OrganizationId",
                table: "InternshipTopics");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Disciplines_DisciplineId",
                table: "Courses",
                column: "DisciplineId",
                principalTable: "Disciplines",
                principalColumn: "DisciplineId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipAssignments_InternshipTopics_TopicId",
                table: "InternshipAssignments",
                column: "TopicId",
                principalTable: "InternshipTopics",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipAssignments_Students_StudentId",
                table: "InternshipAssignments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipTopics_Organizations_OrganizationId",
                table: "InternshipTopics",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Disciplines_DisciplineId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipAssignments_InternshipTopics_TopicId",
                table: "InternshipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipAssignments_Students_StudentId",
                table: "InternshipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipTopics_Organizations_OrganizationId",
                table: "InternshipTopics");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Disciplines_DisciplineId",
                table: "Courses",
                column: "DisciplineId",
                principalTable: "Disciplines",
                principalColumn: "DisciplineId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipAssignments_InternshipTopics_TopicId",
                table: "InternshipAssignments",
                column: "TopicId",
                principalTable: "InternshipTopics",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipAssignments_Students_StudentId",
                table: "InternshipAssignments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipTopics_Organizations_OrganizationId",
                table: "InternshipTopics",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
