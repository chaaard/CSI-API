using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.DTOs
{
    public class AnalyticsProoflistDto
    {
        public int? Id { get; set; }
        public int? AnalyticsId { get; set; }
        public int? ProoflistId { get; set; }
        public int? ActionId { get; set; }
        public int? StatusId { get; set; }
        public int? AdjustmentId { get; set; }
        public bool? DeleteFlag { get; set; }

        public AdjustmentAddDto? AdjustmentAddDto { get; set; }
    }
}
