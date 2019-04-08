using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Controllers;
using Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SampleWebApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ValuesController : BaseController
    {
        public ValuesController(ILogger<ValuesController> logger) : base(logger)
        {
        }

        // GET api/values
        [HttpGet]
        public ActionResult<BaseResponse<IEnumerable<string>>> Get()
        {
            Logger.LogInformation("Get call");
            return WlOkResponse(new [] { "value1", "value2" });
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<BaseResponse<string>> Get(int id)
        {
            Logger.LogInformation("Get by id call");
            return WlOkResponse("value");
        }

        // POST api/values
        [HttpPost]
        public ActionResult<BaseResponse> Post([FromBody] string value)
        {
            Logger.LogInformation("Post call");
            return WlOkResponse();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public ActionResult<BaseResponse> Put(int id, [FromBody] string value)
        {
            Logger.LogInformation("Put call");
            return WlOkResponse();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public ActionResult<BaseResponse> Delete(int id)
        {
            Logger.LogInformation("Delete call");
            return WlOkResponse();
        }
    }
}
