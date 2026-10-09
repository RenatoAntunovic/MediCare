namespace MediCare.Application.Modules.Auth.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdQueryDto>
    {
        private readonly IAppDbContext _context;

        public GetUserByIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<GetUserByIdQueryDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Id == request.Id)
                .Select(u => new GetUserByIdQueryDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    DateOfBirth = u.DateOfBirth,
                    Address = u.Adress,
                    City = u.City,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role.Name  // role name, e.g. "User" or "Admin"
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                throw new MediCareNotFoundException($"User with Id {request.Id} not found.");
            }

            return user;
        }
    }
}
