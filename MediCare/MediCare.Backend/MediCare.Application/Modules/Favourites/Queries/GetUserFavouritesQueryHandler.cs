using MediatR;
using MediCare.Application.Modules.Favourites.Queries;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Application.Modules.Favourites.Query.GetUserFavourites
{
    public class GetUserFavouritesQueryHandler
        : IRequestHandler<GetUserFavouritesQuery, List<FavouriteMedicineDto>>
    {
        private readonly IAppDbContext _context;
        private readonly IAppCurrentUser _currentUser;

        public GetUserFavouritesQueryHandler(
            IAppDbContext context,
            IAppCurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<FavouriteMedicineDto>> Handle(
            GetUserFavouritesQuery request,
            CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new UnauthorizedAccessException();

            int userId = _currentUser.UserId.Value;

            return await _context.Favourites
                .Where(f =>
                    f.UserId == userId &&
                    !f.IsDeleted)
                .Select(f => new FavouriteMedicineDto
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
