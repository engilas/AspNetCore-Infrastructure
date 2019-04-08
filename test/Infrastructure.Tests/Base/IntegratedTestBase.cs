using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using Infrastructure.Configuration;
using Infrastructure.Extensions;
using Infrastructure.Logging;
using Infrastructure.Startup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Infrastructure.Tests.Base
{
    public class IntegratedTestBase : TestBase, IDisposable
    {
        private readonly string _contentRoot;
        private TestServer _server;

        protected HttpClient Client;

        protected string ForcedContentType;

        public IntegratedTestBase(Assembly startupAssembly)
        {
            _contentRoot = Helpers.GetProjectPath(Path.Combine("src"), startupAssembly);
            Configuration = AppConfigurations.Get(_contentRoot, null, "UnitTest", substituteVariables: true);
            LogFactory.Initialize(Configuration);
        }


        public void Dispose()
        {
            Client.Dispose();
            _server.Dispose();
        }

        protected void InitServer(Action<IServiceCollection> configure = null)
        {
            Action<IServiceCollection> methodConfigure = ConfigureServices;
            var configureDelegate = (Action<IServiceCollection>) Delegate.Combine(methodConfigure, configure);

            var builder = new WebHostBuilder()
                .UseContentRoot(_contentRoot)
                .UseConfiguration(Configuration)
                .UseEnvironment("UnitTest")
                .UseStartup(typeof(DefaultStartup))
                .UseSerilog()
                .ConfigureTestServices(configureDelegate);

            _server = new TestServer(builder);

            Client = _server.CreateClient();
            Client.BaseAddress = new Uri("http://localhost");
            ServiceProvider = _server.Host.Services;

            InitService.InitServices(ServiceProvider);
        }

        protected async Task<HttpResponseMessage> SendAsync(HttpMethod method, string address, string input, string contentType, bool ignoreStatusCode = false)
        {
            if (!string.IsNullOrWhiteSpace(ForcedContentType))
                contentType = ForcedContentType;

            var message = new HttpRequestMessage(method, address);
            message.Content = new StringContent(input ?? string.Empty, Encoding.UTF8, contentType);

            var result = await Client.SendAsync(message);
            if (!ignoreStatusCode) result.EnsureSuccessStatusCode();
            return result;
        }

        public async Task<TOut> PostAsync<TOut>(string address, object input, bool ignoreStatusCode = false)
        {
            using (var result = await SendAsync(HttpMethod.Post, address, input?.ToJson(), "application/json", ignoreStatusCode))
            {
                if (!ignoreStatusCode) result.EnsureSuccessStatusCode();
                var content = await result.Content.ReadAsStringAsync();
                if (typeof(TOut) == typeof(string)) return content.CastTo<TOut>();
                return JsonConvert.DeserializeObject<TOut>(content);
            }
        }

        public async Task PostAsync(string address, object input, bool ignoreStatusCode = false)
        {
            await PostAsync<object>(address, input, ignoreStatusCode);
        }

        public async Task<TOut> PostQueryAsync<TOut>(string address, object input)
        {
            var query = input != null ? ObjectToQueryString(input) : null;
            if (!string.IsNullOrWhiteSpace(query)) query = "?" + query;

            using (var result = await SendAsync(HttpMethod.Post, address + query, null, "application/json"))
            {
                result.EnsureSuccessStatusCode();
                var json = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TOut>(json);
            }
        }

        private async Task<HttpResponseMessage> PostStringAsync(string address, string input, string contentType, bool ignoreStatusCode = false)
        {
            if (!string.IsNullOrWhiteSpace(ForcedContentType))
                contentType = ForcedContentType;

            var result = await SendAsync(HttpMethod.Post, address, input, contentType, ignoreStatusCode);
            if (!ignoreStatusCode) result.EnsureSuccessStatusCode();
            return result;
        }

        public Task<HttpResponseMessage> PostJsonAsync(string address, string input, bool ignoreStatusCode = false)
        {
            return PostStringAsync(address, input, "application/json", ignoreStatusCode);
        }

        public Task<HttpResponseMessage> PostXmlAsync(string address, string input, bool ignoreStatusCode = false)
        {
            return PostStringAsync(address, input, "application/xml", ignoreStatusCode);
        }

        public async Task<TOut> PostXmlAsync<TOut>(string address, string input, bool ignoreStatusCode = false)
        {
            using (var result = await PostXmlAsync(address, input, ignoreStatusCode))
            {
                var xml = await result.Content.ReadAsStringAsync();
                var serializer = new XmlSerializer(typeof(TOut));
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                var reader = new StringReader(xml);
                var resultObj = (TOut) serializer.Deserialize(reader);
                return resultObj;
            }
        }

        public async Task<TOut> PostJsonAsync<TOut>(string address, string input, bool ignoreStatusCode = false)
        {
            using (var result = await PostJsonAsync(address, input, ignoreStatusCode))
            {
                var str = await result.Content.ReadAsStringAsync();
                return str.FromJson<TOut>();
            }
        }

        public async Task<TOut> SendJsonAsync<TOut>(HttpMethod method, string address, string input, bool ignoreStatusCode = false)
        {
            using (var result = await SendJsonAsync(method, address, input, ignoreStatusCode))
            {
                var str = await result.Content.ReadAsStringAsync();
                return str.FromJson<TOut>();
            }
        }

        public async Task<HttpResponseMessage> SendJsonAsync(HttpMethod method, string address, string input,
            bool ignoreStatusCode = false)
        {
            var result = await SendAsync(method, address, input, "application/json", ignoreStatusCode);
            if (!ignoreStatusCode) result.EnsureSuccessStatusCode();
            return result;
        }

        private string ObjectToQueryString(object item)
        {
            var jObj = JObject.FromObject(item, JsonSerializer.Create(new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new StringEnumConverter()}
            }));
            var dict = jObj.ToObject<Dictionary<string, string>>();

            return ToQueryString(dict);
        }

        private string ToQueryString(IDictionary<string, string> dic)
        {
            var array = (from key in dic.Keys
                    where !string.IsNullOrWhiteSpace(dic[key])
                    select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(dic[key])))
                .ToArray();

            return string.Join("&", array);
        }
    }
}