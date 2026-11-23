namespace MediCare.Application.Modules.Procurement.Suppliers.Queries.GetById;

public sealed class GetSupplierByIdQuery : IRequest<GetSupplierByIdQueryDto>
{
    public int Id { get; set; }
}

public sealed class GetSupplierByIdQueryDto
{
    public required int Id { get; init; }
    public required string CompanyName { get; init; }
    public required string ContactName { get; init; }
    public required string Email { get; init; }
    public required string Phone { get; init; }
    public required string Address { get; init; }
    public required string City { get; init; }
    public required bool IsEnabled { get; init; }
    public required List<int> MedicineIds { get; init; }
}

public sealed class GetSupplierByIdQueryHandler(IAppDbContext ctx)
    : IRequestHandler<GetSupplierByIdQuery, GetSupplierByIdQueryDto>
{
    public async Task<GetSupplierByIdQueryDto> Handle(GetSupplierByIdQuery request, CancellationToken ct)
    {
        return await ctx.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == request.Id)
            .Select(s => new GetSupplierByIdQueryDto
            {
                Id = s.Id,
                CompanyName = s.CompanyName,
                ContactName = s.ContactName,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                City = s.City,
                IsEnabled = s.IsEnabled,
                MedicineIds = s.MedicineSuppliers.Select(l => l.MedicineId).ToList()
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new MediCareNotFoundException($"Dobavljač (ID={request.Id}) nije pronađen.");
    }
}
