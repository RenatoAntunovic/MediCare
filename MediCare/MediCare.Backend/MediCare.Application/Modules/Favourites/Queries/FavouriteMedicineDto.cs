using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;

namespace MediCare.Application.Modules.Favourites.Queries
{
    public class FavouriteMedicineDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        
    }
}
