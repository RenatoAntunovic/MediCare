using MediCare.Application.Modules.ForLater.Command.Create;


public class AddToForLaterCommandHandler
    : IRequestHandler<AddToForLaterCommand, int>
{
    private readonly IAppDbContext _context;
    private readonly IAppCurrentUser _currentUser;

    public AddToForLaterCommandHandler(
        IAppDbContext context,
        IAppCurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(
        AddToForLaterCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == null)
            throw new UnauthorizedAccessException();

        int userId = _currentUser.UserId.Value;

        var existing = await _context.ForLater
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.MedicineId == request.MedicineId &&
                !x.IsDeleted,
                cancellationToken);

        if (existing != null)
        {        
            return existing.Id;
        }

        var forLater = new ForLater
        {
            UserId = userId,
            MedicineId = request.MedicineId
        };

        _context.ForLater.Add(forLater);
        await _context.SaveChangesAsync(cancellationToken);

        return forLater.Id;
    }
}
