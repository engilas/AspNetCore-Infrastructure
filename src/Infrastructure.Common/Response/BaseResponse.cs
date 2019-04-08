using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Infrastructure.Response
{
    public class BaseResponse
    {
        public string Message { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ResponseCode Status { get; set; } = ResponseCode.OK;
    }

    public class BaseResponse<T> : BaseResponse
    {
        public T Result { get; set; }
    }
}