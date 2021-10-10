using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Truprogram.Models;
using Truprogram.Services;

namespace Truprogram.Controllers
{
    public class UserController : Controller
    {
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly DataBaseContext _db;

        public UserController(DataBaseContext context, IWebHostEnvironment appEnvironment)
        {
            _db = context;
            _appEnvironment = appEnvironment;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            var user = _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == HttpContext.User.Identity.Name);

            if (user == null)
                return LocalRedirect("/");

            return View(user);
        }

        [HttpPost]
        [Authorize(Policy = "OnlyVerification")]
        public string Profile(string name, string email, IFormFile avatar, string password, string new_password,
            string new_password_2)
        {
            var user = _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == HttpContext.User.Identity.Name);
            if (user == null)
                return "Такого пользователя нет";
            
            var changes = false;

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                return "Подтвердите пароль!";

            if (name != null)
            {
                if (user.Name == name)
                    return "Вы ввели ваше старое имя!";
                if (!Regex.IsMatch(name, "/^[А-Я][а-я]{1,11}$/u"))
                    return "Имя не соответствует требованиям!";

                user.Name = name;
                changes = true;
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
                changes = true;
            }

            if (new_password != null)
            {
                if (new_password.Length > 50)
                    return "Некорректный пароль!";
                if (new_password != new_password_2)
                    return "Пароли не совпадают";
                if (BCrypt.Net.BCrypt.Verify(new_password, user.Password))
                    return "Новый и старый пароли совпадают!";
                if (!Regex.IsMatch(new_password, "/^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{6,}/"))
                    return "Пароль не соответствует требованиям!";

                user.Password = BCrypt.Net.BCrypt.HashPassword(new_password);
                changes = true;
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
                changes = true;
            }

            if (!changes) return "Данные не менялись!";
            _db.SaveChanges();

            HttpContext.Session.SetString("userAvatar", user.Avatar);
            HttpContext.Session.SetString("user", user.Name);

            return "Данные изменены, обновите страницу!";
        }

        [HttpGet]
        public IActionResult Login()
        {
            var referUrl = Request.GetTypedHeaders().Referer.ToString();
            if (!referUrl.Equals(""))
            {
                if (!referUrl.Contains("User/Register"))
                    HttpContext.Session.SetString("BackUrl", referUrl);
            }
            else
            {
                HttpContext.Session.SetString("BackUrl", "/");
            }

            return View();
        }

        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new(ClaimsIdentity.DefaultRoleClaimType, user.Role.Name),
                new ("veref", user.Verification.ToString())
            };
            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            foreach (var key in HttpContext.Session.Keys) HttpContext.Session.Remove(key);

            Response.Cookies.Delete(".AspNetCore.Session");
            return RedirectToAction("Index", "OtherPages");
        }

        [HttpPost]
        public async Task<string> Login(string email, string password)
        {
            if (email == null || password == null)
                return "Данные не корректны!";

            var user = _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == email);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                return "Введенные данные не верны!";
            
            await Authenticate(user);
            
            HttpContext.Session.SetString("userAvatar", user.Avatar);
            HttpContext.Session.SetString("user", user.Name);
            
            return "1";
        }

        [HttpGet]
        public IActionResult Register()
        {
            var referUrl = Request.GetTypedHeaders().Referer.ToString();
            if (!referUrl.Equals(""))
            {
                if (!referUrl.Contains("User/Login"))
                    HttpContext.Session.SetString("BackUrl", referUrl);
            }
            else
            {
                ViewBag.BackUrl = "/";
            }

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
            if (!ModelState.IsValid)
                return "Ошибка";
            var userRole = await _db.Roles.FirstOrDefaultAsync(role => role.Name == uv.Role);
            if (userRole == null)
                return "Ошибка";
            var user = new User
            {
                Email = uv.Email,
                Name = uv.Name,
                Avatar = "/upload/def_avatar.svg",
                Password = BCrypt.Net.BCrypt.HashPassword(uv.Password),
                Role = userRole,
                Verification = false,
                TimeRegistration = DateTime.Now
            };
            _db.Users.Add(user);
            var result = _db.SaveChangesAsync();

            var token = Encoding.UTF8.GetBytes(uv.Email);

            var message = "Для подтверждения регистрации пройдите по <a href='"
                          + "https://localhost:44352/User/Activation?token="
                          + string.Join("-", token)
                          + "'>ссылке</a>.";
            await service.Send(uv.Email, "Подтверждение регистрации", message);

            await result;
            await Authenticate(user);

            HttpContext.Session.SetString("userAvatar", user.Avatar);
            HttpContext.Session.SetString("user", user.Name);
            HttpContext.Session.Remove("BackUrl");

            return "1";
        }

        public async Task<IActionResult> Activation(string token)
        {
            // проверка валидности токена
            if (token == null || !token.Contains("-"))
                return LocalRedirect("/");

            var tempStr = token.Split("-");
            var arr = Array.ConvertAll(tempStr, byte.Parse);
            var email = Encoding.UTF8.GetString(arr);
            
            var user = _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == email);
            if (user == null || user.Verification)
                return LocalRedirect("/");
            
            user.Verification = true;
            await _db.SaveChangesAsync();
            
            await Authenticate(user);
            
            HttpContext.Session.SetString("userAvatar", user.Avatar);
            HttpContext.Session.SetString("user", user.Name);
            
            return LocalRedirect("/");
        }
        
        [Authorize(Policy = "OnlyVerification")]
        public async Task<IActionResult> DelProfileAsync()
        {
            var user = _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Email == HttpContext.User.Identity.Name);

            if (user == null) return LocalRedirect("/");
            
            if (user.Role.Name == "Лектор")
            {
                var courses = _db.Courses.Where(course => course.AuthorId == user.Id);
                foreach (var course in courses)
                {
                    System.IO.File.Delete(_appEnvironment.WebRootPath + course.Logo);
                    _db.UsersCourses.RemoveRange(_db.UsersCourses.Where(c => c.CourseId == course.Id));
                }
                _db.Courses.RemoveRange(courses);
            }
            else if (user.Role.Name == "Слушатель")
            {
                var usersCourses = _db.UsersCourses.Where(usCourses => usCourses.UserId == user.Id);
                _db.UsersCourses.RemoveRange(usersCourses);
            }

            System.IO.File.Delete(_appEnvironment.WebRootPath + user.Avatar);
            _db.Users.Remove(user);

            await _db.SaveChangesAsync();

            await Logout();
            return LocalRedirect("/");
        }
    }
}