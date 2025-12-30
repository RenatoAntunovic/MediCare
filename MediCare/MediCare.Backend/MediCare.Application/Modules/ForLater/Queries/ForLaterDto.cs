using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.ForLater.Queries
{
    public class ForLaterDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
    }
}
