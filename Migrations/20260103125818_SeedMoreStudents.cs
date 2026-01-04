using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Workshop1.Migrations
{
    /// <inheritdoc />
    public partial class SeedMoreStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "AcquiredCredits", "CurrentSemester", "EducationLevel", "EnrollmentDate", "FirstName", "LastName", "ProfileImagePath", "StudentId" },
                values: new object[,]
                {
                    { 6L, null, 1, "Bachelor", new DateTime(2023, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bojan", "Denkovski", null, "201006" },
                    { 7L, null, 2, "Bachelor", new DateTime(2023, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Elma", "Ristova", null, "201007" },
                    { 8L, null, 1, "Bachelor", new DateTime(2024, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dimitar", "Nacev", null, "201008" },
                    { 9L, null, 8, "Bachelor", new DateTime(2019, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Katerina", "Spasova", null, "201009" },
                    { 10L, null, 7, "Bachelor", new DateTime(2020, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Aleksandar", "Popov", null, "201010" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 10L);
        }
    }
}
