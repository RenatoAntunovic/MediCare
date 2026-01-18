namespace MediCare.Application.Modules.Reservations.CreateReservation.Queries.GetReservations
{
    public class GetUserReservationsQueryHandler : IRequestHandler<GetUserReservationsQuery, List<ReservationDto>>
    {
        private readonly IAppDbContext _context;

        public GetUserReservationsQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReservationDto>> Handle(GetUserReservationsQuery request, CancellationToken cancellationToken)
        {
            var reservations = await _context.Reservations
                .Where(r => r.UserId == request.UserId)
                .Include(r => r.Treatment)
                .Include(r => r.OrderStatus)
                .OrderByDescending(r => r.ReservationDate)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    TreatmentId = r.TreatmentId,
                    TreatmentName = r.Treatment.ServiceName,
                    ReservationDate = r.ReservationDate,
                    ReservationTime = r.ReservationTime,
                    OrderStatus = r.OrderStatus.StatusName,
                    Price = r.Price
                })
                .ToListAsync(cancellationToken);

            return reservations;
        }
    }
}
