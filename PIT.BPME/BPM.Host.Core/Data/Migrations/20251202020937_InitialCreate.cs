using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BPME.BPM.Host.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "process_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    StartStepId = table.Column<string>(type: "text", nullable: true),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "step_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StepType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NextStepIdsJson = table.Column<string>(type: "jsonb", nullable: true),
                    SettingsJson = table.Column<string>(type: "jsonb", nullable: true),
                    InputMapping = table.Column<string>(type: "text", nullable: true),
                    TimeoutSeconds = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_step_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_step_configs_process_configs_ProcessConfigId",
                        column: x => x.ProcessConfigId,
                        principalTable: "process_configs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_process_configs_PublicId",
                table: "process_configs",
                column: "PublicId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_step_configs_ProcessConfigId_PublicId",
                table: "step_configs",
                columns: new[] { "ProcessConfigId", "PublicId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "step_configs");

            migrationBuilder.DropTable(
                name: "process_configs");
        }
    }
}
