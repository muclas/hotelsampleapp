using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReadModels
{
    public interface IChefTodoListQueries
    {
        List<ChefTodoList.TodoListGroup> GetTodoList();
    }
}
