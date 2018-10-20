using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events.Restaurant
{
    public class DrinksOrdered
    {
        public Guid Id;
        public List<OrderedItem> Items;
    }
}
