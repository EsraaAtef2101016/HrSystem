using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLeavePolicySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LeavePolicyId",
                table: "LeaveRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeavePolicyId",
                table: "LeaveRequests",
                column: "LeavePolicyId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_LeavePolicies_LeavePolicyId",
                table: "LeaveRequests",
                column: "LeavePolicyId",
                principalTable: "LeavePolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_LeavePolicies_LeavePolicyId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_LeavePolicyId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LeavePolicyId",
                table: "LeaveRequests");
        }
    }
}
