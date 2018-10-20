using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Bill
{
    public class BillNotOpen : Exception
    {
    }

    public class DrinksNotOutstanding : Exception
    {
    }

    public class FoodNotOutstanding : Exception
    {
    }

    public class FoodNotPrepared : Exception
    {
    }

    public class TabHasUnservedItems : Exception
    {
    }

    public class MustPayEnough : Exception
    {
    }
}
