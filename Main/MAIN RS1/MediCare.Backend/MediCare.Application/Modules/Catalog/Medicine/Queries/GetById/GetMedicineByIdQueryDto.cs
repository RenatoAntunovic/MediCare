using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Catalog.Medicine.Queries.GetById
{
    public class GetMedicineByIdQueryDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public required string Description { get; set; }
        public required int MedicineCategoryId { get; set; }
        public required string MedicineCategoryName { get; set; } //ovdje sam stavio samo ime kategorije zato sto baca error jer nije preporucljiuvo staviti cijeli entitet u dto
        public required string ImagePath { get; set; }
        public required int Weight { get; set; }
        public required bool isEnabled { get; set; }
    }
}
