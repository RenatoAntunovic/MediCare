namespace MediCare.Application.Modules.Cart.Command.Delete
{
    public class DeleteCartItemCommandHandler : IRequestHandler<DeleteCartItemCommand, Unit>
    {
        private readonly IAppDbContext _context;
        private readonly IAppCurrentUser _currentUser;

        public DeleteCartItemCommandHandler(IAppDbContext context, IAppCurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Unit> Handle(DeleteCartItemCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new UnauthorizedAccessException();

            int userId = _currentUser.UserId.Value;

            // Find the item only inside the current user's cart
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.Id == request.Id &&
                    ci.Cart.UserId == userId,
                    cancellationToken);

            // Someone else's item looks the same as a missing one (404), so IDs can't be probed
            if (cartItem == null)
                throw new MediCareNotFoundException($"Cart item with Id {request.Id} not found.");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}