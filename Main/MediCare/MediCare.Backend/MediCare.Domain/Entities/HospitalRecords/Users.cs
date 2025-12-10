using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Common;
using MediCare.Domain.Entities.Identity;

namespace MediCare.Domain.Entities.Catalog
{
    public class Users : BaseEntity
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Adress { get; set; }
        public string City { get; set; }

        public Roles Role { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber {  get; set; }
        public bool IsEnabled { get; set; }
        public string PasswordHash { get; set; }
        public int TokenVersion { get; set; } = 0;// For global revocation
        public ICollection<RefreshTokenEntity> RefreshTokens { get; private set; } = new List<RefreshTokenEntity>();
    }
}
