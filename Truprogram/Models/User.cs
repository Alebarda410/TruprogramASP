using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truprogram.Models
{
    /// <summary>
    ///     Описывает как преподавателей так и обучающихся
    /// </summary>
    [Table("Users")]
    public class User
    {
        /// <summary>
        ///     Ид пользователя
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        ///     Имя пользователя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Электронная почта пользователя
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Пароль пользователя
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Аватар пользователя
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        ///     Дата и время регистрации пользователя
        /// </summary>
        public DateTime TimeRegistration { get; set; }

        /// <summary>
        ///     Роль пользователя
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        ///     Показывает подтвердил ли пользователь свою почту
        /// </summary>
        public bool Verification { get; set; }
    }
}