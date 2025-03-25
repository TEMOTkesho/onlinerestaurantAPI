    using System.Threading.Tasks;

    namespace OnlineRestaurantAPI.Services
    {
        public interface IEmailService
        {
            Task SendEmailAsync(string toEmail, string subject, string body);
        }
    }
