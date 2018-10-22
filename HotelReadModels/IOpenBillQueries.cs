using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReadModels
{
    public interface IOpenBillQueries
    {
        List<int> ActiveTableNumbers();
        OpenBills.BillInvoice InvoiceForTable(int table);
        Guid BillIdForTable(int table);
        OpenBills.BillStatus BillForTable(int table);
        Dictionary<int, List<OpenBills.BillItem>> TodoListForWaiter(string waiter);
    }
}
