using System;
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


            // 1️⃣ Nađi ili kreiraj cart
            var cart = await _context.Carts
                .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken);

            if (cart == null)
            {
                cart = new Carts { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // 2️⃣ Nađi medicine
            var medicine = await _context.Medicine
                .FirstAsync(x => x.Id == request.MedicineId, cancellationToken);

            // 3️⃣ Provjeri postoji li već item
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(x =>
                    x.CartId == cart.Id &&
                    x.MedicineId == request.MedicineId &&
                    !x.IsDeleted,
                    cancellationToken);

            if (cartItem != null)
            {
                cartItem.Quantity += request.Quantity;
                cartItem.Price = medicine.Price * cartItem.Quantity;
            }
            else
            {
                cartItem = new CartItems
                {
                    CartId = cart.Id,
                    MedicineId = request.MedicineId,
                    Quantity = request.Quantity,
                    Price = medicine.Price * request.Quantity
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return cartItem.Id;
        }
    }

}
