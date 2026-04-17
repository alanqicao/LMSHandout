using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from e in db.Enrolleds
                        where e.Student == uid
                        join c in db.Classes on e.Class equals c.ClassId
                        join cor in db.Courses on c.Listing equals cor.CatalogId
                        select new { 
                            subject = cor.Department, 
                            number = cor.Number, 
                            name = cor.Name,
                            season = c.Season, 
                            year = c.Year, 
                            grade = e.Grade == null ? "--" : e.Grade };                 

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {

            var query = from a in db.Assignments
                        join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                        join c in db.Classes on ac.InClass equals c.ClassId
                        join e in db.Enrolleds on c.ClassId equals e.Class
                        join cor in db.Courses on c.Listing equals cor.CatalogId
                        join s in db.Submissions on a.AssignmentId equals s.Assignment into grouped
                        from sub in grouped.DefaultIfEmpty()

                        where cor.Department == subject && cor.Number == num && c.Season == season && c.Year == year && e.Student == uid
                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.Due,
                            score = sub == null ? (uint?)null : sub.Score
                        };

            return Json(query.ToArray());      
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var assignment = (from a in db.Assignments
                              where a.Name == asgname
                              select a).FirstOrDefault();

            if (db.Submissions.Any(s => s.Student == uid && s.Assignment == assignment.AssignmentId))
            {
                var query =
                    (from s in db.Submissions
                     join a in db.Assignments on s.Assignment equals a.AssignmentId
                     join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                     join c in db.Classes on ac.InClass equals c.ClassId
                     join cor in db.Courses on c.Listing equals cor.CatalogId
                     where s.Student == uid && cor.Department == subject && c.Season == season && c.Year == year && cor.Number == num && ac.Name == category
                     select s).FirstOrDefault();

                if (query != null)
                {
                    query.SubmissionContents = contents;
                    query.Time = DateTime.Now;
                    db.SaveChanges();

                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            else
            {

                var query =
                             from a in db.Assignments
                             where a.Name == asgname
                             select a.AssignmentId;
                                  
                Submission submit = new Submission();
                submit.Assignment = query.FirstOrDefault();
                submit.Student = uid;
                submit.Score = 0;
                submit.SubmissionContents = contents;
                submit.Time = DateTime.Now;
                db.Submissions.Add(submit);
                db.SaveChanges();

                return Json(new { success = true });
            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var query = from e in db.Enrolleds
                        join c in db.Classes on e.Class equals c.ClassId
                        join cor in db.Courses on c.Listing equals cor.CatalogId
                        where cor.Department == subject && cor.Number == num && c.Season == season && c.Year == year && e.Student == uid
                        select e.Student;

            var course =
                        from cor in db.Courses
                        join c in db.Classes on cor.CatalogId equals c.Listing
                        where cor.Number == num && c.Season == season && c.Year == year
                        select c.ClassId;

            if (query.Any())
            {
                return Json(new { success = false });
            }
            else 
            {
                Enrolled enrolled = new Enrolled();
                enrolled.Student = uid;
                enrolled.Class = course.FirstOrDefault();
                enrolled.Grade = "--";
                db.Enrolleds.Add(enrolled);
                db.SaveChanges();
                return Json(new { success = true });

            }              
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            Dictionary<string, double> gpas = new Dictionary<string, double>
                {
                    { "A",   4.0 },
                    { "A-",  3.7 },
                    { "B+",  3.3 },
                    { "B",   3.0 },
                    { "B-",  2.7 },
                    { "C+",  2.3 },
                    { "C",   2.0 },
                    { "C-",  1.7 },
                    { "D+",  1.3 },
                    { "D",   1.0 },
                    { "D-",  0.7 },
                    { "E",   0.0 }
                };

            var grades =
                from e in db.Enrolleds
                where e.Student == uid
                select e.Grade;

            double gradeNumber = 0;
            int totalClass = grades.Count();
            double finalGpa = 0;

            foreach (string e in grades)
            {
                if (e == "--")
                {
                    totalClass -= 1;
                }
                else 
                {
                    gradeNumber += gpas[e];
                }
                
            }

            if (totalClass >= 1)
            {
                finalGpa = gradeNumber / totalClass;
            }
            else 
            {
                finalGpa = 0;
            }

            return Json(new { gpa = finalGpa });
        }
                
        /*******End code to modify********/

    }
}

