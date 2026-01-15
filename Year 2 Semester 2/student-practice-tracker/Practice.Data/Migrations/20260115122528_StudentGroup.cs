using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudentGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentGroups",
                columns: table => new
                {
                    group_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    specialty_id = table.Column<int>(type: "INTEGER", nullable: false),
                    group_code = table.Column<string>(type: "TEXT", nullable: false),
                    entry_year = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentGroups", x => x.group_id);
                    table.ForeignKey(
                        name: "FK_StudentGroups_Specialties_specialty_id",
                        column: x => x.specialty_id,
                        principalTable: "Specialties",
                        principalColumn: "specialty_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentGroups_specialty_id",
                table: "StudentGroups",
                column: "specialty_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentGroups");
        }
    }
}
