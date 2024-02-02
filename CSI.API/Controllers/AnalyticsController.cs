﻿using CSI.Application.DTOs;
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

        [HttpPost("GetAnalytics")]
        public async Task<IActionResult> GetAnalytics(AnalyticsParamsDto analyticsParamsDto)
        {
            try
            {
                var result = await _analyticsService.GetAnalytics(analyticsParamsDto);

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
        public async Task<IActionResult> GetAnalyticsProofListVariance(AnalyticsParamsDto analyticsParamsDto)
        {
            try
            {
                var result = await _analyticsService.GetAnalyticsProofListVariance(analyticsParamsDto);

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

        [HttpPost("GetTotalAmountPerMechant")]
        public async Task<IActionResult> GetTotalAmountPerMechant(AnalyticsParamsDto analyticsParamsDto)
        {
            try
            {
                var result = await _analyticsService.GetTotalAmountPerMechant(analyticsParamsDto);

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

        [HttpPost("RefreshAnalytics")]
        public async Task RefreshAnalytics(RefreshAnalyticsDto refreshAnalyticsDto)
        {
            await _analyticsService.RefreshAnalytics(refreshAnalyticsDto);
        }

        [HttpPost("SubmitAnalytics")]
        public async Task<IActionResult> SubmitAnalytics(AnalyticsParamsDto analyticsParamsDto)
        {
            var result = await _analyticsService.SubmitAnalytics(analyticsParamsDto);

            if (result != null)
            {
                return (Ok(result));
            }
            return (NotFound());
        }

        [HttpPost("GenerateInvoiceAnalytics")]
        public async Task<IActionResult> GenerateInvoiceAnalytics(AnalyticsParamsDto analyticsParamsDto)
        {
            var result = await _analyticsService.GenerateInvoiceAnalytics(analyticsParamsDto);

            if (result.Item1 != null)
            {
                var data = new
                {
                    InvoiceList = result.Item1,
                    IsPending = result.Item2
                };

                return (Ok(data));
            }
            return (NotFound());
        }

        [HttpPost("IsSubmitted")]
        public async Task<IActionResult> IsSubmitted(AnalyticsParamsDto analyticsParamsDto)
        {
            var result = await _analyticsService.IsSubmitted(analyticsParamsDto);
            return Ok(result); 
        }

        [HttpPost("UpdateUploadStatus")]
        public async Task<IActionResult> UpdateUploadStatus(AnalyticsParamsDto analyticsParamsDto)
        {
            await _analyticsService.UpdateUploadStatus(analyticsParamsDto);
            return Ok();
        }

        [HttpPost("GenerateWeeklyReport")]
        public async Task<IActionResult> GenerateWeeklyReport(AnalyticsParamsDto analyticsParamsDto)
        {
            var result = await _analyticsService.GenerateWeeklyReport(analyticsParamsDto);

            if (result.Item1 != null)
            {
                var data = new
                {
                    WeeklyReport = result.Item1,
                    RecapSummary = result.Item2
                };

                return (Ok(data));
            }

            return Ok(result);
        }

        [HttpPost("GetLocations")]
        public async Task<IActionResult> GetLocations()
        {
            var result = await _analyticsService.GetLocations();

            var formatted = result
                .Where(x => x.LocationName.ToLower().Contains("kareila"))
                .Select(x =>
                {
                    // Replace "kareila" with a blank and trim the string
                    x.LocationName = x.LocationName.Replace("KAREILA - ", "").Trim();
                    return x;
                })
                .ToList();


            if (formatted.Any())
            {
                return Ok(formatted);
            }

            return (NotFound());
        }

        [HttpPost("GetClubs")]
        public async Task<IActionResult> GetClubs()
        {
            var result = await _analyticsService.GetClubs();

            if (result != null)
            {
                return (Ok(result));
            }

            return Ok(result);
        }

        [HttpPost("GetGeneratedInvoice")]
        public async Task<IActionResult> GetGeneratedInvoice(AnalyticsParamsDto analyticsParamsDto)
        {
            var result = await _analyticsService.GetGeneratedInvoice(analyticsParamsDto);

            if (result != null)
            {
                return (Ok(result));
            }

            return Ok(result);
        }
    }
}
