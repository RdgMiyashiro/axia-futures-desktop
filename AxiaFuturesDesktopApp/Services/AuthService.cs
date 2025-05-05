using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace AxiaFuturesDesktopApp.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<bool> LoginAsync(string username, string password)
        {
            var payload = new { username, password };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://beta.axiafutures.com/api/mock-login", content);
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }
    }
}
