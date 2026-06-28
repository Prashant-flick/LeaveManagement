using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leave.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToLeaveBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaveBalances_EmployeeId",
                schema: "leave",
                table: "LeaveBalances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId",
                schema: "leave",
                table: "LeaveBalances",
                column: "EmployeeId",
                unique: true);
        }
    }
}
