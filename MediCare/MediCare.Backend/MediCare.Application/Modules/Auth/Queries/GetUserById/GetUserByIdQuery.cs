using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Application.Modules.Auth.Commands.Register;

namespace MediCare.Application.Modules.Auth.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<GetUserByIdQueryDto>
    {
        public int Id { get; }
        public GetUserByIdQuery(int id) // <--- constructor that takes the id
        {
            Id = id;
        }
    }
}
