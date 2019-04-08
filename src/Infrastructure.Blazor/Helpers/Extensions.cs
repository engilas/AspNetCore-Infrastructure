using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Blazor.Exceptions;
using Infrastructure.Response;
using Microsoft.JSInterop;

namespace Infrastructure.Blazor.Helpers
{
    public static class Extensions
    {
        public static void CheckSuccess(this BaseResponse response)
        {
            if (response.Status != ResponseCode.OK)
            {
                JsFunctions.Alert("Error: " + response.Message);
                throw new ApiException();
            }
        }

        public static async Task<T> DeleteAsync<T>(this HttpClient http, string url)
        {
            var response = await http.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return Json.Deserialize<T>(json);
        }

        public static async Task<T> DeleteAsync<T>(this HttpClient http, string url, object content)
        {
            var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url)
                {Content = new StringContent(Json.Serialize(content), Encoding.UTF8, "application/json")});
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return Json.Deserialize<T>(json);
        }
    }
}
