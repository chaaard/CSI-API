using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.DTOs
{
    public class SalesAnalyticsParamsDto
    {
        public List<DateTime> dates { get; set; } = new List<DateTime>();
        public List<string> memCode { get; set; } = new List<string>();
    }
}
