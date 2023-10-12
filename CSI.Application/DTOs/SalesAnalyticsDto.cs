using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.DTOs
{
    public class SalesAnalyticsDto
    {
        public string CSSTOR { get; set; } = string.Empty;
        public string CSDATE { get; set; } = string.Empty;
        public string CSCUST { get; set; } = string.Empty;
        public int CSREG { get; set; }
        public int CSTRAN { get; set; }
        public string CSCARD { get; set; } = string.Empty;
        public decimal CSQTY { get; set; }
        public decimal CSEXPR { get; set; }
        public decimal CSTAMT { get; set; }
    }
}
