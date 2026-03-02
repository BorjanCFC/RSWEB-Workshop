# EDU Manage

EDU Manage is a role-based education management web application built with **ASP.NET Core MVC**.  
It helps academic institutions manage **students**, **teachers**, **courses**, and **enrollments**, while providing tailored functionality depending on the logged-in user’s role (**Admin / Teacher / Student**).

## Overview

EDU Manage is designed to centralize the most common academic workflows:

- Maintaining **student and teacher records**
- Creating and organizing **courses**
- Handling **enrollment lists** by **academic year** and **semester**
- Tracking **performance** (points, grade, status, completion date)
- Enforcing **permissions and visibility** through role-based access

---

## Key Features

### Admin
Admins have full control over the system and can:
- Manage (create/edit/delete) **courses**
- Manage (create/edit/delete) **students**
- Manage (create/edit/delete) **teachers**
- Assign teachers to courses (primary / secondary where applicable)
- Manage enrollments (add/remove students from courses)
- Filter/search entities in the UI (students/courses lists)

### Teacher
Teachers can:
- View only the courses they teach (first or second teacher assignment)
- View enrolled students per course and filter by academic year
- Update student performance details for enrollments such as:
  - exam points
  - seminar points
  - project points
  - additional points
  - final grade
  - completion date
  - active/inactive status
- Access course student lists in a dedicated teacher view

### Student
Students can:
- View only the courses they are enrolled in
- See their enrollment status and results (points/grade/completion date)
- View course-related links (e.g., seminar/project URL) depending on what the system allows for students

---

## Tech Stack

- **Backend:** ASP.NET Core MVC (C#)
- **Database:** SQLite (via Entity Framework Core)
- **Auth:** ASP.NET Core Identity (role-based authorization)
- **Frontend:** Razor Views (HTML), CSS, minimal JavaScript
- **Styling/UI:** Bootstrap-based layout with custom CSS theme

---

## Project Structure (High level)

Common folders you will find in the solution:

- `Controllers/` – MVC controllers (Courses, Students, etc.)
- `Models/` – domain models (e.g., Course, Student, Enrollment, Teacher)
- `Models/ViewModels/` – view models used by the UI forms and pages
- `Views/` – Razor views for UI pages
- `wwwroot/` – static assets (CSS/JS/images)

---

## Getting Started (Local Setup)

### Prerequisites
- **.NET SDK** (recommended: .NET 6/7/8 depending on what the solution targets)
- Optional: Visual Studio 2022 / Rider / VS Code

### Run the app
1. Clone the repository
2. Open the solution in your IDE
3. Restore dependencies (usually automatic)
4. Run the project:
   - Visual Studio: **Run (IIS Express / Kestrel)**
   - CLI:
     ```bash
     dotnet restore
     dotnet run
     ```

The application will start and be available on a local URL (displayed in your console output).

---

## Database

EDU Manage uses **SQLite** and **Entity Framework Core** for persistence.

Typical steps (depending on how your repo is configured):
- If migrations are included and database is not created:
  ```bash
  dotnet ef database update
  ```
- If the repository already includes a prebuilt `.db` file, you can run immediately.

> If you want, tell me what your `appsettings.json` connection string looks like and I can tailor this section exactly to your setup.

---

## Authentication & Roles

The application uses **ASP.NET Core Identity** and `[Authorize]` / `[Authorize(Roles="...")]` to secure endpoints.

Role-based behavior examples:
- Admin can access full CRUD actions (e.g., course creation)
- Teacher is restricted to courses assigned to them
- Student is restricted to courses where they have an enrollment record

---

## Usage Notes

- **Course visibility**
  - Admin: sees all courses
  - Teacher: sees only courses where they are assigned as first or second teacher
  - Student: sees only enrolled courses

- **Enrollment data**
  - Enrollments are tied to a course and a student and typically include:
    - academic year + semester
    - points breakdown
    - grade
    - completion date
    - active/inactive status

---

## Screenshots

You can include screenshots in the repo like this:

```md
![Home page](docs/screenshots/home.png)
```

Suggested screenshots to add:
- Home/Landing page (EduManage Platform)
- Admin: Students list + filters
- Teacher: Course enrollments view (points/grade/status)
- Student: My courses / results page

---

## License

Add your preferred license here (MIT, Apache-2.0, etc.).  
If this is a university/course project, you can also state: “For educational purposes”.

---
