using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Blazor.Exceptions;
using Infrastructure.Blazor.Helpers;
using Infrastructure.Response;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Infrastructure.Blazor.Api
{
    public abstract class BaseApiClient
    {
        //protected readonly LocalStorage LocalStorage;
        protected readonly HttpClient Client;
        private readonly string _baseUrl;

        protected BaseApiClient(/*LocalStorage localStorage, */HttpClient client)
        {
            //LocalStorage = localStorage;
            Client = client;
            //_baseUrl = baseUrl;
        }

        //protected string GetUrl(string url) => Path.Combine(_baseUrl, url);

        protected async Task<T> WrapRequest<T>(Func<Task<BaseResponse<T>>> func)
        {
            try
            {
                var response = await func();
                response.CheckSuccess();
                return response.Result;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                JsFunctions.Alert("Request error " + ex.Message);
                throw;
            }
        }
        protected async Task<T> WrapRequest<T>(Func<Task<T>> func)
        where T : BaseResponse
        {
            try
            {
                var response = await func();
                response.CheckSuccess();
                return response;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                JsFunctions.Alert("Request error " + ex.Message);
                throw;
            }
        }

        protected async Task<T> SendAsync<T>(HttpMethod method, string uri, object content = null)
        {
            var message = new HttpRequestMessage(method, uri);
            if (content != null)
                message.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");

            //var token = LocalStorage.GetItem("token");
            //if (token != null)
            //    message.Headers.Add("Authorization", "Bearer " + token);

            using (var response = await Client.SendAsync(message))
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    OnUnauthorizedRequest();

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json,
                    new JsonSerializerSettings { Converters = { new StringEnumConverter() } });
            }
        }

        protected virtual void OnUnauthorizedRequest() { }
    }
}
