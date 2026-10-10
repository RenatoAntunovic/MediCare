namespace MediCare.Infrastructure.Database.Configurations.Identity;

public sealed class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshTokenEntity>
{
    public void Configure(EntityTypeBuilder<RefreshTokenEntity> b)
    {
        b.ToTable("RefreshTokens");

        b.HasKey(x => x.Id);

        b.HasIndex(x => new { x.UserId, x.TokenHash })
            .IsUnique();

        b.Property(x => x.TokenHash)
            .IsRequired();

        b.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        b.Property(x => x.IsRevoked)
            .HasDefaultValue(false);

        b.Property(x => x.Fingerprint)
            .HasMaxLength(200);

        b.HasOne(x => x.User)          // A refresh token belongs to one user
    .WithMany(u => u.RefreshTokens) // A user can have many refresh tokens
    .HasForeignKey(x => x.UserId)
    .IsRequired();
    }
}
