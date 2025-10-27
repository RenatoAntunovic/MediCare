using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Market.Domain.Common;

namespace Market.Domain.Entities.Catalog
{
    public class Payments : BaseEntity
    {
        public int OrderId { get; set; }
        public Orders Order { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public int PaymentStatusId { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}
