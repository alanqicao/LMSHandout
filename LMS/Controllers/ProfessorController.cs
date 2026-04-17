using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query =
                from stu in db.Students
                join e in db.Enrolleds on stu.UId equals e.Student
                join c in db.Classes on e.Class equals c.ClassId
                join cor in db.Courses on c.Listing equals cor.CatalogId
                where cor.Department == subject && cor.Number == num && c.Season == season && c.Year == year
                select new { fname = stu.FName, lname = stu.LName, uid = stu.UId, dob = stu.Dob, grade = e.Grade };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category == null)
            {
                var query =
                    from a in db.Assignments
                    join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                    join c in db.Classes on ac.InClass equals c.ClassId
                    join cor in db.Courses on c.Listing equals cor.CatalogId                  
                    where cor.Department == subject && cor.Number == num && c.Season == season && c.Year == year
                    select new { aname = a.Name, cname = ac.Name, due = a.Due, submissions = db.Submissions.Count(s => s.Assignment == a.AssignmentId) };

                return Json(query.ToArray());
            }
            else 
            {
                var query =
                  from a in db.Assignments
                  join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                  join c in db.Classes on ac.InClass equals c.ClassId
                  join cor in db.Courses on c.Listing equals cor.CatalogId
                  where cor.Department == subject && cor.Number == num && c.Season == season && c.Year == year && ac.Name == category
                  select new { aname = a.Name, cname = ac.Name, due = a.Due, submissions = db.Submissions.Count(s => s.Assignment == a.AssignmentId) };

                return Json(query.ToArray());
            }
            
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query =
                    from ac in db.AssignmentCategories
                    join c in db.Classes on ac.InClass equals c.ClassId
                    join cor in db.Courses on c.Listing equals cor.CatalogId
                    where cor.Department == subject && cor.Number == num && c.Season == season && c.Year == year
                    select new { name = ac.Name, weight = ac.Weight };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var classes = from ac in db.AssignmentCategories
                          join c in db.Classes on ac.InClass equals c.ClassId
                          join cor in db.Courses on c.Listing equals cor.CatalogId
                          where cor.Department == subject && cor.Number == (uint)num && c.Season == season && c.Year == (uint)year && ac.Name == category
                          select c;

            if (classes.Any())
            {
                return Json(new { success = false });
            }
            else 
            {
                var currentClass = (
                              from c in db.Classes
                              join cor in db.Courses on c.Listing equals cor.CatalogId
                              where cor.Department == subject && cor.Number == (uint)num && c.Season == season && c.Year == (uint)year
                              select c).FirstOrDefault();

                AssignmentCategory assignmentCategory = new AssignmentCategory();

                assignmentCategory.Name = category;
                assignmentCategory.Weight = (uint)catweight;
                assignmentCategory.InClass = currentClass.ClassId;

                db.AssignmentCategories.Add(assignmentCategory);
                db.SaveChanges();

                return Json(new { success = true });
            }
           
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            
            var assignmentCategory = from ac in db.AssignmentCategories 
                                     join c in db.Classes on ac.InClass equals c.ClassId
                                     join cor in db.Courses on c.Listing equals cor.CatalogId
                                     where cor.Department == subject && cor.Number == num && c.Season == season && c.Year == year && ac.Name == category
                                     select ac;
            if (assignmentCategory.Any())
            {
                Assignment a = new Assignment();

                a.Name = asgname;
                a.Contents = asgcontents;
                a.Due = asgdue;
                a.MaxPoints = (uint)asgpoints;
                a.Category = assignmentCategory.FirstOrDefault().CategoryId;
                db.Assignments.Add(a);
                db.SaveChanges();
                updateGrade(subject, num, season, year, category, null);

                return Json(new { success = true });
            }
            else 
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var submissions = from s in db.Submissions
                              join stu in db.Students on s.Student equals stu.UId
                              join a in db.Assignments on s.Assignment equals a.AssignmentId
                              join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                              join c in db.Classes on ac.InClass equals c.ClassId
                              join cor in db.Courses on c.Listing equals cor.CatalogId
                              where cor.Department == subject && cor.Number == num && c.Season == season && c.Year == year && ac.Name == category && a.Name == asgname
                              select new { fname = stu.FName, lname = stu.LName, uid = stu.UId, time = s.Time, score = s.Score};

            return Json(submissions.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var submissions = from s in db.Submissions
                              join stu in db.Students on s.Student equals stu.UId
                              join a in db.Assignments on s.Assignment equals a.AssignmentId
                              join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                              join c in db.Classes on ac.InClass equals c.ClassId
                              join cor in db.Courses on c.Listing equals cor.CatalogId
                              where cor.Department == subject && cor.Number == num && c.Season == season && c.Year == year && ac.Name == category && a.Name == asgname && stu.UId == uid
                              select s;

            if (submissions.Any()) 
            {
                submissions.FirstOrDefault().Score = (uint)score;
                db.SaveChanges();

                updateGrade(subject, num, season, year, category, uid);
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
     
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var myClasses =
                    from pro in db.Professors
                    join c in db.Classes on pro.UId equals c.TaughtBy
                    join cor in db.Courses on c.Listing equals cor.CatalogId
                    where pro.UId == uid
                    select new {subject = cor.Department, number = cor.Number, name = cor.Name, season = c.Season, year = c.Year };

            return Json(myClasses.ToArray());
        }

        /// <summary>
        /// Helper method to update the grade of a student in a class based on the scores of their assignment submissions.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class</param>
        /// <param name="year">The year part of the semester for the class</param>
        /// <param name="category">The name of the assignment category</param>
        /// <param name="uid">The uid of the student</param>
        private void updateGrade(string subject, int num, string season, int year, string category, string uid) 
        {     
            if (uid != null)
            {
                var asCategories = from a in db.Assignments
                                   join s in db.Submissions on a.AssignmentId equals s.Assignment
                                   join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                                   join c in db.Classes on ac.InClass equals c.ClassId
                                   join en in db.Enrolleds on c.ClassId equals en.Class
                                   join cor in db.Courses on c.Listing equals cor.CatalogId
                                   where cor.Department == subject
                                            && cor.Number == num
                                            && en.Student == uid
                                            && c.Season == season
                                            && c.Year == year
                                            && db.Assignments.Any(a => a.Category == ac.CategoryId)
                                   select new { weight = ac.Weight, maxPoints = a.MaxPoints, score = s.Score, grade = en.Grade, };

                double totalScaledScore = 0;
                double totalCategoryWeight = 0;

                foreach (var c in asCategories)
                {
                    double score = c.score == null ? 0 : (double)c.score;

                    double percentage = score / c.maxPoints;

                    totalScaledScore += percentage * c.weight;

                    totalCategoryWeight += c.weight;

                }
                double ScalingFactor = 100 / totalCategoryWeight;
                double finalPercentage = Math.Round((totalScaledScore * ScalingFactor), 1);

                string letterGrade;
                if (finalPercentage >= 93) letterGrade = "A";
                else if (finalPercentage >= 90) letterGrade = "A-";
                else if (finalPercentage >= 87) letterGrade = "B+";
                else if (finalPercentage >= 83) letterGrade = "B";
                else if (finalPercentage >= 80) letterGrade = "B-";
                else if (finalPercentage >= 77) letterGrade = "C+";
                else if (finalPercentage >= 73) letterGrade = "C";
                else if (finalPercentage >= 70) letterGrade = "C-";
                else if (finalPercentage >= 67) letterGrade = "D+";
                else if (finalPercentage >= 63) letterGrade = "D";
                else if (finalPercentage >= 60) letterGrade = "D-";
                else
                {
                    letterGrade = "E";
                }

                var enrolled = from a in db.Assignments
                               join s in db.Submissions on a.AssignmentId equals s.Assignment
                               join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                               join c in db.Classes on ac.InClass equals c.ClassId
                               join en in db.Enrolleds on c.ClassId equals en.Class
                               join cor in db.Courses on c.Listing equals cor.CatalogId
                               where cor.Department == subject
                                        && en.Student == uid
                                        && cor.Number == num
                                        && c.Season == season
                                        && c.Year == year
                                        && db.Assignments.Any(a => a.Category == ac.CategoryId)
                               select en;

                enrolled.FirstOrDefault().Grade = letterGrade;

                db.SaveChanges();
            }
            else 
            {
                var asCategories = from a in db.Assignments

                                   join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                                   join c in db.Classes on ac.InClass equals c.ClassId
                                   join en in db.Enrolleds on c.ClassId equals en.Class
                                   join cor in db.Courses on c.Listing equals cor.CatalogId
                                   join s in db.Submissions
                                   on new { A = a.AssignmentId, B = uid } equals new { A = s.Assignment, B = s.Student } into joined
                                   from j in joined.DefaultIfEmpty()
                                   where cor.Department == subject
                                         && cor.Number == num
                                         && en.Student == uid
                                         && c.Season == season
                                         && c.Year == year
                                   select new { weight = ac.Weight, maxPoints = a.MaxPoints, score = j == null ? (uint?)null : j.Score };

                double totalScaledScore = 0;
                double totalCategoryWeight = 0;

                foreach (var c in asCategories)
                {
                    double score = c.score == null ? 0 : (double)c.score;

                    double percentage = score / c.maxPoints;

                    totalScaledScore += percentage * c.weight;

                    totalCategoryWeight += c.weight;
                }

                if (totalCategoryWeight == 0)
                {
                    return;
                }

                double ScalingFactor = 100 / totalCategoryWeight;
                double finalPercentage = Math.Round((totalScaledScore * ScalingFactor), 1);
                string letterGrade;
                if (finalPercentage >= 93) letterGrade = "A";
                else if (finalPercentage >= 90) letterGrade = "A-";
                else if (finalPercentage >= 87) letterGrade = "B+";
                else if (finalPercentage >= 83) letterGrade = "B";
                else if (finalPercentage >= 80) letterGrade = "B-";
                else if (finalPercentage >= 77) letterGrade = "C+";
                else if (finalPercentage >= 73) letterGrade = "C";
                else if (finalPercentage >= 70) letterGrade = "C-";
                else if (finalPercentage >= 67) letterGrade = "D+";
                else if (finalPercentage >= 63) letterGrade = "D";
                else if (finalPercentage >= 60) letterGrade = "D-";
                else {
                    letterGrade = "E";
                }
                    
                var enrolled = from a in db.Assignments
                               join s in db.Submissions on a.AssignmentId equals s.Assignment
                               join ac in db.AssignmentCategories on a.Category equals ac.CategoryId
                               join c in db.Classes on ac.InClass equals c.ClassId
                               join en in db.Enrolleds on c.ClassId equals en.Class
                               join cor in db.Courses on c.Listing equals cor.CatalogId
                               where cor.Department == subject
                                        && cor.Number == num
                                        && c.Season == season
                                        && c.Year == year
                                        && db.Assignments.Any(a => a.Category == ac.CategoryId)
                               select en;

                foreach (var e in enrolled)
                {
                    e.Grade = letterGrade;
                }

                db.SaveChanges();
            }
        }

        /*******End code to modify********/
    }
}

