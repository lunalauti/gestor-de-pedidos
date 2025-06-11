using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations.NotificationDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notifications");

            migrationBuilder.CreateTable(
                name: "device_tokens",
                schema: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    user_role = table.Column<int>(type: "integer", nullable: false),
                    device_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    app_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_used = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_tokens", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_device_tokens_is_active",
                schema: "notifications",
                table: "device_tokens",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "idx_device_tokens_last_used",
                schema: "notifications",
                table: "device_tokens",
                column: "last_used");

            migrationBuilder.CreateIndex(
                name: "idx_device_tokens_token",
                schema: "notifications",
                table: "device_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_device_tokens_user_id",
                schema: "notifications",
                table: "device_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_device_tokens_user_role",
                schema: "notifications",
                table: "device_tokens",
                column: "user_role");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_tokens",
                schema: "notifications");
        }
    }
}
