using SupplierEntity = MediCare.Domain.Entities.HospitalRecords.Suppliers;


namespace MediCare.Application.Modules.Procurement.Suppliers.Commands;

/// <summary>Fields shared by the create and update commands (one validator for both).</summary>
public abstract class SupplierFields
{
    public string CompanyName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;

    /// <summary>Medicines the supplier delivers (may be empty).</summary>
    public List<int> MedicineIds { get; set; } = [];
}

/// <summary>Backend validation – the same rules as the Angular supplier form.</summary>
public sealed class SupplierFieldsValidator : AbstractValidator<SupplierFields>
{
    // Digits, spaces and + - / ( ), e.g. "+387 61 123-456"
    public const string PhonePattern = @"^[0-9+\-\s/()]{6,30}$";

    public SupplierFieldsValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MinimumLength(2).WithMessage("Company name must have at least 2 characters.")
            .MaximumLength(SupplierEntity.Constraints.CompanyNameMaxLength)
            .WithMessage($"Company name can be at most {SupplierEntity.Constraints.CompanyNameMaxLength} characters long.");

        RuleFor(x => x.ContactName)
            .NotEmpty().WithMessage("Contact person is required.")
            .MinimumLength(2).WithMessage("Contact person must have at least 2 characters.")
            .MaximumLength(SupplierEntity.Constraints.ContactNameMaxLength)
            .WithMessage($"Contact person can be at most {SupplierEntity.Constraints.ContactNameMaxLength} characters long.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not valid.")
            .MaximumLength(SupplierEntity.Constraints.EmailMaxLength)
            .WithMessage($"Email can be at most {SupplierEntity.Constraints.EmailMaxLength} characters long.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Matches(PhonePattern).WithMessage("Phone may contain only digits, spaces and + - / ( ) (6 to 30 characters).");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(SupplierEntity.Constraints.AddressMaxLength)
            .WithMessage($"Address can be at most {SupplierEntity.Constraints.AddressMaxLength} characters long.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(SupplierEntity.Constraints.CityMaxLength)
            .WithMessage($"City can be at most {SupplierEntity.Constraints.CityMaxLength} characters long.");

        RuleFor(x => x.MedicineIds)
            .Must(ids => ids.All(id => id > 0)).WithMessage("Medicine ids must be positive.")
            .Must(ids => ids.Count <= 100).WithMessage("At most 100 medicines can be linked to one supplier.");
    }
}

/// <summary>Business checks shared by create and update (uniqueness, existing medicines, link sync).</summary>
internal static class SupplierRules
{
    public static async Task EnsureUniqueAsync(IAppDbContext ctx, SupplierFields cmd, int? exceptId, CancellationToken ct)
    {
        var name = cmd.CompanyName.Trim().ToLower();
        if (await ctx.Suppliers.AnyAsync(s => s.Id != exceptId && s.CompanyName.ToLower() == name, ct))
            throw new MediCareConflictException("Dobavljač s tim nazivom već postoji.");

        var email = cmd.Email.Trim().ToLower();
        if (await ctx.Suppliers.AnyAsync(s => s.Id != exceptId && s.Email.ToLower() == email, ct))
            throw new MediCareConflictException("Dobavljač s tim emailom već postoji.");
    }

    public static async Task<List<int>> ValidMedicineIdsAsync(IAppDbContext ctx, List<int> medicineIds, CancellationToken ct)
    {
        var ids = medicineIds.Distinct().ToList();
        if (ids.Count == 0) return ids;

        var existing = await ctx.Medicine
            .Where(m => ids.Contains(m.Id))
            .Select(m => m.Id)
            .ToListAsync(ct);

        if (existing.Count != ids.Count)
            throw new MediCareNotFoundException("Jedan ili više odabranih lijekova ne postoji.");

        return ids;
    }

    public static void Apply(SupplierEntity entity, SupplierFields cmd)
    {
        entity.CompanyName = cmd.CompanyName.Trim();
        entity.ContactName = cmd.ContactName.Trim();
        entity.Email = cmd.Email.Trim();
        entity.Phone = cmd.Phone.Trim();
        entity.Address = cmd.Address.Trim();
        entity.City = cmd.City.Trim();
        entity.IsEnabled = cmd.IsEnabled;
    }
}
