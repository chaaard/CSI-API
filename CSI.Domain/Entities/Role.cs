using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Domain.Entities
{
    public class Roles
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool DeleteFlag { get; set; }
    }
}
