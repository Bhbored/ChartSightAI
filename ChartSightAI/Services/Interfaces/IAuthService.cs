using ChartSightAI.MVVM.Models;
using System.Threading.Tasks;

namespace ChartSightAI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserSession> GetSession();
        Task<UserSession> Login(string email, string password);
        Task Logout();
        Task<UserSession> SignUp(string email, string password);
    }
}
