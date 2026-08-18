using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TmsApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditConcurrencyAndArchives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Students",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Students",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Students",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<decimal>(
                name: "Grade",
                table: "Enrollments",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldMaxLength: 2,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Enrollments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Enrollments");

            migrationBuilder.AlterColumn<decimal>(
                name: "Grade",
                table: "Enrollments",
                type: "numeric",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);
        }
    }
}
