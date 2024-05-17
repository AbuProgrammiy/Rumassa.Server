using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rumassa.Domain.Entities.DTOs;
using Rumassa.Domain.Entities;
using Rumassa.Application.UseCases.CategoryCases.Commands;
using Rumassa.Application.UseCases.CategoryCases.Queries;
using Microsoft.Extensions.Caching.Memory;

namespace Rumassa.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMemoryCache _memoryCache;
        
        public CategoryController(IMediator mediator, IMemoryCache memoryCache)
        {
            _mediator = mediator;
            _memoryCache = memoryCache;
        }

        [HttpPost]
        public async Task<ResponseModel> Create(CreateCategoryCommand request)
        {
            var result = await _mediator.Send(request);

            return result;
        }

        [HttpGet]
        public async Task<IEnumerable<Category>> GetAll()
        {
            var value = _memoryCache.Get("categories");

            if (value == null)
            {
                var categories = await _mediator.Send(new GetAllCategoriesQuery());

                _memoryCache.Set("categories", categories, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(4),
                    SlidingExpiration = TimeSpan.FromSeconds(5)
                });
            }

            return _memoryCache.Get("categories") as IEnumerable<Category>;
        }

        [HttpGet]
        public async Task<Category> GetById(short id)
        {
            var result = await _mediator.Send(new GetCategoryByIdQuery()
            {
                Id = id
            });

            return result;
        }

        [HttpPut]
        public async Task<ResponseModel> Update(UpdateCategoryCommand request)
        {
            var result = await _mediator.Send(request);

            return result;
        }

        [HttpDelete]
        public async Task<ResponseModel> Delete(DeleteCategoryCommand request)
        {
            var result = await _mediator.Send(request);

            return result;
        }

    }
}
