using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Auth.Commands.Register
{
    public class RegisterCommandDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
