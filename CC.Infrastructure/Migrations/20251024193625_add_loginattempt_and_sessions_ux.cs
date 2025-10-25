using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_loginattempt_and_sessions_ux : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoginAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    DocTypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TraceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_IsActive",
                table: "Sessions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_IssuedAt",
                table: "Sessions",
                column: "IssuedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_LastSeenAt",
                table: "Sessions",
                column: "LastSeenAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_DateCreated",
                table: "LoginAttempts",
                column: "DateCreated");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_DocNumber_DateCreated",
                table: "LoginAttempts",
                columns: new[] { "DocNumber", "DateCreated" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_Success_DateCreated",
                table: "LoginAttempts",
                columns: new[] { "Success", "DateCreated" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_IsActive",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_IssuedAt",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_LastSeenAt",
                table: "Sessions");
        }
    }
}
