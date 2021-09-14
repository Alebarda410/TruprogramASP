using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Truprogram.Models;
using Truprogram.Services;

namespace Truprogram.Controllers
{
    public class UserController : Controller
    {
        private readonly DataBaseContext _db;
        private readonly IWebHostEnvironment _appEnvironment;
        public UserController(DataBaseContext context, IWebHostEnvironment appEnvironment)
        {
            _db = context;
            _appEnvironment = appEnvironment;
        }
        [HttpGet]
        public IActionResult Profile()
        {
            var user = _db.Users
                .Find(HttpContext.Session.GetInt32("userId"));

            if (user == null)
                return LocalRedirect("/");

            return View(user);
        }
        [HttpPost]
        public string Profile(string name, string email, IFormFile avatar, string password, string new_password, string new_password_2)
        {
            if (!HttpContext.Session.Keys.Contains("user"))
                return "Пользователь не активен!";

            var user = _db.Users
                .Find(HttpContext.Session.GetInt32("userId"));
            var changes = 0;

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                return "Подтвердите пароль!";

            if (name != null)
            {
                if (user.Name.Equals(name))
                    return "Вы ввели ваше старое имя!";
                if (!Regex.IsMatch(name, "/^[А-Я][а-я]{1,11}$/u"))
                    return "Имя не соответствует требованиям!";

                user.Name = name;
                changes = 1;
            }

            if (email != null)
            {
                if (_db.Users.FirstOrDefault(u => u.Email == email) != null)
                    return "Такой Email уже зарегистроирован!";
                if (email.Length > 50)
                    return "Слишком длинный Email!";
                if (!email.Contains("@"))
                    return "Не корректный Email!";

                user.Email = email;
                changes = 1;
            }

            if (new_password != null)
            {
                if (new_password.Length > 50)
                    return "Некорректный пароль!";
                if (!new_password.Equals(new_password_2))
                    return "Пароли не совпадают";
                if (BCrypt.Net.BCrypt.Verify(new_password, user.Password))
                    return "Новый и старый пароли совпадают!";
                if (!Regex.IsMatch(new_password, "/^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{6,}/"))
                    return "Пароль не соответствует требованиям!";

                user.Password = BCrypt.Net.BCrypt.HashPassword(new_password);
                changes = 1;
            }

            if (avatar != null)
            {
                if (avatar.Length > 5242880)
                    return "Файл должен быть меньше 5 мегабайт!";
                if (!avatar.ContentType.Contains("image/"))
                    return "Неверный тип файла!";
                if (avatar.FileName.Contains("def_avatar"))
                    return "Переименуйте файл!";

                if (!user.Avatar.Contains("def_avatar"))
                    System.IO.File.Delete(_appEnvironment.WebRootPath + user.Avatar);

                var rnd = new Random();
                var path = "/upload/" + rnd.Next() + $"_{user.Id}_" + avatar.FileName.Normalize();

                using var fs = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create);
                avatar.CopyTo(fs);
                user.Avatar = path;
                changes = 1;
            }

            if (changes != 1) return "Данные не менялись!";
            _db.SaveChanges();
            HttpContext.Session.SetString("userAvatar", user.Avatar);
            HttpContext.Session.SetString("user", user.Name);
            return "Данные изменены! Обновите страницу!";
        }
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.Keys.Contains("user"))
                return LocalRedirect("/");

            var referUrl = Request.GetTypedHeaders().Referer.ToString();
            if (!referUrl.Equals(""))
            {
                if (!referUrl.Equals("https://localhost:44352/User/Register"))
                    HttpContext.Session.SetString("BackUrl", referUrl);
            }
            else HttpContext.Session.SetString("BackUrl", "/"); ;

            return View();
        }
        [HttpPost]
        public string Login(string email, string password)
        {
            if (HttpContext.Session.Keys.Contains("user"))
                return "Вы уже авторизированы!";
            if (email == null || password == null)
                return "Данные не корректны!";

            var user = _db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null) return "Email не зарегистрирован!";

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return "Пароль введен неправильно!";

            HttpContext.Session.SetInt32("userId", user.Id);
            HttpContext.Session.SetInt32("userRole", user.Role);
            HttpContext.Session.SetString("user", user.Name);
            HttpContext.Session.SetString("userAvatar", user.Avatar);
            HttpContext.Session.SetInt32("userVerification", Convert.ToInt32(user.Verification));
            HttpContext.Session.Remove("BackUrl");

            return "1";
        }
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.Keys.Contains("user"))
                return LocalRedirect("/");

            var referUrl = Request.GetTypedHeaders().Referer.ToString();
            if (!referUrl.Equals(""))
            {
                if (!referUrl.Equals("https://localhost:44352/User/Login"))
                    HttpContext.Session.SetString("BackUrl", referUrl);
            }
            else ViewBag.BackUrl = "https://localhost:44352";

            return View();
        }
        [AcceptVerbs("Get", "Post")]
        public IActionResult CheckEmail(string email)
        {
            return Json(!_db.Users.Any(u => u.Email == email));
        }
        [HttpPost]
        public async Task<string> Register(EmailService service, UserValidation uv)
        {
            if (HttpContext.Session.Keys.Contains("user"))
                return "Вы уже авторизированы!";

            if (!ModelState.IsValid)
                return "Ошибка";

            var user = new User
            {
                Email = uv.Email,
                Name = uv.Name,
                Avatar = "/upload/def_avatar.svg",
                Password = BCrypt.Net.BCrypt.HashPassword(uv.Password),
                Role = uv.Role,
                Verification = false,
                TimeRegistration = DateTime.Now
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetInt32("userId", user.Id);
            HttpContext.Session.SetInt32("userRole", user.Role);
            HttpContext.Session.SetString("user", user.Name);
            HttpContext.Session.SetString("userAvatar", user.Avatar);
            HttpContext.Session.SetInt32("userVerification", Convert.ToInt32(user.Verification));
            HttpContext.Session.Remove("BackUrl");
            var token = Encoding.UTF8.GetBytes(uv.Email);

            var message = "Для подтверждения регистрации пройдите по <a href='"
                          + "https://localhost:44352/User/Activation?token="
                          + string.Join("-", token)
                          + "'>ссылке</a>.";
            await service.Send(uv.Email, "Подтверждение регистрации", message);

            return "1";
        }

        public IActionResult Activation(string token)
        {
            if (token == null || !token.Contains("-"))
                return LocalRedirect("/");

            var tempStr = token.Split("-");
            var arr = Array.ConvertAll(tempStr, byte.Parse);
            var email = Encoding.UTF8.GetString(arr);
            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return LocalRedirect("/");
            if (user.Verification)
                return LocalRedirect("/");

            user.Verification = true;
            _db.SaveChanges();

            HttpContext.Session.SetInt32("userId", user.Id);
            HttpContext.Session.SetInt32("userRole", user.Role);
            HttpContext.Session.SetString("user", user.Name);
            HttpContext.Session.SetString("userAvatar", user.Avatar);
            HttpContext.Session.SetInt32("userVerification", Convert.ToInt32(user.Verification));

            return LocalRedirect("/");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            if (!HttpContext.Session.Keys.Contains("user"))
                return LocalRedirect("/");

            foreach (var key in HttpContext.Session.Keys)
            {
                HttpContext.Session.Remove(key);
            }

            Response.Cookies.Delete(".AspNetCore.Session");
            return LocalRedirect("/");
        }

        public async Task<IActionResult> DelProfileAsync()
        {
            if (!HttpContext.Session.Keys.Contains("user"))
                return LocalRedirect("/");

            var user = await _db.Users.FindAsync(HttpContext.Session.GetInt32("userId"));
            if (user == null)
                return LocalRedirect("/");

            if (user.Role == 1)
            {
                var courses = _db.Courses.Where(course => course.AuthorId == user.Id);
                if (courses.Any())
                {
                    _db.Courses.RemoveRange(courses);
                }
            }

            var usersCourses = _db.UsersCourses.Where(usCourses => usCourses.UserId == user.Id);
            _db.UsersCourses.RemoveRange(usersCourses);

            System.IO.File.Delete(_appEnvironment.WebRootPath + user.Avatar);
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            foreach (var key in HttpContext.Session.Keys)
            {
                HttpContext.Session.Remove(key);
            }

            Response.Cookies.Delete(".AspNetCore.Session");
            return LocalRedirect("/");
        }
    }
}
