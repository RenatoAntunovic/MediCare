using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Application.Modules.ForLater.Command.Delete;


public class DeleteForLaterCommandHandler : IRequestHandler<DeleteForLaterCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly IAppCurrentUser _currentUser;

    public DeleteForLaterCommandHandler(IAppDbContext context, IAppCurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteForLaterCommand request, CancellationToken cancellationToken)
    {

        if (_currentUser.UserId == null)
        {
            throw new UnauthorizedAccessException();
        }

        int userId = _currentUser.UserId.Value;

        var forLater = await _context.ForLater
            .FirstOrDefaultAsync(f =>
                f.Id == request.Id &&
                f.UserId == userId &&
                !f.IsDeleted,
                cancellationToken);

        if (forLater == null)
        {
            throw new KeyNotFoundException($"Medicine for later with Id {request.Id} not found.");
        }

        // Obriši stavku iz baze
        _context.ForLater.Remove(forLater);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

