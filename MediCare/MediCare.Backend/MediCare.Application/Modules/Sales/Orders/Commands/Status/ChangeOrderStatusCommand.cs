using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Sales.Orders.Commands.Status
{
    public class ChangeOrderStatusCommand:IRequest
    {
        public int Id { get; set; }
        public int NewStatusId { get; set; }   
    }
}
