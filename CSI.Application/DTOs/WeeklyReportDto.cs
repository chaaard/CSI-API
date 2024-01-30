using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.DTOs
{
    public class WeeklyReportDto
    {
        public string? LocationName { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? MembershipNo { get; set; } = string.Empty;
        public string? RegisterNo { get; set; } = string.Empty;
        public string? TransactionNo { get; set; } = string.Empty;
        public string? OrderNo { get; set; } = string.Empty;
        public int? Qty { get; set; }
        public decimal? Amount { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? Member { get; set; }
        public decimal? NonMember { get; set; }
        public decimal? OriginalAmout { get; set; }
        public string? AccountsPayment { get; set; }
        public string? APTRX { get; set; }
        public decimal? TotalBilled { get; set; }
    }
}
