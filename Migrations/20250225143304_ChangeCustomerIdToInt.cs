using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCustomerIdToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old column
            migrationBuilder.DropColumn(
            name: "CustomerId",
            table: "JobApplications");

            // Add a new column with int type
            migrationBuilder.AddColumn<int>(
            name: "CustomerId",
            table: "JobApplications",
            nullable: false,
            defaultValue: 0); 
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "CustomerId",
            table: "JobApplications");

            migrationBuilder.AddColumn<Guid>(
            name: "CustomerId",
            table: "JobApplications",
            nullable: false);
        }
    }
}
