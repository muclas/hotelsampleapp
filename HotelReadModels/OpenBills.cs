using Edument.CQRS;
using Events.Restaurant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReadModels
{
    public class OpenBills : IOpenBillQueries,
        ISubscribeTo<BillOpened>,
        ISubscribeTo<DrinksOrdered>,
        ISubscribeTo<FoodOrdered>,
        ISubscribeTo<FoodPrepared>,
        ISubscribeTo<DrinksServed>,
        ISubscribeTo<FoodServed>,
        ISubscribeTo<BillClosed>
    {
        public class BillItem
        {
            public int MenuNumber;
            public string Description;
            public decimal Price;
        }

        public class BillStatus
        {
            public Guid BillId;
            public int TableNumber;
            public List<BillItem> ToServe;
            public List<BillItem> InPreparation;
            public List<BillItem> Served;
        }

        public class BillInvoice
        {
            public Guid BillId;
            public int TableNumber;
            public List<BillItem> Items;
            public decimal Total;
            public bool HasUnservedItems;
        }

        private class Bill
        {
            public int TableNumber;
            public string Waiter;
            public List<BillItem> ToServe;
            public List<BillItem> InPreparation;
            public List<BillItem> Served;
        }

        private Dictionary<Guid, Bill> todoByBill = new Dictionary<Guid, Bill>();

        public List<int> ActiveTableNumbers()
        {
            lock (todoByBill)
            {
                return (from bill in todoByBill
                        select bill.Value.TableNumber
                       ).OrderBy(i => i).ToList();
            }
        }

        public Dictionary<int, List<BillItem>> TodoListForWaiter(string waiter)
        {
            lock (todoByBill)
            {
                return (from tab in todoByBill
                        where tab.Value.Waiter == waiter
                        select new
                        {
                            TableNumber = tab.Value.TableNumber,
                            ToServe = CopyItems(tab.Value, t => t.ToServe)
                        })
                        .Where(t => t.ToServe.Count > 0)
                        .ToDictionary(k => k.TableNumber, v => v.ToServe);
            }
        }

        public Guid BillIdForTable(int table)
        {
            lock (todoByBill)
            {
                return (from tab in todoByBill
                        where tab.Value.TableNumber == table
                        select tab.Key
                       ).First();
            }
        }

        public BillStatus BillForTable(int table)
        {
            lock (todoByBill)
            {
                return (from bill in todoByBill
                        where bill.Value.TableNumber == table
                        select new BillStatus
                        {
                            BillId = bill.Key,
                            TableNumber = bill.Value.TableNumber,
                            ToServe = CopyItems(bill.Value, t => t.ToServe),
                            InPreparation = CopyItems(bill.Value, t => t.InPreparation),
                            Served = CopyItems(bill.Value, t => t.Served)
                        })
                        .First();
            }
        }

        public BillInvoice InvoiceForTable(int table)
        {
            KeyValuePair<Guid, Bill> bill;
            lock (todoByBill)
            {
                bill = todoByBill.First(t => t.Value.TableNumber == table);
            }

            lock (bill.Value)
            {
                return new BillInvoice
                {
                    BillId = bill.Key,
                    TableNumber = bill.Value.TableNumber,
                    Items = new List<BillItem>(bill.Value.Served),
                    Total = bill.Value.Served.Sum(i => i.Price),
                    HasUnservedItems = bill.Value.InPreparation.Any() || bill.Value.ToServe.Any()
                };
            }
        }

        public void Handle(BillOpened e)
        {
            lock (todoByBill)
                todoByBill.Add(e.Id, new Bill
                {
                    TableNumber = e.TableNumber,
                    Waiter = e.Waiter,
                    ToServe = new List<BillItem>(),
                    InPreparation = new List<BillItem>(),
                    Served = new List<BillItem>()
                });
        }

        public void Handle(BillClosed e)
        {
            lock (todoByBill)
                todoByBill.Remove(e.Id);
        }

        public void Handle(DrinksOrdered e)
        {
            AddItems(e.Id,
                e.Items.Select(drink => new BillItem
                {
                    MenuNumber = drink.MenuNumber,
                    Description = drink.Description,
                    Price = drink.Price
                }),
                t => t.ToServe);
        }

        public void Handle(FoodOrdered e)
        {
            AddItems(e.Id,
                e.Items.Select(drink => new BillItem
                {
                    MenuNumber = drink.MenuNumber,
                    Description = drink.Description,
                    Price = drink.Price
                }),
                t => t.InPreparation);
        }

        public void Handle(FoodPrepared e)
        {
            MoveItems(e.Id, e.MenuNumbers, t => t.InPreparation, t => t.ToServe);
        }

        public void Handle(DrinksServed e)
        {
            MoveItems(e.Id, e.MenuNumbers, t => t.ToServe, t => t.Served);
        }

        public void Handle(FoodServed e)
        {
            MoveItems(e.Id, e.MenuNumbers, t => t.ToServe, t => t.Served);
        }

        private List<BillItem> CopyItems(Bill tableTodo, Func<Bill, List<BillItem>> selector)
        {
            lock (tableTodo)
            {
                return new List<BillItem>(selector(tableTodo));
            }
        }

        private Bill getBill(Guid id)
        {
            lock (todoByBill)
            {
                return todoByBill[id];
            }
        }

        private void AddItems(Guid billId, IEnumerable<BillItem> newItems, Func<Bill, List<BillItem>> to)
        {
            var bill = getBill(billId);
            lock (bill)
            {
                to(bill).AddRange(newItems);
            }
        }

        private void MoveItems(Guid tabId, List<int> menuNumbers, Func<Bill, List<BillItem>> from, Func<Bill, List<BillItem>> to)
        {
            var bill = getBill(tabId);
            lock (bill)
            {
                var fromList = from(bill);
                var toList = to(bill);
                foreach (var num in menuNumbers)
                {
                    var serveItem = fromList.First(f => f.MenuNumber == num);
                    fromList.Remove(serveItem);
                    toList.Add(serveItem);
                }
            }
        }
    }
}
