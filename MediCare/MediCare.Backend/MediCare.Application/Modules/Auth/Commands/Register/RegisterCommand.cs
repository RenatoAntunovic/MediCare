using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Auth.Commands.Register
{
    public class RegisterCommand:IRequest<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }  // popravio tipografsku grešku Adress -> Address
        public string City { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // plain text iz inputa
        public string PhoneNumber { get; set; }
    }
}
