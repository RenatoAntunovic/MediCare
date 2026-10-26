using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Medicine.Medicine.Queries.GetById
{
    public class GetMedicineByIdQueryDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public required string Description { get; set; }
        public required int MedicineCategoryId { get; set; }
        public required string MedicineCategoryName { get; set; } //only the category name is used here, because putting the whole entity in the DTO throws an error and is not recommended
        public required string ImagePath { get; set; }
        public required int Weight { get; set; }
        public required bool isEnabled { get; set; }

        public List<string> Doses { get; set; } = new();
        public List<string> Packages { get; set; } = new();
    }
}
