using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.Cart.Queries
{
    public class GetUserCartQueryHandler : IRequestHandler<GetUserCartQuery, UserCartDto>
    {
        private readonly IAppDbContext _context;
        private readonly IAppCurrentUser _currentUser;

        public GetUserCartQueryHandler(IAppDbContext context, IAppCurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<UserCartDto> Handle(GetUserCartQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == null)
                throw new Exception("User not logged in.");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Medicine)
                .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId && !c.IsDeleted, cancellationToken);

            if (cart == null)
                return new UserCartDto();

            var dto = new UserCartDto
            {
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    MedicineId = ci.MedicineId,
                    Name = ci.Medicine.Name,
                    Quantity = ci.Quantity,
                    Price = ci.Price,
                    ImagePath = ci.Medicine.ImagePath
                }).ToList()
            };

            return dto;
        }
    }
}
