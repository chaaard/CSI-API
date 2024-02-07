using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.DTOs
{
    public class UpdateAnalyticsDto
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
    }
}
