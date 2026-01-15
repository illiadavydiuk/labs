using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Data.Migrations
{
    /// <inheritdoc />
    public partial class Supervisor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_user_id",
                table: "Students");

            migrationBuilder.CreateTable(
                name: "Supervisors",
                columns: table => new
                {
                    supervisor_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_id = table.Column<int>(type: "INTEGER", nullable: false),
                    department_id = table.Column<int>(type: "INTEGER", nullable: false),
                    position_id = table.Column<int>(type: "INTEGER", nullable: false),
                    phone = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supervisors", x => x.supervisor_id);
                    table.ForeignKey(
                        name: "FK_Supervisors_Departments_department_id",
                        column: x => x.department_id,
                        principalTable: "Departments",
                        principalColumn: "department_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Supervisors_Positions_position_id",
                        column: x => x.position_id,
                        principalTable: "Positions",
                        principalColumn: "position_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Supervisors_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_user_id",
                table: "Students",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_department_id",
                table: "Supervisors",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_position_id",
                table: "Supervisors",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_Supervisors_user_id",
                table: "Supervisors",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Supervisors");

            migrationBuilder.DropIndex(
                name: "IX_Students_user_id",
                table: "Students");

            migrationBuilder.CreateIndex(
                name: "IX_Students_user_id",
                table: "Students",
                column: "user_id");
        }
    }
}
