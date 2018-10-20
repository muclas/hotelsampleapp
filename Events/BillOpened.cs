using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Restaurant
{
    public class BillOpened
    {
        public Guid Id;
        public int TableNumber;
        public string Waiter;
    }
}
