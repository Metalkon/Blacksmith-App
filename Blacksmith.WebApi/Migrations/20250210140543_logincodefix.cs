using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blacksmith.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class logincodefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LockedCode",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockedCode",
                table: "Users");
        }
    }
}
