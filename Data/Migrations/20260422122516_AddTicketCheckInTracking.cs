using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_PROJECT.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCheckInTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckedInByName",
                table: "RegistrationTickets",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInOnUtc",
                table: "RegistrationTickets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCheckedIn",
                table: "RegistrationTickets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckedInByName",
                table: "RegistrationTickets");

            migrationBuilder.DropColumn(
                name: "CheckedInOnUtc",
                table: "RegistrationTickets");

            migrationBuilder.DropColumn(
                name: "IsCheckedIn",
                table: "RegistrationTickets");
        }
    }
}
