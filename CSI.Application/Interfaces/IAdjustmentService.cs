﻿using CSI.Application.DTOs;
using CSI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.Interfaces
{
    public interface IAdjustmentService
    {
        Task<(List<AdjustmentDto>, int totalPages)> GetAdjustmentsAsync(AdjustmentParams adjustmentParams);
        Task<AnalyticsProoflist> CreateAnalyticsProofList(AnalyticsProoflistDto adjustmentTypeDto);
        Task<bool> UpdateAnalyticsProofList(AnalyticsProoflistDto adjustmentTypeDto);
        Task<bool> UpdateJO(AnalyticsProoflistDto adjustmentTypeDto);
        Task<bool> UpdatePartner(AnalyticsProoflistDto adjustmentTypeDto);
        Task<List<Reasons>> GetReasonsAsync();
        Task<TransactionDtos> GetTotalCountAmount(TransactionCountAmountDto transactionCountAmountDto);
        Task<List<AdjustmentDto>> ExportExceptions(AdjustmentParams adjustmentParams);
    }
}
