using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Favourites.Command.Create
{
    public class AddToFavouritesCommand : IRequest<int>
    {
        public int MedicineId { get; set; }
    }
}
