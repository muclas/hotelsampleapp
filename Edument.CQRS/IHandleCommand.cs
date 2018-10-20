using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edument.CQRS
{
    public interface IHandleCommand<TCommand>
    {
        IEnumerable Handle(TCommand c);
    }
}
