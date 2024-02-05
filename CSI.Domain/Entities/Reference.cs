using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Domain.Entities
{
    public class Reference
    {
        public int Id { get; set; }
        public string CustomerNo { get; set; } = string.Empty;
        public string MerchReference { get; set; } = string.Empty;
        public bool DeleteFlag { get; set; }
    }
}
