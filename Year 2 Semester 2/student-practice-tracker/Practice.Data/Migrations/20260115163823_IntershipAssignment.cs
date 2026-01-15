using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Data.Migrations
{
    /// <inheritdoc />
    public partial class IntershipAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntershipAssignments",
                columns: table => new
                {
                    assignment_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    student_id = table.Column<int>(type: "INTEGER", nullable: false),
                    topic_id = table.Column<int>(type: "INTEGER", nullable: false),
                    supervisor_id = table.Column<int>(type: "INTEGER", nullable: false),
                    status_id = table.Column<int>(type: "INTEGER", nullable: false),
                    individual_task = table.Column<string>(type: "TEXT", nullable: false),
                    start_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    end_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    final_grade = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntershipAssignments", x => x.assignment_id);
                    table.ForeignKey(
                        name: "FK_IntershipAssignments_AssignmentStatuses_status_id",
                        column: x => x.status_id,
                        principalTable: "AssignmentStatuses",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntershipAssignments_IntershipTopics_topic_id",
                        column: x => x.topic_id,
                        principalTable: "IntershipTopics",
                        principalColumn: "topic_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntershipAssignments_Students_student_id",
                        column: x => x.student_id,
                        principalTable: "Students",
                        principalColumn: "student_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IntershipAssignments_Supervisors_supervisor_id",
                        column: x => x.supervisor_id,
                        principalTable: "Supervisors",
                        principalColumn: "supervisor_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntershipAssignments_status_id",
                table: "IntershipAssignments",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_IntershipAssignments_student_id",
                table: "IntershipAssignments",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_IntershipAssignments_supervisor_id",
                table: "IntershipAssignments",
                column: "supervisor_id");

            migrationBuilder.CreateIndex(
                name: "IX_IntershipAssignments_topic_id",
                table: "IntershipAssignments",
                column: "topic_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntershipAssignments");
        }
    }
}
