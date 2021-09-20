using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Truprogram.Models
{
    
    [Table("UsersCourses")]
    public class UsersCourses
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
    }
}