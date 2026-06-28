using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leave.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedFeildNameInLeaveRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ApprovedBy",
                schema: "leave",
                table: "LeaveRequests",
                newName: "ProcessedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProcessedBy",
                schema: "leave",
                table: "LeaveRequests",
                newName: "ApprovedBy");
        }
    }
}
