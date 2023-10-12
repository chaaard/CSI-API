using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using CSI.Application.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace CSI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class SalesAnalyticsController : ControllerBase
    {
        public readonly ISalesAnalyticsService _salesAnalyticsService;

        public SalesAnalyticsController(ISalesAnalyticsService salesAnalyticsService)
        {
            _salesAnalyticsService = salesAnalyticsService;
        }

        [HttpPost("SalesAnalytics")]
        public async  Task<IActionResult> SalesAnalytics(SalesAnalyticsParamsDto salesAnalyticsParamsDto)
        {
            var result =  await _salesAnalyticsService.SalesAnalytics(salesAnalyticsParamsDto);

            if (result != null)
            {
                return (Ok(result));
            }
            return (NotFound());
        }
    }
}
