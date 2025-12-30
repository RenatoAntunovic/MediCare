using MediatR;
using MediCare.Application.Modules.ForLater.Queries;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Application.Modules.ForLater.Query.GetForLaterQueryHandler
{
    public class GetForLaterQueryHandler
        : IRequestHandler<GetForLaterQuery, List<ForLaterDto>>
    {
        private readonly IAppDbContext _context;
        private readonly IAppCurrentUser _currentUser;

        public GetForLaterQueryHandler(
            IAppDbContext context,
            IAppCurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<ForLaterDto>> Handle(
            GetForLaterQuery request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new UnauthorizedAccessException();

            int userId = _currentUser.UserId.Value;

            return await _context.ForLater
                .Where(f =>
                    f.UserId == userId &&
                    !f.IsDeleted)
                .Select(f => new ForLaterDto
                {
                    Id = f.Id,
                    MedicineId = f.MedicineId,
                    MedicineName = f.Medicine.Name,
                    Price = f.Medicine.Price,
                    ImagePath = f.Medicine.ImagePath
                })
                .ToListAsync(cancellationToken);
        }
    }
}
