using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leave.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddYearToLeaveBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Year",
                schema: "leave",
                table: "LeaveBalances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId_Year",
                schema: "leave",
                table: "LeaveBalances",
                columns: new[] { "EmployeeId", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeaveBalances_EmployeeId_Year",
                schema: "leave",
                table: "LeaveBalances");

            migrationBuilder.DropColumn(
                name: "Year",
                schema: "leave",
                table: "LeaveBalances");
        }
    }
}
