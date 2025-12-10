using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace spa_web_app.Migrations
{
    /// <inheritdoc />
    public partial class FixAppointmentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AspNetUsers_EmployeeId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "Appointments",
                newName: "TherapistId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_EmployeeId",
                table: "Appointments",
                newName: "IX_Appointments_TherapistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AspNetUsers_TherapistId",
                table: "Appointments",
                column: "TherapistId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_AspNetUsers_TherapistId",
                table: "Appointments");

            migrationBuilder.RenameColumn(
                name: "TherapistId",
                table: "Appointments",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_TherapistId",
                table: "Appointments",
                newName: "IX_Appointments_EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AspNetUsers_EmployeeId",
                table: "Appointments",
                column: "EmployeeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
