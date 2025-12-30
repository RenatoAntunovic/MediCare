using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Cart.Command.Delete
{
    public class DeleteCartItemCommandHandler : IRequestHandler<DeleteCartItemCommand, Unit>
    {
        private readonly IAppDbContext _context;

        public DeleteCartItemCommandHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteCartItemCommand request, CancellationToken cancellationToken)
        {
            // Pronađi cart item po Id
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == request.Id, cancellationToken);

            if (cartItem == null)
            {
                throw new KeyNotFoundException($"CartItem with Id {request.Id} not found.");
            }

            // Obriši stavku iz baze
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
