using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Truprogram.Models
{
    [Table("UsersCourses")]
    public class UsersCourses
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public int CourseId { get; set; }
    }
}