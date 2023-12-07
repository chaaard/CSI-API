using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.DTOs
{
    public class AdjustmentDto
    {
        public int Id { get; set; }
        public string? CustomerId { get; set; } = string.Empty;
        public string? JoNumber { get; set; } = string.Empty;
        public DateTime? TransactionDate { get; set; }
        public decimal? Amount { get; set; }
        public string? AdjustmentType { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
    }
}
