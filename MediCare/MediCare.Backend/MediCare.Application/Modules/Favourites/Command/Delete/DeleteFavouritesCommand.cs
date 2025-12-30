using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Favourites.Command.Delete
{
    public class DeleteFavouritesCommand:IRequest<Unit>
    {
        public required int Id { get; set; }
    }
}
