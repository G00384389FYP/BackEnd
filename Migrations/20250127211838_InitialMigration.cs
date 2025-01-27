using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TradesmanData",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Trade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfJobsCompleted = table.Column<int>(type: "int", nullable: false),
                    TradeBio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkDistance = table.Column<double>(type: "float", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradesmanData", x => x.UserId);
                });
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerData");

            migrationBuilder.DropTable(
                name: "TradesmanData");

            migrationBuilder.DropTable(
                name: "UserData");
        }
    }
}
