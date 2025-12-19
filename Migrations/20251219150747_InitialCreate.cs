using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workshop1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcquiredCredits = table.Column<int>(type: "int", nullable: true),
                    CurrentSemester = table.Column<int>(type: "int", nullable: true),
                    EducationLevel = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Degree = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AcademicRank = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    OfficeNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProfileImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Credits = table.Column<int>(type: "int", nullable: false),
                    Semester = table.Column<int>(type: "int", nullable: false),
                    Programme = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EducationLevel = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    FirstTeacherId = table.Column<int>(type: "int", nullable: true),
                    SecondTeacherId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Teachers_FirstTeacherId",
                        column: x => x.FirstTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Courses_Teachers_SecondTeacherId",
                        column: x => x.SecondTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    Semester = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    Grade = table.Column<int>(type: "int", nullable: true),
                    SeminarUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ProjectUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ExamPoints = table.Column<int>(type: "int", nullable: true),
                    SeminarPoints = table.Column<int>(type: "int", nullable: true),
                    ProjectPoints = table.Column<int>(type: "int", nullable: true),
                    AdditionalPoints = table.Column<int>(type: "int", nullable: true),
                    FinishDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_FirstTeacherId",
                table: "Courses",
                column: "FirstTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SecondTeacherId",
                table: "Courses",
                column: "SecondTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_CourseId_StudentId",
                table: "Enrollments",
                columns: new[] { "CourseId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentId",
                table: "Students",
                column: "StudentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Teachers");
        }
    }
}
