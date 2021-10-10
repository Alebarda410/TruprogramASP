using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Truprogram.Models;

namespace Truprogram.Controllers
{
    public class CourseController : Controller
    {
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly DataBaseContext _db;

        public CourseController(DataBaseContext context, IWebHostEnvironment appEnvironment)
        {
            _db = context;
            _appEnvironment = appEnvironment;
        }

        [Authorize(Policy = "OnlyVerification")]
        public IActionResult MyListCourses()
        {
            var user = _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == HttpContext.User.Identity.Name);
            if (user == null) return LocalRedirect("/");
            
            ViewBag.info = "Мои курсы";

            if (user.Role.Name == "Слушатель")
            {
                var coursesIds = _db.UsersCourses
                    .Select(courses => courses.Id)
                    .Where(id => id == user.Id);
                
                return View("ListCourses", _db.Courses.Where(course => coursesIds.Contains(course.Id)));
            }

            if (user.Role.Name == "Лектор")
                return View("ListCourses", _db.Courses.Where(course => course.AuthorId == user.Id));
            
            return LocalRedirect("/");
        }

        [HttpGet]
        public IActionResult ListCourses()
        {
            ViewBag.info = "Активные курсы";
            return View(_db.Courses.Take(5));
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

            if (course == null)
                return LocalRedirect("/");
            
            var user = _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == HttpContext.User.Identity.Name);

            ViewBag.role = user.Role.Name;
            ViewBag.author = user.Id;
            ViewBag.verefication = user.Verification;
            
            var daysLeft = (course.DateTimeStart - DateTime.Today).Days;

            ViewBag.daysLeft = daysLeft;

            return View(course);
        }

        [HttpPost]
        [Authorize(Policy = "OnlyVerification")]
        public async Task<string> CoursePage(string zap, string courseId)
        {
            if (zap == null || !int.TryParse(courseId, out var resultId))
                return "Данные не валидны!";

            var user = _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == HttpContext.User.Identity.Name);
            
            var course = await _db.Courses.FindAsync(resultId);
            
            if (user == null || course == null) return "Данные не найдены";
            
            var userCourse = _db.UsersCourses
                .FirstOrDefault(uc => uc.CourseId == course.Id && uc.UserId == user.Id);
            
            ViewBag.UserCourse = userCourse;

            // запись на курс
            if (zap.Equals("zap") && user.Role.Name == "Слушатель")
            {
                if (userCourse != null) return "Вы уже записаны на этот курс!";

                _db.UsersCourses.Add(new UsersCourses {CourseId = course.Id, UserId = user.Id});
                await _db.SaveChangesAsync();
                return "zap";
            }

            // отписаться от курса
            if (zap.Equals("otp") && user.Role.Name == "Слушатель")
            {
                if (userCourse == null) return "Вы не можете отписаться от этого курса!";

                _db.UsersCourses.Remove(userCourse);
                await _db.SaveChangesAsync();
                return "otp";
            }

            // удалить курс
            if (zap.Equals("del") && user.Role.Name == "Лектор")
            {
                if (course.AuthorId != user.Id) return "Вы не можете удалить этот курс!";
                var delUsCo = _db.UsersCourses.Where(c => c.CourseId == course.Id);
                
                System.IO.File.Delete(_appEnvironment.WebRootPath + course.Logo);
                _db.Courses.Remove(course);
                
                _db.UsersCourses.RemoveRange(delUsCo);
                
                await _db.SaveChangesAsync();

                return "del";
            }
            return "Ошибка!";
        }

        [HttpGet]
        [Authorize(Roles = "Лектор", Policy = "OnlyVerification")]
        public IActionResult AddCourse()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Лектор", Policy = "OnlyVerification")]
        public async Task<string> AddCourseAsync(string urlCourse, string name, string author, IFormFile logo,
            string date_time_start, string description, string password)
        {
            var user = _db.Users
                .FirstOrDefault(u => u.Email == HttpContext.User.Identity.Name);

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

            var course = new Course
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
            await using var fs = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create);
            await logo.CopyToAsync(fs);
            
            await _db.Courses.AddAsync(course);
            await _db.SaveChangesAsync();
            
            return "Курс успешно добавлен!";
        }
    }
}