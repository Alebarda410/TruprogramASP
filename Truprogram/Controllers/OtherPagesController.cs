using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Truprogram.Services;

namespace Truprogram.Controllers
{
    public class OtherPagesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<string> Contact(EmailService service, string name, string email, string text)
        {
            if (name == null || email == null || text == null)
                return "Некорректные данные!";
            if (name.Length < 2 || name.Length > 10)
                return "Некорректная длина имени!";
            if (text.Length < 10 || text.Length > 5000)
                return "Некорректная длина обращения!";
            if (email.Length < 5 || email.Length > 50 || !email.Contains("@"))
                return "Некорректный email!";

            await service.Send(email, email, text);
            return "Обращение зарегистрировано";
        }
    }
}