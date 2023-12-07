using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Domain.Entities
{
    public class Actions
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ActionDescription { get; set; } = string.Empty;
        public bool DeleteFlag { get; set; }
    }
}
