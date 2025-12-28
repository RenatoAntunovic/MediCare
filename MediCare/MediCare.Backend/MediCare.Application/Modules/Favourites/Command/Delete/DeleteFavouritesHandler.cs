using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Application.Modules.Favourites.Command.Delete;


    public class DeleteFavouritesCommandHandler : IRequestHandler<DeleteFavouritesCommand, Unit>
    {
        private readonly IAppDbContext _context;
        private readonly IAppCurrentUser _currentUser;

        public DeleteFavouritesCommandHandler(IAppDbContext context,IAppCurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Unit> Handle(DeleteFavouritesCommand request, CancellationToken cancellationToken)
        {

            if (_currentUser.UserId == null) { 
                throw new UnauthorizedAccessException();
            }

            int userId = _currentUser.UserId.Value;

        var favourites = await _context.Favourites
            .FirstOrDefaultAsync(f =>
                f.Id == request.Id &&
                f.UserId == userId &&
                !f.IsDeleted,
                cancellationToken);

        if (favourites == null)
            {
                throw new KeyNotFoundException($"Favourite with Id {request.Id} not found.");
            }

            // Obriši stavku iz baze
            _context.Favourites.Remove(favourites);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

