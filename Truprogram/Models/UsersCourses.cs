using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Truprogram.Models
{
    [Keyless]
    [Table("UsersCourses")]
    public class UsersCourses
    {
        public int UserId { get; set; }
        public int CourseId { get; set; }
    }
}