using CSI.Application.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CSI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class CategoryController : ControllerBase
    {
        public readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("GetCategoryAsync")]
        public async Task<IActionResult> GetCategoryAsync()
        {
            var result = await _categoryService.GetCategoryAsync();

            if (result != null)
            {
                return (Ok(result));
            }
            return (NotFound());
        }
    }
}
