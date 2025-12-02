using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BPME.BPM.Host.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessInstances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "process_instances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstanceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    ProcessPublicId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    InputArgumentsJson = table.Column<string>(type: "jsonb", nullable: true),
                    OutputResultJson = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_instances", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_process_instances_CorrelationId",
                table: "process_instances",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_process_instances_CreatedAt",
                table: "process_instances",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_process_instances_InstanceId",
                table: "process_instances",
                column: "InstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_process_instances_ProcessPublicId",
                table: "process_instances",
                column: "ProcessPublicId");

            migrationBuilder.CreateIndex(
                name: "IX_process_instances_Status",
                table: "process_instances",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "process_instances");
        }
    }
}
