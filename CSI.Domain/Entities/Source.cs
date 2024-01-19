using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Domain.Entities
{
    public class Source
    {
        public int Id { get; set; }
        public string SourceType { get; set; } = string.Empty;
        public bool DeleteFlag { get; set; }
    }
}
