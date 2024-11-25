using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerData",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JobsPosted = table.Column<int>(type: "int", nullable: false),
                    IsSuspended = table.Column<bool>(type: "bit", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerData", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_CustomerData_UserData_UserId",
                        column: x => x.UserId,
                        principalTable: "UserData",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerData");
        }
    }
}
