using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Restaurant
{
    public class BillClosed
    {
        public Guid Id;
        public decimal AmountPaid;
        public decimal OrderValue;
        public decimal TipValue;
    }
}
