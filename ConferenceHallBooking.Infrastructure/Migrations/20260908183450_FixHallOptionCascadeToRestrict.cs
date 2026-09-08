using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceHallBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixHallOptionCascadeToRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HallOptions_Options_OptionId",
                table: "HallOptions");

            migrationBuilder.AddForeignKey(
                name: "FK_HallOptions_Options_OptionId",
                table: "HallOptions",
                column: "OptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HallOptions_Options_OptionId",
                table: "HallOptions");

            migrationBuilder.AddForeignKey(
                name: "FK_HallOptions_Options_OptionId",
                table: "HallOptions",
                column: "OptionId",
                principalTable: "Options",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
