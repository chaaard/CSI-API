using CSI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.Interfaces
{
    public interface ISalesAnalyticsService
    {
        Task<List<SalesAnalyticsDto>> SalesAnalytics(SalesAnalyticsParamsDto salesParam);
    }
}
