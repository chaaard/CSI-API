using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSI.Application.DTOs
{
    public class TransactionCountAmountDto
    {
        public List<string> dates { get; set; } = new List<string>();
        public List<string> memCode { get; set; } = new List<string>();
        public List<int> storeId { get; set; } = new List<int>();
        public int actionId { get; set; }
        public int statusId { get; set; }
    }
}
