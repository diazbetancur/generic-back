using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CC.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatesDataPolicyAcceptances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPolicyAcceptances_DocTypes_DocumentTypeId",
                table: "DataPolicyAcceptances");

            migrationBuilder.RenameColumn(
                name: "DocumentTypeId",
                table: "DataPolicyAcceptances",
                newName: "DocTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_DataPolicyAcceptances_DocumentTypeId",
                table: "DataPolicyAcceptances",
                newName: "IX_DataPolicyAcceptances_DocTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_DocTypeId",
                table: "Sessions",
                column: "DocTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataPolicyAcceptances_DocTypes_DocTypeId",
                table: "DataPolicyAcceptances",
                column: "DocTypeId",
                principalTable: "DocTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_DocTypes_DocTypeId",
                table: "Sessions",
                column: "DocTypeId",
                principalTable: "DocTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPolicyAcceptances_DocTypes_DocTypeId",
                table: "DataPolicyAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_DocTypes_DocTypeId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_DocTypeId",
                table: "Sessions");

            migrationBuilder.RenameColumn(
                name: "DocTypeId",
                table: "DataPolicyAcceptances",
                newName: "DocumentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_DataPolicyAcceptances_DocTypeId",
                table: "DataPolicyAcceptances",
                newName: "IX_DataPolicyAcceptances_DocumentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataPolicyAcceptances_DocTypes_DocumentTypeId",
                table: "DataPolicyAcceptances",
                column: "DocumentTypeId",
                principalTable: "DocTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
