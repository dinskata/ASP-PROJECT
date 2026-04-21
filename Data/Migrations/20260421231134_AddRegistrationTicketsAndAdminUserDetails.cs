using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_PROJECT.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationTicketsAndAdminUserDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrationTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationId = table.Column<int>(type: "int", nullable: false),
                    TicketNumber = table.Column<int>(type: "int", nullable: false),
                    TicketCode = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    VerificationCode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    SeatLabel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TicketNote = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    IssuedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationTickets_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationTickets_RegistrationId_TicketNumber",
                table: "RegistrationTickets",
                columns: new[] { "RegistrationId", "TicketNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationTickets_TicketCode",
                table: "RegistrationTickets",
                column: "TicketCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationTickets");
        }
    }
}
