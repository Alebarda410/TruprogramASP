using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Truprogram.Models
{
    public class UserValidation
    {
        [Required(ErrorMessage = "Не указано имя")]
        [RegularExpression(@"^[А-Я][а-я]{1,11}$", ErrorMessage = "Некорректное имя")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указан email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        [Remote(action: "CheckEmail", controller: "User", ErrorMessage = "Email уже используется")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9]).{6,}", ErrorMessage = "Некорректный пароль")]
        [StringLength(100, ErrorMessage = "Максимальная длина пароля 100 символов")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string PasswordConfirm { get; set; }

        [Required(ErrorMessage = "Не указана роль")]
        [Range(0, 1, ErrorMessage = "Недопустимая роль")]
        public byte Role { get; set; }
    }
}