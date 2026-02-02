using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Practice.Data.Migrations
{
    /// <inheritdoc />
    public partial class LogColumnRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecordId",
                table: "AuditLogs",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "AuditLogs");
        }
    }
}
