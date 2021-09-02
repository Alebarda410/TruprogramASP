using System.Threading.Tasks;

namespace Truprogram.Services
{
    public interface ISendInfo
    {
        Task Send(string email, string subject, string message);
    }
}