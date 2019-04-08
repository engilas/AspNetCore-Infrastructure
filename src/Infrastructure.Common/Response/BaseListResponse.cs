using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Response
{
    public class BaseListResponse<T> : BaseResponse
    {
        public BaseListResponse()
        {
        }

        public BaseListResponse(IEnumerable<T> list)
        {
            Items = list?.ToArray() ?? new T[0];
        }

        public T[] Items { get; set; }
    }
}