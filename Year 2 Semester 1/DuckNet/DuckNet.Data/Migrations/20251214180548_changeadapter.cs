using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DuckNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class changeadapter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dns",
                table: "AdapterProfiles");

            migrationBuilder.DropColumn(
                name: "Gateway",
                table: "AdapterProfiles");

            migrationBuilder.DropColumn(
                name: "IsDhcpEnabled",
                table: "AdapterProfiles");

            migrationBuilder.DropColumn(
                name: "StaticIp",
                table: "AdapterProfiles");

            migrationBuilder.DropColumn(
                name: "SubnetMask",
                table: "AdapterProfiles");

            migrationBuilder.AddColumn<string>(
                name: "ActiveAdaptersData",
                table: "AdapterProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveAdaptersData",
                table: "AdapterProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Dns",
                table: "AdapterProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gateway",
                table: "AdapterProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDhcpEnabled",
                table: "AdapterProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StaticIp",
                table: "AdapterProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubnetMask",
                table: "AdapterProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
