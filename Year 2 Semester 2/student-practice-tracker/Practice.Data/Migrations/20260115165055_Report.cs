using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Data.Migrations
{
    /// <inheritdoc />
    public partial class Report : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    report_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    assignment_id = table.Column<int>(type: "INTEGER", nullable: false),
                    status_id = table.Column<int>(type: "INTEGER", nullable: false),
                    report_file_url = table.Column<string>(type: "TEXT", nullable: false),
                    work_archive_url = table.Column<string>(type: "TEXT", nullable: false),
                    student_comment = table.Column<string>(type: "TEXT", nullable: false),
                    supervisor_feedback = table.Column<string>(type: "TEXT", nullable: false),
                    submission_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    review_date = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.report_id);
                    table.ForeignKey(
                        name: "FK_Reports_IntershipAssignments_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "IntershipAssignments",
                        principalColumn: "assignment_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_ReportStatuses_status_id",
                        column: x => x.status_id,
                        principalTable: "ReportStatuses",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_assignment_id",
                table: "Reports",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_status_id",
                table: "Reports",
                column: "status_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
