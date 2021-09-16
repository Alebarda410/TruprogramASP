using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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

            IQueryable<Course> courses = null;
            // если пользователь слушатель
            if (user.Role == 0)
            {
                var coursesIds =
                    from userCourse in _db.UsersCourses
                    where userCourse.UserId == user.Id
                    select userCourse.CourseId;

                courses = _db.Courses.Where(course => coursesIds.Contains(course.Id));
            }
            // если пользователь лектор
            if (user.Role == 1)
            {
                courses = _db.Courses.Where(course => course.AuthorId==user.Id);
            }
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
            var course = await _db.Courses.FindAsync(resultId);
            var userCourse = _db.UsersCourses.FirstOrDefault(uCourse =>
                uCourse.CourseId == course.Id && uCourse.UserId == userId);

            ViewBag.UserCourse = userCourse;

            if (user == null || course == null)
            {
                return "Данные не найдены";
            }
            // если слушатель хочет записаться на курс
            if (zap.Equals("zap") && user.Role == 0)
            {
                if (userCourse != null)
                {
                    return "Вы уже записаны на этот курс!";
                }

                _db.UsersCourses.Add(new UsersCourses { CourseId = course.Id, UserId = userId });
                await _db.SaveChangesAsync();
                return "zap";
            }
            // если слушатель хочет отписаться от курса
            if (zap.Equals("otp") && user.Role == 0)
            {
                if (userCourse == null)
                {
                    return "Вы не можете отписаться от этого курса!";
                }

                _db.UsersCourses.Remove(userCourse);
                await _db.SaveChangesAsync();
                return "otp";
            }
            // если лектор хочет удалить курс
            if (zap.Equals("del") && user.Role == 1)
            {
                if (userCourse == null)
                {
                    return "Вы не можете удалить этот курс!";
                }

                var usersCourses = _db.UsersCourses.Where(usCourses => usCourses.CourseId == course.Id);
                _db.UsersCourses.RemoveRange(usersCourses);

                _db.Courses.Remove(course);

                await _db.SaveChangesAsync();

                System.IO.File.Delete(_appEnvironment.WebRootPath + course.Logo);
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
                AuthorId = user.Id,
                Author = author,
                DateTimePost = DateTime.Now,
                DateTimeStart = dateTimeStart,
                Description = description,
                UrlCourse = urlCourse,
                Logo = path
            };
            await _db.Courses.AddAsync(course);
            await _db.SaveChangesAsync();
            
            await using var fs = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create);
            await logo.CopyToAsync(fs);
            return "Курс успешно добавлен!";
        }
    }
}
