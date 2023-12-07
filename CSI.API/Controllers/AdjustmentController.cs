using CSI.Application.DTOs;
using CSI.Application.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CSI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowOrigin")]
    public class AdjustmentController : ControllerBase
    {
        public readonly IAdjustmentService _adjustmentService;

        public AdjustmentController(IAdjustmentService adjustmentService)
        {
            _adjustmentService = adjustmentService;
        }

        [HttpPost("GetAdjustmentsAsync")]
        public async Task<IActionResult> GetAdjustmentsAsync(AdjustmentParams adjustmentParams)
        {
            var result = await _adjustmentService.GetAdjustmentsAsync(adjustmentParams);

            if (result.Item1 != null)
            {
                var data = new
                {
                    ExceptionList = result.Item1,
                    TotalPages = result.Item2
                };

                return (Ok(data));                                                                                                                                               
            }
            return (NotFound());
        }

        [HttpPost("CreateAnalyticsProofList")]
        public async Task<IActionResult> CreateAnalyticsProofList(AnalyticsProoflistDto adjustmentTypeDto)
        {
            var result = await _adjustmentService.CreateAnalyticsProofList(adjustmentTypeDto);

            if (result != null)
            {
                return (Ok(result)); 
            }
            return (NotFound());
        }

        [HttpGet("GetReasonsAsync")]
        public async Task<IActionResult> GetReasonsAsync()
        {
            var result = await _adjustmentService.GetReasonsAsync();

            if (result != null)
            {
                return (Ok(result));
            }
            return (NotFound());
        }

        [HttpPut("UpdateJO")]
        public async Task<IActionResult> UpdateJO(AnalyticsProoflistDto adjustmentTypeDto)
        {
            var result = await _adjustmentService.UpdateJO(adjustmentTypeDto);

            if (result != null)
            {
                return (Ok(result));
            }
            return (NotFound());
        }

        [HttpPut("UpdatePartner")]
        public async Task<IActionResult> UpdatePartner(AnalyticsProoflistDto adjustmentTypeDto)
        {
            var result = await _adjustmentService.UpdatePartner(adjustmentTypeDto);

            if (result != null)
            {
                return (Ok(result));
            }
            return (NotFound());
        }
    }
}
