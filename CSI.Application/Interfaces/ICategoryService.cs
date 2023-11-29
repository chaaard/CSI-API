using CSI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.Interfaces
{
        public interface ICategoryService
        {
                Task<List<AnalyticsDto>> GetAnalytics(AnalyticsParamsDto analyticsParamsDto);
                Task<List<MatchDto>> GetAnalyticsProofListVariance(AnalyticsParamsDto analyticsParamsDto);
        }
}
