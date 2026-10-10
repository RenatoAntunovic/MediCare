using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Catalog.Treatments.Queries.GetById
{
    public class GetTreatmentsByIdQueryDto
    {
        public required int Id { get; set; }
        public required string ServiceName { get; set; }
        public required decimal Price { get; set; }
        public required string Description { get; set; }
        public required int TreatmentsCategoryId { get; set; }
        public required string TreatmentsCategoryName { get; set; } //only the category name is used here, because putting the whole entity in the DTO throws an error and is not recommended
        public required string ImagePath { get; set; }  
        public required bool isEnabled { get; set; }
    }
}
