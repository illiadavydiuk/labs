using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Data.Migrations
{
    /// <inheritdoc />
    public partial class TableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_AssignmentStatuses_StatusId",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_Courses_CourseId",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_IntershipTopics_TopicId",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_Students_StudentId",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_Supervisors_SupervisorId",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipTopics_Organizations_OrganizationId",
                table: "IntershipTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_IntershipAssignments_AssignmentId",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IntershipTopics",
                table: "IntershipTopics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IntershipAssignments",
                table: "IntershipAssignments");

            migrationBuilder.RenameTable(
                name: "IntershipTopics",
                newName: "InternshipTopics");

            migrationBuilder.RenameTable(
                name: "IntershipAssignments",
                newName: "InternshipAssignments");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipTopics_OrganizationId",
                table: "InternshipTopics",
                newName: "IX_InternshipTopics_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_TopicId",
                table: "InternshipAssignments",
                newName: "IX_InternshipAssignments_TopicId");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_SupervisorId",
                table: "InternshipAssignments",
                newName: "IX_InternshipAssignments_SupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_StudentId",
                table: "InternshipAssignments",
                newName: "IX_InternshipAssignments_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_StatusId",
                table: "InternshipAssignments",
                newName: "IX_InternshipAssignments_StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_CourseId",
                table: "InternshipAssignments",
                newName: "IX_InternshipAssignments_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InternshipTopics",
                table: "InternshipTopics",
                column: "TopicId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InternshipAssignments",
                table: "InternshipAssignments",
                column: "AssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipAssignments_AssignmentStatuses_StatusId",
                table: "InternshipAssignments",
                column: "StatusId",
                principalTable: "AssignmentStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipAssignments_Courses_CourseId",
                table: "InternshipAssignments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_InternshipAssignments_Supervisors_SupervisorId",
                table: "InternshipAssignments",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_InternshipTopics_Organizations_OrganizationId",
                table: "InternshipTopics",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_InternshipAssignments_AssignmentId",
                table: "Reports",
                column: "AssignmentId",
                principalTable: "InternshipAssignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InternshipAssignments_AssignmentStatuses_StatusId",
                table: "InternshipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipAssignments_Courses_CourseId",
                table: "InternshipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipAssignments_InternshipTopics_TopicId",
                table: "InternshipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipAssignments_Students_StudentId",
                table: "InternshipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipAssignments_Supervisors_SupervisorId",
                table: "InternshipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_InternshipTopics_Organizations_OrganizationId",
                table: "InternshipTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_InternshipAssignments_AssignmentId",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InternshipTopics",
                table: "InternshipTopics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InternshipAssignments",
                table: "InternshipAssignments");

            migrationBuilder.RenameTable(
                name: "InternshipTopics",
                newName: "IntershipTopics");

            migrationBuilder.RenameTable(
                name: "InternshipAssignments",
                newName: "IntershipAssignments");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipTopics_OrganizationId",
                table: "IntershipTopics",
                newName: "IX_IntershipTopics_OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipAssignments_TopicId",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_TopicId");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipAssignments_SupervisorId",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_SupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipAssignments_StudentId",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipAssignments_StatusId",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_InternshipAssignments_CourseId",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IntershipTopics",
                table: "IntershipTopics",
                column: "TopicId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IntershipAssignments",
                table: "IntershipAssignments",
                column: "AssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_AssignmentStatuses_StatusId",
                table: "IntershipAssignments",
                column: "StatusId",
                principalTable: "AssignmentStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_Courses_CourseId",
                table: "IntershipAssignments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_IntershipTopics_TopicId",
                table: "IntershipAssignments",
                column: "TopicId",
                principalTable: "IntershipTopics",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_Students_StudentId",
                table: "IntershipAssignments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_Supervisors_SupervisorId",
                table: "IntershipAssignments",
                column: "SupervisorId",
                principalTable: "Supervisors",
                principalColumn: "SupervisorId");

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipTopics_Organizations_OrganizationId",
                table: "IntershipTopics",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_IntershipAssignments_AssignmentId",
                table: "Reports",
                column: "AssignmentId",
                principalTable: "IntershipAssignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
