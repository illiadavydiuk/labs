using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeleteRules : Migration
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
                name: "FK_IntershipAssignments_IntershipTopics_TopicId",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_Students_StudentId",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipTopics_Organizations_OrganizationId",
                table: "IntershipTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_Supervisors_Positions_position_id",
                table: "Supervisors");

            migrationBuilder.DropIndex(
                name: "IX_Supervisors_position_id",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "position_id",
                table: "Supervisors");

            migrationBuilder.RenameColumn(
                name: "DisciplineIid",
                table: "Disciplines",
                newName: "DisciplineId");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AuditLogs",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_PositionId",
                table: "Supervisors",
                column: "PositionId");

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
                name: "FK_IntershipTopics_Organizations_OrganizationId",
                table: "IntershipTopics",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Supervisors_Positions_PositionId",
                table: "Supervisors",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "PositionId");
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
                name: "FK_IntershipAssignments_IntershipTopics_TopicId",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_Students_StudentId",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipTopics_Organizations_OrganizationId",
                table: "IntershipTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_Supervisors_Positions_PositionId",
                table: "Supervisors");

            migrationBuilder.DropIndex(
                name: "IX_Supervisors_PositionId",
                table: "Supervisors");

            migrationBuilder.RenameColumn(
                name: "DisciplineId",
                table: "Disciplines",
                newName: "DisciplineIid");

            migrationBuilder.AddColumn<int>(
                name: "position_id",
                table: "Supervisors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "AuditLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_position_id",
                table: "Supervisors",
                column: "position_id");

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
                principalColumn: "DisciplineIid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_IntershipTopics_TopicId",
                table: "IntershipAssignments",
                column: "TopicId",
                principalTable: "IntershipTopics",
                principalColumn: "TopicId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_Students_StudentId",
                table: "IntershipAssignments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipTopics_Organizations_OrganizationId",
                table: "IntershipTopics",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Supervisors_Positions_position_id",
                table: "Supervisors",
                column: "position_id",
                principalTable: "Positions",
                principalColumn: "PositionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
