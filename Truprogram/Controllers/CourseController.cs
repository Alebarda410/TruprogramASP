using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Truprogram.Models;

namespace Truprogram.Controllers
{
    public class CourseController : Controller
    {
        private readonly DataBaseContext _db;
        private readonly IWebHostEnvironment _appEnvironment;

        public CourseController(DataBaseContext context, IWebHostEnvironment appEnvironment)
        {
            _db = context;
            _appEnvironment = appEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> ListCourses(string myCourses)
        {
            if (myCourses == null)
            {
                ViewBag.info = "Активные курсы";
                return View(_db.Courses.Take(5));
            }
            ViewBag.info = "Мои курсы";

            if (!HttpContext.Session.Keys.Contains("user"))
                return LocalRedirect("/");

            var user = await _db.Users.FindAsync(HttpContext.Session.GetInt32("userId"));
            if (user == null)
                return LocalRedirect("/");

            var courses = _db.Courses.Where(course => user.UserCourses.Contains(course.Id));

            return View(courses);
        }

        public JsonResult JsonCourses(string currentCourses)
        {
            if (!int.TryParse(currentCourses, out var result))
                LocalRedirect("/");

            var courses = _db.Courses.Skip(result).Take(5);
            return Json(courses);
        }

        [HttpGet]
        public IActionResult CoursePage(string id)
        {
            if (!int.TryParse(id, out var resultId))
                return LocalRedirect("/");

            var course = _db.Courses.Find(resultId);

            var daysLeft = (course.DateTimeStart - DateTime.Today).Days;

            ViewBag.daysLeft = daysLeft;

            return View(course);
        }
        [HttpPost]
        public async Task<string> CoursePage(string zap, string courseId)
        {
            if (zap == null || !int.TryParse(courseId, out var resultId))
                return "Данные не валидны!";

            var userId = (int)HttpContext.Session.GetInt32("userId");
            var user = await _db.Users.FindAsync(userId);
            var course = await _db.Courses.FindAsync(courseId);

            if (user == null || course == null)
                return "Данные не найдены";

            if (zap.Equals("zap") && user.Role == 0)
            {
                if (user.UserCourses.Contains(resultId) || course.Learners.Contains(userId))
                    return "Вы уже записаны на этот курс!";

                user.UserCourses.Add(resultId);
                course.Learners.Add(userId);

                await _db.SaveChangesAsync();
                return "zap";
            }
            if (zap.Equals("otp") && user.Role == 0)
            {
                if (!user.UserCourses.Contains(resultId) || !course.Learners.Contains(userId))
                    return "Вы не можете отписаться от этого курса!";

                user.UserCourses.Remove(resultId);
                course.Learners.Remove(userId);

                await _db.SaveChangesAsync();
                return "otp";
            }
            if (zap.Equals("del") && user.Role == 1)
            {
                user.UserCourses.Remove(resultId);

                //при наличии вторичного ключа этого можно избежать путем каскадного удаления

                Parallel.ForEach(course.Learners,
                    new ParallelOptions { MaxDegreeOfParallelism = 10 },
                    async i =>
                    {
                        var u = await _db.Users.FindAsync(i);
                        u.UserCourses.Remove(resultId);
                    });

                _db.Courses.Remove(course);
                System.IO.File.Delete(_appEnvironment.WebRootPath + course.Logo);
                await _db.SaveChangesAsync();
                return "del";
            }
            return "Ошибка!";
        }
        [HttpGet]
        public IActionResult AddCourse()
        {
            if (!HttpContext.Session.Keys.Contains("user"))
                return LocalRedirect("/");

            if (HttpContext.Session.GetInt32("userRole") != 1)
                return LocalRedirect("/");

            return View();
        }
        [HttpPost]
        public async Task<string> AddCourseAsync(string urlCourse, string name, string author, IFormFile logo, string date_time_start, string description, string password)
        {
            if (!HttpContext.Session.Keys.Contains("user"))
                LocalRedirect("/");

            if (HttpContext.Session.GetInt32("userRole") != 1)
                LocalRedirect("/");

            var user = await _db.Users
                .FindAsync(HttpContext.Session.GetInt32("userId"));
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return "Пароль введен неправильно!";

            if (name.Length is < 2 or > 50)
                return "Некорректное имя курса!";
            if (author.Length is < 2 or > 50)
                return "Некорректное имя автора!";
            if (description.Length is < 300 or > 20000)
                return "Превышена длина описания!";
            if (!DateTime.TryParse(date_time_start, out var dateTimeStart))
                return "Некорректная дата!";
            if (logo == null)
                return "Загрузите логотип курса!";
            if (logo.Length > 5242880)
                return "Файл должен быть меньше 5 мегабайт!";
            if (!logo.ContentType.Contains("image/"))
                return "Неверный тип файла!";
            if (logo.FileName.Contains("def_avatar"))
                return "Переименуйте файл!";

            var rnd = new Random();
            var path = "/upload/" + rnd.Next() + $"_{user.Id}_" + logo.FileName.Normalize();

            var course = new Course()
            {
                Name = name,
                AuthorId = (int)HttpContext.Session.GetInt32("userId"),
                Author = author,
                DateTimePost = DateTime.Now,
                DateTimeStart = dateTimeStart,
                Description = description,
                UrlCourse = urlCourse,
                Logo = path,
                Learners = new List<int>()
            };
            await _db.Courses.AddAsync(course);
            await _db.SaveChangesAsync();

            user.UserCourses.Add(course.Id);
            await _db.SaveChangesAsync();
            await using var fs = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create);
            await logo.CopyToAsync(fs);
            return "Курс успешно добавлен!";
        }
    }
}
