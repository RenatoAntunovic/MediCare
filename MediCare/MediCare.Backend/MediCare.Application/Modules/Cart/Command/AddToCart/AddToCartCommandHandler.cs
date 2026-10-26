using System;
using MediCare.Application.Modules.Cart.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Domain.Entities.HospitalRecords;

namespace MediCare.Application.Modules.Cart.Command.AddToCart
{
    public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, int>
    {
        private readonly IAppDbContext _context;
        private readonly IAppCurrentUser _currentUser;

        public AddToCartCommandHandler(
            IAppDbContext context,
            IAppCurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new UnauthorizedAccessException();

            int userId = _currentUser.UserId.Value;

            // Missing medicine → 404; disabled medicine or over the cart limit → 409 (CartRules)
            var medicine = await _context.Medicine
                .FirstOrDefaultAsync(x => x.Id == request.MedicineId, cancellationToken)
                ?? throw new MediCareNotFoundException($"Medicine with Id {request.MedicineId} not found.");

            var cartItem = await CartRules.AddAsync(_context, userId, medicine, request.Quantity, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            return cartItem.Id;
        }
    }

}
