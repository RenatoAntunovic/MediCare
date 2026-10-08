namespace MediCare.Application.Modules.Reservations.Queries.GetById
{
    public class GetReservationByIdQueryHandler : IRequestHandler<GetReservationByIdQuery, ReservationDetailDto>
    {
        private readonly IAppDbContext _context;

        public GetReservationByIdQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ReservationDetailDto> Handle(GetReservationByIdQuery request, CancellationToken cancellationToken)
        {
            var reservation = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.Id == request.ReservationId && r.UserId == request.UserId)
                .Select(r => new ReservationDetailDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    TreatmentId = r.TreatmentId,
                    TreatmentName = r.Treatment.ServiceName,
                    TreatmentDescription = r.Treatment.Description,
                    ReservationDate = r.ReservationDate,
                    ReservationTime = r.ReservationTime,
                    OrderStatus = r.OrderStatus.StatusName,
                    Price = r.Price,
                    Notes = r.Notes
                })
                .FirstOrDefaultAsync(cancellationToken);

            return reservation
                ?? throw new MediCareNotFoundException("Rezervacija ne postoji ili nemate pristup.");
        }
    }
}
