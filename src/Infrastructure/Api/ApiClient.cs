using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Exceptions;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Api
{
    public abstract class ApiClient
    {
        protected readonly HttpClient HttpClient;
        protected readonly ILogger Logger;

        protected ApiClient(ILogger logger, HttpClient httpClient)
        {
            Logger = logger;
            HttpClient = httpClient;
        }

        protected async Task<string> SendAsync(HttpRequestMessage request)
        {
            string body;
            try
            {
                using (var response = await HttpClient.SendAsync(request))
                {
                    //https://stackoverflow.com/questions/39065988/utf8-is-not-a-supported-encoding-name
                    try
                    {
                        body = await response.Content.ReadAsStringAsync();
                    }
                    catch (InvalidOperationException)
                    {
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        body = Encoding.UTF8.GetString(bytes);
                    }

                    if (!response.IsSuccessStatusCode)
                        throw new ApiException(response.StatusCode, response.ReasonPhrase, body);
                }
            }
            catch (HttpRequestException ex)
            {
                var msg =
                    $"Exception occurred while calling HTTP request: Method {request.Method}, URL {request.RequestUri}";
                if (request.Content != null) msg += $", content: {request.Content.ReadAsStringAsync()}";
                throw new ApiException(msg, ex);
            }

            return body;
        }

        protected Task<string> GetStringAsync(string url, Dictionary<string, string> headers = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            PopulateRequestWithHeaders(request, headers);

            return SendAsync(request);
        }

        protected async Task<T> GetAsync<T>(string url, Dictionary<string, string> headers = null)
        {
            var result = await GetStringAsync(url, headers);
            return result.FromJson<T>();
        }

        protected Task<TOut> PostAsync<TOut>(string url, object input, Dictionary<string, string> headers = null)
        {
            return CallAsync<TOut>(url, input, HttpMethod.Post, headers);
        }

        protected async Task<TOut> CallAsync<TOut>(string url, object input, HttpMethod method,
            Dictionary<string, string> headers = null)
        {
            var request = new HttpRequestMessage(method, url);

            if (input != null)
            {
                if (input is string s)
                    request.Content = new StringContent(s, Encoding.UTF8, "application/json");
                else request.Content = new ObjectContent(input.GetType(), input, new JsonMediaTypeFormatter());
            }

            PopulateRequestWithHeaders(request, headers);

            var result = await SendAsync(request);
            return result.FromJson<TOut>();
        }

        private void PopulateRequestWithHeaders(HttpRequestMessage requestMessage, Dictionary<string, string> headers)
        {
            if (headers?.Any() == true)
                foreach (var header in headers)
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }
}