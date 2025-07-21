using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blacksmith.WebApi.Migrations.DbContextSqliteMigrations
{
    /// <inheritdoc />
    public partial class RenameBaseProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BaseScore",
                table: "Items",
                newName: "Score");

            migrationBuilder.RenameColumn(
                name: "BaseProtectionPhysical",
                table: "Items",
                newName: "ProtectionPhysical");

            migrationBuilder.RenameColumn(
                name: "BaseProtectionMagic",
                table: "Items",
                newName: "ProtectionMagic");

            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "Items",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "BaseMagicPower",
                table: "Items",
                newName: "MagicPower");

            migrationBuilder.RenameColumn(
                name: "BaseDurability",
                table: "Items",
                newName: "Durability");

            migrationBuilder.RenameColumn(
                name: "BaseAttackSpeed",
                table: "Items",
                newName: "AttackSpeed");

            migrationBuilder.RenameColumn(
                name: "BaseAttackPower",
                table: "Items",
                newName: "AttackPower");

            migrationBuilder.AddColumn<string>(
                name: "CraftId",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Prefix",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Suffix",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CraftId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Prefix",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Suffix",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "Items",
                newName: "BaseScore");

            migrationBuilder.RenameColumn(
                name: "ProtectionPhysical",
                table: "Items",
                newName: "BaseProtectionPhysical");

            migrationBuilder.RenameColumn(
                name: "ProtectionMagic",
                table: "Items",
                newName: "BaseProtectionMagic");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Items",
                newName: "BasePrice");

            migrationBuilder.RenameColumn(
                name: "MagicPower",
                table: "Items",
                newName: "BaseMagicPower");

            migrationBuilder.RenameColumn(
                name: "Durability",
                table: "Items",
                newName: "BaseDurability");

            migrationBuilder.RenameColumn(
                name: "AttackSpeed",
                table: "Items",
                newName: "BaseAttackSpeed");

            migrationBuilder.RenameColumn(
                name: "AttackPower",
                table: "Items",
                newName: "BaseAttackPower");
        }
    }
}
