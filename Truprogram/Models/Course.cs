using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truprogram.Models
{
    /// <summary>
    ///     Описывает курс
    /// </summary>
    [Table("Courses")]
    public class Course
    {
        /// <summary>
        ///     Ид курса
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        ///     Название курса
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Автор курса
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///     Ид автора курса
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        ///     Логотип курса
        /// </summary>
        public string Logo { get; set; }

        /// <summary>
        ///     Описание курса
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Ссылка на курс
        /// </summary>
        public string UrlCourse { get; set; }

        /// <summary>
        ///     Дата создания курса
        /// </summary>
        public DateTime DateTimePost { get; set; }

        /// <summary>
        ///     Дата старта курса
        /// </summary>
        public DateTime DateTimeStart { get; set; }
    }
}