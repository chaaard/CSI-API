using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using CSI.Application.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CSI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class AnalyticsController : ControllerBase
    {
        public readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpPost("Analytics")]
        public async Task<IActionResult> Analytics(AnalyticsParamsDto salesAnalyticsParamsDto)
        {
            try
            {
                var result = await _analyticsService.SalesAnalytics(salesAnalyticsParamsDto);

                if (result != null)
                {
                    return Ok(result);
                }

                return NotFound();
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if needed
                return StatusCode(499, "Request canceled"); // 499 Client Closed Request is a common status code for cancellation
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("GetAnalytics")]
        public async Task<IActionResult> GetAnalytics(AnalyticsParamsDto salesAnalyticsParamsDto)
        {
            try
            {
                var result = await _analyticsService.GetAnalytics(salesAnalyticsParamsDto);

                if (result != null)
                {
                    return Ok(result);
                }

                return NotFound();
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if needed
                return StatusCode(499, "Request canceled"); // 499 Client Closed Request is a common status code for cancellation
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("GetAnalyticsProofListVariance")]
        public async Task<IActionResult> GetAnalyticsProofListVariance(AnalyticsParamsDto salesAnalyticsParamsDto)
        {
            try
            {
                var result = await _analyticsService.GetAnalyticsProofListVariance(salesAnalyticsParamsDto);

                if (result != null)
                {
                    return Ok(result);
                }

                return NotFound();
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if needed
                return StatusCode(499, "Request canceled"); // 499 Client Closed Request is a common status code for cancellation
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
