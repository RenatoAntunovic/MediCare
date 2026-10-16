using MediatR;
using MediCare.Application.Modules.Cart.Common;
using MediCare.Application.Modules.Cart.Command.AddToCartFromForLater;
using Microsoft.EntityFrameworkCore;

public class AddToCartFromForLaterHandler : IRequestHandler<AddToCartFromForLaterCommand, bool>
{
    private readonly IAppDbContext _context;

    public AddToCartFromForLaterHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AddToCartFromForLaterCommand request, CancellationToken cancellationToken)
    {
        var forLater = await _context.ForLater
            .Include(f => f.Medicine)
            .FirstOrDefaultAsync(f => f.Id == request.ForLaterId && f.UserId == request.UserId, cancellationToken);

        if (forLater == null || forLater.Medicine == null)
            return false;

        // Same rules as everywhere else: disabled medicine and the cart limit → 409
        await CartRules.AddAsync(_context, request.UserId, forLater.Medicine, request.Quantity, cancellationToken);

        // "For later" means "not yet" – once it's in the cart it leaves the list (soft delete)
        _context.ForLater.Remove(forLater);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
