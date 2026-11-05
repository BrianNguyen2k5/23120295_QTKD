using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QTKD.Api.Migrations
{
    /// <inheritdoc />
    public partial class Week06_UpdateContractForUS03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalId",
                table: "Contracts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "Contracts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "Contracts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmitComment",
                table: "Contracts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "Contracts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Contracts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApprovalId", "AttachmentUrl", "IdempotencyKey", "SubmitComment", "SubmittedAt" },
                values: new object[] { null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SubmitComment",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "Contracts");
        }
    }
}
