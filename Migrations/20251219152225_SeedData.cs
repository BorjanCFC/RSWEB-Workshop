using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Workshop1.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "Id", "AcquiredCredits", "CurrentSemester", "EducationLevel", "EnrollmentDate", "FirstName", "LastName", "StudentId" },
                values: new object[,]
                {
                    { 1L, null, 3, null, new DateTime(2022, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Petar", "Nikolov", "201001" },
                    { 2L, null, 3, null, new DateTime(2022, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Marija", "Stankova", "201002" },
                    { 3L, null, 5, null, new DateTime(2021, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jovan", "Trajkov", "201003" },
                    { 4L, null, 5, null, new DateTime(2021, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sara", "Mihajlova", "201004" },
                    { 5L, null, 6, null, new DateTime(2020, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "David", "Kirilov", "201005" }
                });

            migrationBuilder.InsertData(
                table: "Teachers",
                columns: new[] { "Id", "AcademicRank", "Degree", "FirstName", "HireDate", "LastName", "OfficeNumber", "ProfileImagePath" },
                values: new object[,]
                {
                    { 1, "Professor", "PhD", "Ivan", null, "Petrovski", "A101", null },
                    { 2, "Assistant", "MSc", "Ana", null, "Stojanova", "B202", null },
                    { 3, "Associate Professor", "PhD", "Marko", null, "Iliev", "A203", null },
                    { 4, "Lecturer", "MSc", "Elena", null, "Kostova", "C104", null },
                    { 5, "Professor", "PhD", "Stefan", null, "Dimitrov", "A105", null }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "Credits", "EducationLevel", "FirstTeacherId", "Programme", "SecondTeacherId", "Semester", "Title" },
                values: new object[,]
                {
                    { 1, 6, "Undergraduate", 1, "IT", null, 3, "Databases" },
                    { 2, 6, "Undergraduate", 2, "IT", null, 4, "Web Programming" },
                    { 3, 7, "Undergraduate", 3, "SE", null, 5, "Software Engineering" },
                    { 4, 6, "Undergraduate", 4, "IT", null, 4, "Computer Networks" },
                    { 5, 7, "Undergraduate", 5, "SE", null, 6, "Artificial Intelligence" }
                });

            migrationBuilder.InsertData(
                table: "Enrollments",
                columns: new[] { "Id", "AdditionalPoints", "CourseId", "ExamPoints", "FinishDate", "Grade", "ProjectPoints", "ProjectUrl", "Semester", "SeminarPoints", "SeminarUrl", "StudentId", "Year" },
                values: new object[,]
                {
                    { 1L, null, 1, null, null, 8, null, null, "Winter", null, null, 1L, 2023 },
                    { 2L, null, 2, null, null, 9, null, null, "Winter", null, null, 1L, 2023 },
                    { 3L, null, 1, null, null, 7, null, null, "Winter", null, null, 2L, 2023 },
                    { 4L, null, 3, null, null, 10, null, null, "Summer", null, null, 3L, 2024 },
                    { 5L, null, 4, null, null, 8, null, null, "Summer", null, null, 4L, 2024 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Enrollments",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Enrollments",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Enrollments",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Enrollments",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Enrollments",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Students",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Teachers",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
