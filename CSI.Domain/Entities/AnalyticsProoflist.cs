using CSI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Domain.Entities
{
    public class AnalyticsProoflist
    {
        public int Id { get; set; }
        public int AnalyticsId  { get; set; }
        public int ProoflistId  { get; set; }
        public int ActionId  { get; set; }
        public int StatusId { get; set; } 
        public int AdjustmentId { get; set; }
        public bool DeleteFlag { get; set; }
    }
}
