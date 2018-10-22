using Edument.CQRS;
using Events.Restaurant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReadModels
{
    public class ChefTodoList : IChefTodoListQueries,
        ISubscribeTo<FoodOrdered>,
        ISubscribeTo<FoodPrepared>
    {
        private List<TodoListGroup> todoList = new List<TodoListGroup>();

        public class TodoListItem
        {
            public int MenuNumber;
            public string Description;
        }

        public class TodoListGroup
        {
            public Guid Bill;
            public List<TodoListItem> Items;
        }

        public void Handle(FoodOrdered e)
        {
            var group = new TodoListGroup
            {
                Bill = e.Id,
                Items = new List<TodoListItem>(
                    e.Items.Select(i => new TodoListItem
                    {
                        MenuNumber = i.MenuNumber,
                        Description = i.Description,
                    }))
            };

            lock(todoList)
            {
                todoList.Add(group);
            }
        }

        public void Handle(FoodPrepared e)
        {
            lock(todoList)
            {
                var group = todoList.First(g => g.Bill == e.Id);

                foreach (var num in e.MenuNumbers)
                {
                    group.Items.Remove(group.Items.First(i => i.MenuNumber == num));
                }

                if (group.Items.Count == 0)
                {
                    todoList.Remove(group);
                }
            }
        }

        public List<TodoListGroup> GetTodoList()
        {
            lock (todoList)
            {
                return (from grp in todoList
                        select new TodoListGroup
                        {
                            Bill = grp.Bill,
                            Items = new List<TodoListItem>(grp.Items)
                        }).ToList();
            }
        }
    }
}
