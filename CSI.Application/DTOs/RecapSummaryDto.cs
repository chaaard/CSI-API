using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.DTOs
{
    public class RecapSummaryDto
    {
        public string? DAYOFWEEK { get; set; } = string.Empty;
        public string? DATE { get; set; }
        public decimal? SAAMOUNT { get; set; }
        public int? NOOFTRX { get; set; }
        public decimal? PERIINVOICEENTRY { get; set; }
        public decimal? VARIANCE { get; set; }
        public string? REMARKS { get; set; } = string.Empty;
    }
}
