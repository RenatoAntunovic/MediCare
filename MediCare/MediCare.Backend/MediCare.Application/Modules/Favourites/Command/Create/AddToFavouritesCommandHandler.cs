using MediCare.Application.Abstractions;
using MediCare.Application.Modules.Favourites.Command.Create;
using MediCare.Domain.Entities.HospitalRecords;
using Microsoft.EntityFrameworkCore;


    public class AddToFavouritesCommandHandler
        : IRequestHandler<AddToFavouritesCommand, int>
    {
        private readonly IAppDbContext _context;
        private readonly IAppCurrentUser _currentUser;

        public AddToFavouritesCommandHandler(
            IAppDbContext context,
            IAppCurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(
            AddToFavouritesCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId == null)
                throw new UnauthorizedAccessException();

            int userId = _currentUser.UserId.Value;

            // 🔎 Check whether it is already in favourites
            var existing = await _context.Favourites
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.MedicineId == request.MedicineId &&
                    !x.IsDeleted,
                    cancellationToken);

            if (existing != null)
            {
                // already exists → just return the ID
                return existing.Id;
            }

            // ➕ Add a new favourite
            var favourite = new Favourites
            {
                UserId = userId,
                MedicineId = request.MedicineId
            };

            _context.Favourites.Add(favourite);
            await _context.SaveChangesAsync(cancellationToken);

            return favourite.Id;
        }
    }
