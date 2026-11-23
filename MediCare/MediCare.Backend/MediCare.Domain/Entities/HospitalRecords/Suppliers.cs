using MediCare.Domain.Common;

namespace MediCare.Domain.Entities.HospitalRecords
{
    public class Suppliers : BaseEntity
    {
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;

        /// <summary>Medicines this supplier delivers (join table MedicineSuppliers).</summary>
        public ICollection<MedicineSuppliers> MedicineSuppliers { get; set; } = new List<MedicineSuppliers>();

        public static class Constraints
        {
            public const int CompanyNameMaxLength = 100;
            public const int ContactNameMaxLength = 100;
            public const int EmailMaxLength = 100;
            public const int PhoneMaxLength = 30;
            public const int AddressMaxLength = 200;
            public const int CityMaxLength = 50;
        }
    }
}
