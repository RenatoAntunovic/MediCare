using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.ForLater.Command.Create
{
    public class AddToForLaterCommand : IRequest<int>
    {
        public int MedicineId { get; set; }
    }
}
