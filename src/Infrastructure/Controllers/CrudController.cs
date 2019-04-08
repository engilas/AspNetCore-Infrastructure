using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Repository;
using Infrastructure.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Controllers
{
    public abstract class CrudController : BaseController
    {
        protected readonly IDbConnectionFactory DbConnectionFactory;
        protected readonly ISqlParamCombiner ParamCombiner;

        protected CrudController(ILogger logger, IDbConnectionFactory dbConnectionFactory, ISqlParamCombiner paramCombiner) : base(logger)
        {
            DbConnectionFactory = dbConnectionFactory;
            ParamCombiner = paramCombiner;
        }
    }

    public abstract class CrudController<TDto, TShortDto> : CrudController
    {
        protected CrudController(ILogger logger, IDbConnectionFactory dbConnectionFactory, ISqlParamCombiner paramCombiner) : base(logger, dbConnectionFactory, paramCombiner)
        {
        }

        protected abstract string TableName { get; }
        protected abstract Task<IEnumerable<TShortDto>> GetAll();
        protected abstract Task<TDto> GetById(int id);
        protected abstract Task<int> CreateAndGetId(TDto dto);
        protected abstract Task<int> UpdateAndGetUpdatedRows(int id, TDto dto);
        protected abstract Task<int> UpdateGroupAndGetUpdatedRows(TDto[] items);
        protected abstract Task<int> DeleteAndGetUpdatedRows(int id);

        [HttpGet]
        public async Task<ActionResult<BaseResponse<TShortDto[]>>> GetList()
        {
            return WlOkResponse(await GetAll());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponse<TDto>>> Get(int id)
        {
            var item = await GetById(id);
            if (item != null)
                return WlOkResponse(item);
            return WlNotFound();
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponse>> Create([FromBody] TDto dto)
        {
            var id = await CreateAndGetId(dto);
            return WlOkResponse(new {id});
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponse>> Update(int id, [FromBody] TDto dto)
        {
            var rows = await UpdateAndGetUpdatedRows(id, dto);
            SqlHelper.CheckUpdateAffected(rows, TableName, id);
            return WlOkResponse();
        }

        [HttpPut]
        public async Task<ActionResult<BaseResponse>> UpdateGroup([FromBody] TDto[] items)
        {
            await UpdateGroupAndGetUpdatedRows(items);
            return WlOkResponse();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var rows = await DeleteAndGetUpdatedRows(id);
            SqlHelper.CheckUpdateAffected(rows, TableName, id);
            return WlOkResponse();
        }
    }

    public abstract class CrudGroupController<TDto, TShortDto> : CrudController
    {
        protected CrudGroupController(ILogger logger, IDbConnectionFactory dbConnectionFactory, ISqlParamCombiner paramCombiner) : base(logger, dbConnectionFactory, paramCombiner)
        {
        }

        protected abstract Task<TShortDto[]> GetAllByParentId();
        protected abstract Task CreateGroup(TDto[] items);
        protected abstract Task UpdateGroup(TDto[] items);
        protected abstract Task DeleteGroup(int[] ids);

        [HttpGet]
        public async Task<ActionResult<BaseResponse<TShortDto[]>>> GetAllByParentIdMethod()
        {
            return WlOkResponse(await GetAllByParentId());
        }

        [HttpPost]
        public async Task<ActionResult> CreateGroupMethod([FromBody] TDto[] items)
        {
            await CreateGroup(items);
            return WlOkResponse();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateGroupMethod([FromBody] TDto[] items)
        {
            await UpdateGroup(items);
            return WlOkResponse();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteGroupMethod([FromBody] int[] ids)
        {
            await DeleteGroup(ids);
            return WlOkResponse();
        }

    }
}
