﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.DTOs
{
    public class MatchDto
    {
        public int? AnalyticsId { get; set; }
        public string? AnalyticsPartner { get; set; } = string.Empty;
        public string? AnalyticsLocation { get; set; } = string.Empty;
        public DateTime? AnalyticsTransactionDate { get; set; }
        public string? AnalyticsOrderNo { get; set; } = string.Empty;
        public decimal? AnalyticsAmount { get; set; }
        public int? ProofListId { get; set; }
        public DateTime? ProofListTransactionDate { get; set; }
        public string? ProofListOrderNo { get; set; } = string.Empty;
        public decimal? ProofListAmount { get; set; }
        public decimal? Variance { get; set; }
        public bool? IsUpload { get; set; }
    }
}
