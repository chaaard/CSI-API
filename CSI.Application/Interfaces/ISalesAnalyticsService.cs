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
<<<<<<< Updated upstream:CSI.Application/Interfaces/ISalesAnalyticsService.cs
        Task<List<SalesAnalyticsDto>> SalesAnalytics(SalesAnalyticsParamsDto salesParam);
=======
        Task<List<Analytics>> SalesAnalytics(AnalyticsParamsDto salesParam);
        Task<List<AnalyticsDto>> GetAnalytics(AnalyticsParamsDto analyticsParamsDto);
        Task<List<MatchDto>> GetAnalyticsProofListVariance(AnalyticsParamsDto analyticsParamsDto);
>>>>>>> Stashed changes:CSI.Application/Interfaces/IAnalyticsService.cs
    }
}
