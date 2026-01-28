using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewEntitiesAndRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_user_id",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_AssignmentStatuses_status_id",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_IntershipTopics_topic_id",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_Students_student_id",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipAssignments_Supervisors_supervisor_id",
                table: "IntershipAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_IntershipTopics_Organizations_organization_id",
                table: "IntershipTopics");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_IntershipAssignments_assignment_id",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ReportStatuses_status_id",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGroups_Specialties_specialty_id",
                table: "StudentGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_StudentGroups_group_id",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_user_id",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Supervisors_Departments_department_id",
                table: "Supervisors");

            migrationBuilder.DropForeignKey(
                name: "FK_Supervisors_Users_user_id",
                table: "Supervisors");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_role_id",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Supervisors_department_id",
                table: "Supervisors");

            migrationBuilder.DropIndex(
                name: "IX_IntershipTopics_organization_id",
                table: "IntershipTopics");

            migrationBuilder.DropIndex(
                name: "IX_IntershipAssignments_status_id",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "record_book_number",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "status_name",
                table: "ReportStatuses");

            migrationBuilder.DropColumn(
                name: "report_file_url",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "student_comment",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "submission_date",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "position_name",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "is_available",
                table: "IntershipTopics");

            migrationBuilder.DropColumn(
                name: "end_date",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "final_grade",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "individual_task",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "status_id",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "department_name",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "status_name",
                table: "AssignmentStatuses");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "Users",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Users",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Users",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_role_id",
                table: "Users",
                newName: "IX_Users_RoleId");

            migrationBuilder.RenameColumn(
                name: "phone",
                table: "Supervisors",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Supervisors",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "supervisor_id",
                table: "Supervisors",
                newName: "SupervisorId");

            migrationBuilder.RenameIndex(
                name: "IX_Supervisors_user_id",
                table: "Supervisors",
                newName: "IX_Supervisors_UserId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Students",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "group_id",
                table: "Students",
                newName: "GroupId");

            migrationBuilder.RenameColumn(
                name: "student_id",
                table: "Students",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_user_id",
                table: "Students",
                newName: "IX_Students_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Students_group_id",
                table: "Students",
                newName: "IX_Students_GroupId");

            migrationBuilder.RenameColumn(
                name: "specialty_id",
                table: "StudentGroups",
                newName: "SpecialtyId");

            migrationBuilder.RenameColumn(
                name: "group_code",
                table: "StudentGroups",
                newName: "GroupCode");

            migrationBuilder.RenameColumn(
                name: "entry_year",
                table: "StudentGroups",
                newName: "EntryYear");

            migrationBuilder.RenameColumn(
                name: "group_id",
                table: "StudentGroups",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentGroups_specialty_id",
                table: "StudentGroups",
                newName: "IX_StudentGroups_SpecialtyId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Specialties",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "Specialties",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "specialty_id",
                table: "Specialties",
                newName: "SpecialtyId");

            migrationBuilder.RenameColumn(
                name: "role_name",
                table: "Roles",
                newName: "RoleName");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "Roles",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "status_id",
                table: "ReportStatuses",
                newName: "StatusId");

            migrationBuilder.RenameColumn(
                name: "work_archive_url",
                table: "Reports",
                newName: "SupervisorFeedback");

            migrationBuilder.RenameColumn(
                name: "supervisor_feedback",
                table: "Reports",
                newName: "SubmissionDate");

            migrationBuilder.RenameColumn(
                name: "status_id",
                table: "Reports",
                newName: "StatusId");

            migrationBuilder.RenameColumn(
                name: "review_date",
                table: "Reports",
                newName: "StudentComment");

            migrationBuilder.RenameColumn(
                name: "assignment_id",
                table: "Reports",
                newName: "AssignmentId");

            migrationBuilder.RenameColumn(
                name: "report_id",
                table: "Reports",
                newName: "ReportId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_status_id",
                table: "Reports",
                newName: "IX_Reports_StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_assignment_id",
                table: "Reports",
                newName: "IX_Reports_AssignmentId");

            migrationBuilder.RenameColumn(
                name: "position_id",
                table: "Positions",
                newName: "PositionId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Organizations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Organizations",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "contact_email",
                table: "Organizations",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "Organizations",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "IntershipTopics",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "IntershipTopics",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "IntershipTopics",
                newName: "IsAvailable");

            migrationBuilder.RenameColumn(
                name: "topic_id",
                table: "IntershipTopics",
                newName: "TopicId");

            migrationBuilder.RenameColumn(
                name: "topic_id",
                table: "IntershipAssignments",
                newName: "TopicId");

            migrationBuilder.RenameColumn(
                name: "supervisor_id",
                table: "IntershipAssignments",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "student_id",
                table: "IntershipAssignments",
                newName: "StatusId");

            migrationBuilder.RenameColumn(
                name: "assignment_id",
                table: "IntershipAssignments",
                newName: "AssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_topic_id",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_TopicId");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_supervisor_id",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_student_id",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_StatusId");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "Departments",
                newName: "DepartmentId");

            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "AuditLogs",
                newName: "TimeStamp");

            migrationBuilder.RenameColumn(
                name: "action",
                table: "AuditLogs",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "AuditLogs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "entity_id",
                table: "AuditLogs",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "entity_affected",
                table: "AuditLogs",
                newName: "EntityAffected");

            migrationBuilder.RenameColumn(
                name: "log_id",
                table: "AuditLogs",
                newName: "LogId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_user_id",
                table: "AuditLogs",
                newName: "IX_AuditLogs_UserId");

            migrationBuilder.RenameColumn(
                name: "status_id",
                table: "AssignmentStatuses",
                newName: "StatusId");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Supervisors",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionId",
                table: "Supervisors",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecordBookNumber",
                table: "Students",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StatusName",
                table: "ReportStatuses",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewDate",
                table: "Reports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionName",
                table: "Positions",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Organizations",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "IntershipTopics",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "IntershipAssignments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "IntershipAssignments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinalGrade",
                table: "IntershipAssignments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndividualTask",
                table: "IntershipAssignments",
                type: "TEXT",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "IntershipAssignments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                table: "IntershipAssignments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "Departments",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StatusName",
                table: "AssignmentStatuses",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    AttachmentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReportId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "FK_Attachments_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Disciplines",
                columns: table => new
                {
                    DisciplineIid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisciplineName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplines", x => x.DisciplineIid);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisciplineId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK_Courses_Disciplines_DisciplineId",
                        column: x => x.DisciplineId,
                        principalTable: "Disciplines",
                        principalColumn: "DisciplineIid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseEnrollments",
                columns: table => new
                {
                    EnrollmentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseEnrollments", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_StudentGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK_CourseEnrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_DepartmentId",
                table: "Supervisors",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_IntershipTopics_OrganizationId",
                table: "IntershipTopics",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_IntershipAssignments_CourseId",
                table: "IntershipAssignments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_IntershipAssignments_SupervisorId",
                table: "IntershipAssignments",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_ReportId",
                table: "Attachments",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_CourseId",
                table: "CourseEnrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_GroupId",
                table: "CourseEnrollments",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_StudentId",
                table: "CourseEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_DisciplineId",
                table: "Courses",
                column: "DisciplineId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

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
                principalColumn: "CourseId");

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
                principalColumn: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_IntershipAssignments_AssignmentId",
                table: "Reports",
                column: "AssignmentId",
                principalTable: "IntershipAssignments",
                principalColumn: "AssignmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ReportStatuses_StatusId",
                table: "Reports",
                column: "StatusId",
                principalTable: "ReportStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGroups_Specialties_SpecialtyId",
                table: "StudentGroups",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_StudentGroups_GroupId",
                table: "Students",
                column: "GroupId",
                principalTable: "StudentGroups",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_UserId",
                table: "Students",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Supervisors_Departments_DepartmentId",
                table: "Supervisors",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Supervisors_Users_UserId",
                table: "Supervisors",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                table: "AuditLogs");

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

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ReportStatuses_StatusId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentGroups_Specialties_SpecialtyId",
                table: "StudentGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_StudentGroups_GroupId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Users_UserId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Supervisors_Departments_DepartmentId",
                table: "Supervisors");

            migrationBuilder.DropForeignKey(
                name: "FK_Supervisors_Users_UserId",
                table: "Supervisors");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "CourseEnrollments");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Disciplines");

            migrationBuilder.DropIndex(
                name: "IX_Supervisors_DepartmentId",
                table: "Supervisors");

            migrationBuilder.DropIndex(
                name: "IX_IntershipTopics_OrganizationId",
                table: "IntershipTopics");

            migrationBuilder.DropIndex(
                name: "IX_IntershipAssignments_CourseId",
                table: "IntershipAssignments");

            migrationBuilder.DropIndex(
                name: "IX_IntershipAssignments_SupervisorId",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Supervisors");

            migrationBuilder.DropColumn(
                name: "RecordBookNumber",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StatusName",
                table: "ReportStatuses");

            migrationBuilder.DropColumn(
                name: "ReviewDate",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "PositionName",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "IntershipTopics");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "FinalGrade",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "IndividualTask",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "SupervisorId",
                table: "IntershipAssignments");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "StatusName",
                table: "AssignmentStatuses");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Users",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Users",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Users",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Users",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                newName: "IX_Users_role_id");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Supervisors",
                newName: "phone");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Supervisors",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "SupervisorId",
                table: "Supervisors",
                newName: "supervisor_id");

            migrationBuilder.RenameIndex(
                name: "IX_Supervisors_UserId",
                table: "Supervisors",
                newName: "IX_Supervisors_user_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Students",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "Students",
                newName: "group_id");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Students",
                newName: "student_id");

            migrationBuilder.RenameIndex(
                name: "IX_Students_UserId",
                table: "Students",
                newName: "IX_Students_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Students_GroupId",
                table: "Students",
                newName: "IX_Students_group_id");

            migrationBuilder.RenameColumn(
                name: "SpecialtyId",
                table: "StudentGroups",
                newName: "specialty_id");

            migrationBuilder.RenameColumn(
                name: "GroupCode",
                table: "StudentGroups",
                newName: "group_code");

            migrationBuilder.RenameColumn(
                name: "EntryYear",
                table: "StudentGroups",
                newName: "entry_year");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "StudentGroups",
                newName: "group_id");

            migrationBuilder.RenameIndex(
                name: "IX_StudentGroups_SpecialtyId",
                table: "StudentGroups",
                newName: "IX_StudentGroups_specialty_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Specialties",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Specialties",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "SpecialtyId",
                table: "Specialties",
                newName: "specialty_id");

            migrationBuilder.RenameColumn(
                name: "RoleName",
                table: "Roles",
                newName: "role_name");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Roles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "ReportStatuses",
                newName: "status_id");

            migrationBuilder.RenameColumn(
                name: "SupervisorFeedback",
                table: "Reports",
                newName: "work_archive_url");

            migrationBuilder.RenameColumn(
                name: "SubmissionDate",
                table: "Reports",
                newName: "supervisor_feedback");

            migrationBuilder.RenameColumn(
                name: "StudentComment",
                table: "Reports",
                newName: "review_date");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "Reports",
                newName: "status_id");

            migrationBuilder.RenameColumn(
                name: "AssignmentId",
                table: "Reports",
                newName: "assignment_id");

            migrationBuilder.RenameColumn(
                name: "ReportId",
                table: "Reports",
                newName: "report_id");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_StatusId",
                table: "Reports",
                newName: "IX_Reports_status_id");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_AssignmentId",
                table: "Reports",
                newName: "IX_Reports_assignment_id");

            migrationBuilder.RenameColumn(
                name: "PositionId",
                table: "Positions",
                newName: "position_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Organizations",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Organizations",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Organizations",
                newName: "contact_email");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Organizations",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "IntershipTopics",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "IntershipTopics",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "IntershipTopics",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "TopicId",
                table: "IntershipTopics",
                newName: "topic_id");

            migrationBuilder.RenameColumn(
                name: "TopicId",
                table: "IntershipAssignments",
                newName: "topic_id");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "IntershipAssignments",
                newName: "supervisor_id");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "IntershipAssignments",
                newName: "student_id");

            migrationBuilder.RenameColumn(
                name: "AssignmentId",
                table: "IntershipAssignments",
                newName: "assignment_id");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_TopicId",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_topic_id");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_StudentId",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_supervisor_id");

            migrationBuilder.RenameIndex(
                name: "IX_IntershipAssignments_StatusId",
                table: "IntershipAssignments",
                newName: "IX_IntershipAssignments_student_id");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Departments",
                newName: "department_id");

            migrationBuilder.RenameColumn(
                name: "TimeStamp",
                table: "AuditLogs",
                newName: "timestamp");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "AuditLogs",
                newName: "action");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "AuditLogs",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "AuditLogs",
                newName: "entity_id");

            migrationBuilder.RenameColumn(
                name: "EntityAffected",
                table: "AuditLogs",
                newName: "entity_affected");

            migrationBuilder.RenameColumn(
                name: "LogId",
                table: "AuditLogs",
                newName: "log_id");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_user_id");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "AssignmentStatuses",
                newName: "status_id");

            migrationBuilder.AddColumn<int>(
                name: "department_id",
                table: "Supervisors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "record_book_number",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "status_name",
                table: "ReportStatuses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "report_file_url",
                table: "Reports",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "student_comment",
                table: "Reports",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "submission_date",
                table: "Reports",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "position_name",
                table: "Positions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_available",
                table: "IntershipTopics",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "end_date",
                table: "IntershipAssignments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "final_grade",
                table: "IntershipAssignments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "individual_task",
                table: "IntershipAssignments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "IntershipAssignments",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "status_id",
                table: "IntershipAssignments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "department_name",
                table: "Departments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "status_name",
                table: "AssignmentStatuses",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_department_id",
                table: "Supervisors",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_IntershipTopics_organization_id",
                table: "IntershipTopics",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_IntershipAssignments_status_id",
                table: "IntershipAssignments",
                column: "status_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_user_id",
                table: "AuditLogs",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_AssignmentStatuses_status_id",
                table: "IntershipAssignments",
                column: "status_id",
                principalTable: "AssignmentStatuses",
                principalColumn: "status_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_IntershipTopics_topic_id",
                table: "IntershipAssignments",
                column: "topic_id",
                principalTable: "IntershipTopics",
                principalColumn: "topic_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_Students_student_id",
                table: "IntershipAssignments",
                column: "student_id",
                principalTable: "Students",
                principalColumn: "student_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipAssignments_Supervisors_supervisor_id",
                table: "IntershipAssignments",
                column: "supervisor_id",
                principalTable: "Supervisors",
                principalColumn: "supervisor_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IntershipTopics_Organizations_organization_id",
                table: "IntershipTopics",
                column: "organization_id",
                principalTable: "Organizations",
                principalColumn: "organization_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_IntershipAssignments_assignment_id",
                table: "Reports",
                column: "assignment_id",
                principalTable: "IntershipAssignments",
                principalColumn: "assignment_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ReportStatuses_status_id",
                table: "Reports",
                column: "status_id",
                principalTable: "ReportStatuses",
                principalColumn: "status_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentGroups_Specialties_specialty_id",
                table: "StudentGroups",
                column: "specialty_id",
                principalTable: "Specialties",
                principalColumn: "specialty_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_StudentGroups_group_id",
                table: "Students",
                column: "group_id",
                principalTable: "StudentGroups",
                principalColumn: "group_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Users_user_id",
                table: "Students",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Supervisors_Departments_department_id",
                table: "Supervisors",
                column: "department_id",
                principalTable: "Departments",
                principalColumn: "department_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Supervisors_Users_user_id",
                table: "Supervisors",
                column: "user_id",
                principalTable: "Users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_role_id",
                table: "Users",
                column: "role_id",
                principalTable: "Roles",
                principalColumn: "role_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
